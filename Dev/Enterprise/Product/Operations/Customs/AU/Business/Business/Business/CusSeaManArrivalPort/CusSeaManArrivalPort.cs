using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPort : Customs.Business.CusSeaManArrivalPort, ICMRMessageRespondee, Integration.Customs.AU.ICusSeaManArrivalPort
	{
		public CusSeaManArrivalPort(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ActualArrivalStatusCalculator = new CusSeaManArrivalPortStatusCalculator(this);
			CargoListStatusCalculator = new CusSeaManArrivalPortCargoListStatusCalculator(this);
		}

		public readonly CusSeaManArrivalPortStatusCalculator ActualArrivalStatusCalculator;
		public readonly CusSeaManArrivalPortCargoListStatusCalculator CargoListStatusCalculator;

		public CusSeaManOBLDetailCargoLine[] GetCargoListDetails()
		{
			List<CusSeaManOBLDetailCargoLine> resultList = new List<CusSeaManOBLDetailCargoLine>();
			if (!BA_RL_NKArrivalPort.IsEmpty)
			{
				foreach (CusSeaManOBLHeaderCargoLine line in CargoLines)
				{
					if (line.Detail != null)
					{
						resultList.Add(line.Detail);
					}
				}
			}
			return resultList.ToArray();
		}

		#region Schema

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new abstract class Schema : Customs.Business.CusSeaManArrivalPort.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string ActualArrivalResponseStatusCode = "ActualArrivalResponseStatusCode";
			public const string ActualArrivalResponseStatusDescription = "ActualArrivalResponseStatusDescription";
			public const string StevedoreID = "StevedoreID";
			public const string CTOEstablishmentID = "CTOEstablishmentID";
		}

		#endregion

		#region Overrides

		protected override Customs.Business.CusSeaManArrivalPortLookups GetNewLookups()
		{
			return new CusSeaManArrivalPortLookups(this);
		}

		public new CusSeaManArrivalPortLookups Lookups
		{
			get { return (CusSeaManArrivalPortLookups)base.Lookups; }
		}

		protected override Customs.Business.CusSeaManArrivalPortValidation GetNewValidation()
		{
			return new CusSeaManArrivalPortValidation(this);
		}

		[List(nameof(Lookups) + "." + nameof(CusSeaManArrivalPortLookups.ArrivalPorts))]
		public override ZString BA_RL_NKArrivalPort
		{
			get { return base.BA_RL_NKArrivalPort; }
			set
			{
				if (value != base.BA_RL_NKArrivalPort)
				{
					CargoLines.MarkAsNeedingValidationIncludingChildren();
					SetPortCodeOnAllCargoLines(value);
					base.BA_RL_NKArrivalPort = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusSeaManArrivalPortLookups.BerthCodeList))]
		public override ZString BA_BerthCode { get => base.BA_BerthCode; set => base.BA_BerthCode = value; }

		public override void Delete()
		{
			CargoLines.RemoveAndDeleteAll();
			base.Delete();
		}

		public new CusSeaManTranHead Header
		{
			get { return base.Header as CusSeaManTranHead; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSeaManArrivalPortLookups.CTOAddressOrgs))]
		public override ZGuid BA_OA_CTOAddress
		{
			get { return base.BA_OA_CTOAddress; }
			set
			{
				base.BA_OA_CTOAddress = value;
				HookupCTOAddressSubscriptionManager();
			}
		}

		#endregion

		#region Properties

		#region Messages

		#region Actual Arrival

		public EDIMessageCollectionView ActualArrivalMessages
		{
			get
			{
				if (fActualArrivalMessages == null)
				{
					ZQuery filter = new ZQuery(EDIMessageSchema.EM_MessageType, CMRMessage.CMRMessageTypes.SEAAAR);
					fActualArrivalMessages = new EDIMessageCollectionView(Messages, filter);
				}
				return fActualArrivalMessages;
			}
		}
		EDIMessageCollectionView fActualArrivalMessages;

		#endregion

		#region Cargo List

		public EDIMessageCollectionView CargoListMessages
		{
			get
			{
				if (fCargoListMessages == null)
				{
					ZQuery filter = new ZQuery(EDIMessageSchema.EM_MessageType, CMRMessage.CMRMessageTypes.CARLST);
					fCargoListMessages = new EDIMessageCollectionView(Messages, filter);
				}
				return fCargoListMessages;
			}
		}
		EDIMessageCollectionView fCargoListMessages;

		public NonDependentEDIMessageCollection CargoLineMessagesCombined
		{
			get
			{
				if (fCargoLineMessagesCombined == null)
				{
					fCargoLineMessagesCombined = new NonDependentEDIMessageCollection(Factory);

					var cargoLineFilter = new ZQuery(EDIMessageSchema.EM_MessageType, CMRMessage.CMRMessageTypes.CARST);
					foreach (CusSeaManOBLHeaderCargoLine cargoLine in CargoLines)
					{
						fCargoLineMessagesCombined.AddRange(cargoLine.Messages.Find(cargoLineFilter));
					}
				}
				return fCargoLineMessagesCombined;
			}
		}
		NonDependentEDIMessageCollection fCargoLineMessagesCombined;

		public NonDependentEDIMessageCollection CargoListAndLineMessagesCombined
		{
			get
			{
				var cargoListAndLineMessagesCombined = new NonDependentEDIMessageCollection(Factory);
				cargoListAndLineMessagesCombined.AddRange(CargoListMessages);
				cargoListAndLineMessagesCombined.AddRange(CargoLineMessagesCombined);

				return cargoListAndLineMessagesCombined;
			}
		}

		#endregion

		#endregion

		#region ICMRMessageRespondee.Details

		ZString ICMRMessageRespondee.Details
		{
			get
			{
				ZString result = ((ICMRMessageRespondee)Header).Details;
				result += "Arrival Port: " + BA_RL_NKArrivalPort + "\r\n";
				return result;
			}
		}

		#endregion

		#region ICMRMessageRespondee..ShortDescription

		ZString ICMRMessageRespondee.ShortDescription
		{
			get { return ((ICMRMessageRespondee)Header).ShortDescription + " Arrival Port: " + BA_RL_NKArrivalPort; }
		}

		#endregion

		#region ActualArrivalResponseStatus

		public CusEntryNumStatus ActualArrivalResponseStatus
		{
			get
			{
				if (fActualArrivalResponseStatus == null)
				{
					fActualArrivalResponseStatus = new CusEntryNumStatus(this, Lookups.ActualArrivalStatusList, CMRBaseStatuses.Codes.NotSent, CusEntryNumber.EntryType.ActualArrivalResponseStatus, Core.Constants.CountryCodes.Australia);
				}

				return fActualArrivalResponseStatus;
			}
		}

		CusEntryNumStatus fActualArrivalResponseStatus;

		#endregion

		#region CargoListStatus

		public CusEntryNumStatus CargoListStatus
		{
			get
			{
				if (fCargoListStatus == null)
				{
					fCargoListStatus = new CusEntryNumStatus(this, Lookups.ActualArrivalStatusList, CMRBaseStatuses.Codes.NotSent, CusEntryNumber.EntryType.CargoListStatus, Core.Constants.CountryCodes.Australia);
				}
				return fCargoListStatus;
			}
		}

		CusEntryNumStatus fCargoListStatus;

		#endregion

		#region IsFirstArrival

		public override ZBool BA_IsFirstArrival
		{
			get { return base.BA_IsFirstArrival; }
			set
			{
				base.BA_IsFirstArrival = value;

				if (value)
				{
					foreach (CusSeaManArrivalPort port in Header.Arrivals)
					{
						if (port.PK != this.PK && port.BA_IsFirstArrival)
						{
							port.BA_IsFirstArrival = false;
						}
					}
				}
			}
		}

		#endregion

		#region CargoListLines

		[ChildEditable(true)]
		public CusSeaManOBLHeaderCargoLineCollection CargoLines
		{
			get
			{
				if (fCargoLines == null)
				{
					fCargoLines = new CusSeaManOBLHeaderCargoLineCollection(this);
					fCargoLines.Load();
					RegisterEditableChildObject(fCargoLines);
				}
				return fCargoLines;
			}
		}
		CusSeaManOBLHeaderCargoLineCollection fCargoLines;

		#endregion

		#region StevedoreID

		public ZString StevedoreID
		{
			get { return CTOAddress != null ? CTOAddress.Header.PrimaryRegistrationNumber.Number : ZString.Empty; }
		}

		public ZPropertyInfo StevedoreIDInfo
		{
			get { return GetZPropertyInfo(Schema.StevedoreID); }
		}

		#endregion

		#region CTOEstablishmentID

		public ZString CTOEstablishmentID
		{
			get { return CTOAddress != null ? CTOAddress.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZPropertyInfo CTOEstablishmentIDInfo
		{
			get { return GetZPropertyInfo(Schema.CTOEstablishmentID); }
		}

		#endregion

		#region EstimatedDateTimeOfArrivalUTC

		public ZDateTime EstimatedDateTimeOfArrivalUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (ArrivalPort != null && !BA_ArrivalPortETA.IsEmpty && BA_ArrivalPortETA.IsValid)
				{
					result = ArrivalPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(BA_ArrivalPortETA.ToDateTime());
				}
				return result;
			}
		}

		#endregion

		#region ActualDateTimeOfArrivalUTC

		public ZDateTime ActualDateTimeOfArrivalUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (ArrivalPort != null && !BA_ArrivalPortATA.IsEmpty && BA_ArrivalPortATA.IsValid)
				{
					result = ArrivalPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(BA_ArrivalPortATA.ToDateTime());
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region CTOAddress Datarefresh

		void HookupCTOAddressSubscriptionManager()
		{
			if (CTOAddress != null && CTOAddress.Header != null)
			{
				CTOAddressSubscriptionManager.BusinessObject = CTOAddress.Header;
			}
			else
			{
				CTOAddressSubscriptionManager.BusinessObject = null;
			}
		}

		DataRefreshSubscriptionManager CTOAddressSubscriptionManager
		{
			get
			{
				if (fCTOAddressSubscriptionManager == null)
				{
					fCTOAddressSubscriptionManager = new DataRefreshSubscriptionManager(new EventHandler(CTOAddressUpdatedHandler));
					fCTOAddressSubscriptionManager.Enabled = true;
					HookupCTOAddressSubscriptionManager();
				}

				return fCTOAddressSubscriptionManager;
			}
		}
		DataRefreshSubscriptionManager fCTOAddressSubscriptionManager;

#if DEBUG
		protected virtual
#endif
 void CTOAddressUpdatedHandler(object @object, EventArgs args)
		{
			RefreshBinding();
		}

		#endregion

		#region Implementation

		void SetPortCodeOnAllCargoLines(ZString portCode)
		{
			foreach (CusSeaManOBLHeaderCargoLine line in CargoLines)
			{
				line.BO_RL_NKDischargePort = portCode;
			}
		}

		#endregion
	}
}
