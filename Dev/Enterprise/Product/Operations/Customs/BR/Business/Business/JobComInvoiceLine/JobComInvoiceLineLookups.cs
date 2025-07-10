using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public CodeDescriptionPairList CargoPriorityList => Factory.GetCachedValue<CargoPriorityList>();

		public override ICodeDescriptionPairList Procedures => BRRefCusProcedure.GetRefCusProcedureList(Factory, Parent.MessageType);

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			OrgSupplierPartCollection result = null;
			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				result = new OrgSupplierPartCollection(Factory, Parent, Parent.InvoiceHeader?.IsExport ?? ZBool.False);
			}
			return result;
		}

		public TariffViewCollection NaladiNccaTariffList
		{
			get
			{
				var effectiveAssessmentDate = Parent.EffectiveAssessmentDate.IsValid ? Parent.EffectiveAssessmentDate : ZDateTime.Today;
				return TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Brazil, Constants.TariffTypes.NCCA,
					effectiveAssessmentDate);
			}
		}

		public TariffViewCollection NaladiHsTariffList
		{
			get
			{
				var effectiveAssessmentDate = Parent.EffectiveAssessmentDate.IsValid ? Parent.EffectiveAssessmentDate : ZDateTime.Today;
				return TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Brazil, Constants.TariffTypes.NALADIHS,
					effectiveAssessmentDate);
			}
		}

		public CodeDescriptionPairList CapacityUnitList => Factory.GetCachedValue<CapacityUnitList>();

		public CodeDescriptionPairList ContainerTypeList => Factory.GetCachedValue<ContainerTypeList>();

		public RefCurrencyCollection Currencies => new RefCurrencyCollection(Factory);

		public CodeDescriptionPairList DrawbackModalityList => Factory.GetCachedValue<DrawbackModalityList>();

		public CodeDescriptionPairList CertificateTypeList => Factory.GetCachedValue<CertificateTypeList>();

		public CodeDescriptionPairList DutyTaxRegimeList => Parent.DutyTaxRegimeSupportingInfo.Lookups.TaxRegimeList;

		public CodeDescriptionPairList IPITaxRegimeList => Parent.IPITaxRegimeSupportingInfo.Lookups.TaxRegimeList;

		public CodeDescriptionPairList PisCofinsTaxRegimeList => Parent.PisCofinsTaxRegimeSupportingInfo.Lookups.TaxRegimeList;

		public CodeDescriptionPairList DutyLegalBaseList => Parent.DutyTaxRegimeSupportingInfo.Lookups.LegalBaseList;

		public CodeDescriptionPairList PisCofinsLegalBaseList => Parent.PisCofinsTaxRegimeSupportingInfo.Lookups.LegalBaseList;

		public CodeDescriptionPairList ExTariffLegalActList => BRRefCusCodeListTypes.GetExTariffLegalActList(Factory);

		public CodeDescriptionPairList RatePreferencesList => UniversalReferenceDataHelper.GetPreferenceListByCountry(Factory, Core.Constants.CountryCodes.Brazil);

		public CodeDescriptionPairList LegalActIssuingAuthorityList => BRRefCusCodeListTypes.GetLegalActIssuingAuthorityList(Factory);

		public CodeDescriptionPairList ICMSTaxRegimeList => Parent.ICMSTaxRegimeSupportingInfo.Lookups.TaxRegimeList;

		public CodeDescriptionPairList ICMSLegalBaseList => Parent.ICMSTaxRegimeSupportingInfo.Lookups.LegalBaseList;

		public CodeDescriptionPairList ImportLicenseTypeList => Factory.GetCachedValue<ImportLicenseType>();

		public CodeDescriptionPairList ImportLicenseFeeTypeList => Parent.ImportLicenseSupportingInfo.Lookups.FeeTypeList;

		public CodeDescriptionPairList TariffAgreementList => BRRefCusCodeListTypes.GetTariffAgreementCodeList(Factory, Parent.EffectiveAssessmentDate);

		public CodeDescriptionPairList FMMBenefitList => Parent.FMMTaxRegimeSupportingInfo.Lookups.TaxRegimeList;

		public CodeDescriptionPairList DuimpLegalBaseList
		{
			get
			{
				if (Parent.IsImportOnly)
				{
					var list = Parent.Declaration.GetDuimpLegalBaseListFromMessage(InvoiceLine.JI_Tariff, InvoiceLine.JI_CountryOfOrigin, optionalOnly: true);
					if (list == null)
					{
						var effectiveDate = Parent.EffectiveAssessmentDate.Date;
						list = Factory.GetCachedValue($"BR_DuimpLegalBaseList_{InvoiceLine.JI_Tariff}_{InvoiceLine.JI_CountryOfOrigin}_{effectiveDate}", () =>
						{
							var result = new CodeDescriptionPairList();

							var profiles = Parent.GetRequiredTTProfiles(isMandatory: false);
							if (profiles != null && profiles.Length > 0)
							{
								var legalBaseLis = BRRefCusCodeListTypes.GetDuimpLegalBaseList(Factory);
								var legalCodes = profiles.Select(p => p.LegalCode).Where(c => !c.IsEmpty).Distinct();
								result.AddRange(legalBaseLis.Cast<ICodeDescription>().Where(pair => legalCodes.Contains(pair.Code)).ToList());
								result.Sort();
							}

							return result;
						});
					}
					return list;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public override ConsignorCollection SupplierList
		{
			get
			{
				var isImportOnly = Parent.IsImportOnly;

				return Factory.GetCachedValue("BR|JobComInvoiceLineLookups|SupplierList_" + isImportOnly, () =>
				{
					var suppliers = base.SupplierList;
					if (BRCustomsDataRegistry.Instance.EnableForeignOperator.Value && isImportOnly)
					{
						suppliers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.IsForeignOperator.FilterName, "Property", (ZString)OrgConstants.FilterControl.IsForeignOperator.Code.Yes));
					}
					return suppliers;
				});
			}
		}

		public CodeDescriptionPairList ComplementaryNoteList => new CodeDescriptionPairList();

		public CodeDescriptionPairList RefCusCodeListMATMPList => BRRefCusCodeListTypes.GetRefCusCodeListMATMPList(Factory);

		public CodeDescriptionPairList GoodsApplicationTypeList => Parent.IsImportSiscomex ? Factory.GetCachedValue<GoodsApplicationTypeList>() : Factory.GetCachedValue<ImportGoodsApplicationTypeList>();

		public CodeDescriptionPairList GoodsConditionTypeList => Parent.IsImportSiscomex ? Factory.GetCachedValue<GoodsConditionTypeList>() : Factory.GetCachedValue<ImportGoodsConditionTypeList>();

		public CodeDescriptionPairList ICMSFormulaList => Factory.GetCachedValue<ICMSFormulaList>();

		public CodeDescriptionPairList UsedMaterialRegimeList => Factory.GetCachedValue<UsedMaterialRegimeList>();

		public CodeDescriptionPairList GoodsConditionOperationTypeList => Factory.GetCachedValue<GoodsConditionOperationTypeList>();

		public CodeDescriptionPairList ManufacturerIndicatorList => Factory.GetCachedValue<ManufacturerIndicatorList>();
	}
}
