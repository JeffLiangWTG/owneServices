using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.EU;
using static Enterprise.Integration.Customs.EUExitControl;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.Business
{
	[DependentBusinessObject(typeof(CusEntryInstruction), "CusAuthorizationUsages")]
	public class CusAuthorizationUsage :
		AutoCusAuthorizationUsage,
		ICusAuthorizationUsage,
		IClusterKeyWorker,
		IOptionalClusterKeyEntity,
		ISupportMultipleResourceStringData
	{
		public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusAuthorizationUsageTypeDecider TypeDecider = new CusAuthorizationUsageTypeDecider();

		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|CustomsCode", Caption = "Customs Code")]
		[ResourceStringData("5DD22F17-A3B4-42C8-BB9B-BBA7C5897BA7", Caption = "Customs Code", FullDescription = "[12 12 002 000] Authorization < Type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public ZString CustomsCode => Factory.GetValue(ref customsCodeCached, () => GetCustomsCode());
		CachedProperty<ZString> customsCodeCached;

		ZString GetCustomsCode()
		{
			var countryCode = AuthorisationHeader is CusAuthorisationHeader header && !header.CPH_RN_NKCountryCode.IsEmpty ? header.CPH_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var countryMappedCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, countryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, AGC_Code, ZDateTime.Today);
			return countryMappedCode.IsEmpty
									? ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, AGC_Code, ZDateTime.Today)
									: countryMappedCode;
		}

		public ZPropertyInfo CustomsCodeInfo => GetZPropertyInfo(nameof(CustomsCode));

		[List(nameof(Lookups) + "." + nameof(CusAuthorizationUsageLookups.CodeList))]
		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|AGC_Code", Caption = "Code")]
		[ResourceStringData("FE64B20A-7CEB-4252-9FC1-E87B627195BA", Caption = "Type", FullDescription = "[12 12 001 000] Type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString AGC_Code
		{
			get => base.AGC_Code;
			set
			{
				var oldValue = AGC_Code;
				base.AGC_Code = value;
				if (oldValue != AGC_Code && !IsCopying)
				{
					DefaultReferenceNumber();
					DefaultAGC_OH_Owner();
					DefaultAGC_Location();

					if (AGC_Code == CusAuthorizationHeaderTypeList.Codes.CentralizedClearance)
					{
						PopulateOfficeOfPresentation();
					}
				}
			}
		}

		void PopulateOfficeOfPresentation()
		{
			if (Instruction?.JobDeclaration is not { IsUCC6AndIsExport: true } declaration
				|| !declaration.Configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration)
				|| declaration.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfPresentation))
			{
				return;
			}

			var officeOfPresentation = declaration.CustomsOffices.AddNew();
			officeOfPresentation.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		}

		[ReadOnlyMember(nameof(AGC_NumberReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusAuthorizationUsageLookups.NumberList))]
		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|AGC_Number", Caption = "Number")]
		[ResourceStringData("7CDBF610-5467-4A63-B9EC-F35C54C9EFA8", Caption = "Reference", FullDescription = "[12 12 080 000] Reference Number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("0E4FC7BB-F829-4F6B-994A-B34CAAD0BC05", Caption = "Number", FullDescription = "[12 12 001 000] Authorization < Reference number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString AGC_Number
		{
			get => base.AGC_Number;
			set
			{
				var oldValue = AGC_Number;
				base.AGC_Number = value;
				if (oldValue != AGC_Number && !IsCopying)
				{
					SetAuthorisationProperties();
				}
			}
		}

		public virtual bool AGC_NumberReadOnly => EnableAdHoc;

		public ZString AGC_NumberFieldType => EnableAdHoc || !IsAgcNumberFieldALookup ? nameof(FieldType.Text) : nameof(FieldType.TextCodeFindBox);

		[List(nameof(Lookups) + "." + nameof(CusAuthorizationUsageLookups.NumberList))]
		[MaxLength(Schema.AGC_NumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|EffectiveReferenceNumber", Caption = "Authorization Number")]
		public ZString EffectiveReferenceNumber
		{
			get => UseEffectiveReferenceNumber ? AuthorisationHeader?.CPH_Number ?? AGC_Number : AGC_Number;
			set
			{
				var oldValue = EffectiveReferenceNumber;
				CheckMaximumLength(EffectiveReferenceNumberInfo, value);
				var newValue = value;
				if (oldValue != newValue && !IsCopying)
				{
					if (UseEffectiveReferenceNumber)
					{
						if (!newValue.IsEmpty
							&& GetCusAuthorisationHeader(newValue) is ICusAuthorisationHeader authorisationHeader)
						{
							AGC_CPH_Authorization = authorisationHeader.PK;
							AGC_Code = authorisationHeader.CPH_Type;
							AGC_OH_Owner = authorisationHeader.CPH_OH_PermitHolder;
						}
						else
						{
							AGC_Number = newValue;
						}
					}
					else
					{
						AGC_Number = newValue;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateEffectiveReferenceNumber();
				}
				EffectiveReferenceNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo EffectiveReferenceNumberInfo => GetZPropertyInfo(nameof(EffectiveReferenceNumber));

		public ZBool EnableAdHoc => AuthorisationHeaderProvider.EnableAdHoc;

		public ZBool IsAgcNumberFieldALookup => AuthorisationHeaderProvider.IsAgcNumberFieldALookup;

		#region ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber

		[ReadOnlyMember(nameof(IsCPHNumberReadOnly))]
		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber", Caption = "Related Authorization")]
		public ZString ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber
		{
			get => RelatedAuthorisationHeaderIgnoringReferenceNumber == null ? ZString.Empty : RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number;
			set => RelatedAuthorisationHeaderIgnoringReferenceNumber.CPH_Number = value;
		}

		public ZBool ShowRelatedAuthorisationWithoutReference => AuthorisationHeaderProvider.ShowRelatedAuthorisationWithoutReference;

		public bool IsCPHNumberReadOnly => true;

		public bool IsCPHNumberVisible => ShowRelatedAuthorisationWithoutReference;

		#endregion

		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|AGC_OH_Owner", Caption = "Owner")]
		[ResourceStringData("29BC3D14-EE96-4F4B-BA9A-99473ADAE74D", Caption = "Owner", FullDescription = "[12 12 080 000] Authorization < Holder of the authorization", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid AGC_OH_Owner
		{
			get => base.AGC_OH_Owner;
			set
			{
				var oldValue = AGC_OH_Owner;
				base.AGC_OH_Owner = value;
				if (oldValue != AGC_OH_Owner && !IsCopying)
				{
					DefaultReferenceNumber();
					DefaultAGC_Code();
					DefaultAGC_Location();
				}
			}
		}

		public ICusAuthorisationHeader AuthorisationHeader => Factory.Load<ICusAuthorisationHeader>(AGC_CPH_Authorization);

		public Customs.Business.CusAuthorisationHeaderProvider AuthorisationHeaderProvider
		{
			get
			{
				if (authorisationHeaderProvider == null)
				{
					var countryCode = AuthorisationHeader is CusAuthorisationHeader header && !header.CPH_RN_NKCountryCode.IsEmpty ? header.CPH_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					authorisationHeaderProvider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(countryCode);
				}
				return authorisationHeaderProvider;
			}
		}
		Customs.Business.CusAuthorisationHeaderProvider authorisationHeaderProvider;

		public override ZGuid AGC_ParentID
		{
			get => base.AGC_ParentID;
			set
			{
				var oldValue = AGC_ParentID;
				base.AGC_ParentID = value;
				if (!IsCopying && oldValue != AGC_ParentID)
				{
					if (Parent is TemporaryStorageHeader temporaryStorageHeader)
					{
						AGC_ClusterKey = temporaryStorageHeader.AMA_ClusterKey;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|AGC_CPH_Authorization", Caption = "Authorization")]
		public override ZGuid AGC_CPH_Authorization
		{
			get => base.AGC_CPH_Authorization;
			set
			{
				var oldValue = AGC_CPH_Authorization;
				base.AGC_CPH_Authorization = value;
				if (oldValue != AGC_CPH_Authorization)
				{
					authorisationHeaderProvider = null;
				}
			}
		}

		public bool UseEffectiveReferenceNumber => UseEffectiveReferenceNumberCore;

		protected virtual bool UseEffectiveReferenceNumberCore => false;

		void DefaultReferenceNumber()
		{
			if (!AGC_OH_Owner.IsEmpty && !AGC_Code.IsEmpty && EffectiveReferenceNumber.IsEmpty)
			{
				if (Instruction?.JobDeclaration?.Configuration?.InstructionConfiguration?.UseEoriForAuthorisationReference ?? false)
				{
					EffectiveReferenceNumber = Owner.GetEuIdentificationNumber();
				}
				else
				{
					var numberList = Lookups.NumberList;

					if (numberList.Count == 1)
					{
						var authorisationHeader = numberList[0];
						if (!authorisationHeader.CPH_IsAdHoc)
						{
							EffectiveReferenceNumber = authorisationHeader.CPH_Number;
							SetAGC_Location(authorisationHeader);
						}
					}
				}
			}
		}

		void DefaultAGC_OH_Owner()
		{
			var referenceNumber = EffectiveReferenceNumber;
			if (!AGC_Code.IsEmpty && !referenceNumber.IsEmpty && AGC_OH_Owner.IsEmpty)
			{
				var numberList = Lookups.NumberList.Where(x => x.CPH_Number == referenceNumber);

				if (numberList.Count() == 1)
				{
					var authorisationHeader = numberList.First();
					AGC_OH_Owner = authorisationHeader.CPH_OH_PermitHolder;
					SetAGC_Location(authorisationHeader);
				}
			}
		}

		void DefaultAGC_Code()
		{
			var referenceNumber = EffectiveReferenceNumber;
			if (!AGC_OH_Owner.IsEmpty && !referenceNumber.IsEmpty && AGC_Code.IsEmpty)
			{
				var numberList = Lookups.NumberList.Where(x => x.CPH_Number == referenceNumber);

				if (numberList.Count() == 1)
				{
					var authorisationHeader = numberList.First();
					AGC_Code = authorisationHeader.CPH_Type.Left(4);
					SetAGC_Location(authorisationHeader);
				}
			}
		}

		void SetAuthorisationProperties()
		{
			if (!AGC_Number.IsEmpty)
			{
				var numberList = Lookups.NumberList.Where(x => x.CPH_Number == AGC_Number);

				if (numberList.Count() == 1)
				{
					var authorisation = numberList.First();
					AGC_Code = authorisation.CPH_Type.Left(4);
					AGC_OH_Owner = authorisation.CPH_OH_PermitHolder;
					SetAGC_Location(authorisation);
				}
			}
		}

		protected virtual void DefaultAGC_Location() { }

		protected virtual void SetAGC_Location(CusAuthorisationHeader authorisationHeader) { }

		public CusEntryInstruction Instruction => Parent as CusEntryInstruction;

		public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

		protected override IValueSetStrategy GetValueSetStrategy() => cusAuthorizationUsageValueSetStrategy ??= new CusAuthorizationUsageValueSetStrategy(this);
		IValueSetStrategy cusAuthorizationUsageValueSetStrategy;

		public BusinessObject Parent
		{
			get => ParentLoaders.LoadBusinessObject(Factory, AGC_ParentTableCode, AGC_ParentID);
			set => ParentLoaders.SetTablePrefixAndPK(value, AGC_ParentTableCodeInfo, AGC_ParentIDInfo);
		}

		protected TypeLoaderCollection ParentLoaders => parentLoaders ??= GetParentLoaders();
		TypeLoaderCollection parentLoaders;

		protected TypeLoaderCollection GetParentLoaders() => new (
			typeof(CusEntryInstruction),
			typeof(JobComInvoiceLine),
			typeof(CusInBondHeader),
			typeof(TemporaryStorageHeader),
			typeof(CusInBondMoveHeader),
			ObjectFactory.GetType(typeof(ICusExitReport)),
			ObjectFactory.GetType(typeof(ICusExitReportItem)),
			ObjectFactory.GetType(typeof(ICusTempStorageRegPremises))
		);

		protected override ZString HumanReadableNameCore => Res.GetString("654751F8-9889-4C89-842F-8DC6EE0BDF1A", "Customs Authorization {0}", EffectiveReferenceNumber);

		protected override bool SupportsCloneCore() => true;

		public virtual IReadOnlyList<string> MultipleKeysToUse => Instruction?.MultipleKeysToUse ?? Array.Empty<string>();

		public CusAuthorisationHeader RelatedAuthorisationHeader => GetCusAuthorisationHeader();

		public CusAuthorisationHeader RelatedAuthorisationHeaderIgnoringReferenceNumber => GetCusAuthorisationHeader(ignoreReferenceNumber: true);

		CusAuthorisationHeader GetCusAuthorisationHeader(bool ignoreReferenceNumber = false)
		{
			if (AuthorisationHeader is CusAuthorisationHeader header)
			{
				return header;
			}

			return GetCusAuthorisationHeader(EffectiveReferenceNumber, ignoreReferenceNumber);
		}

		CusAuthorisationHeader GetCusAuthorisationHeader(ZString referenceNumber, bool ignoreReferenceNumber = false)
		{
			var authorisationHeaderQuery = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, true);
			if (!AGC_Code.IsEmpty)
			{
				authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_Type, AGC_Code);
			}
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);

			var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			authorisationHeaderQuery.AddToFilter(endDateQuery);

			if (!AGC_OH_Owner.IsEmpty)
			{
				authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, AGC_OH_Owner);
			}

			if (!ignoreReferenceNumber)
			{
				authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_Number, referenceNumber);
			}

			return Factory.LoadTop1<CusAuthorisationHeader>(authorisationHeaderQuery);
		}

		public CusAuthorisationHeader CreateTemporaryAuthorisationFromUsage()
		{
			var factory = new BusinessObjectFactory();
			var authorisation = factory.New<CusAuthorisationHeader>();
			authorisation.CPH_IsAdHoc = true;
			var defaultTemporaryAuthorizationNumber = AuthorisationHeaderProvider.DefaultTemporaryAuthorizationNumber;
			authorisation.CPH_Number = defaultTemporaryAuthorizationNumber.IsEmpty ? EffectiveReferenceNumber : defaultTemporaryAuthorizationNumber;
			authorisation.CPH_OH_PermitHolder = AGC_OH_Owner;
			authorisation.CPH_Type = AGC_Code;
			return authorisation;
		}

		public void CopyFromTemporaryAuthorization(CusAuthorisationHeader tempAuthorisation)
		{
			AGC_OH_Owner = tempAuthorisation.CPH_OH_PermitHolder;
			AGC_Code = tempAuthorisation.CPH_Type.Left(4);
			AGC_CPH_Authorization = tempAuthorisation.PK;
		}

		public static CusAuthorizationUsage Load(BusinessObject parent)
		{
			var filter = new ZQuery(CusAuthorizationUsageSchema.AGC_ParentID, SQLComparisonOperator.Equal, parent.PK);
			filter.AddToFilter(CusAuthorizationUsageSchema.AGC_ParentTableCode, parent.TablePrefix);

			filter.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			filter.OrderBy = CusAuthorizationUsageSchema.Constants.AGC_SystemCreateTimeUtc;
			var result = parent.Factory.LoadTop1<CusAuthorizationUsage>(filter);

			return result;
		}

		public static CusAuthorizationUsage New(BusinessObject parent)
		{
			var result = parent.Factory.New<CusAuthorizationUsage>();
			using (result.SuspendSettingHasChanges())
			{
				result.Parent = parent;
			}
			return result;
		}

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)AGC_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType
		{
			get
			{
				return (string)AGC_ParentTableCode switch
				{
					CusEntryInstructionSchema.Constants.Prefix => typeof(Customs.Business.CusEntryInstruction),
					JobComInvoiceLineSchema.Constants.Prefix => typeof(JobComInvoiceLine),
					AsycudaManifestHeaderSchema.Constants.Prefix => typeof(TemporaryStorageHeader),
					CusExitReportSchema.Constants.Prefix => ObjectFactory.GetType(typeof(ICusExitReport)),
					CusExitReportItemSchema.Constants.Prefix => ObjectFactory.GetType(typeof(ICusExitReportItem)),
					_ => null,
				};
			}
		}

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)AGC_ParentIDInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		bool IOptionalClusterKeyEntity.UseClusterKey => AGC_ParentTableCode != CusInBondHeaderSchema.Constants.Prefix
													&&  AGC_ParentTableCode != CusInBondMoveHeaderSchema.Constants.Prefix
													&&  AGC_ParentTableCode != CusTempStorageRegPremisesSchema.Constants.Prefix;

		#endregion;

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			EffectiveReferenceNumber = "123";
		}

#endif
		#endregion
	}
}
