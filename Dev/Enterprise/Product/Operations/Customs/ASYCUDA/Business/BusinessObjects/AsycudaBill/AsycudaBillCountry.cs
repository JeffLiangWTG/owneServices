using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[UserDefinedValues]
	partial class AsycudaBill : IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider
		, IInterchangeSenderIdProvider
		, IStatusSupporter
		, IMessageParent
		, IWorkflowProvider
		, ICustomFieldProvider
		, IWorkflowAffectedPropertyProvider
	{
		public new partial class Schema
		{
			public const string BillIssuerName = "BillIssuerName";
			public const string DutyAmount = "DutyAmount";
			public const string TaxAmount = "TaxAmount";

			public const string CustomsEntryNumber = "CustomsEntryNumber";
			public const string CustomsEntryNumberType = "CustomsEntryNumberType";

			public const string RegistrationDate = "RegistrationDate";
			public const string RegistrationNumber = "RegistrationNumber";

			public const string CustomsJobNumber = "CustomsJobNumber";
			public const int CustomsJobNumberMaxLength = Customs.Business.BaseJobDeclaration.Schema.JE_DeclarationReferenceMaxLength;
			public const string StatusDescription = "StatusDescription";

			public const string CountryCode = "CountryCode";
			public const int CustomsEntryNumberTypeMaxLength = 3;

			public const string ABL_BillStatusDescription = "ABL_BillStatusDescription";
		}

		[ResourceStringData("AsycudaBill.StatusDescription", Caption = "Message Status Desc.")]
		public ZString StatusDescription
		{
			get
			{
				var statusDescription = ZString.Empty;

				if (ABL_MessageStatus == MessageStatusCodeList.Codes.Error)
				{
					var logQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Like, "ERR - %");
					var log = Logs.MostRecentLogByEventTime(Events.MessageStatusChange, logQuery);
					if (log != null)
					{
						statusDescription = log.ReferenceFreeText;
					}
				}

				if (statusDescription.IsEmpty)
				{
					statusDescription = Lookups.MessageStatusList.GetDescriptionFromCode(ABL_MessageStatus);
				}

				return statusDescription;
			}
		}

		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(Schema.StatusDescription);

		public string SetNewCountryMessagingStatus(ZString newStatus)
		{
			string oldStatusForRollback = ABL_MessageStatus;
			ABL_MessageStatus = newStatus;
			return oldStatusForRollback;
		}

		public string SetNewCountryCustomsStatus(ZString newStatus)
		{
			var oldStatusForRollback = ABL_BillStatus;
			ABL_BillStatus = newStatus;
			return oldStatusForRollback;
		}

		public ZString CountryCode => GetCountryCode();

		public ZPropertyInfo CountryCodeInfo => GetZPropertyInfo(Schema.CountryCode);

		protected internal virtual void DefaultOnCountryChanged()
		{
			CalculateShipmentType(this);
			DefaultCustomsEntryNumberType();
		}

		void DefaultCustomsEntryNumberType()
		{
			if (!CountryCode.IsEmpty)
			{
				var list = Lookups.CustomsEntryNumberTypes;
				if (list.Count == 1)
				{
					CustomsEntryNumberType = new ZString(list[0].Code).Left(Schema.CustomsEntryNumberTypeMaxLength);
				}
			}
		}

		[ResourceStringData("AsycudaBill.ABL_BillIssuer", Caption = "Bill Issuer")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BillIssuers))]
		[MaxLength(20)]
		public override ZString ABL_BillIssuer
		{
			get => base.ABL_BillIssuer;
			set => base.ABL_BillIssuer = value;
		}

		[ResourceStringData("AsycudaBill.BillIssuerName", Caption = "Bill Issuer Name", ShortCaption = "Name")]
		public ZString BillIssuerName
		{
			get => CountryCode.IsEmpty || ABL_BillIssuer.IsEmpty
				? ZString.Empty
				: (ZZCarrierLoader.LoadFromCode(CountryCode, ABL_BillIssuer)?.ZZ4_Description ?? ZString.Empty);
		}

		public ZBool IsIssuerCodeMandatory
		{
			get
			{
				var manifestType = Header?.AMA_ManifestType ?? ZString.Empty;
				return ZZDatabaseValidationHelper.IsMandatoryForOneCountryWhenAttributeMatches(Factory, CountryCode, ManifestValidationRuleCodes.BillIssuer, ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, manifestType)
					|| ZZDatabaseValidationHelper.IsMandatoryForOneCountryWhenAttributeMatches(Factory, CountryCode, ManifestValidationRuleCodes.BillIssuer, ManifestValidationRuleCodes.MandatoryForManifestType, manifestType);
			}
		}

		public FieldType BillIssuerFieldType
		{
			get
			{
				var result = FieldType.Text;
				if (!IsDeleted && ZZCarrierLoader.AnyRecordsForCountry(CountryCode))
				{
					result = FieldType.TextCodeFindBox;
				}
				return result;
			}
		}

		public ZString BillIssuerNameFieldType => BillIssuerFieldType.ToString();

		ZZRefCarrierCombined.Loader zZCarrierLoader;
		internal ZZRefCarrierCombined.Loader ZZCarrierLoader => zZCarrierLoader ?? (zZCarrierLoader = new ZZRefCarrierCombined.Loader(Factory));

		public ZPropertyInfo BillIssuerNameInfo => GetZPropertyInfo(Schema.BillIssuerName);

		void CalculateShipmentType(AsycudaBill bill)
		{
			if (bill != null)
			{
				CalculateShipmentTypeCore(bill);
			}
		}

		protected virtual void CalculateShipmentTypeCore(AsycudaBill bill)
		{
			ABL_ShipmentType = new BillShipmentTypeCalculator(bill).CalculateShipmentTypeFor(CountryCode);
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ShipmentTypes))]
		[ReadOnlyMember(nameof(ABL_ShipmentType_ReadOnly))]
		public override ZString ABL_ShipmentType
		{
			get => base.ABL_ShipmentType;
			set => base.ABL_ShipmentType = value;
		}

		protected virtual bool ABL_ShipmentType_ReadOnly => false;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Locations))]
		[MaxLength(30)]
		[ResourceStringData("AsycudaBill.ABL_GoodsLocation", Caption = "Location of Goods", ShortCaption = "Location", MediumCaption = "Goods Location")]
		public override ZString ABL_GoodsLocation
		{
			get => base.ABL_GoodsLocation;
			set => base.ABL_GoodsLocation = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsStatusList))]
		[ResourceStringData("AsycudaBill.ABL_BillStatus", Caption = "Bill Status")]
		[ReadOnlyMember(nameof(ABL_BillStatus_ReadOnly))]
		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set
			{
				if (ABL_BillStatus != value)
				{
					base.ABL_BillStatus = value;
					((IStatusSupporter)this).LogEventsOnParent(Events.CustomsManifestStatus, ABL_BillStatus);
					ABL_BillStatusDescriptionInfo.RefreshBinding();
				}
			}
		}

		protected virtual bool ABL_BillStatus_ReadOnly => false;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.MessageStatusList))]
		[ResourceStringData("AsycudaBill.ABL_MessageStatus", Caption = "Message Status")]
		[ReadOnly(true)]
		public override ZString ABL_MessageStatus
		{
			get => base.ABL_MessageStatus;
			set
			{
				if (ABL_MessageStatus != value)
				{
					base.ABL_MessageStatus = value;
					((IStatusSupporter)this).LogEventsOnParent(Events.MessageStatusChange, ABL_MessageStatus);
					Header?.MessageStatusInfo.RefreshBinding();
				}
			}
		}

		public virtual bool IsImport => ABL_ShipmentType == ShipmentTypeList.Codes.Import23;

		public virtual bool IsExport => ABL_ShipmentType == ShipmentTypeList.Codes.Export22;

		[ResourceStringData("AsycudaBill.DutyAmount", Caption = "Duty Amount")]
		[ReadOnlyMember(nameof(DutyAmount_ReadOnly))]
		public ZDecimal DutyAmount
		{
			get { return this.GetSystemDefinedValue<ZDecimal>(Schema.DutyAmount); }
			set
			{
				var oldValue = DutyAmount;
				this.SetSystemDefinedValue(Schema.DutyAmount, value);
				DutyAmountInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DutyAmountInfo => GetZPropertyInfo(Schema.DutyAmount);

		protected virtual bool DutyAmount_ReadOnly => false;

		[ResourceStringData("AsycudaBill.TaxAmount", Caption = "Tax Amount")]
		[ReadOnlyMember(nameof(TaxAmount_ReadOnly))]
		public ZDecimal TaxAmount
		{
			get { return this.GetSystemDefinedValue<ZDecimal>(Schema.TaxAmount); }
			set
			{
				var oldValue = TaxAmount;
				this.SetSystemDefinedValue(Schema.TaxAmount, value);
				TaxAmountInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TaxAmountInfo => GetZPropertyInfo(Schema.TaxAmount);

		protected virtual bool TaxAmount_ReadOnly => false;

		[MaxLength(Schema.CustomsJobNumberMaxLength)]
		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsJobNumberList))]
		[ResourceStringData("AsycudaBill.CustomsJobNumber", Caption = "Customs Job Number", ShortCaption = "Cus. Job No.")]
		public virtual ZString CustomsJobNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CustomsJobNumber);
			set
			{
				var oldValue = CustomsJobNumber;
				CheckMaximumLength(CustomsJobNumberInfo, value);
				this.SetSystemDefinedValue(Schema.CustomsJobNumber, value);
				CustomsJobNumberInfo.RefreshBinding(oldValue);
				ABL_BillNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsJobNumberInfo => GetZPropertyInfo(Schema.CustomsJobNumber);

		[ChildEditable]
		public ABLEntryNumCollection CustomsEntryNumbers
		{
			get
			{
				if (customsEntryNumbers == null)
				{
					customsEntryNumbers = CreateNewABLEntryNumCollection();
					customsEntryNumbers.Load();
					RegisterEditableChildObject(customsEntryNumbers);
				}
				return customsEntryNumbers;
			}
		}
		ABLEntryNumCollection customsEntryNumbers;
		protected virtual ABLEntryNumCollection CreateNewABLEntryNumCollection() => new ABLEntryNumCollection(this);

		protected virtual void DeleteCustomsNumbersNoLongerApplicable()
		{
			var supportMultipleCustomsNumbers = Header?.SupportMultipleCustomsNumbers ?? false;
			var entryNumber = CusEntryNumber;
			var customsNumbersToDelete = CustomsEntryNumbers.Cast<ABLEntryNum>().Where(x => !supportMultipleCustomsNumbers && x != entryNumber && x.CE_EntryType != CusEntryNumberTypes.ASYCUDA.AsycudaRegistration || x.CE_EntryNum.IsEmpty && x.CE_EntryLineReference.IsEmpty).ToArray();
			foreach (ABLEntryNum customsNumber in customsNumbersToDelete)
			{
				CustomsEntryNumbers.RemoveAndDelete(customsNumber);
			}
		}

		public ZString DataGrouping => Header?.DataGrouping ?? CountryCode;

		public bool IsCustomsCleared => AsycudaUniversalReference.CustomsStatusAttributeHelper.IsCustomsCleared(Factory, DataGrouping, ABL_BillStatus);

		public bool HasManifestBeenSubmittedToCustoms => MessageStatusProvider?.HasManifestBeenSubmittedToCustoms(this) ?? false;

		public MessageStatusProvider MessageStatusProvider => Header?.MessageStatusProvider;

		public ABLEntryNum CusEntryNumber
		{
			get
			{
				if (cusEntryNumber == null || cusEntryNumber.IsDeleted)
				{
					var supportMultipleCustomsNumbers = Header?.SupportMultipleCustomsNumbers ?? false;

					cusEntryNumber = CustomsEntryNumbers.OfType<ABLEntryNum>()
						.Where(x => FilterForSingleEntryType.IsEmpty || !supportMultipleCustomsNumbers || supportMultipleCustomsNumbers && x.CE_EntryType == FilterForSingleEntryType)
						.OrderBy(x => x.CE_SystemCreateTimeUtc).FirstOrDefault();
				}
				return cusEntryNumber;
			}
		}
		protected ABLEntryNum cusEntryNumber;

		public virtual ZString FilterForSingleEntryType
		{
			get { return ZString.Empty; }
		}

		[MaxLength("CustomsEntryNumberMaxLength")]
		[ReadOnlyMember(nameof(CustomsEntryNumber_ReadOnly))]
		[ResourceStringData("AsycudaBill.CustomsEntryNumber", Caption = "Customs Number")]
		public virtual ZString CustomsEntryNumber
		{
			get => CusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				CheckMaximumLength(CustomsEntryNumberInfo, value);
				var cusEntryNum = CusEntryNumber;
				var oldValue = cusEntryNum?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNum == null)
					{
						// use cached variable for performance
						cusEntryNumber = CustomsEntryNumbers.AddNew();
					}
					cusEntryNumber.CE_EntryNum = value;
				}
				else
				{
					if (cusEntryNum != null)
					{
						cusEntryNum.CE_EntryNum = value;
					}
				}
				CustomsEntryNumberInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsEntryNumber();
				}

				CustomsEntryNumberInfo.RefreshBinding();
			}
		}

		public int CustomsEntryNumberMaxLength
		{
			get
			{
				var result = ABLEntryNum.Schema.CE_EntryNumMaxLength;
				var supportMultipleCustomsNumbers = Header?.SupportMultipleCustomsNumbers ?? false;
				var maxLengthConfig = CustomsEntryNumberMaxLengthConfig.FirstOrDefault(x => !CustomsEntryNumberType.IsEmpty && x.Key == CustomsEntryNumberType || CustomsEntryNumberType.IsEmpty && !supportMultipleCustomsNumbers);
				if (maxLengthConfig.Value != 0)
				{
					result = maxLengthConfig.Value;
				}
				return result;
			}
		}

		IEnumerable<KeyValuePair<ZString, int>> CustomsEntryNumberMaxLengthConfig
		{
			get { return GetCustomsEntryNumberMaxLengthConfig(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<KeyValuePair<ZString, int>> GetCustomsEntryNumberMaxLengthConfig()
		{
			return System.Array.Empty<KeyValuePair<ZString, int>>();
		}

		public ZPropertyInfo CustomsEntryNumberInfo => GetZPropertyInfo(Schema.CustomsEntryNumber);

		protected virtual bool CustomsEntryNumber_ReadOnly => false;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsEntryNumberTypes))]
		[ResourceStringData("AsycudaBill.CustomsEntryNumberType", Caption = "Number Type")]
		[MaxLength(Schema.CustomsEntryNumberTypeMaxLength)]
		public virtual ZString CustomsEntryNumberType
		{
			get => CusEntryNumber?.CE_EntryType ?? ZString.Empty;
			set
			{
				var cusEntryNum = CusEntryNumber;
				var oldValue = cusEntryNum?.CE_EntryType ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNum == null)
					{
						// use cached variable for performance
						cusEntryNumber = CustomsEntryNumbers.AddNew();
					}
					cusEntryNumber.CE_EntryType = value;
				}
				else
				{
					if (cusEntryNum != null)
					{
						cusEntryNum.CE_EntryType = value;
					}
				}

				if (oldValue != CustomsEntryNumberType && CustomsEntryNumber.Length > CustomsEntryNumberMaxLength)
				{
					CustomsEntryNumber = ZString.Empty;
				}

				CustomsEntryNumberTypeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsEntryNumberType();
				}
			}
		}

		public ZPropertyInfo CustomsEntryNumberTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsEntryNumberType); }
		}

		[MaxLength(ABLEntryNum.Schema.CE_EntryNumMaxLength)]
		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		public ZString RegistrationNumber => RegistrationEntryNumber?.CE_EntryNum ?? ZString.Empty;

		public ZPropertyInfo RegistrationNumberInfo
		{
			get { return GetZPropertyInfo(Schema.RegistrationNumber); }
		}

		protected virtual bool RegistrationDetails_ReadOnly => true;

		[ReadOnly(true)]
		public ABLEntryNum RegistrationEntryNumber
		{
			get
			{
				if (registrationEntryNumber == null || registrationEntryNumber.IsDeleted || registrationEntryNumber.CE_EntryType != CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
				{
					registrationEntryNumber = Common.CusEntryNumber.Load<ABLEntryNum>(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, CountryCode);
					if (registrationEntryNumber != null)
					{
						RegisterEditableChildObject(registrationEntryNumber);
					}
				}
				return registrationEntryNumber;
			}
		}
		ABLEntryNum registrationEntryNumber;

		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		[BusinessObjectTestExclude]
		[ResourceStringData("AsycudaBill.RegistrationDate", ShortCaption = "Date", Caption = "Registration Date")]
		public ZDateTime RegistrationDate
		{
			get => RegistrationEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				var currentRegistrationEntryNumber = RegistrationEntryNumber;
				var oldValue = currentRegistrationEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				if (value.IsValid)
				{
					if (currentRegistrationEntryNumber == null)
					{
						// use cached variable for performance
						registrationEntryNumber = Common.CusEntryNumber.LoadOrCreate<ABLEntryNum>(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, CountryCode);
						RegisterEditableChildObject(registrationEntryNumber);
					}

					registrationEntryNumber.CE_IssueDate = value;
				}
				RegistrationDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRegistrationDate();
				}
			}
		}

		public ZPropertyInfo RegistrationDateInfo
		{
			get { return GetZPropertyInfo(Schema.RegistrationDate); }
		}

		[ResourceStringData("AsycudaBill.ABL_BillStatusDescription", Caption = "Bill Status Desc.")]
		public virtual ZString ABL_BillStatusDescription => Lookups.CustomsStatusList.GetDescriptionFromCode(ABL_BillStatus);

		public ZPropertyInfo ABL_BillStatusDescriptionInfo => GetZPropertyInfo(Schema.ABL_BillStatusDescription);

		#region IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider

		EDIFACTMessageStatusCalculator IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider.GetCalculator(string country)
		{
			return Header?.ApplicationBusinessProvider.MessagingProvider.GetEDIFACTStatusCalculator();
		}

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			((IEDIMessageCollectionProvider)this).Messages.Add(message);
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get => ABL_MessageStatus;
			set => ABL_MessageStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get => ABL_BillStatus;
			set => ABL_BillStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobIdentification => ((IJobNumber)Header).JobNumber;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => Header;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = new EDIMessageCollection(this, Factory);
					ediMessages.Load();
					ediMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessages);
				}
				return ediMessages;
			}
		}
		EDIMessageCollection ediMessages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => Factory;

		ZString IInterchangeSenderIdProvider.SenderID
		{
			get
			{
				return Header?.ApplicationBusinessProvider.MessagingProvider.GetInterchangeSenderID(Factory) ?? ZString.Empty;
			}
		}

		#endregion

		public bool IsDeclarationCreationEnabled => Factory.GetValue(ref isDeclarationCreationEnabledCached, () =>
		{
			var result = CustomsJobNumber.IsEmpty;
			if (result)
			{
				result = !ABL_BillNumber.IsEmpty && (Header?.IsDeclarationCreationEnabled ?? false);
			}

			return result;
		});

		CachedProperty<bool> isDeclarationCreationEnabledCached;

		#region IMessageParent

		ZString IMessageParent.ManifestType => Header?.AMA_ManifestType ?? ZString.Empty;

		bool IMessageParent.HasCustomsNumbers => !((IMessageParent)this).RegistrationNumber.IsEmpty;

		ISelectionItem IMessageParent.SelectionItem => this;

		ZString IMessageParent.MessageStatus => ABL_MessageStatus;

		ZString IMessageParent.CustomsStatus => ABL_BillStatus;

		void IMessageParent.ResetMessageStatus()
		{
			ABL_MessageStatus = ZString.Empty;
		}

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				BillScreenings.RemoveAndDeleteAll();
				AsycudaTaxes.RemoveAndDeleteAll();
				FetchStrategy.FetchForDelete();
				Messages.RemoveAndDeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				this.DeleteChildren<ABLEntryNum>(CusEntryNumSchema.CE_ParentID);
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}

			base.Delete();
		}

		#region IStatusSupporter

		ZString IStatusSupporter.ManifestPermitNumber
		{
			set => AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<ABLEntryNum>(this, value, CountryCode);
		}

		ZString IStatusSupporter.CustomsStatus
		{
			get => ABL_BillStatus;
			set => ABL_BillStatus = value;
		}

		ZString IStatusSupporter.MessageStatus
		{
			set => ABL_MessageStatus = value;
		}

		ZString IStatusSupporter.CustomsEntryNumber
		{
			set => AsycudaEntryNumberCreator.CreateOrUpdateCustomsEntryNumber<ABLEntryNum>(this, value, CountryCode);
		}

		ZBool IStatusSupporter.SupportsPackLevelMessages => Header?.IsPackedItemLevelManifestType ?? false;

		void IStatusSupporter.LogEventsOnParent(Event eventType, ZString reference)
		{
			Logs.AddNew(eventType, reference, ZDateTimeOffset.Now);
		}

		#endregion

		#region IWorkflowProvider Members

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<AsycudaBillProcessTask, AsycudaBill>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection<AsycudaBillProcessTask, AsycudaBill> workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			var header = Header;
			if (header != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, header.AMA_RN_NKCountry, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType2, header.AMA_ManifestType, ZString.Empty);
			}

			result.Add(ProcessTaskTemplateSchema.P0_SubType3, ABL_ShipmentType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GB, GlbBranch.CurrentBranch.PK, ZGuid.Empty);

			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria() => null;

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		[ChildEditable]
		protected CustomBusinessObject CustomBusinessObject
		{
			get
			{
				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
					customBusinessObject = new CustomBusinessObject(Factory, this, properties);
					RegisterEditableChildObject(customBusinessObject);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;

		#region IWorkflowAffectedPropertyProvider Members

		ZPropertyInfo[] IWorkflowAffectedPropertyProvider.PropertyThatAffectWorkflowChanged
		{
			get
			{
				if (!IsDeleted)
				{
					var header = Header;
					if (header != null)
					{
						return new[]
						{
							header.AMA_RN_NKCountryInfo,
							header.AMA_ManifestTypeInfo
						};
					}
				}

				return null;
			}
		}

		#endregion

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (Header != null)
			{
				Header.AMA_JobReference = PK.ToString().Substring(0, AsycudaManifestHeader.Schema.AMA_JobReferenceMaxLength);
			}
			else
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				header.AMA_ManifestType = "ASY";
				header.AMA_JobReference = PK.ToString().Substring(0, AsycudaManifestHeader.Schema.AMA_JobReferenceMaxLength);
				ABL_ClusterKey = header.AMA_ClusterKey;
				header.Bills.Add(this);
			}
		}
#endif
	}
}
