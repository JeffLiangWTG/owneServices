using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public abstract class GlobalElectronicMessagingProcessingServiceTask : ElectronicMessagingProcessingServiceTask
	{
		protected GlobalElectronicMessagingProcessingServiceTask()
			: this(dataProvider: null)
		{
		}

		protected GlobalElectronicMessagingProcessingServiceTask(IElectronicMessagingProcessingServiceTaskDataProvider dataProvider = null)
			: base(dataProvider)
		{
			CountryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode);
		}

		protected ICountryEInvoicingObjectFactory CountryFactory { get; }

		public override ZString MessageName => (NoResString)"Electronic Invoice";             // service task label.

		public override ZString TaskName => $"{CountryCode} E-Invoice Processing";      // service task label.

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
			=> CountryFactory.GetBatchCreator(company);

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			var suffix = AccountingElectronicMessagingRegistry.Instance.eInvoicingServicePointSuffix.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			return new GlobalEDIInterchangeCreator(company, CountryFactory, suffix);
		}

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company)
			=> CountryFactory.GetDataValidator(company);
	}
}
