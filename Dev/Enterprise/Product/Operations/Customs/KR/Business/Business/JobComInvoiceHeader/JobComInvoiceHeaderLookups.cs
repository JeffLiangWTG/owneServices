using System.Collections;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceHeaderLookups : AutoKRJobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public override ICodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<ValuationCodeList>();

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		public CodeDescriptionPairList CountryOfOriginLabelLocationCodeList => Factory.GetCachedValue<CountryOfOriginLabelLocationCodeList>();

		public RefCountryCollection CountryCollection => new RefCountryCollection(Factory);

		public CodeDescriptionPairList DRWApplicantTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.IsLocalExport)
				{
					result = Factory.GetCachedValue<LocalExportDrawbackApplicantTypeList>();
				}
				else
				{
					result = Factory.GetCachedValue<DrawbackApplicantTypeList>();
				}
				result.Sort();
				return result;
			}
		}

		public CodeDescriptionPairList CertificateOfOriginIssuedCodeList => Factory.GetCachedValue<CertificateOfOriginIssuedCodeList>();
		public CodeDescriptionPairList CountryOfOriginDeterminationRuleCodeList => Factory.GetCachedValue<CountryOfOriginDeterminationRuleCodeList>();
		public CodeDescriptionPairList CountryOfOriginLabelTypeCodeList => Factory.GetCachedValue<CountryOfOriginLabelTypeCodeList>();
		public CodeDescriptionPairList CountryOfOriginExemptionReasonCodeList => Factory.GetCachedValue<CountryOfOriginExemptionReasonCodeList>();
		public CodeDescriptionPairList CertificateOfOriginSplitCodeList => Factory.GetCachedValue<CertificateOfOriginSplitCodeList>();

		public CodeDescriptionPairList InvoicePaymentTermCodeList => Factory.GetCachedValue<InvoicePaymentTermCodeList>();
		public override CodeDescriptionPairList JZ_IncoTerm_List
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.IsLocalExport)
				{
					result = Factory.GetCachedValue<LocalExportIncotermList>();
				}
				else
				{
					result = Factory.GetCachedValue<IncotermList>();
				}

				return result;
			}
		}

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);
		public ZZRefCusCodeListCombinedCollection IPCCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.IndustrialParkCode, ZDateTime.Today);
		public CodeDescriptionPairList LocalExportDocumentTypeList => Factory.GetCachedValue<LocalExportDocumentTypeList>();
		public CodeDescriptionPairList COOIssuedCodeList => Factory.GetCachedValue<ImportCertificateOfOriginIssuedCodeList>();
		public CodeDescriptionPairList ValueDeclarationAttachedCodeList => Factory.GetCachedValue<ValueDeclarationAttachedCodeList>();
		public CodeDescriptionPairList OnlineTradeTypeList => Factory.GetCachedValue<OnlineTradeTypeCodeList>();
		public CodeDescriptionPairList YesNoCodeList => Factory.GetCachedValue<YesNoList>();
		public CodeDescriptionPairList SpecialRelationshipCodeList => Factory.GetCachedValue<SpecialRelationshipCodeList>();
		public CodeDescriptionPairList ValuationDeclarationPricingMethodsList => Factory.GetCachedValue<PricingCodeList>();
		public CodeDescriptionPairList CostRateCodeList => Factory.GetCachedValue<CostRateCodeList>();
		public CodeDescriptionPairList SpecificUseProductTypeList => Factory.GetCachedValue<SpecificUseProductTypeList>();
		public ICollection GoodsDestination => new RefCountryCollection(Factory);
	}
}
