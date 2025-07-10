using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[SystemDefinedValues]
	public partial class QuarantineExDocHeader : AutoQuarantineExDocHeader,
		Integration.Customs.AU.IQuarantineExdocHeader,
		IDeclarationProvider,
		IAddInfoManager,
		ICusAddInfoTypeSupporter,
		IEXDOCRefCodeTypeProvider,
		IAdditionalBusinessObjectFetchStrategyProvider,
		ICusSupportingInfoTypeSupporter,
		ICusCodeDataTypeSupporter,
		INEXDOCResponse,
		INEXDOCMessageParent,
		IClusterKeyWorker
	{
		public QuarantineExDocHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoQuarantineExDocHeader.Schema
		{
			public const string QH_PrintLocation = "QH_PrintLocation";
			public const string QH_StorageLocation = "QH_StorageLocation";
			public const string QH_TransferEDIUserLocation = "QH_TransferEDIUserLocation";
			public const string QH_TransferExporterLocation = "QH_TransferExporterLocation";
			public const string QH_RequestForPermitNumber = "QH_RequestForPermitNumber";
			public const string QH_RequestForPermitNumberStatusDescription = "QH_RequestForPermitNumberStatusDescription";
			public const string QH_RequestForPermitNumberPrintDescription = "QH_RequestForPermitNumberPrintDescription";
			public const string QH_ExportPermitNumber = "QH_ExportPermitNumber";
			public const string QH_AuthorisationFlag = "QH_AuthorisationFlag";
			public const string QH_AuthorisationLocation = "QH_AuthorisationLocation";
			public const string QH_ForwardLocation = "QH_ForwardLocation";
			public const string QH_ProductUseIndicator = "QH_ProductUseIndicator";
			public const string QH_TrueAndCompleteIndicator = "QH_TrueAndCompleteIndicator";
			public const string QH_ApprovedCertifier = "QH_ApprovedCertifier";
			public const string QH_AvAnimalAge = "QH_AvAnimalAge";
			public const string QH_QuarantineMessageMaxLine = "QH_QuarantineMessageMaxLine";
			public const string QH_ApprovalNumber = "QH_ApprovalNumber";
			public const string QH_TransitLocationType = "QH_TransitLocationType";
			public const string QH_ForwardRequiresAcceptance = "QH_ForwardRequiresAcceptance";

			public const int QH_ExportPermitNumberMaxLength = AutoCusEntryNum.Schema.CE_EntryNumMaxLength;
		}

		[ChildEditable(true)]
		public QuarantineExDocShipsCompartmentCollection Compartments
		{
			get
			{
				if (fCompartments == null)
				{
					fCompartments = new QuarantineExDocShipsCompartmentCollection(this);
					fCompartments.Load();
					RegisterEditableChildObject(fCompartments);
				}
				return fCompartments;
			}
		}
		QuarantineExDocShipsCompartmentCollection fCompartments;

		[ChildEditable(true)]
		public QuarantineExDocRexAcknowledgementCollection Acknowledgements
		{
			get
			{
				if (fAcknowledgements == null)
				{
					fAcknowledgements = new QuarantineExDocRexAcknowledgementCollection(this);
					fAcknowledgements.Load();
					RegisterEditableChildObject(fAcknowledgements);
				}
				return fAcknowledgements;
			}
		}
		QuarantineExDocRexAcknowledgementCollection fAcknowledgements;

		[ChildEditable(true)]
		public QuarantineCatchZoneCollection NexDocCatchZones
		{
			get
			{
				if (fNexDocCatchZones == null)
				{
					fNexDocCatchZones = new QuarantineCatchZoneCollection(this);
					fNexDocCatchZones.Load();
					RegisterEditableChildObject(fNexDocCatchZones);
				}
				return fNexDocCatchZones;
			}
		}
		QuarantineCatchZoneCollection fNexDocCatchZones;

		#region SupportingInfos

		[ChildEditable(true)]
		public QuarantineSupportingInfoCollection SupportingInfos
		{
			get
			{
				if (fSupportingInfos == null)
				{
					fSupportingInfos = new QuarantineSupportingInfoCollection(this);
					fSupportingInfos.Load();
					RegisterEditableChildObject(fSupportingInfos);
				}

				return fSupportingInfos;
			}
		}
		QuarantineSupportingInfoCollection fSupportingInfos;

		#endregion

		public JobDeclaration Declaration => InvoiceHeader?.JobDeclaration;

		public JobComInvoiceHeader InvoiceHeader
		{
			get { return Factory.Load<JobComInvoiceHeader>(QH_JZ); }
		}

		#region IDeclarationProvider Members
		BaseJobDeclaration IDeclarationProvider.Declaration => Declaration;
		#endregion

		#region AddInfo Members
		public AddInfoQuarantineExDocHeader AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoQuarantineExDocHeader(QH_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}

		AddInfoQuarantineExDocHeader fAddInfo;
		#endregion

		public override void Delete()
		{
			if (!IsDeleted && CanDelete)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Compartments.RemoveAndDeleteAll();
				RecommendationLetters.RemoveAndDeleteAll();
				SupportingInfos.RemoveAndDeleteAll();
				CertificateNumbers.DeleteAll();
			}
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (AddInfo.SuspendMarkingAsNeedingValidation())
			using (AddInfo.SuspendSettingHasChanges())
			{
				QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.No;
				QH_PrintLocation = EXDOCCodeOrganisation.Codes.Code;
				QH_StorageLocation = EXDOCCodeOrganisation.Codes.Code;
				QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Code;
				QH_ForwardLocation = EXDOCCodeOrganisation.Codes.Code;
				QH_TransferEDIUserLocation = EXDOCCodeOrganisation.Codes.Code;
				QH_TransferExporterLocation = EXDOCCodeOrganisation.Codes.Code;
			}
		}

		#region QH_RN_NKOriginCountry
		[BusinessObjectTestExclude]
		public override ZString QH_RN_NKOriginCountry
		{
			get
			{
				var result = base.QH_RN_NKOriginCountry;
				if (result.IsEmpty)
				{
					result = Core.Constants.CountryCodes.Australia;
				}
				return result;
			}
			set { base.QH_RN_NKOriginCountry = value; }
		}
		#endregion

		#region QH_PrintLocation
		public ZString QH_PrintLocation
		{
			get { return AddInfo.ZH_PrintLocation; }
			set
			{
				if (AddInfo.ZH_PrintLocation != value)
				{
					AddInfo.ZH_PrintLocation = value;
					QH_OH_PrintLocationOrganisation = ZGuid.Empty;
				}
			}
		}

		public ZPropertyInfo QH_PrintLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_PrintLocation, x => AddInfo.ZH_PrintLocationInfo); }
		}

		#endregion

		#region QH_OH_PrintLocationOrganisation
		public override ZGuid QH_OH_PrintLocationOrganisation
		{
			get { return base.QH_OH_PrintLocationOrganisation; }
			set
			{
				var oldValue = base.QH_OH_PrintLocationOrganisation;
				base.QH_OH_PrintLocationOrganisation = value;
				var organisation = Factory.Load<OrgHeader>(value);
				if (oldValue != base.QH_OH_PrintLocationOrganisation && !IsCopying)
				{
					if (organisation != null)
					{
						QH_CertificateRequiredLocation = organisation.CustomsCodes.GetCustomsRegNo(UserIdentifierCusCode, Core.Constants.CountryCodes.Australia);
					}
				}
			}
		}
		#endregion

		#region QH_StorageLocation
		public ZString QH_StorageLocation
		{
			get { return AddInfo.ZH_StorageLocation; }
			set
			{
				var hasChanges = QH_StorageLocation != value;
				AddInfo.ZH_StorageLocation = value;
				if (hasChanges && !IsCopying)
				{
					QH_OA_StorageEstablishment = ZGuid.Empty;
				}
			}
		}

		public ZPropertyInfo QH_StorageLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_StorageLocation, x => AddInfo.ZH_StorageLocationInfo); }
		}
		#endregion

		#region QH_OA_StorageEstablishment

		public override ZGuid QH_OA_StorageEstablishment
		{
			get { return base.QH_OA_StorageEstablishment; }
			set
			{
				var hasChanges = QH_OA_StorageEstablishment != value;
				base.QH_OA_StorageEstablishment = value;
				if (hasChanges && !IsCopying)
				{
					var storageEstablishment = StorageEstablishment;
					if (storageEstablishment != null)
					{
						QH_StorageEstablishment = storageEstablishment.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, Core.Constants.CountryCodes.Australia);
					}
				}

				Validation.ValidateQH_OA_StorageEstablishment();
			}
		}

		#endregion

		#region QH_AuthorisationFlag

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineExDocHeader|QH_AuthorisationFlag", Caption = "Authorized", FullDescription = "Is this Exporter registered as an automated export permit issuer (AEPI)?")]
		public ZBool QH_AuthorisationFlag
		{
			get { return AddInfo.ZH_AuthorisationFlag; }
			set { AddInfo.ZH_AuthorisationFlag = value; }
		}

		public ZPropertyInfo QH_AuthorisationFlagInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_AuthorisationFlag, x => AddInfo.ZH_AuthorisationFlagInfo); }
		}
		#endregion

		#region QH_AuthorisationLocation
		public ZString QH_AuthorisationLocation
		{
			get { return AddInfo.ZH_AuthorisationLocation; }
			set
			{
				var hasChanges = QH_AuthorisationLocation != value;
				AddInfo.ZH_AuthorisationLocation = value;
				if (hasChanges && !IsCopying)
				{
					QH_OA_AuthorisationEstablishment = ZGuid.Empty;
					QH_AuthorisationEstablishment = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo QH_AuthorisationLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_AuthorisationLocation, x => AddInfo.ZH_AuthorisationLocationInfo); }
		}
		#endregion

		#region QH_OA_AuthorisationEstablishment

		public override ZGuid QH_OA_AuthorisationEstablishment
		{
			get { return base.QH_OA_AuthorisationEstablishment; }
			set
			{
				var hasChanges = QH_OA_AuthorisationEstablishment != value;
				base.QH_OA_AuthorisationEstablishment = value;
				if (hasChanges && !IsCopying)
				{
					var authorisationEstablishment = AuthorisationEstablishment;
					if (authorisationEstablishment != null)
					{
						QH_AuthorisationEstablishment = authorisationEstablishment.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, Core.Constants.CountryCodes.Australia);
					}
				}

				Validation.ValidateQH_OA_AuthorisationEstablishment();
			}
		}

		#endregion

		#region QH_ForwardLocation
		public ZString QH_ForwardLocation
		{
			get { return AddInfo.ZH_ForwardLocation; }
			set
			{
				if (AddInfo.ZH_ForwardLocation != value)
				{
					AddInfo.ZH_ForwardLocation = value;
					QH_OH_ForwardLocationOrganisation = ZGuid.Empty;
				}
			}
		}
		public ZPropertyInfo QH_ForwardLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_ForwardLocation, x => AddInfo.ZH_ForwardLocationInfo); }
		}
		#endregion

		#region QH_OH_ForwardLocationOrganisation
		public override ZGuid QH_OH_ForwardLocationOrganisation
		{
			get { return base.QH_OH_ForwardLocationOrganisation; }
			set
			{
				bool hasChanges = value != QH_OH_ForwardLocationOrganisation;
				if (hasChanges)
				{
					base.QH_OH_ForwardLocationOrganisation = value;
					QH_ForwardeeEDIUserIdentifier = ZString.Empty;
				}

				var organisation = Factory.Load<OrgHeader>(value);
				if (organisation != null)
				{
					QH_ForwardeeEDIUserIdentifier = organisation.CustomsCodes.GetCustomsRegNo(UserIdentifierCusCode, Core.Constants.CountryCodes.Australia);
				}
			}
		}

		#endregion

		#region QH_TransferEDIUserLocation
		public ZString QH_TransferEDIUserLocation
		{
			get { return AddInfo.ZH_TransferEDIUserLocation; }
			set
			{
				if (AddInfo.ZH_TransferEDIUserLocation != value)
				{
					AddInfo.ZH_TransferEDIUserLocation = value;
					QH_OH_TransferEDIUserLocationOrganisation = ZGuid.Empty;
				}
			}
		}
		public ZPropertyInfo QH_TransferEDIUserLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_TransferEDIUserLocation, x => AddInfo.ZH_TransferEDIUserLocationInfo); }
		}
		#endregion

		#region QH_OH_TransferEDIUserLocationOrganisation
		public override ZGuid QH_OH_TransferEDIUserLocationOrganisation
		{
			get { return base.QH_OH_TransferEDIUserLocationOrganisation; }
			set
			{
				bool hasChanges = value != QH_OH_TransferEDIUserLocationOrganisation;
				if (hasChanges)
				{
					base.QH_OH_TransferEDIUserLocationOrganisation = value;
					QH_TransfereeEDIUserIdentifier = ZString.Empty;
				}

				var organisation = Factory.Load<OrgHeader>(value);
				if (organisation != null)
				{
					QH_TransfereeEDIUserIdentifier = organisation.CustomsCodes.GetCustomsRegNo(UserIdentifierCusCode, Core.Constants.CountryCodes.Australia);
				}
			}
		}

		#endregion

		#region QH_TransferExporterLocation
		public ZString QH_TransferExporterLocation
		{
			get { return AddInfo.ZH_TransferExporterLocation; }
			set
			{
				if (AddInfo.ZH_TransferExporterLocation != value)
				{
					AddInfo.ZH_TransferExporterLocation = value;
					QH_OH_TransferExporterLocationOrganisation = ZGuid.Empty;
				}
			}
		}
		public ZPropertyInfo QH_TransferExporterLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_TransferExporterLocation, x => AddInfo.ZH_TransferExporterLocationInfo); }
		}
		#endregion

		#region QH_DecOfCompliance

		[List(nameof(Lookups) + "." + nameof(QuarantineExDocHeaderLookups.DeclarationOfCompliance))]
		public override ZString QH_DecOfCompliance
		{
			get => base.QH_DecOfCompliance;
			set => base.QH_DecOfCompliance = value;
		}

		#endregion

		#region QH_ImportedProductFlag

		[List(nameof(Lookups) + "." + nameof(QuarantineExDocHeaderLookups.ImportedProduct))]
		public override ZString QH_ImportedProductFlag
		{
			get => base.QH_ImportedProductFlag;
			set => base.QH_ImportedProductFlag = value;
		}

		#endregion

		#region QH_ProductUseIndicator
		[List(nameof(Lookups) + "+" + nameof(QuarantineExDocHeaderLookups.ProductUseIndicatorList))]
		public ZString QH_ProductUseIndicator
		{
			get { return AddInfo.ZH_ProductUseIndicator; }
			set { AddInfo.ZH_ProductUseIndicator = value; }
		}
		public ZPropertyInfo QH_ProductUseIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_ProductUseIndicator, x => AddInfo.ZH_ProductUseIndicatorInfo); }
		}
		#endregion

		#region QH_TrueAndCompleteIndicator
		[List(nameof(Lookups) + "." + nameof(QuarantineExDocHeaderLookups.TrueAndCompleteIndicatorList))]
		public ZString QH_TrueAndCompleteIndicator
		{
			get { return AddInfo.ZH_TrueAndCompleteIndicator; }
			set { AddInfo.ZH_TrueAndCompleteIndicator = value; }
		}
		public ZPropertyInfo QH_TrueAndCompleteIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_TrueAndCompleteIndicator, x => AddInfo.ZH_TrueAndCompleteIndicatorInfo); }
		}
		#endregion

		#region QH_OH_TransferExporterLocationOrganisation
		public override ZGuid QH_OH_TransferExporterLocationOrganisation
		{
			get { return base.QH_OH_TransferExporterLocationOrganisation; }
			set
			{
				base.QH_OH_TransferExporterLocationOrganisation = value;
				var organisation = Factory.Load<OrgHeader>(value);
				if (organisation != null)
				{
					QH_TransfereeExporterNumber = organisation.CustomsCodes.GetCustomsRegNo(ExporterNumberCusCode, Core.Constants.CountryCodes.Australia);
				}
			}
		}
		#endregion

		#region QH_RequestForPermitNumber

		[BusinessObjectTestExclude]
		[ReadOnly(true)]
		public ZString QH_RequestForPermitNumber
		{
			get { return RFPNumberExisting?.CE_EntryNum ?? ZString.Empty; }
			set
			{
				CheckMaximumLength(QH_RequestForPermitNumberInfo, value);
				RFPNumberExistingOrNew.CE_EntryNum = value;
				RefreshBinding();
			}
		}

		public ZPropertyInfo QH_RequestForPermitNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_RequestForPermitNumber, x => RFPNumberExisting?.CE_EntryNumInfo); }
		}

		#endregion

		#region QH_RequestForPermitNumberStatusDescription

		[BusinessObjectTestExclude]
		public ZString QH_RequestForPermitNumberStatusDescription
		{
			get
			{
				var result = ZString.Empty;
				var status = RequestForPermitStatus;

				if (IsNEXDOCSActive)
				{
					result = new NEXDOCMessageStatus().GetDescriptionFromCode(status);
				}
				if (result.IsEmpty)
				{
					result = new EXDOCComplianceStatusCodesForCusEntryNumber().GetDescriptionFromCode(status);
				}

				return result;
			}
		}

		public ZPropertyInfo QH_RequestForPermitNumberStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.QH_RequestForPermitNumberStatusDescription); }
		}

		#endregion

		#region QH_RequestForPermitNumberPrintDescription

		[BusinessObjectTestExclude]
		public ZString QH_RequestForPermitNumberPrintDescription
		{
			get { return new EXDOCComplianceStatusCodesForRFPDocument().GetDescriptionFromCode(RequestForPermitStatus); }
		}

		public ZPropertyInfo QH_RequestForPermitNumberPrintDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.QH_RequestForPermitNumberPrintDescription); }
		}

		#endregion

		#region QH_ExportPermitNumber

		[BusinessObjectTestExclude]
		public ZString QH_ExportPermitNumber
		{
			get { return EPNNumberExisting?.CE_EntryNum ?? ZString.Empty; }
			set
			{
				CheckMaximumLength(QH_ExportPermitNumberInfo, value);
				EPNNumberExistingOrNew.CE_EntryNum = value;
			}
		}

		public ZPropertyInfo QH_ExportPermitNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_ExportPermitNumber, x => EPNNumberExisting?.CE_EntryNumInfo); }
		}

		protected bool QH_ExportPermitNumber_ReadOnly
		{
			get { return true; }
		}

		public ZString ExportPermitStatus
		{
			get => EPNNumberExisting?.CE_EntryStatus ?? ZString.Empty;
			set => EPNNumberExistingOrNew.CE_EntryStatus = value;
		}

		#endregion

		#region QH_ApprovedCertifier
		[List(nameof(Lookups) + "." + nameof(QuarantineExDocHeaderLookups.EXDOCApprovedCertifiers))]
		public ZString QH_ApprovedCertifier
		{
			get { return AddInfo.ZH_ApprovedCertifier; }
			set { AddInfo.ZH_ApprovedCertifier = value; }
		}
		public ZPropertyInfo QH_ApprovedCertifierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_ApprovedCertifier, x => AddInfo.ZH_ApprovedCertifierInfo); }
		}
		#endregion

		#region QH_AvAnimalAge
		[List(nameof(Lookups) + "." + nameof(QuarantineExDocHeaderLookups.EXDOCAverageAgeOfAnimalsList))]
		public ZString QH_AvAnimalAge
		{
			get { return AddInfo.ZH_AvAnimalAge; }
			set { AddInfo.ZH_AvAnimalAge = value; }
		}
		public ZPropertyInfo QH_AvAnimalAgeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_AvAnimalAge, x => AddInfo.ZH_AvAnimalAgeInfo); }
		}
		#endregion

		#region QH_ApprovalNumber
		public ZString QH_ApprovalNumber
		{
			get { return AddInfo.ZH_ApprovalNumber; }
			set { AddInfo.ZH_ApprovalNumber = value; }
		}
		public ZPropertyInfo QH_ApprovalNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_ApprovalNumber, x => AddInfo.ZH_ApprovalNumberInfo); }
		}
		#endregion

		#region QH_TransitLocationType
		[List(nameof(Lookups) + "." + nameof(QuarantineExDocHeaderLookups.EXDOCTransitLocationTypeList))]
		public ZString QH_TransitLocationType
		{
			get { return AddInfo.ZH_TransitLocationType; }
			set { AddInfo.ZH_TransitLocationType = value; }
		}
		public ZPropertyInfo QH_TransitLocationTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_TransitLocationType, x => AddInfo.ZH_TransitLocationTypeInfo); }
		}
		#endregion

		#region QH_AMLCQuota

		public override ZBool QH_AMLCQuota
		{
			get { return base.QH_AMLCQuota; }
			set
			{
				bool hasChanged = base.QH_AMLCQuota != value;
				base.QH_AMLCQuota = value;
				if (hasChanged && !IsValidationSuspended)
				{
					Validation.ValidateQH_AMLCQuotaYear();
				}
			}
		}

		#endregion

		#region QH_AQISRegion

		[List(nameof(Lookups) + "+" + nameof(QuarantineExDocHeaderLookups.AqisPlaces))]
		public override ZString QH_AQISRegion
		{
			get { return base.QH_AQISRegion; }
			set { base.QH_AQISRegion = value; }
		}

		#endregion

		#region QH_AuthorisingOfficerID

		[Password]
		public override ZString QH_AuthorisingOfficerID
		{
			get { return base.QH_AuthorisingOfficerID; }
			set { base.QH_AuthorisingOfficerID = value; }
		}

		#endregion

		#region QH_AuthorisationEstablishment

		[List(nameof(Lookups) + "+" + nameof(QuarantineExDocHeaderLookups.AqisPlaces))]
		public override ZString QH_AuthorisationEstablishment
		{
			get { return base.QH_AuthorisationEstablishment; }
			set { base.QH_AuthorisationEstablishment = value; }
		}

		#endregion

		#region QH_CertificateRequiredLocation

		[List(nameof(Lookups) + "+" + nameof(QuarantineExDocHeaderLookups.AqisPlaces))]
		public override ZString QH_CertificateRequiredLocation
		{
			get { return base.QH_CertificateRequiredLocation; }
			set { base.QH_CertificateRequiredLocation = value; }
		}

		#endregion

		#region QH_ForwardRequiresAcceptance

		public ZBool QH_ForwardRequiresAcceptance
		{
			get { return AddInfo.ZH_ForwardRequiresAcceptance; }
			set { AddInfo.ZH_ForwardRequiresAcceptance = value; }
		}
		public ZPropertyInfo QH_ForwardRequiresAcceptanceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QH_ForwardRequiresAcceptance, x => AddInfo.ZH_ForwardRequiresAcceptanceInfo); }
		}

		#endregion

		#region QH_QuotaType

		[MaxLength(nameof(QuotaTypeMaxLength))]
		public override ZString QH_QuotaType
		{
			get => base.QH_QuotaType;
			set => base.QH_QuotaType = value;
		}

		int QuotaTypeMaxLength => IsNEXDOCSActive ? 3 : 5;

		#endregion

		#region Loading Establishment

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.QuarantineExDocHeader|QH_LoadingDate", Caption = "Date", FullDescription = "Date in which the products were loaded.")]
		public override ZDate QH_LoadingDate
		{
			get => base.QH_LoadingDate;
			set => base.QH_LoadingDate = value;
		}

		#endregion

		public ZInt QH_QuarantineMessageMaxLine
		{
			get { return this.GetSystemDefinedValue<ZInt>(Schema.QH_QuarantineMessageMaxLine); }
			set { this.SetSystemDefinedValue(Schema.QH_QuarantineMessageMaxLine, value); }
		}

		public bool HasQuarantineHeaderBeenAccepted()
		{
			var lastOutgoingMessage = Messages?.LastOutgoingMessage;
			var lastIncomingMessage = Messages?.LastIncomingMessage;

			return QH_QuarantineMessageMaxLine != ZInt.Zero
				|| (!QH_RequestForPermitNumber.IsEmpty
				&& lastOutgoingMessage != null
				&& new ZString[] { EXDOCMessageTypeCodes.Descriptions.ORD, EXDOCMessageTypeCodes.Descriptions.LDG, EXDOCMessageTypeCodes.Descriptions.RPL }.Contains(lastOutgoingMessage.EM_MessageType)
				&& !IsWaitingForResponse
				&& lastIncomingMessage != null
				&& lastIncomingMessage.EM_MessageType == Messaging.Integration.EDIMessageStatusList.Codes.Acknowledged);
		}

		#region RFPNumber

		CusEntryNumber RFPNumberExistingOrNew => rfpNumber ?? (rfpNumber = GetEntryNumber(CusEntryNumber.EntryType.RequestForPermitStatus, true));

		CusEntryNumber RFPNumberExisting => rfpNumber ?? (rfpNumber = GetEntryNumber(CusEntryNumber.EntryType.RequestForPermitStatus, false));

		CusEntryNumber rfpNumber;

		public ZString RequestForPermitStatus
		{
			get => RFPNumberExisting?.CE_EntryStatus ?? ZString.Empty;
			set => RFPNumberExistingOrNew.CE_EntryStatus = value;
		}

		public ZString RequestForPermitEntryType => RFPNumberExisting?.CE_EntryType ?? ZString.Empty;

		CusEntryNumber GetEntryNumber(string entryType, bool createNewIfNotExist)
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
			filter.FetchOnlyFromLocalCache = !IsInDatabase;
			filter.OrderBy = CusEntryNumSchema.CE_SystemLastEditTimeUtc.Name + OrderByClause.Descending;
			var result = Factory.LoadTop1<CusEntryNumber>(filter);
			if (result == null && createNewIfNotExist)
			{
				result = CusEntryNumber.New(this, entryType, Core.Constants.CountryCodes.Australia);
			}
			return result;
		}

		#endregion

		#region CertificateRequestNumber

		public ZString CertificateStatus
		{
			get => CRINumberExisting?.CE_EntryStatus ?? ZString.Empty;
			set => CRINumberExistingOrNew.CE_EntryStatus = value;
		}

		public ZString CertificateStatusDescription
		{
			get { return new EXDOCCertificateRequestStatusCodes().GetDescriptionFromCode(CertificateStatus); }
		}

		public ZString CertificateRequestNumber
		{
			get => CRINumberExisting?.CE_EntryNum ?? ZString.Empty;
			set => CRINumberExistingOrNew.CE_EntryNum = value;
		}

		CusEntryNumber CRINumberExistingOrNew => certRequestNumber ?? (certRequestNumber = GetEntryNumber(CusEntryNumber.EntryType.CertificateRequestID, true));

		CusEntryNumber CRINumberExisting => certRequestNumber ?? (certRequestNumber = GetEntryNumber(CusEntryNumber.EntryType.CertificateRequestID, false));

		CusEntryNumber certRequestNumber;

		#endregion

		#region EPNNumber

		CusEntryNumber EPNNumberExistingOrNew => epnNumber ?? (epnNumber = GetEntryNumber(CusEntryNumber.EntryType.ExdocPermitNumber, true));

		CusEntryNumber EPNNumberExisting => epnNumber ?? (epnNumber = GetEntryNumber(CusEntryNumber.EntryType.ExdocPermitNumber, false));

		CusEntryNumber epnNumber;

		#endregion

		#region Formatted Consignee Details for RFP Document
		public ZString ConsigneeDetails
		{
			get
			{
				var builder = new StringBuilder();
				if (Declaration.Consignee != null)
				{
					builder.AppendLine(Declaration.Consignee.OH_FullNameTruncated);
					ZString combinedAddress = InvoiceHeader.JobDeclaration.Consignee.MainAddress.OA_Address1 + " " + InvoiceHeader.JobDeclaration.Consignee.MainAddress.OA_Address2;
					if (!combinedAddress.SubstringSafe(0, 70).IsEmpty)
					{
						builder.AppendLine(combinedAddress.SubstringSafe(0, 70));
					}

					var locationInformation = !Declaration.Consignee.CityFallback.IsEmpty ? InvoiceHeader.JobDeclaration.Consignee.CityFallback : (ZString)"UNKNOWN";
					if (!Declaration.Consignee.MainAddress.OA_PostCode.IsEmpty)
					{
						locationInformation += " " + Declaration.Consignee.MainAddress.OA_PostCode;
					}
					if (Declaration.Consignee.UNLOCO != null)
					{
						locationInformation += " " + Declaration.Consignee.UNLOCO.RL_RN_NKCountryCode;
					}
					if (!Declaration.Consignee.MainAddress.OA_State.IsEmpty)
					{
						locationInformation += " " + Declaration.Consignee.MainAddress.OA_State;
					}
					if (!locationInformation.IsEmpty)
					{
						builder.AppendLine(locationInformation);
					}
				}
				return builder.ToString();
			}
		}
		#endregion

		[ChildEditable]
		public RecommendationLetterCollection RecommendationLetters
		{
			get
			{
				if (fRecommendationLetters == null)
				{
					fRecommendationLetters = new RecommendationLetterCollection(this);
					fRecommendationLetters.Load();
					RegisterEditableChildObject(fRecommendationLetters);
				}
				return fRecommendationLetters;
			}
		}
		RecommendationLetterCollection fRecommendationLetters;

		public ZBool IsWaitingForResponse
		{
			get
			{
				var result = (Declaration?.JE_MessageStatus ?? ZString.Empty) == RFPMessage.Status.AwaitingResponse;
				var lastOutgoingMessage = Messages.LastOutgoingMessage;
				if (lastOutgoingMessage != null)
				{
					result &= !(lastOutgoingMessage.EM_Status == Messaging.Integration.EDIMessageStatusList.Codes.Failed);
				}
				return result;
			}
		}

		#region EdiMessageCollection

		protected EDIMessageCollection fMessages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					AddUxmlEventMessages(fMessages);
					fMessages.IsManagedForDataRefresh = true;
					fMessages.CountChanged += (sender, e) => hasExDocMessages = null;
				}
				return fMessages;
			}
		}

		public bool HasExDocMessages() => (hasExDocMessages ?? (hasExDocMessages = Messages.Cast<EDIMessage>().Any(msg => msg.EM_ApplicationCode == EDIInterchange.ApplicationCodes.EXDOC))).Value;
		bool? hasExDocMessages;

		void AddUxmlEventMessages(EDIMessageCollection messages)
		{
			// equivalent SQL for this ZQuery:

			//select * from dbo.EdiMessage
			//join dbo.GenPivot on XX_Relation2ID = EM_PK and XX_RelationType = 'XEM'
			//join dbo.StmALog on SL_PK = XX_Relation1ID and SL_Parent in (<dec.PK>, <ship.PK>) and SL_SE_NKEvent in ('MRR', 'MRJ')
			//join dbo.EdiInterchange on EI_PK = EM_EI and EI_From in ('NEXDOCS', 'NEXDOCSTest')

			var declaration = Declaration;
			if (declaration != null && IsNEXDOCSActive)
			{
				var shipment = declaration.Shipment;
				var parentIds = shipment != null ? new[] { declaration.PK, shipment.PK } : new[] { declaration.PK };

				var logSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.PK, GenPivotSchema.XX_Relation1ID);
				logSubQuery.AddToFilter(StmALogSchema.SL_Parent, parentIds);
				logSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.MessageReceivedCode, AutoEvents.MessageRejectedCode });

				var genPivotSubQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID, EDIMessageSchema.PK);
				genPivotSubQuery.AddSubQuery(logSubQuery, JoinCondition.And);
				genPivotSubQuery.AddToFilter(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.XmlEdiMessage);

				var interchangeSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK, EDIMessageSchema.EM_EI);
				interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_From, new[] { Constants.DataProvider.NEXDOCS, Constants.DataProvider.NEXDOCSTest });

				var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
				messageQuery.AddSubQuery(genPivotSubQuery, JoinCondition.And);
				messageQuery.AddSubQuery(interchangeSubQuery, JoinCondition.And);

				var rfpMessages = Factory.Load<RFPEDIMessage>(messageQuery);

				var linkedRFPMessages = messages.Where(x => rfpMessages.Any(y => y.PK == x.PK)).ToList();
				messages.RemoveRange(linkedRFPMessages);
				messages.AddRange(rfpMessages);
			}
		}

		#endregion

		#region Mark As Needing Validation

		public override ZBool QH_ObtainExportCustomsPermit
		{
			get { return base.QH_ObtainExportCustomsPermit; }
			set
			{
				bool hasChanged = base.QH_ObtainExportCustomsPermit != value;
				base.QH_ObtainExportCustomsPermit = value;
				if (hasChanged && InvoiceHeader != null)
				{
					InvoiceHeader.MarkAsNeedingValidation();
					InvoiceHeader.JobComInvoiceLines.MarkAsNeedingValidation();
					InvoiceHeader.JobComInvoiceLines.MarkQuarantineLineAsNeedingValidation();
				}
			}
		}

		// This property is used in the 'Request for Export' document
		public ZString QH_ProduceTypeDescription => Lookups.ProduceType.GetDescriptionFromCode(QH_ProduceType)?.ToUpperInvariant() ?? string.Empty;

		public override ZString QH_ProduceType
		{
			get { return base.QH_ProduceType; }
			set
			{
				ZBool oldNEXDOCSActive = IsNEXDOCSActive;
				var oldValue = QH_ProduceType;
				base.QH_ProduceType = value;
				var newNEXDOCSActive = IsNEXDOCSActive;
				var header = InvoiceHeader;
				if (oldValue != QH_ProduceType)
				{
					if (header != null)
					{
						var declaration = Declaration;
						if (declaration != null)
						{
							if (!IsCopying)
							{
								declaration.MarkAsNeedingValidation();
							}
						}
						header.MarkAsNeedingValidation();
						header.JobComInvoiceLines.MarkAsNeedingValidation();
						header.JobComInvoiceLines.MarkQuarantineLineAsNeedingValidation();
						Compartments.MarkAsNeedingValidation();
						RecommendationLetters.MarkAsNeedingValidation();
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateQH_AMLCQuota();
						Validation.ValidateQH_QuotaType();
					}
					if (!IsCopying && QH_ImportedProductFlag_ReadOnly)
					{
						QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.No;
					}
					if (newNEXDOCSActive && IsWoolOrSkinsProduceType)
					{
						ClearAuthorisationEstablishmentDetails();
					}
				}
				if (!IsCloning)
				{
					if (header != null)
					{
						foreach (JobComInvoiceLine invoiceLine in header.JobComInvoiceLines)
						{
							if (oldNEXDOCSActive != newNEXDOCSActive)
							{
								invoiceLine.ResetIsLookupsCached();
								invoiceLine.QuarantineExDocLine.ResetIsLookupsCached();
								foreach (QuarantineExDocEstablishmentAndTime process in invoiceLine.QuarantineExDocLine.Processes)
								{
									process.ResetIsLookupsCached();
								}
							}
							if (invoiceLine.QuarantineExDocLine != null)
							{
								invoiceLine.QuarantineExDocLine.RefreshBinding();
							}
						}
					}
				}
			}
		}

		void ClearAuthorisationEstablishmentDetails()
		{
			QH_AuthorisationLocation = ZString.Empty;
			QH_AuthorisationEstablishment = ZString.Empty;
			QH_OA_AuthorisationEstablishment = ZGuid.Empty;
			QH_AuthorisationDate = ZDate.Empty;
			QH_AuthorisationFlag = ZBool.False;
			QH_AuthorisationComments = ZString.Empty;
		}

		[RelatedBusinessObject(nameof(InvoiceHeader))]
		public override ZGuid QH_JZ
		{
			get { return base.QH_JZ; }
			set
			{
				var hasChanged = base.QH_JZ != value;
				base.QH_JZ = value;
				if (hasChanged && InvoiceHeader != null)
				{
					Declaration?.MarkAsNeedingValidation();
					InvoiceHeader.MarkAsNeedingValidation();
					InvoiceHeader.JobComInvoiceLines.MarkAsNeedingValidation();
					InvoiceHeader.JobComInvoiceLines.MarkQuarantineLineAsNeedingValidation();
				}
			}
		}

		public override ZDateTime QH_InspectionRequestedDate
		{
			get { return base.QH_InspectionRequestedDate; }
			set
			{
				bool hasChanged = base.QH_InspectionRequestedDate != value;
				base.QH_InspectionRequestedDate = value;
				if (hasChanged && InvoiceHeader != null)
				{
					InvoiceHeader.JobComInvoiceLines.MarkQuarantineLineAsNeedingValidation();
				}
			}
		}

		public override ZString QH_CertificatePrintIndicator
		{
			get { return base.QH_CertificatePrintIndicator; }
			set
			{
				base.QH_CertificatePrintIndicator = value;
				if (!IsCopying)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region Cloning

		public QuarantineExDocHeader Clone(ZGuid invoicePK)
		{
			var clonedExdocHeader = Factory.LoadTop1<QuarantineExDocHeader>(new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invoicePK)) ?? Factory.New<QuarantineExDocHeader>();
			using (clonedExdocHeader.SuspendSettingHasChanges())
			using (clonedExdocHeader.GetValidationSuspender())
			{
				clonedExdocHeader.IsCloning = true;
				try
				{
					clonedExdocHeader.CopyPersistentValuesFrom(this, new BusinessObjectCloneArgs(GetPropertiesToExcludeFromCloning()));

					clonedExdocHeader.AddInfo.CopyPersistentValuesFrom(AddInfo, new BusinessObjectCloneArgs(new[]
					{
						AddInfoQuarantineExDocHeaderSchema.Constants.ZH_TrueAndCompleteIndicator,
						AddInfoQuarantineExDocHeaderSchema.Constants.ZH_AuthorisationLocation,
						AddInfoQuarantineExDocHeaderSchema.Constants.ZH_StorageLocation,
						AddInfoQuarantineExDocHeaderSchema.Constants.ZH_ApprovedCertifier,
						AddInfoQuarantineExDocHeaderSchema.Constants.ZH_AvAnimalAge,
					}));
					clonedExdocHeader.QH_JZ = invoicePK;
				}
				finally
				{
					clonedExdocHeader.IsCloning = false;
				}
			}
			return clonedExdocHeader;
		}

		protected internal bool IsCloning;

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			foreach (var prop in base.GetPropertiesToExcludeFromCloning())
			{
				yield return prop;
			}

			yield return QuarantineExDocHeaderSchema.Constants.QH_JZ;
			yield return QuarantineExDocHeaderSchema.Constants.QH_AddInfo;
			yield return QuarantineExDocHeaderSchema.Constants.QH_PackDate;
			yield return QuarantineExDocHeaderSchema.Constants.QH_DecOfCompliance;
			yield return QuarantineExDocHeaderSchema.Constants.QH_ImportedProductFlag;
			yield return QuarantineExDocHeaderSchema.Constants.QH_ExporterDeclaration;
			yield return QuarantineExDocHeaderSchema.Constants.QH_ManufacturedTreatedPackagedLabelledInAustralia;
			yield return QuarantineExDocHeaderSchema.Constants.QH_LegallyImportedFlag;
			yield return QuarantineExDocHeaderSchema.Constants.QH_AuthorisationEstablishment;
			yield return QuarantineExDocHeaderSchema.Constants.QH_OA_AuthorisationEstablishment;
			yield return QuarantineExDocHeaderSchema.Constants.QH_AuthorisationDate;
			yield return QuarantineExDocHeaderSchema.Constants.QH_AuthorisationComments;
			yield return QuarantineExDocHeaderSchema.Constants.QH_StorageEstablishment;
			yield return QuarantineExDocHeaderSchema.Constants.QH_OA_StorageEstablishment;
			yield return QuarantineExDocHeaderSchema.Constants.QH_LotNumber;
			yield return QuarantineExDocHeaderSchema.Constants.QH_OriginCatchZone;
			yield return QuarantineExDocHeaderSchema.Constants.QH_StartHoldSeal;
			yield return QuarantineExDocHeaderSchema.Constants.QH_EndHoldSeal;
			yield return QuarantineExDocHeaderSchema.Constants.QH_InspectionRequestedDate;
			yield return QuarantineExDocHeaderSchema.Constants.QH_AuthorisedStartDate;
			yield return QuarantineExDocHeaderSchema.Constants.QH_AuthorisedEndDate;
			yield return QuarantineExDocHeaderSchema.Constants.QH_AuthorisingOfficerID;
			yield return QuarantineExDocHeaderSchema.Constants.QH_InspectorComments;
			yield return QuarantineExDocHeaderSchema.Constants.QH_LastAmendDateTime;
		}

		#endregion

		#region Amend Permisssion Matrix processing

		public bool IsChangeAllowed(EXDOCDataFields dataField)
		{
			return IsNEXDOCSActive || ChangePermissions.IsChangeAllowed(dataField, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(QH_ProduceType), RequestForPermitStatus);
		}

		EXDOCChangePermissions ChangePermissions
		{
			get { return Factory.GetCachedValue("EXDOCHeaderChangePermissions", () => new EXDOCChangePermissions(Factory)); }
		}

		protected bool QH_ProduceType_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.CommodityType); }
		}

		protected bool QH_InspectionRequestedDate_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.InspectionRequestedDate); }
		}

		protected bool QH_AuthorisedStartDate_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.RFPAuthorisedStartDate); }
		}

		protected bool QH_AuthorisedEndDate_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.RFPAuthorisedEndDate); }
		}

		protected bool QH_AuthorisationEstablishment_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber); }
		}

		protected bool QH_AuthorisingOfficerID_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.RFPAuthorisingOfficerIdentifier); }
		}

		protected bool QH_DecOfCompliance_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.DeclarationOfComplianceIndicator); }
		}

		protected bool QH_ImportedProductFlag_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.ImportedProductFlag); }
		}

		protected bool QH_RL_NKBorderInspectionPort_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.InspectionPort); }
		}

		protected bool QH_ShipsStores_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.ShipStoresIndicator); }
		}

		protected bool QH_StorageEstablishment_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.StorageEstablishmentNumber); }
		}

		protected bool QH_TrueAndCompleteIndicator_ReadOnly
		{
			get { return !IsChangeAllowed(EXDOCDataFields.TrueAndCompleteIndicator); }
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.AURecommendationLetter, typeof(RecommendationLetter));
			return result;
		}

		#endregion

		#region IEXDOCRefCodeTypeProvider Members

		ZString IEXDOCRefCodeTypeProvider.Type => QH_ProduceType;

		BusinessObjectFactory IEXDOCRefCodeTypeProvider.Factory => Factory;

		#endregion

		#region NEXDOCS

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZBool IsNEXDOCSActive
		{
			get
			{
				var invoice = InvoiceHeader;
				return invoice != null && invoice.IsQuarantine
					&& (!invoice.IsAttachedToPersistentDeclaration || (IsProduceTypeActive(QH_ProduceType) && !HasExDocMessages()));
			}
		}

		public static ZBool IsProduceTypeActive(ZString produceType)
		{
			switch (produceType)
			{
				case EXDOCCommodityCodes.Codes.Dairy:
				case EXDOCCommodityCodes.Codes.Eggs:
				case EXDOCCommodityCodes.Codes.Fish:
				case EXDOCCommodityCodes.Codes.SkinsAndHides:
				case EXDOCCommodityCodes.Codes.Wool:
					return true;
				case EXDOCCommodityCodes.Codes.GrainsAndPlants:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.NEXDOC_GRN, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
				case EXDOCCommodityCodes.Codes.Horticulture:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.NEXDOC_HOR, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
				case EXDOCCommodityCodes.Codes.InedibleMeat:
					return AUCustomsDataRegistry.Instance.EnableNEXDOCForInedibleMeat.Value;
				case EXDOCCommodityCodes.Codes.Meat:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
				case EXDOCCommodityCodes.Codes.OtherGoods:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
				default:
					return false;
			}
		}

		public bool IsWoolOrSkinsProduceType => QH_ProduceType == EXDOCCommodityCodes.Codes.Wool || QH_ProduceType == EXDOCCommodityCodes.Codes.SkinsAndHides;

		public ZString UserIdentifierCusCode => IsNEXDOCSActive ? OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID : OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser;

		public ZString ExporterNumberCusCode => IsNEXDOCSActive ? OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber : OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;

		public ZString ManualAmendmentReasonForMessaging { get; set; }
		public ZString ReissueCertificateNameForMessaging { get; set; }
		public ZString ReissueCertificateReasonForMessaging { get; set; }

		public CertificateNumberCollection CertificateNumbers => fCertificateNumbers ?? (fCertificateNumbers = new CertificateNumberCollection(this));
		CertificateNumberCollection fCertificateNumbers;

		#endregion

		#region INEXDOCResponse Members

		public ZString JobNumber => Declaration?.JE_DeclarationReference ?? ZString.Empty;

		public ZString RexNumber => QH_RequestForPermitNumber;

		public ZString RexStatus => QH_RequestForPermitNumberStatusDescription;

		public ZString ExportPermitNumber => QH_ExportPermitNumber;

		public ZString CustomsAuthorityNumber => Declaration?.DeclarationNumber ?? ZString.Empty;

		public ZString HtmlTemplatePath => "Enterprise.Customs.AU.Declaration.Business.Data.Xml.Universal.NEXDOC.HtmlTemplates.Response.html";

		#endregion

		#region INEXDOCMessageParent

		ZString INEXDOCMessageParent.RexNumber => QH_RequestForPermitNumber;

		#endregion

		#region Implementation

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>
			{
				[QuarantineSupportingInfoCollection.DeclarationConstant] = typeof(QuarantineSupportingInfo)
			};
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				[QuarantineExDocRexAcknowledgement.AcknowledgementCode] = typeof(QuarantineExDocRexAcknowledgement),
				[CusCodeDataTypeList.Codes.NEXDOCSCatchZone] = typeof(QuarantineCatchZone)
			};
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(QuarantineExDocHeader header) : base(header) { }

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGotoCaseOrDefault", Justification = "Baseline")]
			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);

				foreach (var column in columns)
				{
					switch (column.ColumnName)
					{
						case Schema.QH_ExportPermitNumber:
							{
								Factory.AddFetchHint(CusEntryNumSchema.CE_EntryType, (ZString)CusEntryNumber.EntryType.ExdocPermitNumber);
								goto case "EntryNumber";
							}
						case Schema.QH_RequestForPermitNumber:
							{
								Factory.AddFetchHint(CusEntryNumSchema.CE_EntryType, (ZString)CusEntryNumber.EntryType.RequestForPermitStatus);
								goto case "EntryNumber";
							}
						case "EntryNumber":
							{
								Factory.AddFetchHint(RefCountrySchema.RN_Code, (ZString)Core.Constants.CountryCodes.Australia);
								break;
							}
					}
				}
			}
		}

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)QH_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobComInvoiceHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)QH_JZInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(QuarantineExDocShipsCompartment), QuarantineExDocShipsCompartmentSchema.QC_QH);
			}
		}

		#endregion
	}
}
