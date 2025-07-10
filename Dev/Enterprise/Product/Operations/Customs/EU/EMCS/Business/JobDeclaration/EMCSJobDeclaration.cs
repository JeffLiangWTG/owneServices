using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	[SystemDefinedValues]
	public class EMCSJobDeclaration : AutoEMCSJobDeclaration
		, Integration.Customs.EUEMCS.IJobDeclaration
		, ICusContainerTypeSupporter
		, IEuOfficeCodeProvider
		, Integration.Customs.ICusCodeDataTypeSupporter
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, IEuOfficeCodeCollectionSupporter
		, IControllerIDProvider
		, ICusInvPackTypeSupporter
	{
		public EMCSJobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			BuyerSupplierLinksHelper.Deregister();
		}

		public const string EMCSApplicationCode = BaseJobDeclarationTypeDecider.EMCSApplicationCode;
		public const string EMCSMessageTypeCode = "EMC";

		public new static readonly EMCSJobDeclarationTypeDecider TypeDecider = new EMCSJobDeclarationTypeDecider();

		public new class Schema : BaseJobDeclaration.Schema
		{
			public const string ZG_GuarantorType = nameof(EMCSJobDeclaration.ZG_GuarantorType);
			public const string ZG_DeferredSubmission = nameof(EMCSJobDeclaration.ZG_DeferredSubmission);
			public const string ZG_OriginType = nameof(EMCSJobDeclaration.ZG_OriginType);
			public const string ZG_SubmissionType = nameof(EMCSJobDeclaration.ZG_SubmissionType);
			public const string ZG_DispatchReference = nameof(EMCSJobDeclaration.ZG_DispatchReference);
			public const string EADNumber = nameof(EMCSJobDeclaration.EADNumber);
			public const string InvoiceNumber = nameof(EMCSJobDeclaration.InvoiceNumber);
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static T[] LoadFromEadAndSequenceNumber<T>(BusinessObjectFactory factory, ZString eadNumber, ZString sequenceNumber, GlbCompany company)
				where T : EMCSJobDeclaration
			{
				var query = new ZDBOnlyQuery(typeof(T));
				query.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, EMCSJobDeclaration.EMCSApplicationCode);
				query.AddToFilter(JobDeclarationSchema.JE_MessageType, EMCSJobDeclaration.EMCSMessageTypeCode);
				query.AddToFilter(JobDeclarationSchema.JE_GB, company.Branches.GetPKs());
				query.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + OrderByClause.Descending;

				var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(EU.Business.Declaration.JobDeclaration));
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, eadNumber);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, sequenceNumber);
				subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, company.GC_RN_NKCountryCode);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return factory.Load<T>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(EMCSJobDeclaration);
		}

		protected override JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new EMCSJobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.MessageStatusList))]
		public override ZString JE_MessageStatus
		{
			get => base.JE_MessageStatus;
			set
			{
				if (JE_MessageStatus != value)
				{
					base.JE_MessageStatus = value;

					var readOnly = IsMessageStatusSentOrAcknowledged;

					OwnerDocumentaryAddress.ReadOnly = readOnly;
					DispatchWarehouseDocumentaryAddress.ReadOnly = readOnly;

					FilteredInvoiceLines.SetReadOnlyIncludingChildren(readOnly);
					FilteredInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.SuppliersList))]
		[ResourceStringData("EMCSJobDeclaration|JE_OH_Supplier", Caption = "Consignor")]
		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		public override ZGuid JE_OH_Supplier
		{
			get { return base.JE_OH_Supplier; }
			set
			{
				base.JE_OH_Supplier = value;
				UpdateInvoiceHeaderSupplier();
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("EMCSJobDeclaration|JE_DeclarantType", Caption = "Declaration Type")]
		public override ZString JE_DeclarantType
		{
			get => base.JE_DeclarantType;
			set
			{
				var oldValue = JE_DeclarantType;
				base.JE_DeclarantType = value;
				if (!IsCopying && oldValue != JE_DeclarantType)
				{
					InvoiceLines.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		[ResourceStringData("057bb94f-579c-4322-88da-80df470069ec", Caption = "Registration Status Description")]
		public override ZString JE_EntryStatusDescription => base.JE_EntryStatusDescription;

		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.ConsigneesList))]
		[ResourceStringData("EMCSJobDeclaration|JE_OH_Importer", Caption = "Consignee")]
		public override ZGuid JE_OH_Importer
		{
			get => base.JE_OH_Importer;
			set => base.JE_OH_Importer = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.TransportTypeList))]
		[ResourceStringData("2F975A31-D515-4FDD-8453-DE9EEBB7DAC3", Caption = "Transport Mode")]
		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set => base.JE_TransportMode = value;
		}

		[MaxLength(22)]
		[ResourceStringData("BB027401-17DE-49CD-BDFF-A95F4C0F253B|JE_OwnerRef", Caption = "Local Reference Number", MediumCaption = "Local Ref. No.", ShortCaption = "LRN")]
		public override ZString JE_OwnerRef
		{
			get => base.JE_OwnerRef;
			set => base.JE_OwnerRef = value;
		}

		public override ZString JE_MessageType
		{
			get { return base.JE_MessageType; }
			set
			{
				var oldValue = JE_MessageType;
				SuspendMessageTypeChangeProcess();
				base.JE_MessageType = value;
				if (!IsCopying && oldValue != JE_MessageType)
				{
					Invoices.MarkAsNeedingValidation();
					CusContainers.MarkAsNeedingValidation();
					FilteredInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.MessageSubTypeList))]
		[ResourceStringData("04F29E17-852B-4496-B1F7-B887F8C9E7F6", Caption = "Destination Type")]
		public override ZString JE_MessageSubType
		{
			get { return base.JE_MessageSubType; }
			set
			{
				if (base.JE_MessageSubType != value)
				{
					base.JE_MessageSubType = value;
					AddInfoValidation.ValidateZG_GuarantorType();
				}
			}
		}

		[MaxLength(4)]
		[ResourceStringData("EMCSJobDeclaration|ZG_GuarantorType", Caption = "Guarantor(s)")]
		public override ZString ZG_GuarantorType
		{
			get => base.ZG_GuarantorType;
			set
			{
				var oldValue = ZG_GuarantorType;
				base.ZG_GuarantorType = value;
				if (oldValue != ZG_GuarantorType && !OwnerDocumentaryAddress_Enabled)
				{
					OwnerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				}
			}
		}

		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		[ResourceStringData("4F3E6F18-E17C-4A8E-964E-F39A92894AD8", Caption = "Deferred")]
		public override ZString ZG_DeferredSubmission
		{
			get => base.ZG_DeferredSubmission;
			set => base.ZG_DeferredSubmission = value;
		}

		[MaxLength(35)]
		[ResourceStringData("EMCSJobDeclaration|EADNumber", Caption = "EAD Number")]
		public ZString EADNumber
		{
			get
			{
				var entryNumber = LoadCusEntryNumber(false);
				return entryNumber != null ? entryNumber.CE_EntryNum : ZString.Empty;
			}
			set
			{
				var oldValue = EADNumber;
				CheckMaximumLength(EADNumberInfo, value);
				var entryNumber = LoadCusEntryNumber(true);
				entryNumber.CE_EntryNum = value;
				EADNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo EADNumberInfo => GetZPropertyInfo(Schema.EADNumber);

		[ResourceStringData("BA80CAA7-BFA1-4028-976F-BB5CC5A3005D", Caption = "Report Date")]
		public override ZDateTime JE_EntryAuthorisationDate
		{
			get
			{
				var authorizationDate = base.JE_EntryAuthorisationDate;
				if (authorizationDate.IsEmpty)
				{
					var entryNumber = LoadCusEntryNumber(false);
					authorizationDate = entryNumber != null ? entryNumber.CE_IssueDate : ZDateTime.Empty;
				}
				return authorizationDate;
			}
			set
			{
				base.JE_EntryAuthorisationDate = value;
				var entryNumber = LoadCusEntryNumber(true);
				entryNumber.CE_IssueDate = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.EntryStatusList))]
		[ResourceStringData("BC883972-6CD5-401C-8640-AA561CA8B468", Caption = "Registration Status")]
		public override ZString JE_EntryStatus
		{
			get { return base.JE_EntryStatus; }
			set { base.JE_EntryStatus = value; }
		}

		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		[MaxLength(1)]
		[ResourceStringData("EMCSJobDeclaration|ZG_OriginType", Caption = "Origin Type")]
		public override ZString ZG_OriginType
		{
			get => base.ZG_OriginType;
			set => base.ZG_OriginType = value;
		}

		[MaxLength(1)]
		[ResourceStringData("8D1618B5-1E7D-4EB8-8CA5-6602B0995B99", Caption = "Transport Arr.")]
		public override ZString ZG_TransportArrangement
		{
			get => base.ZG_TransportArrangement;
			set => base.ZG_TransportArrangement = value;
		}

		#region JourneyTime

		public virtual ZInt JourneyTimeNumericPart
		{
			get => ZInt.ParseSafe(ZG_JourneyTime.Substring(0, 2), 1);
			set
			{
				var oldValue = JourneyTimeNumericPart;
				UpdateJourneyTime(value, JourneyTimeFormatPart);
				JourneyTimeNumericPartInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JourneyTimeNumericPartInfo => GetZPropertyInfo(nameof(JourneyTimeNumericPart));

		[BusinessObjectMaxLengthTestExclude]
		[List(nameof(Lookups) + "." + nameof(EMCSJobDeclarationLookups.JourneyTimeUnitList))]
		[MaxLength(1)]
		public virtual ZString JourneyTimeFormatPart
		{
			get
			{
				var journeyTime = ZG_JourneyTime;
				return journeyTime.Length != 3 ? new ZString(JourneyTimeUnitList.Codes.Hours) : journeyTime.Substring(2, 1);
			}
			set
			{
				var oldValue = JourneyTimeFormatPart;
				CheckMaximumLength(JourneyTimeFormatPartInfo, value);
				UpdateJourneyTime(JourneyTimeNumericPart, value);
				JourneyTimeFormatPartInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JourneyTimeFormatPartInfo => GetZPropertyInfo(nameof(JourneyTimeFormatPart));

		public void UpdateJourneyTime(ZInt value, ZString format)
		{
			ZG_JourneyTime = ZString.Format("{0,2}{1}", value, format);
		}

		public override ZString ZG_JourneyTime
		{
			get => base.ZG_JourneyTime;
			set
			{
				var oldValue = ZG_JourneyTime;
				base.ZG_JourneyTime = value;
				if (!IsCopying && oldValue != ZG_JourneyTime)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateJourneyTime();
					}
				}
			}
		}

		public override ZBool ZG_ExplanationOnReasonForShortageValidation
		{
			get => base.ZG_ExplanationOnReasonForShortageValidation;
			set
			{
				var oldValue = ZG_ExplanationOnReasonForShortageValidation;
				base.ZG_ExplanationOnReasonForShortageValidation = value;
				if (oldValue != value && !IsValidationSuspended)
				{
					FilteredInvoiceLines.Cast<EMCSJobComInvoiceLine>().ForEach(x => x.Outturn.MarkAsNeedingValidation());
				}
			}
		}

		#endregion

		[MaxLength(35)]
		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		public override ZString ZG_DispatchReference
		{
			get => base.ZG_DispatchReference;
			set => base.ZG_DispatchReference = value;
		}

		[MaxLength(2)]
		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		public override ZString ZG_CCTMSA
		{
			get => base.ZG_CCTMSA;
			set => base.ZG_CCTMSA = value;
		}

		[MaxLength(255)]
		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		public override ZString ZG_CertOfExemption
		{
			get => base.ZG_CertOfExemption;
			set => base.ZG_CertOfExemption = value;
		}

		[MaxLength(1)]
		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		[ResourceStringData("EMCSJobDeclaration|ZG_SubmissionType", Caption = "Submission Type")]
		public override ZString ZG_SubmissionType
		{
			get => base.ZG_SubmissionType;
			set => base.ZG_SubmissionType = value;
		}

		[MaxLength(35)]
		[ResourceStringData("EMCSJobDeclaration|InvoiceNumber", Caption = "Invoice Number")]
		public virtual ZString InvoiceNumber
		{
			get { return InvoiceHeader.JZ_InvoiceNumber; }
			set
			{
				InvoiceHeader.JZ_InvoiceNumber = value;
				InvoiceNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo InvoiceNumberInfo => GetWrappedZPropertyInfo(Schema.InvoiceNumber, (x) => InvoiceHeader.JZ_InvoiceNumberInfo);

		public ZDateTime InvoiceDate
		{
			get { return InvoiceHeader.JZ_InvoiceDate; }
			set { InvoiceHeader.JZ_InvoiceDate = value; }
		}

		public ZPropertyInfo InvoiceDateInfo => GetWrappedZPropertyInfo(nameof(InvoiceDate), (x) => InvoiceHeader.JZ_InvoiceDateInfo);

		public EMCSJobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (invoiceHeader == null)
				{
					if (Invoices.Count > 0)
					{
						invoiceHeader = Invoices.OfType<EMCSJobComInvoiceHeader>().OrderBy(x => x.PK).First();
					}
					else
					{
						using (SuspendSettingHasChanges())
						{
							invoiceHeader = Invoices.AddNew();
							UpdateInvoiceHeaderSupplier();
						}
					}
				}

				return invoiceHeader;
			}
		}
		EMCSJobComInvoiceHeader invoiceHeader;

		public ZBool IsMessageStatusSentOrAcknowledged => Factory.GetValue(ref isMessageStatusSentOrAcknowledgedCached, () => JE_MessageStatus == EDIMessage.Status.Sent || JE_MessageStatus == EDIMessage.Status.Acknowledged);
		CachedProperty<ZBool> isMessageStatusSentOrAcknowledgedCached;

		#endregion

		#region Configurations

		public EMCSJobDeclarationMessageSendingConfiguration MessageSendingConfiguration => messageSendingConfiguration ??= GetNewMessageSendingConfiguration();
		EMCSJobDeclarationMessageSendingConfiguration messageSendingConfiguration;

		protected virtual EMCSJobDeclarationMessageSendingConfiguration GetNewMessageSendingConfiguration() => new EMCSJobDeclarationMessageSendingConfiguration();

		#endregion

		public bool IsConsignor => JE_DeclarantType == EMCSEntryTypeList.Codes.Consignor;

		public bool IsConsignee => JE_DeclarantType == EMCSEntryTypeList.Codes.Consignee;

		protected override void SetDefaultValues()
		{
			using (AddInfo.SuspendSettingHasChanges())
			using (SuspendMarkingAsNeedingValidation())
			{
				base.SetDefaultValues();
				JE_ApplicationCode = EMCSApplicationCode;
				JE_MessageType = EMCSMessageTypeCode;
				JE_MessageType_ReadOnly = true;
				JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				JE_OwnerRef = NextLocalReferenceNumber;
				JE_TransportMode = ZString.Empty;
				JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements;
				ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
				ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;
				ZG_SubmissionType = EMCSSubmissionTypeList.Codes.StandardSubmission;
				ZG_DeferredSubmission = EMCSDeferredSubmissionList.Codes.No;
				CustomsOffices.CreateDefaultEntry(true);
			}
		}

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;
				case TransportTypeList.Codes.Mail:
					return TransportTypeGenericList.Codes.PostMail;
				case TransportTypeList.Codes.Sea:
					return TransportTypeGenericList.Codes.Sea;
				case Core.Constants.TransportModes.Road:
					return TransportTypeGenericList.Codes.Road;
				case Core.Constants.TransportModes.Rail:
					return TransportTypeGenericList.Codes.Rail;
				case Core.Constants.TransportModes.FixedTransportInstallations:
					return TransportTypeGenericList.Codes.FixedTransportInstallations;
				case Core.Constants.TransportModes.InlandWaterwayTransport:
					return TransportTypeGenericList.Codes.InlandWaterwayTransport;
				case Core.Constants.TransportModes.Other:
					return TransportTypeGenericList.Codes.Other;
			}
			return ZString.Empty;
		}

		protected override string GetDefaultMessageType(bool import) => EMCSMessageTypeCode;

		protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			if (!fIsImportingData && ShouldSetDefaultValuesFromSupplier)
			{
				DefaultSupplierDocAddresses(newValue);
			}
		}

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
		}

		public CusEntryNumber LoadCusEntryNumber(bool create)
		{
			if (cusEntryNumber == null || cusEntryNumber.IsDeleted)
			{
				cusEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);

				if (cusEntryNumber == null && create)
				{
					cusEntryNumber = CusEntryNumber.New(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
				}

				if (cusEntryNumber != null)
				{
					RegisterEditableChildObject(cusEntryNumber);
				}
			}
			return cusEntryNumber;
		}
		CusEntryNumber cusEntryNumber;

		#region Invoices

		[ChildEditable]
		public new EMCSInvoiceHeaderActiveCollection Invoices
		{
			get { return (EMCSInvoiceHeaderActiveCollection)base.Invoices; }
		}

		protected override InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection()
		{
			return new EMCSInvoiceHeaderActiveCollection(this);
		}

		[ChildEditable(true)]
		public new EMCSInvoiceLineCompleteCollection InvoiceLines
		{
			get { return (EMCSInvoiceLineCompleteCollection)base.InvoiceLines; }
		}

		protected override InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection()
		{
			return new EMCSInvoiceLineCompleteCollection(this);
		}

		public override List<BaseJobComInvoiceHeader> LoadInvoicesFromQuery(ZQuery query)
		{
			return Factory.Load<EMCSJobComInvoiceHeader>(query).Cast<BaseJobComInvoiceHeader>().ToList();
		}

		#endregion

		#region GroupHeaders

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<EMCSJobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<EMCSJobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<EMCSJobComInvoiceGroupHeader>(this);

		protected override GroupHeaderCollection CreateAllGroupHeadersCollection() => new EMCSGroupHeaderCollection(this);

		#endregion

		#region FilteredInvoiceLines

		public new EMCSInvoiceLineViewCollection FilteredInvoiceLines
		{
			get => (EMCSInvoiceLineViewCollection)base.FilteredInvoiceLines;
		}

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection()
		{
			return new EMCSInvoiceLineViewCollection(this);
		}

		#endregion

		#region DispatchTime
		[ReadOnlyMember(nameof(IsMessageStatusSentOrAcknowledged))]
		[ResourceStringData("f5860596-1967-452c-93a8-99bc84eaa7e4", Caption = "Dispatch Time")]
		public override ZDateTime JE_DateAtOrigin
		{
			get => base.JE_DateAtOrigin;
			set => base.JE_DateAtOrigin = value;
		}

		#endregion

		#region SpecialInstructions

		[MaxLength(350)]
		[BusinessObjectTestExclude]
		[ResourceStringData("0EE01A4A-8117-47B2-B775-E3435E42E85B", Caption = "Complementary Information (Transport Mode)")]
		public ZString SpecialInstructions
		{
			get { return SpecialInstructionsNoteWriter.Value; }
			set
			{
				CheckMaximumLength(SpecialInstructionsInfo, value);
				if (SpecialInstructionsNoteWriter.UpdateValue(value))
				{
					SpecialInstructionsInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateSpecialInstructions();
					}
				}
			}
		}

		public ZPropertyInfo SpecialInstructionsInfo => GetZPropertyInfo(nameof(SpecialInstructions));

		PredefinedNoteWriter SpecialInstructionsNoteWriter
		{
			get { return specialInstructionsNoteWriter ?? (specialInstructionsNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.SpecialInstructions)); }
		}
		PredefinedNoteWriter specialInstructionsNoteWriter;

		#endregion

		#region DocumentaryAddresses

		#region PiggyBackedDocAddressValidation

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new EMCSJobDeclarationJobDocAddressValidation(addressToValidate, this);
		}

		#endregion

		#region OwnerDocumentaryAddress

		public JobDocAddress OwnerDocumentaryAddress
		{
			get
			{
				if (ownerDocumentaryAddress != null && ownerDocumentaryAddress.IsDeleted)
				{
					ownerDocumentaryAddress.DocAddressChanged -= OwnerDocumentaryAddress_DocAddressChanged;
					ownerDocumentaryAddress = null;
				}

				if (ownerDocumentaryAddress == null)
				{
					ownerDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(JobDocAddressRequirementProvider.GoodsOwnerDocAddressRequirement);
					ownerDocumentaryAddress.DocAddressChanged += OwnerDocumentaryAddress_DocAddressChanged;
					ownerDocumentaryAddress.AdditionalValidation = PiggyBackedDocAddressValidation(ownerDocumentaryAddress);
					ownerDocumentaryAddress.ReadOnly = IsMessageStatusSentOrAcknowledged;
				}

				return ownerDocumentaryAddress;
			}
		}
		JobDocAddress ownerDocumentaryAddress;

		public ZBool OwnerDocumentaryAddress_Enabled => ZG_GuarantorType == EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts;

		void OwnerDocumentaryAddress_DocAddressChanged(object sender, EventArgs e)
		{
			UpdateInvoiceHeaderSupplier();
		}

		#endregion

		#region CarrierAgentDocumentaryAddress

		public JobDocAddress CarrierAgentDocumentaryAddress
		{
			get
			{
				if (carrierAgentDocumentaryAddress == null || carrierAgentDocumentaryAddress.IsDeleted)
				{
					carrierAgentDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(JobDocAddressRequirementProvider.CarrierAgentDocumentaryAddressRequirement);
					carrierAgentDocumentaryAddress.AdditionalValidation = PiggyBackedDocAddressValidation(carrierAgentDocumentaryAddress);
				}

				return carrierAgentDocumentaryAddress;
			}
		}
		JobDocAddress carrierAgentDocumentaryAddress;

		#endregion

		#region TransporterDocumentaryAddress

		public JobDocAddress TransporterDocumentaryAddress
		{
			get
			{
				if (transporterDocumentaryAddress == null || transporterDocumentaryAddress.IsDeleted)
				{
					transporterDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(JobDocAddressRequirementProvider.TransporterDocumentaryAddressRequirement);
					transporterDocumentaryAddress.AdditionalValidation = PiggyBackedDocAddressValidation(transporterDocumentaryAddress);
				}

				return transporterDocumentaryAddress;
			}
		}
		JobDocAddress transporterDocumentaryAddress;

		#endregion

		#region DispatchWarehouseDocumentaryAddress

		public JobDocAddress DispatchWarehouseDocumentaryAddress
		{
			get
			{
				if (dispatchWarehouseDocumentaryAddress == null || dispatchWarehouseDocumentaryAddress.IsDeleted)
				{
					dispatchWarehouseDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(JobDocAddressRequirementProvider.DispatchWarehouseDocumentaryAddressRequirement);
					dispatchWarehouseDocumentaryAddress.AdditionalValidation = PiggyBackedDocAddressValidation(dispatchWarehouseDocumentaryAddress);
					dispatchWarehouseDocumentaryAddress.ReadOnly = IsMessageStatusSentOrAcknowledged;
				}

				return dispatchWarehouseDocumentaryAddress;
			}
		}
		JobDocAddress dispatchWarehouseDocumentaryAddress;

		#endregion

		#region DestinationWarehouseDocumentaryAddress

		public JobDocAddress DestinationWarehouseDocumentaryAddress
		{
			get
			{
				if (destinationWarehouseDocumentaryAddress == null || destinationWarehouseDocumentaryAddress.IsDeleted)
				{
					destinationWarehouseDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(JobDocAddressRequirementProvider.DestinationWarehouseDocumentaryAddressRequirement);
					destinationWarehouseDocumentaryAddress.AdditionalValidation = PiggyBackedDocAddressValidation(destinationWarehouseDocumentaryAddress);
				}

				return destinationWarehouseDocumentaryAddress;
			}
		}
		JobDocAddress destinationWarehouseDocumentaryAddress;

		#endregion

		protected override DocAddressType[] SupportedAddressTypesCore => new[]
		{
			DocAddressType.ImporterDocumentaryAddress,
			DocAddressType.SupplierDocumentaryAddress,
			DocAddressType.GoodsOwner,
			DocAddressType.CarrierAgent,
			DocAddressType.Transporter,
			DocAddressType.DispatchWarehouse,
			DocAddressType.DestinationWarehouse
		};

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.SupplierDocumentaryAddress:
					return SupplierDocAddressRequirement;
				case DocAddressType.ImporterDocumentaryAddress:
					return ImporterDocAddressRequirement;
				case DocAddressType.GoodsOwner:
					return JobDocAddressRequirementProvider.GoodsOwnerDocAddressRequirement;
				case DocAddressType.CarrierAgent:
					return JobDocAddressRequirementProvider.CarrierAgentDocumentaryAddressRequirement;
				case DocAddressType.Transporter:
					return JobDocAddressRequirementProvider.TransporterDocumentaryAddressRequirement;
				case DocAddressType.DispatchWarehouse:
					return JobDocAddressRequirementProvider.DispatchWarehouseDocumentaryAddressRequirement;
				case DocAddressType.DestinationWarehouse:
					return JobDocAddressRequirementProvider.DestinationWarehouseDocumentaryAddressRequirement;
				default:
					return null;
			}
		}

		protected override JobDocAddress GetSupplierPickupAddress() => DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierPickupDeliveryAddress);

		protected override JobDocAddressRequirement AddSupplierDocAddressRequirement()
		{
			var requirement = GetSupplierDocAddressRequirement();
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		protected override JobDocAddressRequirement GetSupplierDocAddressRequirement()
		{
			var result = base.GetSupplierDocAddressRequirement();
			JobDocAddressRequirementProvider.SetJobDocAddressRequirement(result);
			return result;
		}

		protected override JobDocAddress GetImporterDeliveryAddress() => DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ImporterPickupDeliveryAddress);

		protected override JobDocAddressRequirement AddImporterDocAddressRequirement()
		{
			var requirement = GetImporterDocumentaryAddressRequirement();
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		protected override JobDocAddressRequirement GetImporterDocumentaryAddressRequirement()
		{
			var result = base.GetImporterDocumentaryAddressRequirement();
			JobDocAddressRequirementProvider.SetJobDocAddressRequirement(result);
			return result;
		}

		JobDocAddressRequirementProvider JobDocAddressRequirementProvider
		{
			get { return fJobDocAddressRequirementProvider ?? (fJobDocAddressRequirementProvider = new JobDocAddressRequirementProvider(Factory)); }
		}
		JobDocAddressRequirementProvider fJobDocAddressRequirementProvider;

		protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.SupplierDocumentaryAddressChanged(sender, e);
			var supplier = SupplierDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
			if (JE_OH_Supplier != supplier)
			{
				JE_OH_Supplier = supplier;
			}
		}

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);
			var importer = ImporterDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
			if (JE_OH_Importer != importer)
			{
				JE_OH_Importer = importer;
			}
		}

		#endregion

		#region ImportSADNumbers

		[ChildEditable(true)]
		public ImportSADNumberCollection ImportSADNumbers
		{
			get
			{
				if (sadNumbers == null)
				{
					sadNumbers = CreateNewImportSADNumberCollection();
					sadNumbers.Load();
					RegisterEditableChildObject(sadNumbers);
				}
				return sadNumbers;
			}
		}
		ImportSADNumberCollection sadNumbers;

		protected virtual ImportSADNumberCollection CreateNewImportSADNumberCollection() => new ImportSADNumberCollection<ImportSADNumber>(this);

		#endregion

		#region Customs Offices

		#region CustomsOffices, OfficeOfExit

		EuOfficeCodeCollection IEuOfficeCodeCollectionSupporter.CustomsOffices => CustomsOffices;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public OfficeCodeCollection CustomsOffices
		{
			get
			{
				if (customsOffices == null)
				{
					customsOffices = GetCustomsOffices();
					customsOffices.DefaultPurposeCode = EMCSCustomsOfficeRequirementHelper.MainOfficeTypeCode;
					customsOffices.Load();
					RegisterEditableChildObject(customsOffices);
				}
				return customsOffices;
			}
		}
		OfficeCodeCollection customsOffices;

		IEnumerable<EuOfficeCode> IEuOfficeCodeProvider.CustomsOffices => CustomsOffices.Cast<EuOfficeCode>();

		protected virtual OfficeCodeCollection GetCustomsOffices() => new OfficeCodeCollection<OfficeCode>(this);

		public ZString OfficeOfExit { get { return IsExport ? JE_CustomsOffice : ZString.Empty; } }

		bool IEuOfficeCodeProvider.IsNCTS => false;

		bool IEuOfficeCodeProvider.IsEMCS => true;

		#endregion

		#region CustomsOfficeRequirementHelper

		public CustomsOfficeRequirementHelper CustomsOfficeRequirementHelper => customsOfficeRequirementHelper ?? (customsOfficeRequirementHelper = GetCustomsOfficeRequirementHelper());
		CustomsOfficeRequirementHelper customsOfficeRequirementHelper;

		protected virtual CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new EMCSCustomsOfficeRequirementHelper(this);

		#endregion

		#endregion

		#region Documents

		[ChildEditable(true)]
		public IEMCSDocumentCollection<EMCSDocument> Documents => fEMCSDocuments ??= GetEMCSDocuments();
		IEMCSDocumentCollection<EMCSDocument> fEMCSDocuments;

		IEMCSDocumentCollection<EMCSDocument> GetEMCSDocuments()
		{
			if (fEMCSDocuments == null)
			{
				fEMCSDocuments = CreateNewEMCSDocumentCollection();
				fEMCSDocuments.EnableMaxCountValidation(9, Res.GetString("eff03d8d-fb46-475f-91df-001bac5fe07d", "There are too many Documents. Maximum of 9."), false);
				fEMCSDocuments.Load();

				RegisterEditableChildObject(fEMCSDocuments);
			}

			return fEMCSDocuments;
		}

		protected virtual IEMCSDocumentCollection<EMCSDocument> CreateNewEMCSDocumentCollection() => new EMCSDocumentCollection<EMCSDocument>(this);

		#endregion

		#region public PreviousDocumentCollection PreviousDocuments

		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments => fPreviousDocuments ?? (fPreviousDocuments = GetPreviousDocuments());
		PreviousDocumentCollection fPreviousDocuments;

		PreviousDocumentCollection GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		#endregion

		#region Transports

		[ChildEditable(true)]
		public new EMCSCusContainerCollection CusContainers => (EMCSCusContainerCollection)base.CusContainers;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new EMCSCusContainerCollection(this);

		public override ZBool ContainersAlwaysRequired => true;

		#endregion

		#region Validation

		public EMCSProvider EMCSProvider => emcsProvider ?? (emcsProvider = EMCSProvider.GetByCountryCode(GetDefaultDataGroupingCode()));
		EMCSProvider emcsProvider;

		public EMCSAddInfoJobComInvoiceLineValidation GetAddInfoJobComInvoiceLineValidation(EMCSAddInfoJobComInvoiceLine addInfoJobComInvoiceLine) => GetAddInfoJobComInvoiceLineValidationCore(addInfoJobComInvoiceLine);
		protected EMCSAddInfoJobComInvoiceLineValidation GetAddInfoJobComInvoiceLineValidationCore(EMCSAddInfoJobComInvoiceLine addInfo) => EMCSProvider.GetNewAddInfoValidation(addInfo);

		public new EMCSJobDeclarationValidation Validation
		{
			get { return (EMCSJobDeclarationValidation)base.Validation; }
		}

		protected override JobDeclarationValidation GetNewValidation()
		{
			return new EMCSJobDeclarationValidation(this);
		}

		#endregion

		#region Lookups

		public new EMCSJobDeclarationLookups Lookups
		{
			get { return (EMCSJobDeclarationLookups)(base.Lookups); }
		}

		protected override JobDeclarationLookups GetNewLookups()
		{
			return new EMCSJobDeclarationLookups(this);
		}

		#endregion

		#region ICusCodeDataTypeSupporter

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, OfficeCodeType }
			};
		}

		protected virtual Type OfficeCodeType => typeof(OfficeCode);

		#endregion

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.ImportSad, typeof(ImportSADNumber) },
			{ CusSupportingInfoTypeList.Codes.Certificate, typeof(EMCSDocument) },
			{ CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
		};

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region ICusContainerTypeSupporter

		Type ICusContainerTypeSupporter.GetCusContainerType() => typeof(EMCSCusContainer);

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.EU.EMCS;
		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		#region Merge Functionality

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override bool HasSplitEntriesCore => false;

		protected override bool UseDeclarationContainersIfNoneFoundOnEntryCore => false;

		#endregion

		#region Packages

		[ChildEditable(true)]
		public EMCSPackageCollection EMCSPackages => fEMCSPackages ?? (fEMCSPackages = GetEMCSPackages());
		EMCSPackageCollection fEMCSPackages;

		EMCSPackageCollection GetEMCSPackages()
		{
			var result = CreateNewEMCSPackagesCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual EMCSPackageCollection CreateNewEMCSPackagesCollection() => new EMCSPackageCollection(this);

		#endregion

		#region ICusInvPackTypeSupporter

		Type ICusInvPackTypeSupporter.PackType => typeof(EMCSPackage);

		TypeDecider ICusInvPackTypeSupporter.PackTypeDecider => null; // Add type decider if need to create any country specific EMCSPackage. Set PackType to null.

		#endregion

		#region PackageLineNumberGenerator

		public IEnumerable<IShortSequenceNumberLine> PackageLines => new TypedEnumerable<IShortSequenceNumberLine>(EMCSPackages);

		public IDisposable GetPackageLineNumberRenumberingSuspender() => PackageLineNumberGenerator.GetLineNumberSuspender();

		public ShortSequenceNumberGenerator PackageLineNumberGenerator => packageLineNumberGenerator ?? (packageLineNumberGenerator = new ShortSequenceNumberGenerator(() => PackageLines));

		ShortSequenceNumberGenerator packageLineNumberGenerator;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(JE_UCRInfo, GetNewUCR);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (IsInDatabase)
				{
					JE_EntryStatus = (ZString)JE_EntryStatusInfo.OriginalValue;
					JE_EntrySubmittedDate = (ZDateTime)JE_EntrySubmittedDateInfo.OriginalValue;
					JE_MessageStatus = (ZString)JE_MessageStatusInfo.OriginalValue;
				}
				else
				{
					JE_EntryStatus = ZString.Empty;
					JE_EntrySubmittedDate = ZDateTime.Empty;
					JE_MessageStatus = ZString.Empty;
					JE_UCR = ZString.Empty;
				}
			}
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				JE_UCRInfo.RefreshBinding();
			}
		}

		public override void Delete()
		{
			base.Delete();

			using (GetPackageLineNumberRenumberingSuspender())
			{
				EMCSPackages.RemoveAndDeleteAll();
			}
		}

		ZString GetNewUCR(BusinessObjectFactory factory)
		{
			ZString year = ZDate.Today.Year.ToString(CultureInfo.InvariantCulture);
			return ZString.Format("{0}-{1}", year.Right(1), JE_DeclarationReference).Left(JE_UCRInfo.MaxLength);
		}

		ZString NextLocalReferenceNumber => ((ZInt)Env.NumberFountains.EMCSLocalReferenceNumber.PeekPreliminary(Factory)).ToString();

		protected override ZString GetNewDeclarationReference(BusinessObjectFactory factory)
		{
			var generatorTarget = new EMCSLocalReferenceNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = GetNewNumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.EMCSLocalReferenceNumber,
				FountainGetter = Env.NumberFountains.GetEMCSLocalReferenceNumberFountain,
				PrimaryTarget = generatorTarget
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(this));

			generator.Generate();
			generator.EnforceMaxLengths();
			return generatorTarget.Value.ToUpper();
		}

		protected override DocumentSupporter CreateNewDocumentSupporter() => new EMCSJobDeclarationDocumentSupporter(this);

		void UpdateInvoiceHeaderSupplier()
		{
			var goodsOwnerAddress = OwnerDocumentaryAddress;
			var currentSupplier = (goodsOwnerAddress != null && goodsOwnerAddress.OrganisationPK.IsValid) ? goodsOwnerAddress.OrganisationPK : JE_OH_Supplier;
			if (InvoiceHeader.JZ_OH_Supplier != currentSupplier)
			{
				InvoiceHeader.JZ_OH_Supplier = currentSupplier;
				MarkAsNeedingValidation();
			}
		}

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new EMCSJobDeclarationBusinessObjectTestDataHelper(this);
		}

		class EMCSJobDeclarationBusinessObjectTestDataHelper : JobDeclarationBusinessObjectTestDataHelper
		{
			public EMCSJobDeclarationBusinessObjectTestDataHelper(EMCSJobDeclaration declaration) : base(declaration)
			{
			}

			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (collectionProperty.Name != nameof(CustomsEntryInstructions))
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}
		}
#endif
	}
}
