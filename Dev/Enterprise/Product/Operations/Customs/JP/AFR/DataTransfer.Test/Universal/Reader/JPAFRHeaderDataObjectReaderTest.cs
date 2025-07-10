using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	partial class JPAFRBillsDataObjectReaderTest : JPManifestDataObjectReaderTestHelper
	{
		public void TestImportingJPAFRHeaderDataDateFallback()
		{
			var headerDataObject = SetupAFRHeader("MB2343");
			headerDataObject.SetDateCollection(() => new List<Date>());
			var loadingActual = Date.New(DateType.LoadingDate, ZBool.True, new ZDateTime(2010, 1, 5));
			headerDataObject.DateCollection.Add(loadingActual);
			var loadingEstimate = Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2010, 1, 2));
			headerDataObject.DateCollection.Add(loadingEstimate);
			var departureActual = Date.New(DateType.Departure, ZBool.True, new ZDateTime(2010, 1, 4));
			headerDataObject.DateCollection.Add(departureActual);
			var departureEstimate = Date.New(DateType.Departure, ZBool.False, new ZDateTime(2010, 1, 3));
			headerDataObject.DateCollection.Add(departureEstimate);
			var dischargeActual = Date.New(DateType.DischargeDate, ZBool.True, new ZDateTime(2010, 1, 6));
			headerDataObject.DateCollection.Add(dischargeActual);
			var dischargeEstimate = Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2010, 1, 9));
			headerDataObject.DateCollection.Add(dischargeEstimate);
			var arrivalActual = Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2010, 1, 7));
			headerDataObject.DateCollection.Add(arrivalActual);
			var arrivalEstimate = Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2010, 1, 8));
			headerDataObject.DateCollection.Add(arrivalEstimate);
			var reader = new JPAFRHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.JPH_ETD", new ZDateTime(2010, 1, 3), headerBO.JPH_ETD);
			AssertEquals("headerBO.JPH_ETA", new ZDateTime(2010, 1, 8), headerBO.JPH_ETA);

			headerDataObject.DateCollection.Remove(departureEstimate);
			headerDataObject.DateCollection.Remove(arrivalEstimate);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.JPH_ETD", new ZDateTime(2010, 1, 4), headerBO.JPH_ETD);
			AssertEquals("headerBO.JPH_ETA", new ZDateTime(2010, 1, 7), headerBO.JPH_ETA);

			headerDataObject.DateCollection.Remove(departureActual);
			headerDataObject.DateCollection.Remove(arrivalActual);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.JPH_ETD", new ZDateTime(2010, 1, 2), headerBO.JPH_ETD);
			AssertEquals("headerBO.JPH_ETA", new ZDateTime(2010, 1, 9), headerBO.JPH_ETA);

			headerDataObject.DateCollection.Remove(loadingEstimate);
			headerDataObject.DateCollection.Remove(dischargeEstimate);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.JPH_ETD", new ZDateTime(2010, 1, 5), headerBO.JPH_ETD);
			AssertEquals("headerBO.JPH_ETA", new ZDateTime(2010, 1, 6), headerBO.JPH_ETA);
		}

		public void TestImportingJPAFRHeaderData()
		{
			var headerDataObject = SetupAFRHeader("MB2343");
			headerDataObject.Branch = Branch.New(CurrentCompanySecondBranch);

			headerDataObject.SetDateCollection(() => new List<Date>());
			headerDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2010, 1, 4)));
			headerDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.False, new ZDateTime(2010, 1, 3)));
			headerDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2010, 1, 7)));
			headerDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2010, 1, 8)));

			headerDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			headerDataObject.NoteCollection.Add(SetupNote());
			headerDataObject.NoteCollection.Add(SetupNote2());

			var bill1DataObject = SetupJPAFRBills("HB8953");
			SetupAFRInBondDetails(bill1DataObject);
			SetupNotificationForwardingParties(bill1DataObject, "NFP1", "NFP3", "NFP2");
			SetupOtherRelevantLawCodes(bill1DataObject, "L1", "L5", "L4", "L2", "L3");
			var bill2DataObject = SetupJPAFRBills("HB2343");
			bill2DataObject.PackingLineCollection[0].GoodsDescription = "HELLO WORLD";

			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { bill1DataObject, bill2DataObject }));
			Factory.SaveForTesting();
			var reader = new JPAFRHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();

			AssertNotNull(headerBO);

			#region Check Contents of header Business Object

			CombineAssertions(delegate
			{
				AssertContents(headerBO, "MB2343");
				AssertEquals("headerBO.JPH_IsShippingLineEntry", ZBool.False, headerBO.JPH_IsShippingLineEntry);
				AssertEquals("headerBO.JPH_ETD", new ZDateTime(2010, 1, 3), headerBO.JPH_ETD);
				AssertEquals("headerBO.JPH_ETA", new ZDateTime(2010, 1, 8), headerBO.JPH_ETA);
				AssertEquals("headerBO.Bills.Count", 2, headerBO.Bills.Count);
				var bill1BO = headerBO.Bills[0];
				var bill2BO = headerBO.Bills[1];
				if (bill2BO.JPB_BillNumber == "HB2343")
				{
					bill1BO = headerBO.Bills[1];
					bill2BO = headerBO.Bills[0];
				}
				AssertEquals("bill1BO.JPB_BillNumber", "HB2343", bill1BO.JPB_BillNumber);
				AssertNull("bill1BO.InBondDetails", bill1BO.InBondDetails);
				AssertEquals("bill1BO.OtherRelevantLaws.Count", 0, bill1BO.OtherRelevantLaws.Count);
				AssertEquals("bill2BO.JPB_BillNumber", "HB8953", bill2BO.JPB_BillNumber);
				AssertContents(bill2BO.InBondDetails);
				var otherLawBOs = bill2BO.OtherRelevantLaws.GetNonEmptyInSortOrder();
				AssertEquals(5, otherLawBOs.Length);
				AssertEquals("Law1", "L1", otherLawBOs[0].CY_Data);
				AssertEquals("Law5", "L5", otherLawBOs[1].CY_Data);
				AssertEquals("Law4", "L4", otherLawBOs[2].CY_Data);
				AssertEquals("Law2", "L2", otherLawBOs[3].CY_Data);
				AssertEquals("Law3", "L3", otherLawBOs[4].CY_Data);
				var notificationForwardingParties = bill2BO.NotificationForwardingParties.GetNonEmptyInSortOrder();
				AssertEquals(3, notificationForwardingParties.Length);
				AssertEquals("Party1", "NFP1", notificationForwardingParties[0].CY_Data);
				AssertEquals("Party2", "NFP3", notificationForwardingParties[1].CY_Data);
				AssertEquals("Part3", "NFP2", notificationForwardingParties[2].CY_Data);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching JPAFRHeader found, creating new JPAFRHeader.
Information - Populating JPAFRHeader...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: WORM EATER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - No matching JPAFRBills found, creating new JPAFRBills.
Information - Populating JPAFRBills...
Information - Added Bill Of Lading HB8953 from UniversalShipment.
Information - No matching JPAFRBills found, creating new JPAFRBills.
Information - Populating JPAFRBills...
Information - Added Bill Of Lading HB2343 from UniversalShipment.
Information - Added Advance Filing Rules (MBOL: MB2343) from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestMatchingToExistingJPAFRHeader()
		{
			var newFactory = new BusinessObjectFactory();
			var shippingLineEntry = newFactory.New<JPAFRHeader>();
			shippingLineEntry.JPH_MasterBillNumber = "MB2343";
			shippingLineEntry.JPH_IsShippingLineEntry = ZBool.True;
			var existingHeader1 = newFactory.New<JPAFRHeader>();
			existingHeader1.JPH_MasterBillNumber = "MB2343";
			var existingHeader2 = newFactory.New<JPAFRHeader>();
			existingHeader2.JPH_MasterBillNumber = "MB2343";
			newFactory.Save();
			shippingLineEntry.JPH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);
			existingHeader1.JPH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);
			existingHeader2.JPH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			newFactory.Save();
			var existingHeader1HAWB1 = existingHeader1.Bills.AddNew();
			existingHeader1HAWB1.JPB_BillNumber = "HB2343";
			var existingHeader1HAWB2 = existingHeader1.Bills.AddNew();
			existingHeader1HAWB2.JPB_BillNumber = "HB8953";
			var existingHeader1HAWB3 = existingHeader1.Bills.AddNew();
			existingHeader1HAWB3.JPB_BillNumber = "HB1TOBEDELETED";

			var existingHeader2HAWB1 = existingHeader2.Bills.AddNew();
			existingHeader2HAWB1.JPB_BillNumber = "HB2343";
			var existingHeader2HAWB2 = existingHeader2.Bills.AddNew();
			existingHeader2HAWB2.JPB_BillNumber = "HB8953";
			var existingHeader2HAWB3 = existingHeader2.Bills.AddNew();
			existingHeader2HAWB3.JPB_BillNumber = "HB1TOBEDELETED";

			var shippingLineEntryHAWB1 = shippingLineEntry.Bills.AddNew();
			shippingLineEntryHAWB1.JPB_BillNumber = "HB2343";
			var shippingLineEntryHAWB2 = shippingLineEntry.Bills.AddNew();
			shippingLineEntryHAWB2.JPB_BillNumber = "HB8953";
			var shippingLineEntryHAWB3 = shippingLineEntry.Bills.AddNew();
			shippingLineEntryHAWB3.JPB_BillNumber = "HB1TOBEDELETED";

			var existingHeader1HAWB2Container1 = existingHeader1HAWB2.Containers.AddNew();
			existingHeader1HAWB2Container1.JPC_ContainerNum = "CONT1TODEL";
			var existingHeader1HAWB2Container2 = existingHeader1HAWB2.Containers.AddNew();
			existingHeader1HAWB2Container2.JPC_ContainerNum = "CONT2";
			var existingHeader1HAWB2Container3 = existingHeader1HAWB2.Containers.AddNew();
			existingHeader1HAWB2Container3.JPC_ContainerNum = "CONT3";

			var existingHeader2HAWB2Container1 = existingHeader2HAWB2.Containers.AddNew();
			existingHeader2HAWB2Container1.JPC_ContainerNum = "CONT1TODEL";
			var existingHeader2HAWB2Container2 = existingHeader2HAWB2.Containers.AddNew();
			existingHeader2HAWB2Container2.JPC_ContainerNum = "CONT2";
			var existingHeader2HAWB2Container3 = existingHeader2HAWB2.Containers.AddNew();
			existingHeader2HAWB2Container3.JPC_ContainerNum = "CONT3";

			var shippingLineEntryHAWB2Container1 = shippingLineEntryHAWB2.Containers.AddNew();
			shippingLineEntryHAWB2Container1.JPC_ContainerNum = "CONT1TODEL";
			var shippingLineEntryHAWB2Container2 = shippingLineEntryHAWB2.Containers.AddNew();
			shippingLineEntryHAWB2Container2.JPC_ContainerNum = "CONT2";
			var shippingLineEntryHAWB2Container3 = shippingLineEntryHAWB2.Containers.AddNew();
			shippingLineEntryHAWB2Container3.JPC_ContainerNum = "CONT3";

			var existingHeader1HAWB1NFT1 = existingHeader1HAWB1.NotificationForwardingParties.AddNewIfNotExist("N1D");
			var existingHeader1HAWB1NFT2 = existingHeader1HAWB1.NotificationForwardingParties.AddNewIfNotExist("N4");
			var existingHeader1HAWB1NFT3 = existingHeader1HAWB1.NotificationForwardingParties.AddNewIfNotExist("N2");

			var existingHeader2HAWB1NFT1 = existingHeader2HAWB1.NotificationForwardingParties.AddNewIfNotExist("N1D");
			var existingHeader2HAWB1NFT2 = existingHeader2HAWB1.NotificationForwardingParties.AddNewIfNotExist("N4");
			var existingHeader2HAWB1NFT3 = existingHeader2HAWB1.NotificationForwardingParties.AddNewIfNotExist("N2");

			var shippingLineEntryHAWB1NFT1 = shippingLineEntryHAWB1.NotificationForwardingParties.AddNewIfNotExist("N1D");
			var shippingLineEntryHAWB1NFT2 = shippingLineEntryHAWB1.NotificationForwardingParties.AddNewIfNotExist("N4");
			var shippingLineEntryHAWB1NFT3 = shippingLineEntryHAWB1.NotificationForwardingParties.AddNewIfNotExist("N2");

			var existingHeader1HAWB1ORL1 = existingHeader1HAWB1.OtherRelevantLaws.AddNewIfNotExist("O1");
			var existingHeader1HAWB1ORL2 = existingHeader1HAWB1.OtherRelevantLaws.AddNewIfNotExist("O3");
			var existingHeader1HAWB1ORL3 = existingHeader1HAWB1.OtherRelevantLaws.AddNewIfNotExist("O2");

			var existingHeader2HAWB1ORL1 = existingHeader2HAWB1.OtherRelevantLaws.AddNewIfNotExist("O1");
			var existingHeader2HAWB1ORL3 = existingHeader2HAWB1.OtherRelevantLaws.AddNewIfNotExist("O2");
			var existingHeader2HAWB1ORL2 = existingHeader2HAWB1.OtherRelevantLaws.AddNewIfNotExist("O3");

			var shippingLineEntryHAWB1ORL1 = shippingLineEntryHAWB1.OtherRelevantLaws.AddNewIfNotExist("O1");
			var shippingLineEntryHAWB1ORL2 = shippingLineEntryHAWB1.OtherRelevantLaws.AddNewIfNotExist("O3");
			var shippingLineEntryHAWB1ORL3 = shippingLineEntryHAWB1.OtherRelevantLaws.AddNewIfNotExist("O2");
			newFactory.Save();

			var headerDataObject = SetupAFRHeader("MB2343");
			var billDataObject = SetupNotificationForwardingParties(SetupOtherRelevantLawCodes(SetupJPAFRBills("HB2343"), "O2", "O3", "O4", "O5", "O6"), "N2", "N4", "N3");
			var billDataObject2 = SetupJPAFRBills("HB6935");
			var billDataObject3 = SetupJPAFRBills("HB8953");

			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { billDataObject, billDataObject2, billDataObject3 }));

			var reader = new JPAFRHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertNotNull(headerBO);
			#region Check Contents of header Business Object

			CombineAssertions(delegate
			{
				AssertEquals("Should have been matched to existingHeader2", existingHeader2.PK, headerBO.PK);
				AssertContents(headerBO, "MB2343");
				AssertEquals("headerBO.Bills.Count", 3, headerBO.Bills.Count);
				existingHeader2HAWB1 = Factory.Load<JPAFRBills>(existingHeader2HAWB1.PK);
				AssertNotNull("existingHeader2HAWB1", existingHeader2HAWB1);
				existingHeader2HAWB2 = Factory.Load<JPAFRBills>(existingHeader2HAWB2.PK);
				AssertNotNull("existingHeader2HAWB2", existingHeader2HAWB2);
				AssertNull("existingHeader2HAWB3.IsDeleted", Factory.Load<JPAFRBills>(existingHeader2HAWB3.PK));
				var billBO1 = headerBO.Bills.OfType<JPAFRBills>().FirstOrDefault(x => x.JPB_BillNumber == "HB2343");
				var billBO2 = headerBO.Bills.OfType<JPAFRBills>().FirstOrDefault(x => x.JPB_BillNumber == "HB6935");
				var billBO3 = headerBO.Bills.OfType<JPAFRBills>().FirstOrDefault(x => x.JPB_BillNumber == "HB8953");
				AssertEquals("billBO1 should have been matched to existingHeader2HAWB1", existingHeader2HAWB1.PK, billBO1.PK);
				AssertJPAFRBillsContents(billBO1, "HB2343");
				AssertEquals("billBO3 should have been matched to existingHeader2HAWB2", existingHeader2HAWB2.PK, billBO3.PK);
				AssertJPAFRBillsContents(billBO3, "HB8953");
				AssertJPAFRBillsContents(billBO2, "HB6935");

				existingHeader2HAWB1NFT2 = Factory.Load<NotificationForwardingParty>(existingHeader2HAWB1NFT2.PK);
				AssertNotNull("existingHeader2HAWB1NFT2", existingHeader2HAWB1NFT2);
				existingHeader2HAWB1NFT3 = Factory.Load<NotificationForwardingParty>(existingHeader2HAWB1NFT3.PK);
				AssertNotNull("existingHeader2HAWB1NFT3", existingHeader2HAWB1NFT3);
				AssertNull("existingHeader2HAWB1NFT1.IsDeleted", Factory.Load<NotificationForwardingParty>(existingHeader2HAWB1NFT1.PK));
				var nfpBOs = billBO1.NotificationForwardingParties.GetNonEmptyInSortOrder();
				AssertEquals("nfpBOs.Length", 3, nfpBOs.Length);
				var nfpBO1 = nfpBOs[0];
				var nfpBO2 = nfpBOs[1];
				var nfpBO3 = nfpBOs[2];
				AssertEquals("nfpBO1 should have been matched to existingHeader2NFT3", existingHeader2HAWB1NFT3.PK, nfpBO1.PK);
				AssertEquals("N2", nfpBO1.CY_Data);
				AssertEquals("nfpBO2 should have been matched to existingHeader2NFT2", existingHeader2HAWB1NFT2.PK, nfpBO2.PK);
				AssertEquals("N4", nfpBO2.CY_Data);
				AssertEquals("N3", nfpBO3.CY_Data);

				existingHeader2HAWB1ORL2 = Factory.Load<OtherRelevantLaw>(existingHeader2HAWB1ORL2.PK);
				AssertNotNull("existingHeader2HAWB1ORL2", existingHeader2HAWB1ORL2);
				existingHeader2HAWB1ORL3 = Factory.Load<OtherRelevantLaw>(existingHeader2HAWB1ORL3.PK);
				AssertNotNull("existingHeader2HAWB1ORL3", existingHeader2HAWB1ORL3);
				AssertNull("existingHeader2HAWB1ORL1.IsDeleted", Factory.Load<OtherRelevantLaw>(existingHeader2HAWB1ORL1.PK));
				AssertEquals("existingHeader2HAWB2.OtherRelevantLaws.Count", 0, existingHeader2HAWB2.OtherRelevantLaws.Count);
				var orlBOs = existingHeader2HAWB1.OtherRelevantLaws.GetNonEmptyInSortOrder();
				AssertEquals("orlBOs.Length", 5, orlBOs.Length);
				var orlBO1 = orlBOs[0];
				var orlBO2 = orlBOs[1];
				var orlBO3 = orlBOs[2];
				var orlBO4 = orlBOs[3];
				var orlBO5 = orlBOs[4];
				AssertEquals("orlBO1 should have been matched to existingHeader2ORL3", existingHeader2HAWB1ORL3.PK, orlBO1.PK);
				AssertEquals("O2", orlBO1.CY_Data);
				AssertEquals("orlBO2 should have been matched to existingHeader2ORL2", existingHeader2HAWB1ORL2.PK, orlBO2.PK);
				AssertEquals("O3", orlBO2.CY_Data);
				AssertEquals("O4", orlBO3.CY_Data);
				AssertEquals("O5", orlBO4.CY_Data);
				AssertEquals("O6", orlBO5.CY_Data);
			});

			#endregion
		}

		public void TestInvalidMAWBRecyclePeriodWillMatchAll()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var newFactory = new BusinessObjectFactory();
			var existingHeader = newFactory.New<JPAFRHeader>();
			existingHeader.JPH_MasterBillNumber = "MB2343";
			existingHeader.JPH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			newFactory.Save();
			var headerDataObject = SetupAFRHeader("MB2343");
			var reader = new JPAFRHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals(existingHeader.PK, headerBO.PK);
		}
	}

	partial class JPManifestDataObjectReaderTestHelper : Customs.DataTransfer.Universal.Testing.DataObjectReaderTestHelper
	{
		#region Implementation

		#region AFR Header Methods

		protected UniversalShipment SetupAFRHeader(ZString? wayBillNumber)
		{
			return SetupAFRHeader(wayBillNumber, "APL VESSEL", "V123", new UNLOCO() { Code = SeaForeignPort1.RL_Code }, "1", ZBool.True, new UNLOCO() { Code = SeaLocalPort1.RL_Code }, CarrierCode1);
		}

		protected UniversalShipment SetupAFRHeader2(ZString? wayBillNumber)
		{
			return SetupAFRHeader(wayBillNumber, "BJS VESSEL", "V897", new UNLOCO() { Code = SeaForeignPort2.RL_Code }, "2", ZBool.False, new UNLOCO() { Code = SeaLocalPort2.RL_Code }, carrierAddress: CarrierOrg.MainAddress);
		}

		protected UniversalShipment SetupAFRHeader(ZString? wayBillNumber, ZString? vessel, ZString? voyage, UNLOCO portOfLoading, ZString? loadingPortSuffix, ZBool? isDepartureFromRelaxedArea, UNLOCO portOfDischarge, ZString? carrierCode = null, OrgAddress carrierAddress = null)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AFRHeader, null);

			var result = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = wayBillNumber,
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				VesselName = vessel,
				VoyageFlightNo = voyage,
				PortOfLoading = portOfLoading,
				PortOfDischarge = portOfDischarge
			};
			if (loadingPortSuffix.HasValue)
			{
				result.SetAddInfoCollection(() => result.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Header.PortOfLoadingSuffix, Value = loadingPortSuffix }));
			}
			if (isDepartureFromRelaxedArea.HasValue)
			{
				result.SetAddInfoCollection(() => result.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Header.IsDepartureFromRelaxedArea, Value = isDepartureFromRelaxedArea.Value ? AddInfoConstants.True : AddInfoConstants.False }));
			}
			if (carrierCode.HasValue)
			{
				result.SetAddInfoCollection(() => result.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Header.CarrierCode, Value = carrierCode }));
			}
			if (carrierAddress != null)
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				result.AddOrgAddress(writeManager, carrierAddress, DocAddressType.Carrier);
			}
			return result;
		}

		protected void AssertContents(JPAFRHeader headerBO, ZString masterBill)
		{
			AssertContents(headerBO, masterBill, CarrierCode1, "APL VESSEL", "V123", SeaForeignPort1.RL_Code, "1", ZBool.True, SeaLocalPort1.RL_Code);
		}

		protected void AssertContents2(JPAFRHeader headerBO, ZString masterBill)
		{
			AssertContents(headerBO, masterBill, CarrierCode2, "BJS VESSEL", "V897", SeaForeignPort2.RL_Code, "2", ZBool.False, SeaLocalPort2.RL_Code);
		}

		protected void AssertContents(JPAFRHeader headerBO, ZString masterBill, ZString carrierCode, ZString vessel, ZString voyage, ZString portOfLoading, ZString loadingPortSuffix, ZBool isDepartureFromRelaxedArea, ZString portOfDischarge)
		{
			AssertEquals("headerBO.JPH_MasterBillNumber", masterBill, headerBO.JPH_MasterBillNumber);
			AssertEquals("headerBO.JPH_CarrierCode", carrierCode, headerBO.JPH_CarrierCode);
			AssertEquals("headerBO.JPH_VesselName", vessel, headerBO.JPH_VesselName);
			AssertEquals("headerBO.JPH_Voyage", voyage, headerBO.JPH_Voyage);
			AssertEquals("headerBO.JPH_RL_NKLoading", portOfLoading, headerBO.JPH_RL_NKLoading);
			AssertEquals("headerBO.JPH_LoadingPortSuffix", loadingPortSuffix, headerBO.JPH_LoadingPortSuffix);
			AssertEquals("headerBO.JPH_RelaxedAppId", isDepartureFromRelaxedArea, headerBO.JPH_RelaxedAppId);
			AssertEquals("headerBO.JPH_RL_NKDischarge", portOfDischarge, headerBO.JPH_RL_NKDischarge);
		}

		#endregion

		#region AFR Notification Forwarding Methods

		protected UniversalShipment SetupNotificationForwardingParties(UniversalShipment shipment, ZString? partCode1, ZString? partCode2, ZString? partCode3)
		{
			if (partCode1.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.NotificationForwardingPartyCode1, Value = partCode1 }));
			}
			if (partCode2.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.NotificationForwardingPartyCode2, Value = partCode2 }));
			}
			if (partCode3.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.NotificationForwardingPartyCode3, Value = partCode3 }));
			}
			return shipment;
		}

		protected void AssertContains(ZString partyCode, CusCodeDataWithSequenceNumberLineCollection<NotificationForwardingParty> collection)
		{
			AssertNotNull("Should contain code: " + partyCode, collection.FirstOrDefault(x => x.CY_Data == partyCode));
		}
		#endregion

		protected RefCurrency JPY
		{
			get
			{
				if (jpy == null)
				{
					jpy = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);
				}
				return jpy;
			}
		}
		RefCurrency jpy;

		protected RefCurrency USD
		{
			get
			{
				if (usd == null)
				{
					usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
				}
				return usd;
			}
		}
		RefCurrency usd;

		protected OrgHeader CarrierOrg
		{
			get
			{
				if (carrierOrg == null)
				{
					carrierOrg = CreateOrganisation("CARRIER ORG", "C!2");
					carrierOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, CarrierCode2, Core.Constants.CountryCodes.Japan);
					AssertNotNull(carrierOrg.MainAddress);
				}
				return carrierOrg;
			}
		}
		OrgHeader carrierOrg;

		protected const string CarrierCode1 = "OTD5";
		protected const string CarrierCode2 = "ODS2";

		protected RefUNLOCO SeaLocalPort1
		{
			get
			{
				if (seaLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort1;
			}
		}
		RefUNLOCO seaLocalPort1;

		protected RefUNLOCO SeaLocalPort2
		{
			get
			{
				if (seaLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, SeaLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort2;
			}
		}
		RefUNLOCO seaLocalPort2;

		protected RefUNLOCO SeaLocalPort3
		{
			get
			{
				if (seaLocalPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { SeaLocalPort1.PK, SeaLocalPort2.PK });
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaLocalPort3 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort3;
			}
		}
		RefUNLOCO seaLocalPort3;

		protected RefUNLOCO SeaForeignPort1
		{
			get
			{
				if (seaForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Japan);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaForeignPort1;
			}
		}
		RefUNLOCO seaForeignPort1;

		protected RefUNLOCO SeaForeignPort2
		{
			get
			{
				if (seaForeignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Japan);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, SeaForeignPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					seaForeignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaForeignPort2;
			}
		}
		RefUNLOCO seaForeignPort2;
		#endregion
	}
}
