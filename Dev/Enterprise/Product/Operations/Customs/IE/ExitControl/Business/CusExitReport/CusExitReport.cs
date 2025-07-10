using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusGoodsLocationQualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;
using CusGoodsLocationUseList = Enterprise.Customs.Business.CusGoodsLocationUseList;
using EuEoriProviderAndValidator = Enterprise.Customs.EU.Business.EuEoriProviderAndValidator;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitReport : EU.ExitControl.Business.CusExitReport
		, Integration.Customs.IEExitControl.ICusExitReport
		, IMessageAttachee,
		ICusGoodsLocationTypeSupporter,
		ISupportingDocObject,
		IDocManagerSupportProvider
	{
		public CusExitReport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.ExitControl.Business.CusExitReport.Schema
		{
			public const string CER_Calc_FormattedDateTime = nameof(CusExitReport.CER_Calc_FormattedDateTime);
			public const string CER_Calc_UNLOCO = nameof(CusExitReport.CER_Calc_UNLOCO);
			public const string CER_Calc_TypeOfLocation = nameof(CusExitReport.CER_Calc_TypeOfLocation);
			public const string DeclarantOrgPK = nameof(CusExitReport.DeclarantOrgPK);
			public const string DeclarantAddressPK = nameof(CusExitReport.DeclarantAddressPK);
			public const string RepresentativeOrgPK = nameof(CusExitReport.RepresentativeOrgPK);
			public const string RepresentativeAddressPK = nameof(CusExitReport.RepresentativeAddressPK);
		}

		public new CusExitHeader Header => (CusExitHeader)base.Header;

		public new EU.ExitControl.Business.IAlternativeEvidenceCollection<AlternativeEvidence> AlternativeEvidences => (EU.ExitControl.Business.IAlternativeEvidenceCollection<AlternativeEvidence>)base.AlternativeEvidences;
		protected override EU.ExitControl.Business.IAlternativeEvidenceCollection<EU.ExitControl.Business.AlternativeEvidence> CreateNewAlternativeEvidenceCollection() => new EU.ExitControl.Business.AlternativeEvidenceCollection<AlternativeEvidence>(this);

		public new CusExitConsignment Consignment => (CusExitConsignment)base.Consignment;

		public new ExitControlBase.Business.ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems => (ExitControlBase.Business.ICusExitReportItemCollection<CusExitReportItem>)base.CusExitReportItems;

		protected override ExitControlBase.Business.ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection(ZQuery filter) => new ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>(this, filter);

		protected override ExitControlBase.Business.ICusExitReportItemCollection<EU.ExitControl.Business.CusExitReportItem> CreateNewCusExitReportItemPackageCollection() => new ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>(this, new ZQuery(CusExitReportItemSchema.ERI_CXP_Package, SQLComparisonOperator.NotEqual, ZGuid.Empty));

		protected override EU.ExitControl.Business.IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new EU.ExitControl.Business.AdditionalInfoCollection<AdditionalInfo>(this);

		public new CusExitReportValidation Validation => (CusExitReportValidation)base.Validation;
		protected override ExitControlBase.Business.CusExitReportValidation GetNewValidation()
		{
			switch (CER_Type.ToUpperInvariant())
			{
				case ExitReportTypeList.Codes.Presentation:
					return new PresentationCusExitReportValidation(this);
				case ExitReportTypeList.Codes.ExitNotification:
					return new ExitNotificationCusExitReportValidation(this);
				case ExitReportTypeList.Codes.InformationOnNonExitedExport:
					return new InformationOnNonExitedReportCusExitReportValidation(this);
				default:
					return new CusExitReportValidation(this);
			}
		}

		public new CusExitReportLookups Lookups => (CusExitReportLookups)base.Lookups;
		protected override ExitControlBase.Business.CusExitReportLookups GetNewLookups() => new CusExitReportLookups(this);

		public bool IsInformationOnNonExitedExport => CER_Type.EqualsIgnoringCase(ExitReportTypeList.Codes.InformationOnNonExitedExport);
		public bool IsPresentation => CER_Type.EqualsIgnoringCase(ExitReportTypeList.Codes.Presentation);
		public bool IsExitNotification => CER_Type.EqualsIgnoringCase(ExitReportTypeList.Codes.ExitNotification);
		public bool IsDiscrepancies => CER_Behavior.EqualsIgnoringCase(ExitReportDiscrepancyTypeList.Codes.Discrepancies);

		[ReadOnlyMember(nameof(CER_AdditionalDeclarationType_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.AdditionalDeclarationTypes))]
		[ResourceStringData("{9B784F82-6137-4FEE-B4B6-774D314717E4}", Caption = "Additional Declaration Type", MediumCaption = "Add. Dec. Type")]
		public override ZString CER_AdditionalDeclarationType
		{
			get => string.IsNullOrEmpty(base.CER_AdditionalDeclarationType) ? CER_AdditionalDeclarationType_DefaultValue : base.CER_AdditionalDeclarationType;
			set => base.CER_AdditionalDeclarationType = value;
		}
		protected bool CER_AdditionalDeclarationType_ReadOnly => !IsExitNotification;
		protected ZString CER_AdditionalDeclarationType_DefaultValue
		{
			get
			{
				if (IsExitNotification)
				{
					return EU.Business.EntrySubStyleList.Codes.NormalDeclaration;
				}
				return ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.DiscrepancyTypeList))]
		public override ZString CER_Behavior
		{
			get => base.CER_Behavior;
			set => base.CER_Behavior = value;
		}

		public new CusGoodsLocation GoodsLocation
		{
			get
			{
				if (cusGoodsLocation is null)
				{
					cusGoodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Arrival);
					RegisterEditableChildObject(cusGoodsLocation);
				}
				return cusGoodsLocation;
			}
		}
		CusGoodsLocation cusGoodsLocation;

		internal CusGoodsLocation GetGoodsLocation() => cusGoodsLocation ?? (Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Arrival));

		public bool IsTransportFieldsRequired => IsPresentation;

		[ReadOnlyMember(nameof(CER_TransportMode_ReadOnly))]
		public override ZString CER_TransportMode { get => base.CER_TransportMode; set => base.CER_TransportMode = value; }
		protected bool CER_TransportMode_ReadOnly => !IsTransportFieldsRequired;

		[ReadOnlyMember(nameof(CER_TransportID_ReadOnly))]
		public override ZString CER_TransportID { get => base.CER_TransportID; set => base.CER_TransportID = value; }
		protected bool CER_TransportID_ReadOnly => !IsTransportFieldsRequired;

		[ReadOnlyMember(nameof(CER_TransportType_ReadOnly))]
		public override ZString CER_TransportType { get => base.CER_TransportType; set => base.CER_TransportType = value; }
		protected bool CER_TransportType_ReadOnly => !IsTransportFieldsRequired;

		[ReadOnlyMember(nameof(CER_RN_NKTransportNationality_ReadOnly))]
		public override ZString CER_RN_NKTransportNationality { get => base.CER_RN_NKTransportNationality; set => base.CER_RN_NKTransportNationality = value; }
		protected bool CER_RN_NKTransportNationality_ReadOnly => !IsTransportFieldsRequired;

		[ResourceStringData("{F8764945-154E-44C6-B02C-41DB8307A27B}", Caption = "Representation Status", MediumCaption = "Rep. Status")]
		public override ZString CER_DeclarantType { get => base.CER_DeclarantType; set => base.CER_DeclarantType = value; }
		protected override bool CER_DeclarantType_ReadOnly => !IsOrganisationFieldsRequired;

		[ResourceStringData("{9317C7C5-26DA-40AD-9CC1-A0105BC1546C}", Caption = "Exit/Arrival Date")]
		public override ZDateTimeOffset CER_DateTime { get => base.CER_DateTime; set => base.CER_DateTime = value; }
		protected bool CER_DateTime_Readonly()
		{
			var result = false;
			if (CER_Type == ExitReportTypeList.Codes.InformationOnNonExitedExport)
			{
				switch (CER_EnquiryInformationCode.ToUpperInvariant())
				{
					case IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExpectedToExit:
					case IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExitedNoAlternativeEvidence:
					case IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExitedAlternativeEvidence:
						break;
					default:
						result = true;
						break;
				}
			}
			return result;
		}

		[MaxLength(15)]
		[BusinessObjectTestExclude]
		[ResourceStringData("{A436B144-707E-47AA-8A90-00C88ECB15D7}", Caption = "Exit/Arrival Date")]
		public ZString CER_Calc_FormattedDateTime
		{
			get => Factory.GetValue(ref cer_Calc_FormattedDateTimeCached, () =>
			{
				var dateTime = CER_DateTime;
				var result = ZString.Empty;
				if (dateTime.IsValid)
				{
					result = IsPresentation ? dateTime.ToZDateTime().ToLongTimeString() : dateTime.ToZDateTime().ToShortDateString();
				}
				return result;
			});
			set
			{
				var oldValue = CER_Calc_FormattedDateTime;
				var format = IsPresentation ? ZDateTime.LongTimeFormat : ZDateTime.ShortDateFormat;
				CER_DateTime = ZDateTime.TryParseExact(value, out var result, format) ? new ZDateTimeOffset(result) : ZDateTimeOffset.Empty;
				if (!IsValidationSuspended && oldValue != CER_Calc_FormattedDateTime)
				{
					Validation.ValidateCER_Calc_FormattedDateTime();
				}
				CER_Calc_FormattedDateTimeInfo.RefreshBinding(oldValue);
			}
		}
		CachedProperty<ZString> cer_Calc_FormattedDateTimeCached;

		public ZPropertyInfo CER_Calc_FormattedDateTimeInfo => GetZPropertyInfo(Schema.CER_Calc_FormattedDateTime);

		public ZString CER_Calc_FormattedDateTime_FieldType => IsPresentation ? nameof(ZArchitecture.FieldType.DateTime) : nameof(ZArchitecture.FieldType.Date);

		[ResourceStringData("{A6F4BE3C-A95F-4C55-994B-E7B20A991232}", Caption = "Type of Location")]
		[List(nameof(CusExitReport.ArrivalGoodsLocation) + "." + nameof(CusGoodsLocation.Lookups) + "." + nameof(CusGoodsLocationLookups.TypeOfLocationList))]
		public ZString CER_Calc_TypeOfLocation { get => ArrivalGoodsLocation.CGL_Type; set => ArrivalGoodsLocation.CGL_Type = value; }
		public ZPropertyInfo CER_Calc_TypeOfLocationInfo => GetWrappedZPropertyInfo(Schema.CER_Calc_TypeOfLocation, x => ArrivalGoodsLocation.CGL_TypeInfo);

		[ResourceStringData("{72A437D4-0C93-4547-A11F-861943D91AF1}", Caption = "UNLOCO")]
		[List(nameof(CusExitReport.ArrivalGoodsLocation) + "." + nameof(CusGoodsLocation.Lookups) + "." + nameof(CusGoodsLocationLookups.UNLOCOs))]
		public ZString CER_Calc_UNLOCO { get => ArrivalGoodsLocation.CGL_AdditionalIdentifier; set => ArrivalGoodsLocation.CGL_AdditionalIdentifier = value; }
		public ZPropertyInfo CER_Calc_UNLOCOInfo => GetWrappedZPropertyInfo(Schema.CER_Calc_UNLOCO, x => ArrivalGoodsLocation.CGL_AdditionalIdentifierInfo);

		public CusGoodsLocation ArrivalGoodsLocation
		{
			get
			{
				if (arrivalGoodsLocation == null || arrivalGoodsLocation.IsDeleted)
				{
					arrivalGoodsLocation = CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Arrival);
					if (!arrivalGoodsLocation.CGL_Qualifier.EqualsIgnoringCase(CusGoodsLocationQualifierList.Codes.UnLocode))
					{
						arrivalGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
					}
					RegisterEditableChildObject(arrivalGoodsLocation);
				}
				return arrivalGoodsLocation;
			}
		}
		CusGoodsLocation arrivalGoodsLocation;

		public bool IsOrganisationFieldsRequired => IsInformationOnNonExitedExport;

		[ResourceStringData("A66AF4C9-75C2-44E7-ADAB-3A6ED30C15EB", MediumCaption = "Arr. Notif. Place", Caption = "Arrival Notification Place")]
		public override ZString CER_Location { get => base.CER_Location; set => base.CER_Location = value; }
		protected override bool CER_Location_ReadOnly => !IsPresentation;

		[ReadOnlyMember(nameof(CER_OfficeOfExport_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.OfficeOfExportList))]
		[ResourceStringData("{C297B8D4-57B4-4839-B9F8-C2DDDEA32E5E}", Caption = "Office of Export")]
		public override ZString CER_OfficeOfExport
		{
			get => base.CER_OfficeOfExport;
			set => base.CER_OfficeOfExport = value;
		}
		protected bool CER_OfficeOfExport_ReadOnly => !IsInformationOnNonExitedExport;

		[ResourceStringData("{A5C54230-FF47-4833-9F69-AB556BB22A97}", Caption = "Declarant")]
		[ReadOnlyMember(nameof(DeclarantOrgPK_ReadOnly))]
		public ZGuid DeclarantOrgPK
		{
			get => Declarant.OrganisationPK;
			set => Declarant.OrganisationPK = value;
		}

		protected bool DeclarantOrgPK_ReadOnly => !IsOrganisationFieldsRequired;
		public ZPropertyInfo DeclarantOrgPKInfo => GetWrappedZPropertyInfo(Schema.DeclarantOrgPK, x => Declarant.OrganisationPKInfo);

		[ResourceStringData("{D4ED4E92-8031-4F38-B583-1EFB058C9CFE}", Caption = "Declarant Address")]
		[ReadOnlyMember(nameof(DeclarantAddressPK_ReadOnly))]
		public ZGuid DeclarantAddressPK { get => Declarant.E2_OA_Address; set => Declarant.E2_OA_Address = value; }
		protected bool DeclarantAddressPK_ReadOnly => !IsOrganisationFieldsRequired;
		public ZPropertyInfo DeclarantAddressPKInfo => GetWrappedZPropertyInfo(Schema.DeclarantAddressPK, x => Declarant.E2_OA_AddressInfo);

		[ResourceStringData("{AF136EC3-2DE1-4E8B-855E-48BFE83FC66B}", Caption = "Representative")]
		[ReadOnlyMember(nameof(RepresentativeOrgPK_ReadOnly))]
		public ZGuid RepresentativeOrgPK
		{
			get => Representative.OrganisationPK;
			set => Representative.OrganisationPK = value;
		}
		protected bool RepresentativeOrgPK_ReadOnly => !IsOrganisationFieldsRequired;
		public ZPropertyInfo RepresentativeOrgPKInfo => GetWrappedZPropertyInfo(Schema.RepresentativeOrgPK, x => Representative.OrganisationPKInfo);

		[ResourceStringData("{BA0ED4AD-46B5-4C9E-A6C8-32942F889DF8}", Caption = "Representative Address")]
		[ReadOnlyMember(nameof(RepresentativeAddressPK_ReadOnly))]
		public ZGuid RepresentativeAddressPK { get => Representative.E2_OA_Address; set => Representative.E2_OA_Address = value; }
		protected bool RepresentativeAddressPK_ReadOnly => !IsOrganisationFieldsRequired;
		public ZPropertyInfo RepresentativeAddressPKInfo => GetWrappedZPropertyInfo(Schema.RepresentativeAddressPK, x => Representative.E2_OA_AddressInfo);

		[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.EnquiryInformationCodeTypes))]
		public override ZString CER_EnquiryInformationCode
		{
			get => base.CER_EnquiryInformationCode;
			set
			{
				var oldValue = CER_EnquiryInformationCode;
				base.CER_EnquiryInformationCode = value;
				if (!IsCopying && oldValue != CER_EnquiryInformationCode && IsInformationOnNonExitedExport)
				{
					ClearCER_OfficeOfExitIfNeeded();
					ClearCER_DateTimeIfNeeded();
				}
			}
		}
		protected override bool CER_EnquiryInformationCode_ReadOnly => !IsInformationOnNonExitedExport;

		protected override ZString StatusDescriptionCore
		{
			get
			{
				var result = ZString.Empty;
				var status = CER_Status;
				if (!status.IsEmpty)
				{
					result = Lookups.StatusList.GetDescriptionFromCode(status) ?? UnknownDescription;
				}
				return result;
			}
		}

		public override void Delete()
		{
			this.DeleteChildren<CusGoodsLocation>(CusGoodsLocationSchema.CGL_ParentID);
			base.Delete();
		}

		void ClearCER_DateTimeIfNeeded()
		{
			if (!CER_DateTime.IsEmpty && !IsExitDateRequired)
			{
				CER_DateTime = ZDateTimeOffset.Empty;
			}
		}

		internal bool IsExitDateRequired
		{
			get
			{
				var result = false;
				switch (CER_EnquiryInformationCode.ToUpperInvariant())
				{
					case IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExpectedToExit:
					case IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExitedNoAlternativeEvidence:
					case IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExitedAlternativeEvidence:
						result = true;
						break;
				}
				return result;
			}
		}

		void ClearCER_OfficeOfExitIfNeeded()
		{
			if (!CER_OfficeOfExit.IsEmpty && (CER_EnquiryInformationCode == IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.WillNotExit || CER_EnquiryInformationCode == IE.Business.UniversalReferenceConstants.ExitReportEnquiryInformationCodeTypes.Codes.ExpectedToExit))
			{
				CER_OfficeOfExit = ZString.Empty;
			}
		}

		void ClearCER_OfficeOfExportIfNeeded()
		{
			if (!CER_OfficeOfExport.IsEmpty)
			{
				CER_OfficeOfExport = ZString.Empty;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusExitReportFetchStrategy(this);

		protected override void OnCER_TypeChanged()
		{
			base.OnCER_TypeChanged();
			var isPresentation = false;
			var isInformationOnNonExitedExport = false;
			switch (CER_Type.ToUpper())
			{
				case ExitReportTypeList.Codes.Presentation:
					isPresentation = true;
					break;
				case ExitReportTypeList.Codes.InformationOnNonExitedExport:
					isInformationOnNonExitedExport = true;
					break;
			}
			if (!isPresentation)
			{
				if (ArrivalGoodsLocation is CusGoodsLocation goodsLocation)
				{
					if (!goodsLocation.CGL_AdditionalIdentifier.IsEmpty)
					{
						goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
					}
					if (!goodsLocation.CGL_Type.IsEmpty)
					{
						goodsLocation.CGL_Type = ZString.Empty;
					}
				}
				if (!CER_Location.IsEmpty)
				{
					CER_Location = ZString.Empty;
				}
				ClearTransportData();
			}

			if (!isInformationOnNonExitedExport)
			{
				ClearCER_OfficeOfExportIfNeeded();
				ClearAddressIfNeeded(Declarant);
				ClearAddressIfNeeded(Representative);
				if (!CER_DeclarantType.IsEmpty)
				{
					CER_DeclarantType = ZString.Empty;
				}
			}
			ArrivalGoodsLocation?.MarkAsNeedingValidation();
		}

		void ClearAddressIfNeeded(JobDocAddress docAddress)
		{
			if (!docAddress.E2_OA_Address.IsEmpty)
			{
				docAddress.OrganisationPK = ZGuid.Empty;
				docAddress.E2_OA_Address = ZGuid.Empty;
			}
		}

		void ClearTransportData()
		{
			if (!CER_TransportID.IsEmpty)
			{
				CER_TransportID = ZString.Empty;
			}
			if (!CER_TransportMode.IsEmpty)
			{
				CER_TransportMode = ZString.Empty;
			}
			if (!CER_TransportType.IsEmpty)
			{
				CER_TransportType = ZString.Empty;
			}
			if (!CER_RN_NKTransportNationality.IsEmpty)
			{
				CER_RN_NKTransportNationality = ZString.Empty;
			}
		}

		public (CusExitContainer container, CusExitConsignmentItem[] consignmentItems)[] GetContainersOrEquipments() => Factory.GetValue(ref containersOrEquipmentsCached, () =>
		{
			var containerDictionary = new Dictionary<CusExitContainer, HashSet<CusExitConsignmentItem>>();
			var consignmentItemDictionary = new Dictionary<CusExitConsignmentItem, Dictionary<CusExitConsignmentPackage, CusExitContainer>>();

			foreach (var reportItem in CusExitReportItems)
			{
				if (reportItem.ConsignmentItem is CusExitConsignmentItem consignmentItem && reportItem.Package is CusExitConsignmentPackage package)
				{
					if (!consignmentItemDictionary.TryGetValue(consignmentItem, out var packageMapping))
					{
						packageMapping = consignmentItem.CusExitConsignmentPivots.Cast<CusExitConsignmentPivot>().Select(x => (x.Container, x.Package)).Where(x => x.Package != null && x.Container != null).GroupBy(x => x.Package).ToDictionary(x => x.Key, y => y.First().Container);
						consignmentItemDictionary.Add(consignmentItem, packageMapping);
					}
					if (packageMapping.TryGetValue(package, out var container))
					{
						containerDictionary.GetOrAdd(container, () => new HashSet<CusExitConsignmentItem>()).Add(consignmentItem);
					}
				}
			}
			return containerDictionary.Select(x => (x.Key, x.Value.OrderBy(y => y.CCI_LineNumber).ThenBy(t => t.CCI_SystemCreateTimeUtc).ToArray())).OrderBy(x => x.Key.CXN_SystemCreateTimeUtc).ToArray();
		});
		CachedProperty<(CusExitContainer container, CusExitConsignmentItem[] consignmentItems)[]> containersOrEquipmentsCached;

		protected override bool IsAlternativeEvidenceRequiredCore => true;

		protected override void ValidateDeclarant(JobDocAddressValidation validation)
		{
			if (IsInformationOnNonExitedExport)
			{
				var declarant = validation.Parent;
				if (declarant.OrganisationPK.IsEmpty)
				{
					declarant.OrganisationPKInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("2A9A315B-0832-4115-8CB5-8C7A9192A669", "Declarant")));
				}
				else if (EuEoriProviderAndValidator.GetEuIdentificationNumber(declarant.Organisation, Core.Constants.CountryCodes.Ireland, true).IsEmpty)
				{
					declarant.OrganisationPKInfo.AddMessageError(Res.GetString("2533DCD0-1D89-4DE9-8D25-2B6C47BA7025", "An EORI number is required for Declarant"));
				}
			}
		}

		protected override void ValidateRepresentative(JobDocAddressValidation validation)
		{
			if (IsInformationOnNonExitedExport)
			{
				var representative = validation.Parent;
				if (!representative.OrganisationPK.IsEmpty)
				{
					var representativeEori = EuEoriProviderAndValidator.GetEuIdentificationNumber(representative.Organisation, Core.Constants.CountryCodes.Ireland, true);
					if (representativeEori.IsEmpty)
					{
						representative.OrganisationPKInfo.AddMessageError(Res.GetString("03C6E269-5AD9-4B9C-B4C8-B3E07EAF785A", "An EORI number is required for Representative"));
					}
					else
					{
						var declarantEori = EuEoriProviderAndValidator.GetEuIdentificationNumber(Declarant?.Organisation, Core.Constants.CountryCodes.Ireland, true);
						if (!declarantEori.IsEmpty && declarantEori == representativeEori && CER_DeclarantType != RepresentationTypeList.Codes._2Direct)
						{
							representative.OrganisationPKInfo.AddMessageError(Res.GetString("2BD82309-0CFB-4670-8CB7-D2E9B73C779B", "The EORI of the Representative must be different to the Declarant"));
						}
					}
				}
			}
		}

		protected override Dictionary<ZString, Type> GetCusCodeDataTypes()
		{
			var result = base.GetCusCodeDataTypes();
			result[EU.Business.CusCodeDataTypeList.Codes.AlternativeEvidence] = typeof(AlternativeEvidence);
			return result;
		}

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch => Header?.Branch;
		GlbStaff IMessageAttachee.CustomsAgent => Header?.CustomsAgent;
		IRelatedJob IMessageAttachee.RelatedJob => Header;
		ZString IMessageAttachee.LogicalStatus { get => CER_MessageStatus; set => CER_MessageStatus = value; }
		ZString IMessageAttachee.EntryStatus { get => CER_Status; set => CER_Status = value; }
		ZString IMessageAttachee.MovementReferenceNumber => Consignment.CXC_MovementReference;
		IEnumerable<Enterprise.Messaging.Business.EDIMessage> IMessageAttachee.Messages => Messages.Cast<Enterprise.Messaging.Business.EDIMessage>();

		#endregion

		#region ICusGoodsLocationTypeSupporter

		Type ICusGoodsLocationTypeSupporter.GoodsLocationType => typeof(CusGoodsLocation);

		#endregion

		#region ICusSupportingInfoTypeSupporter

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		#endregion

		public SupportingDocSendingObject GetSupportingDocSendingObject()
		{
			return new DocumentSendingObject(this);
		}

		public IEnumerable<IDocManagerSupport> DocManagerSupports
		{
			get
			{
				yield return Header;
				if (Header?.Parent is IDocManagerSupport parent)
				{
					yield return parent;
				}
			}
		}
	}
}
