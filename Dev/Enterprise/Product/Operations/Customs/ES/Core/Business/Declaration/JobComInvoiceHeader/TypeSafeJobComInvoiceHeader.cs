using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using InvoiceLineDependentCollection = Enterprise.Customs.Business.InvoiceLineDependentCollection;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class JobComInvoiceHeader
	{
		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

		public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

		protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			var dec = JobDeclaration;
			return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		public new InvoiceChargeCollection<InvoiceCharge> Charges => (InvoiceChargeCollection<InvoiceCharge>)base.Charges;

		public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			JobComInvoiceHeaderValidation result;
			if (IsExport)
			{
				result = new ExportJobComInvoiceHeaderValidation(this);
			}
			else if (IsImport)
			{
				result = new ImportJobComInvoiceHeaderValidation(this);
			}
			else
			{
				result = new JobComInvoiceHeaderValidation(this);
			}

			var declaration = JobDeclaration;
			if (declaration != null)
			{
				return declaration.GetJobComInvoiceHeaderValidation(this) ?? result;
			}
			else
			{
				return result;
			}
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new InvoiceChargeCollection<InvoiceCharge>(this);

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Spain;

		protected override CustomsValuationCalculator GetValuationCalculatorCore() => new ESCustomsValuationCalculator(this);
	}
}
