using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class AdditionalTariff : NonPersistentBusinessObject
	{
		public AdditionalTariff(LegalActInfo legalAct, CusLineTariffDetail tariffDetail)
			: base(legalAct.Factory)
		{
			LegalAct = Argument.NotNull(legalAct, nameof(legalAct));
			Parent = Argument.NotNull(legalAct.Parent as IAdditionalTariffParent, nameof(Parent));

			this.tariffDetail = tariffDetail;
		}
		internal readonly LegalActInfo LegalAct;
		internal readonly IAdditionalTariffParent Parent;

		JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

		#region TariffDetail

		CusLineTariffDetail tariffDetail;

		internal CusLineTariffDetail TariffDetail
		{
			get
			{
				if (tariffDetail == null || tariffDetail.IsDeleted)
				{
					tariffDetail = Parent.CusLineTariffDetails.AddNew() as CusLineTariffDetail;
					using (tariffDetail.SuspendSettingHasChangesIncludingChildren())
					{
						tariffDetail.BZ_LegalActSubject = LegalActSubject;
					}
				}
				return tariffDetail;
			}
		}

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|ExNumber", ShortCaption = "Number", Caption = "EX (Number)")]
		[List(nameof(Lookups) + "." + nameof(AdditionalTariffLookups.ChildTariffs))]
		[BusinessObjectTestExclude]
		public ZString ExNumber
		{
			get => TariffDetail.ExNumber;
			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(ExNumberInfo, value);

				TariffDetail.ExNumber = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateExNumber();
				}
				ExNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExNumberInfo => GetZPropertyInfo(nameof(ExNumber));

		[MaxLength(CusLineTariffDetail.Schema.BZ_TariffMaxLength)]
		public ZString TariffCode
		{
			get => TariffDetail.BZ_Tariff;
			set => TariffDetail.BZ_Tariff = value;
		}

		[MaxLength(CusLineTariffDetail.Schema.BZ_TypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AdditionalTariffLookups.TariffTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|TariffType", Caption = "Type")]
		public ZString TariffType
		{
			get => TariffDetail.BZ_Type;
			set
			{
				var oldValue = TariffDetail.BZ_Type;
				TariffDetail.BZ_Type = value;
				if (!IsCopying && oldValue != TariffType)
				{
					var subject = GetLegalActSubjectByTariffType(TariffType);
					if (!subject.IsEmpty)
					{
						LegalActSubject = subject;
					}
					else
					{
						DefaultLegalActInformation();
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateTariffType();
				}
				TariffTypeInfo.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|TariffTypeDescription", Caption = "Tariff Type Description")]
		public ZString TariffTypeDescription
		{
			get
			{
				return Lookups.TariffTypeList.GetDescriptionFromCode(TariffType);
			}
		}

		public void DefaultLegalActInformation()
		{
			agreementLegalActInImportEntry = null;

			if (LegalActReadOnly)
			{
				char[] delimiterChars = { ' ', '/' };
				var results = AgreementLegalActInImportEntry.Split(delimiterChars);

				LegalActType = results.ElementAtOrDefault(0).Left(LegalActTypeInfo.MaxLength);
				LegalActIssuingBody = results.ElementAtOrDefault(1).Left(LegalActIssuingBodyInfo.MaxLength);
				LegalActNumber = results.ElementAtOrDefault(2).Left(LegalActNumberInfo.MaxLength);
				LegalActYear = results.ElementAtOrDefault(3).Left(LegalActYearInfo.MaxLength);
			}
		}

		public ZPropertyInfo TariffTypeInfo => GetZPropertyInfo(nameof(TariffType));

		public ZZRefCusCodeListCombined TariffAgreementCode => LegalActSubject == AdditionalTaxTypeList.Codes.TariffAgreement && !TariffType.IsEmpty
			? BRRefCusCodeListTypes.GetTariffAgreementCode(Factory, TariffType, InvoiceLine?.EffectiveAssessmentDate ?? ZDateTime.Today) : null;

		ZString AgreementLegalActInImportEntry => agreementLegalActInImportEntry ?? (agreementLegalActInImportEntry = TariffAgreementCode?.GetAttribute(Constants.RefCusCodeList.Attributes.LegalActInImportEntry) ?? ZString.Empty);
		string agreementLegalActInImportEntry;

		#endregion

		#region LegalAct
		[MaxLength(LegalActInfo.Schema.LegalActSubjectMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AdditionalTariffLookups.LegalActSubjectList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|LegalActSubject", Caption = "Legal Act (Subject)")]
		public ZString LegalActSubject
		{
			get => LegalAct.CSI_SubType;
			set
			{
				LegalAct.CSI_SubType = value;
				TariffDetail.BZ_LegalActSubject = value;
				if (!IsCopying)
				{
					DefaultLegalActInformation();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
				LegalActSubjectInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LegalActSubjectInfo => GetZPropertyInfo(nameof(LegalActSubject));

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(AdditionalTariffLookups.LegalActSubjectList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|LegalActSubjectDescription", Caption = "Legal Act(Subject)")]
		public ZString LegalActSubjectDescription
		{
			get
			{
				return Lookups.LegalActSubjectList.GetDescriptionFromCode(LegalActSubject);
			}
			set
			{
				LegalActSubject = Lookups.LegalActSubjectList.GetCodeFromDescription(value);
				LegalActSubjectDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LegalActSubjectDescriptionInfo => GetWrappedZPropertyInfo(nameof(LegalActSubjectDescription), x => LegalActSubjectInfo);

		[List(nameof(LegalActLookups) + "." + nameof(LegalActInfoLookups.ExTariffLegalActList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|LegalActType", Caption = "Legal Act (Type)")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActType
		{
			get => LegalAct.CSI_Code;
			set
			{
				LegalAct.CSI_Code = value;
				LegalActTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo LegalActTypeInfo => GetWrappedZPropertyInfo(nameof(LegalActType), x => LegalAct.CSI_CodeInfo);

		[List(nameof(LegalActLookups) + "." + nameof(LegalActInfoLookups.LegalActIssuingAuthorityList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|LegalActIssuingBody", Caption = "Legal Act (Issuing Body)")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActIssuingBody
		{
			get => LegalAct.CSI_IssuerType;
			set
			{
				LegalAct.CSI_IssuerType = value;
				LegalActIssuingBodyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LegalActIssuingBodyInfo => GetWrappedZPropertyInfo(nameof(LegalActIssuingBody), x => LegalAct.CSI_IssuerTypeInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|LegalActNumber", Caption = "Legal Act (Number)")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActNumber
		{
			get => LegalAct.CSI_ReferenceNumber;
			set
			{
				LegalAct.CSI_ReferenceNumber = value;
				LegalActNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LegalActNumberInfo => GetWrappedZPropertyInfo(nameof(LegalActNumber), x => LegalAct.CSI_ReferenceNumberInfo);

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.BR.Business.AdditionalTariff|LegalActYear", Caption = "Legal Act (Year)")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActYear
		{
			get => LegalAct.CSI_YearOfIssue;
			set
			{
				LegalAct.CSI_YearOfIssue = value;
				LegalActYearInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LegalActYearInfo => GetWrappedZPropertyInfo(nameof(LegalActYear), x => LegalAct.CSI_YearOfIssueInfo);

		public LegalActInfoLookups LegalActLookups => LegalAct.Lookups;

		ZBool LegalActReadOnly => Parent is JobComInvoiceLine && (LegalActSubject != AdditionalTaxTypeList.Codes.ExIPITariff
															&& (InvoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.FreeTradeAgreement
															|| (InvoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.ExTariff && !InvoiceLine.DutyRateIsOverridden)
															|| !AgreementLegalActInImportEntry.IsEmpty));

		#endregion

		#region Implementation

		public AdditionalTariffLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new AdditionalTariffLookups(this);
				}
				return fLookups;
			}
		}

		AdditionalTariffLookups fLookups;

		public AdditionalTariffValidation Validation
		{
			get { return new AdditionalTariffValidation(this); }
		}

		#endregion

		public CodeDescriptionPairList GetLegalActSubjects()
		{
			return GetLegalActSubjects(Factory, LegalActSubject == AdditionalTaxTypeList.Codes.TariffAgreement
				|| InvoiceLine != null && InvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement) == null);
		}

		public static CodeDescriptionPairList GetLegalActSubjects(IAdditionalTariffParent parent)
		{
			return GetLegalActSubjects(parent.Factory, parent is JobComInvoiceLine);
		}

		static CodeDescriptionPairList GetLegalActSubjects(BusinessObjectFactory factory, bool shouldTariffAgreementIncluded)
		{
			return factory.GetCachedValue($"BR_AdditionalTariff_LegalActSubjects_{shouldTariffAgreementIncluded}", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(AdditionalTaxTypeList.Codes.ExDutyTariff, AdditionalTaxTypeList.Descriptions.ExDutyTariff);
				list.AddPair(AdditionalTaxTypeList.Codes.ExIPITariff, AdditionalTaxTypeList.Descriptions.ExIPITariff);
				if (shouldTariffAgreementIncluded)
				{
					list.AddPair(AdditionalTaxTypeList.Codes.TariffAgreement, AdditionalTaxTypeList.Descriptions.TariffAgreement);
				}
				return list;
			});
		}

		public static ZString GetLegalActSubjectByTariffType(ZString tariffType)
		{
			switch (tariffType)
			{
				case ChildTariffTypeList.Codes.LEBIT:
				case ChildTariffTypeList.Codes.LETEC:
				case ChildTariffTypeList.Codes.COVID19:
				case ChildTariffTypeList.Codes.BIT:
				case ChildTariffTypeList.Codes.BK:
					return AdditionalTaxTypeList.Codes.ExDutyTariff;
				case ChildTariffTypeList.Codes.IPI:
					return AdditionalTaxTypeList.Codes.ExIPITariff;
				default:
					return ZString.Empty;
			}
		}

		public static CodeDescriptionPairList GetTariffTypeListByLegalActSubject(BusinessObjectFactory factory, ZDateTime date, ZString legalActSubject)
		{
			CodeDescriptionPairList tariffTypeList = null;
			switch (legalActSubject)
			{
				case AdditionalTaxTypeList.Codes.TariffAgreement:
					tariffTypeList = BRRefCusCodeListTypes.GetTariffAgreementCodeList(factory, date);
					break;
				case AdditionalTaxTypeList.Codes.ExDutyTariff:
					tariffTypeList = factory.GetCachedValue("BR_AdditionalTariff_TariffTypeList_ExDutyTariff", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(ChildTariffTypeList.Codes.LEBIT, ChildTariffTypeList.Descriptions.LEBIT);
						list.AddPair(ChildTariffTypeList.Codes.LETEC, ChildTariffTypeList.Descriptions.LETEC);
						list.AddPair(ChildTariffTypeList.Codes.COVID19, ChildTariffTypeList.Descriptions.COVID19);
						list.AddPair(ChildTariffTypeList.Codes.BIT, ChildTariffTypeList.Descriptions.BIT);
						list.AddPair(ChildTariffTypeList.Codes.BK, ChildTariffTypeList.Descriptions.BK);
						return list;
					});
					break;
				case AdditionalTaxTypeList.Codes.ExIPITariff:
					tariffTypeList = factory.GetCachedValue("BR_AdditionalTariff_TariffTypeList_ExIPITariff", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(ChildTariffTypeList.Codes.IPI, ChildTariffTypeList.Descriptions.IPI);
						return list;
					});
					break;
				default:
					tariffTypeList = factory.GetCachedValue<ChildTariffTypeList>();
					break;
			}
			return tariffTypeList;
		}

		bool IsAdditionalTariffReadOnly => (LegalActSubject == AdditionalTaxTypeList.Codes.TariffAgreement
					&& InvoiceLine != null && InvoiceLine.HasLinkedInvoiceLine && InvoiceLine.AttachedImportLicenseLine is JobComInvoiceLine lic && !lic.JI_SecondaryPreference.IsEmpty);

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return (IsAdditionalTariffReadOnly && property.Name != nameof(ExNumber)) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public override void Delete()
		{
			tariffDetail?.Delete();
			LegalAct.Delete();
			base.Delete();
		}
	}
}
