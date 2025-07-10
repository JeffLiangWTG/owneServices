using System.Collections.Generic;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	internal class CuscarFriConsignmentInserter
	{
		public CuscarFriConsignmentInserter(EDIMessage inboundEdiMessageForAuditing, ILogger serviceLogger, CuscarWithFlagsToShowWhatsSet flagableCuscar, ZString messageSubType)
		{
			this.inboundMsg = inboundEdiMessageForAuditing;
			this.serviceLogger = serviceLogger;
			this.cuscar = flagableCuscar;
			MessageSubType = messageSubType;
		}

		public CuscarFriConsignmentInserter(EDIMessage inboundEdiMessageForAuditing, ILogger serviceLogger, CuscarWithFlagsToShowWhatsSet flagableCuscar)
			: this(inboundEdiMessageForAuditing, serviceLogger, flagableCuscar, CcsukTransmissionMessageFunction.CUSCAR.FRI.Subcode)
		{
		}

		internal ICcsukCusAwb HandleInsertionOfConsignment()
		{
			ICcsukCusAwb bizObjFromWhichToHangMessage = null;
			string interpretation = null;
			if (IsHawb)
			{
				var mawbWithStatusFlag = FindOrMakeMawbWhileLockingConsol();
				var mawb = mawbWithStatusFlag.Mawb;
				var hawb = new CusHAWB.Loader(inboundMsg.Factory).FindExistingHawbOnMawb(mawb, cuscar.HouseAirWaybillNumber) ?? AddNewHawbWhileLockingShipment(mawb);
				string hawbInterpretation = CuscarFrcConsignmentUpdater.UpdateUsingCuscarData_Hawb(hawb, cuscar, ColumnsToShowInInterpretation.FieldNameAndPersistentValue, true);
				interpretation = hawbInterpretation;
				if (mawbWithStatusFlag.IsCreatedNewRatherThanFoundExisting)
				{
					CuscarFrcConsignmentUpdater.UpdateUsingCuscarData_Mawb(mawb, cuscar, ColumnsToShowInInterpretation.FieldNameAndPersistentValue, isNewNotUpdated: true, sendEmailToo: false);
					interpretation = string.Format("<h5>MAWB {2}{3} {0} created</h5> {1}", mawb.CM_MAWB, hawbInterpretation, mawb.CargoTerminalOperatorAirport, mawb.CargoTerminalOperator);
				}
				bizObjFromWhichToHangMessage = hawb;
				hawb.Messages.Add(inboundMsg);
				inboundMsg.EM_GB = hawb.Branch.PK;
				hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				if (inboundMsg.Interchange != null && hawb.Profile.IsEmpty)
				{
					hawb.Profile = inboundMsg.Interchange.EI_To.Left(hawb.ProfileInfo.MaxLength);
				}
			}
			else if (IsMawb)
			{
				var mawb = FindOrMakeMawbWhileLockingConsol().Mawb;
				interpretation = CuscarFrcConsignmentUpdater.UpdateUsingCuscarData_Mawb(mawb, cuscar, ColumnsToShowInInterpretation.FieldNameAndPersistentValue, true);
				bizObjFromWhichToHangMessage = mawb;
				mawb.Messages.Add(inboundMsg);
				inboundMsg.EM_GB = mawb.Branch.PK;
				mawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			}
			inboundMsg.EM_MessageInterpretation = MessagePrettierCss.CSS + "<h3>Record inserted using community data</h3>" + interpretation;
			inboundMsg.EM_Status = EDIMessage.Status.Received;
			inboundMsg.EM_MessageType = CcsukTransmissionMessageFunction.CUSCAR.Code;
			inboundMsg.EM_MessageSubType = MessageSubType;
			return bizObjFromWhichToHangMessage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		CusHAWB AddNewHawbWhileLockingShipment(CusMAWB parentMawb)
		{
			CusHAWB hawb = null;
			var hawbNo = cuscar.HouseAirWaybillNumber;
			var mawbNo = cuscar.AirlinePrefix + cuscar.AirWaybillSerialNumber;

			var shipment = ParserHelper.FindBritishShipmentWithThisHawbNumberAndOnThisMasterAndMaybeOriginToo(hawbNo, mawbNo, cuscar.AirportOfOrigin, inboundMsg);

			if (shipment != null)
			{
				Log(string.Format("Found shipment for house data. FRI-Hawb={0}, FRI-Mawb={1}, FRI-location={2}{5}, shipment found is {3}, message #{4}", hawbNo, mawbNo, cuscar.AirportOfArrival, shipment.JS_UniqueConsignRef, inboundMsg.EM_MessageNum, cuscar.CargoTerminalOperator));
				// No hawbs exist, make new one on shipment
				var mutex = CusHAWB.CreateMutexForShipment(shipment.PK, Core.Constants.CountryCodes.UnitedKingdom);
				if (mutex.Lock())
				{
					hawb = CreateNewHawbOnMawb(parentMawb, hawbNo);
					hawb.CS_JS = shipment.PK;
					hawb.Factory.Saved += delegate
					{ mutex.Unlock(); };
					Log(string.Format("Created new house bill {0} on master {3}{4}{1}, inbound message #{2}", hawbNo, parentMawb.CM_MAWB, inboundMsg.EM_MessageNum, parentMawb.CargoTerminalOperatorAirport, parentMawb.CargoTerminalOperator));
				}
				else
				{
					hawb = CreateNewHawbOnMawb(parentMawb, hawbNo);
					var msg = string.Format("When processing an inbound FRI/FRC message, {0} could not lock the shipment ({1}) for this hawb. Trying to create new CusHAWB using inbound FRI data, mutex is locked, user is probably trying to create a CusHAWB from a shipment with same details at the same time. Hawb is created but not linked to shipment; open the shipment and enter its CCS-UK tab to forge this link if desired. Mawb & Hawb number {2} {3}, message number {4}.", BrandingFactory.Instance.ProductName, shipment.JS_UniqueConsignRef, mawbNo, hawbNo, inboundMsg.EM_MessageNum);
					ParserHelper.SendEmailWarning(msg, hawb, inboundMsg.Factory);
					Log(string.Format("Made empty CusHAWB {3}{1} but not linked to shipment {0} (could not get lock on mutex, warning email sent); message #{2}", shipment.JS_UniqueConsignRef, hawb.ReferenceNumber, inboundMsg.EM_MessageNum, hawb.CS_WarehouseLocation));
				}
			}
			else
			{
				hawb = FindOrMakeHawbOnMawb(parentMawb, hawbNo);
			}
			return hawb;
		}

		CusHAWB FindOrMakeHawbOnMawb(CusMAWB mawb, string hawbNo)
		{
			CusHAWB hawb = null;
			if (mawb.ChildBills.Count > 0)
			{
				hawb = new CusHAWB.Loader(inboundMsg.Factory).FindExistingHawbOnMawb(mawb, hawbNo);
			}
			if (hawb == null)
			{
				hawb = CreateNewHawbOnMawb(mawb, hawbNo);
			}
			return hawb;
		}

		CusHAWB CreateNewHawbOnMawb(CusMAWB mawb, ZString hawbNo)
		{
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = hawbNo;
			serviceLogger.Log(LogType.Information, string.Format("Created new house bill {0} on master {3}{4}{1}; message #{2}", hawbNo, mawb.CM_MAWB, inboundMsg.EM_MessageNum, mawb.CargoTerminalOperatorAirport, mawb.CargoTerminalOperator));
			return hawb;
		}

		MawbAndNewOrFoundFlag FindOrMakeMawbWhileLockingConsol()
		{
			CusMAWB mawb = null;
			var createdNewRatherThanFoundExistingMawb = false;
			var loader = new CusMAWB.Loader(inboundMsg.Factory);
			var mawbNo = cuscar.AirlinePrefix + cuscar.AirWaybillSerialNumber;
			var airportAndShedInFri = cuscar.AirportOfArrival + cuscar.CargoTerminalOperator;
			var consol = ParserHelper.FindBritishConsolWithThisMawbNumber(inboundMsg.Factory, mawbNo);
			if (consol != null)
			{
				Log(string.Format("Found consol with inbound data. FRI-Mawb={3}{0}, Consol={1}; message #{2}", mawbNo, consol.JK_UniqueConsignRef, inboundMsg.EM_MessageNum, airportAndShedInFri));
				var allMawbsOnConsol = loader.FindMatchingMAWBs(consol.PK, true);
				var allCcsukMawbsAtThisLocationTiedToThisConsol = (from CusMAWB mb in allMawbsOnConsol
																   where mb.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk
																   && mb.CargoTerminalOperatorAirport + mb.CargoTerminalOperator == airportAndShedInFri
																   orderby mb.CM_SystemCreateTimeUtc descending
																   select mb);
				var ccsukMawbCount = allCcsukMawbsAtThisLocationTiedToThisConsol.Count();
				if (ccsukMawbCount == 1)
				{
					// Consol with matching mawb number exists, has a key link to one CusMawb at same location as message
					mawb = allCcsukMawbsAtThisLocationTiedToThisConsol.First();
					Log(string.Format("Found existing CusMAWB {3}{1} on consol {0}; message #{2}", consol.JK_UniqueConsignRef, mawbNo, inboundMsg.EM_MessageNum, airportAndShedInFri));
				}
				else if (ccsukMawbCount == 0)
				{
					// No CusMAWBs are tied to this consol, but one might exist unlinked
					mawb = (from CusMAWB m in loader.FindFromMawbNumber(mawbNo, 12, "", airportAndShedInFri) orderby m.CM_SystemCreateTimeUtc descending select m).FirstOrDefault();
					if (mawb == null)
					{
						mawb = CreateNewCusMAWBOnConsolAtLocation(out mawb, mawbNo, consol, airportAndShedInFri);
						createdNewRatherThanFoundExistingMawb = true;
					}
					else
					{
						mawb.CM_JK = consol.PK; // link for opening from the CCSUK module						
					}
				}
				else // more than one CusMAWB exists at this location
				{
					mawb = allCcsukMawbsAtThisLocationTiedToThisConsol.First();
					serviceLogger.Log(LogType.Warning, string.Format("Found more than one existing CusMAWB {0} on consol {1} at this location of goods {4}; selected first mawb {2} ; message #{3}", mawbNo, consol.JK_UniqueConsignRef, mawb.ReferenceNumber, inboundMsg.EM_MessageNum, airportAndShedInFri));
				}
			}
			else
			{
				// No consol
				var britishMawbsWithoutConsolAtThisLocation = FindAllMawbsAtThisLocationWithinAYearOrPreArrival(loader, mawbNo, airportAndShedInFri);
				var mawbsWithoutConsolCount = britishMawbsWithoutConsolAtThisLocation.Count();
				if (mawbsWithoutConsolCount == 0)
				{
					mawb = MakeNewEmptyMawbForOrphanHawb();
					createdNewRatherThanFoundExistingMawb = true;
				}
				else if (mawbsWithoutConsolCount == 1)
				{
					mawb = britishMawbsWithoutConsolAtThisLocation.First();
					Log(string.Format("Found one existing CusMAWB {3}{0} without consol at this location of goods; selected mawb {1} ; message #{2}", mawbNo, mawb.ReferenceNumber, inboundMsg.EM_MessageNum, airportAndShedInFri));
				}
				else
				{
					mawb = britishMawbsWithoutConsolAtThisLocation.First();
					serviceLogger.Log(LogType.Warning, string.Format("Found more than one existing CusMAWB {0} without consol at this location of goods {3}; selected first mawb {1} ; message #{2}", mawbNo, mawb.ReferenceNumber, inboundMsg.EM_MessageNum, airportAndShedInFri));
				}
			}
			return new MawbAndNewOrFoundFlag(mawb, createdNewRatherThanFoundExistingMawb);
		}

		public static IEnumerable<CusMAWB> FindAllMawbsAtThisLocationWithinAYearOrPreArrival(CusMAWB.Loader loader, string mawbNo, string airportAndShed)
		{
			var mawbsWithThisMawbNumber = loader.FindMatchingMAWBs(mawbNo);
			var britishMawbsWithoutConsolAtThisLocation = (from CusMAWB mb in mawbsWithThisMawbNumber
														   where mb.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk
														   && mb.CM_JK == ZGuid.Empty
														   && (mb.CM_ArrivalDate.IsEmpty || mb.CM_ArrivalDate > ZDateTime.Now.AddMonths(-12))
														   && mb.CargoTerminalOperatorAirport + mb.CargoTerminalOperator == airportAndShed
														   orderby mb.CM_SystemCreateTimeUtc descending 
														   select mb);
			return britishMawbsWithoutConsolAtThisLocation;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		CusMAWB CreateNewCusMAWBOnConsolAtLocation(out CusMAWB mawb, string mawbNo, ForwardingConsol consol, string airportAndShed)
		{
			var mutex = CusMAWB.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedKingdom);
			if (mutex.Lock())
			{
				mawb = MakeNewEmptyMawbForOrphanHawb();
				mawb.MasterLevelHouseHelper.CS_WarehouseLocation = airportAndShed;
				mawb.CM_JK = consol.PK;
				Log(string.Format("Made empty CusMAWB {3}{1} on consol {0}; message #{2}", consol.JK_UniqueConsignRef, mawb.ReferenceNumber, inboundMsg.EM_MessageNum, airportAndShed));
				mawb.Factory.Saved += delegate
				{ mutex.Unlock(); };
			}
			else
			{
				mawb = MakeNewEmptyMawbForOrphanHawb();
				mawb.MasterLevelHouseHelper.CS_WarehouseLocation = airportAndShed;
				var msg = string.Format("When processing an inbound FRI/FRC message, {0} could not lock the consol ({1}) for this mawb. Trying to create new CusMAWB using inbound FRI data, mutex is locked, user is probably trying to create a CusMAWB from a consol with same mawb# at the same time. Mawb is created but not linked to consol; open the consol and enter its CCS-UK tab to forge this link if desired. Mawb number {2}, message number {3}.", BrandingFactory.Instance.ProductName, consol.JK_UniqueConsignRef, mawbNo, inboundMsg.EM_MessageNum);
				ParserHelper.SendEmailWarning(msg, mawb, inboundMsg.Factory);
				Log(string.Format("Made empty CusMAWB {3}{1} but not linked to consol {0} (could not get lock on mutex, warning email sent); message #{2}", consol.JK_UniqueConsignRef, mawb.ReferenceNumber, inboundMsg.EM_MessageNum, airportAndShed));
			}
			return mawb;
		}

		CusMAWB MakeNewEmptyMawbForOrphanHawb()
		{
			var mawb = inboundMsg.Factory.New<CusMAWB>();
			mawb.CM_MAWB = cuscar.AirlinePrefix + cuscar.AirWaybillSerialNumber;
			if (inboundMsg.Interchange != null)
			{
				mawb.Profile = inboundMsg.Interchange.EI_To.Replace("/", "");
				LicenceAndPimaHelper.SetBranchFromPimaForNewAwb(mawb);
			}
			Log(string.Format("Created new master {0} using inbound data from community because an insertable message was received and no mawb was found. Message #{1}", mawb.CM_MAWB, inboundMsg.EM_MessageNum));
			return mawb;
		}

		bool IsHawb
		{
			get { return !cuscar.HouseAirWaybillNumber.IsEmpty && cuscar.HouseAirWaybillNumber != "M"; }
		}

		bool IsMawb
		{
			get { return !cuscar.AirlinePrefix.IsEmpty && (cuscar.HouseAirWaybillNumber.IsEmpty || cuscar.HouseAirWaybillNumber == "M"); }
		}

		void Log(string msg)
		{
			if (serviceLogger != null)
			{
				serviceLogger.Log(LogType.Information, msg);
			}
		}

		public ZString MessageSubType { get; set; }

		readonly EDIMessage inboundMsg;
		readonly ILogger serviceLogger;
		readonly CuscarWithFlagsToShowWhatsSet cuscar;
	}
}
