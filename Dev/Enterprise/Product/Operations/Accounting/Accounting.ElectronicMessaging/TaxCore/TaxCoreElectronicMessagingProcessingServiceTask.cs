using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	#region SuppressResourceStringsCheckRegion

	// Murray Note: new countries should inherit from GlobalElectronicMessagingProcessingServiceTask rather than ElectronicMessagingProcessingServiceTask.
	// Please talk to Murray if you think it is a good idea to inherit from ElectronicMessagingProcessingServiceTask for your new country.
	public abstract class TaxCoreElectronicMessagingProcessingServiceTask : ElectronicMessagingProcessingServiceTask
	{
		ITaxCoreCountryEInvoicingObjectFactory CountryFactory => countryFactory ?? (countryFactory = TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode));
		ITaxCoreCountryEInvoicingObjectFactory countryFactory;

		public override ZString MessageName => "Electronic Invoice"; // service task label.

		public override ZString TaskName => $"{CountryName} E-Reporting Invoice Processing"; // service task label.

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company) => new EInvoicingBatchCreatorForTaxCore(company);

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company) => new EDIInterchangeCreatorForTaxCore(company, CountryFactory);

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company) => new NullEInvoicingDataValidator(company);

		ZString CountryName => RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), CountryCode)?.RN_Desc ?? ZString.Empty;
	}

	#endregion
}
