namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Customs.Universal;
	using Enterprise.Edifact.D96A.Elements;
	using Enterprise.Edifact.D96A.Messages.CUSRES;
	using Enterprise.ZArchitecture.Schema;
	using static Enterprise.Integration.Customs.CA;

	public sealed class ReleaseStatus : AutoReleaseStatus, IReleaseStatus
	{
		public ReleaseStatus(EDIReleaseMessage message)
			: base(message != null ? message.Factory : new BusinessObjectFactory())
		{
			this.message = message;
			if (message != null)
			{
				cusresMessage = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			}
			RL_ShouldBePrinted = false;
		}

		public ReleaseStatus(BusinessObjectFactory factory, ZGuid shipmentPK)
			: base(factory ?? new BusinessObjectFactory())
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.Equal, shipmentPK);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, Enterprise.Customs.CA.Business.MessageTypeList.Codes.EDIRelease);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, Enterprise.Customs.CA.Business.EDIReleaseImportEntryStatusList.Codes.MessageContentRejected);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, SQLComparisonOperator.Equal, Enterprise.Messaging.Business.EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CAIMP);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";

			this.message = factory.LoadTop1<EDIReleaseMessage>(query);
			if (message != null)
			{
				cusresMessage = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			}
			RL_ShouldBePrinted = false;
		}

		internal ReleaseStatus(CusEntryNumber ccn1, EDIReleaseMessage message = null)
			: this(CreateOrLoadCACargoControlNumberFromCusEntryNumber(ccn1), message)
		{
			this.ccn1 = ccn1;
		}

		internal ReleaseStatus(CargoControlNumber ccn2, EDIReleaseMessage message = null)
			: this(message)
		{
			this.ccn2 = ccn2;
			RL_ShouldBePrinted = CanBePrinted && ccn2 != null;
		}

		static CargoControlNumber CreateOrLoadCACargoControlNumberFromCusEntryNumber(CusEntryNumber ccn)
		{
			CargoControlNumber result = null;

			var declaration = ccn.Parent as JobDeclaration;
			if (declaration != null)
			{
				result = declaration.CargoControlNumbers.FirstOrDefault(number => number.CA_CCNInfoNumber == ccn.CE_EntryNum);
				if (result == null)
				{
					result = declaration.CargoControlNumbers.AddNew();
					result.CA_CCNInfoNumber = ccn.CE_EntryNum;
				}
				result.CA_IsFromNumbersTab = true;
			}

			return result;
		}

		[ChildEditable(true)]
		public BusinessObjectCollection Bills
		{
			get { return ccn2 != null && ccn2.Parent != null ? ccn2.Parent.Bills : new BillNonDependentCollection(Factory); }
		}

		public IEnumerable<ZString> Containers
		{
			get
			{
				IEnumerable<ZString> result = Array.Empty<ZString>();
				if (ccn2?.Parent?.IsIID ?? false)
				{
					var declaration = ccn2.Parent;
					result = declaration.CusContainers.Cast<CusContainer>().Select(x => x.CO_ContainerNumber);
				}
				else if (cusresMessage != null)
				{
					result = D96AMessageUtilities.GetContainers(cusresMessage.EQD);
				}
				return result;
			}
		}

		public ZString ProcessingIndicatorCodeDescription
		{
			get
			{
				var indicator = RL_ProcessingIndicator;
				if (indicator != ProcessingIndicatorCodedList.TransactionUnknown.ToString())
				{
					var builder = new ZStringBuilder();
					builder.Append(indicator);
					builder.AppendIfNotEmpty(RL_ReleaseStatusDescription);
					return builder.ToStringWithDelimiterBetweenAppends(" - ");
				}
				return ZString.Empty;
			}
		}

		#region Overrides of AutoReleaseStatus

		#region RL_CargoControlNumber

		[ReadOnlyMember(nameof(RL_CargoControlNumber_ReadOnly))]
		public override ZString RL_CargoControlNumber
		{
			get
			{
				return ccn1 != null ? ccn1.CE_EntryNum
						: ccn2 != null ? ccn2.CY_CargoControlNumber
							: message != null ? message.CargoControlNumber : ZString.Empty;
			}
			set
			{
				if (ccn1 != null)
				{
					ccn1.CE_EntryNum = value;
				}

				if (ccn2 != null)
				{
					ccn2.CY_CargoControlNumber = value;
				}

				RL_CargoControlNumberInfo.RefreshBinding();
				var declaration = ccn2.Parent;
				if (declaration != null)
				{
					declaration.EffectiveCCNInfo.RefreshBinding();
				}
			}
		}

		bool RL_CargoControlNumber_ReadOnly
		{
			get { return !IsPersistent; }
		}

		public bool IsPersistent
		{
			get { return PersistentCCN != null; }
		}

		#endregion

		#region RL_Bill

		[ReadOnlyMember(nameof(RL_Bill_ReadOnly))]
		public override ZGuid RL_Bill
		{
			get
			{
				return ccn2 != null ? ccn2.CA_CU_CCNInfoBill : ZGuid.Empty;
			}
			set
			{
				if (ccn2 != null)
				{
					ccn2.CA_CU_CCNInfoBill = value;
				}

				RL_BillInfo.RefreshBinding();
			}
		}

		bool RL_Bill_ReadOnly
		{
			get { return PersistentCCN == null; }
		}

		#endregion

		public override ZString RL_TransactionNumber
		{
			get { return message != null ? message.TransactionNumber : ZString.Empty; }
		}

		public override ZString RL_ServiceOption
		{
			get { return ValueOrDefault(c => D96AMessageUtilities.GetServiceOption(c.BGM)); }
		}

		public override ZString RL_ProcessingIndicator
		{
			get { return ValueOrDefault(c => D96AMessageUtilities.GetProcessingIndicator(c.GIS)); }
		}

		public override ZString RL_ReleaseStatus
		{
			get
			{
				var messageSubType = message != null ? message.EM_MessageSubType.ToString() : MessageSubTypeCodes.Codes.Original;
				return EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(RL_ProcessingIndicator, messageSubType);
			}
		}

		public override ZString RL_ReleaseStatusDescription
		{
			get { return new EDIReleaseImportEntryStatusList().GetDescriptionFromCode(RL_ReleaseStatus); }
		}

		public override ZDateTime RL_ProcessingDate
		{
			get { return ValueOrDefault(c => D96AMessageUtilities.GetDate(c.DTM, DateTimePeriodQualifierList.ProcessingDateTime)); }
		}

		public override ZDateTime RL_ReleaseDate
		{
			get { return ValueOrDefault(c => D96AMessageUtilities.GetDate(c.DTM, DateTimePeriodQualifierList.ClearanceDateCustoms)); }
		}

		public override ZString RL_DeliveryInstructions
		{
			get { return ValueOrDefault(c => D96AMessageUtilities.GetFreeText(c.FTX, TextSubjectQualifierList.PartyInstructions)); }
		}

		[List(nameof(CBSAOffices))]
		public override ZString RL_ReleaseOffice
		{
			get { return ValueOrDefault(c => D96AMessageUtilities.GetLocation(c.LOC, PlaceLocationQualifierList.CustomsOfficeOfClearance)); }
		}

		public ZZRefCusCodeListCombinedCollection CBSAOffices => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		[List(nameof(SubLocationCodes))]
		public override ZString RL_WarehouseCode
		{
			get { return ValueOrDefault(c => D96AMessageUtilities.GetRelatedLocation(c.LOC, PlaceLocationQualifierList.CustomsOfficeOfClearance)); }
		}

		public CACSubLocationCollection SubLocationCodes
		{
			get { return new CACSubLocationCollection(Factory); }
		}

		public override ZBool RL_ShouldBePrinted
		{
			get { return base.RL_ShouldBePrinted; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.RL_ShouldBePrinted = value;
				}
			}
		}

		#endregion

		#region Implementation

		internal bool CanBePrinted
		{
			get { return message != null; }
		}

		internal BusinessObject PersistentCCN
		{
			get { return (BusinessObject)ccn1 ?? ccn2; }
		}

		internal CargoControlNumber CACCN
		{
			get { return ccn2; }
		}

		internal bool IsAwaitingReply(ZDateTime requestDate)
		{
			return message == null || message.EM_SystemCreateTimeUtc < requestDate;
		}

		T ValueOrDefault<T>(Func<CUSRESMessage, T> getValue)
		{
			return cusresMessage == null ? default(T) : getValue(cusresMessage);
		}
		#endregion

		ZString IReleaseStatus.GetServiceOptionDescription()
		{
			var serviceOption = RL_ServiceOption;
			return (serviceOption + " " + ServiceOptions.GetShortDescription(serviceOption)).Trim();
		}

		ZString IReleaseStatus.GetWarehouseCodeDescription()
		{
			var result = RL_WarehouseCode.TrimStart('0');
			if (!result.IsEmpty)
			{
				var warehouse = CACSubLocation.Load(Factory, result);
				if (warehouse != null)
				{
					result += " - " + warehouse.Description;
				}
			}
			return result;
		}

		readonly CusEntryNumber ccn1;
		readonly CargoControlNumber ccn2;
		readonly EDIReleaseMessage message;
		readonly CUSRESMessage cusresMessage;
	}
}
