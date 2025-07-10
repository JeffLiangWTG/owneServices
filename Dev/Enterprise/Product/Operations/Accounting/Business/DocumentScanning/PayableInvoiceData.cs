using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(PayableInvoiceData),
	Enterprise.Core.Constants.DocManagerCodes.PayableInvoice)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using Enterprise.ZArchitecture.Schema;

	class PayableInvoiceData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARAP.Invoicing.APInvoice); } }
		protected override Type CollectionType
		{
			get { return typeof(APInvoiceCollectionForDocScanning); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("71002986-b58d-4726-a79d-ee9923b9d173", "Payable Invoice"); } }
		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			APInvoiceCollectionForDocScanning collection = new APInvoiceCollectionForDocScanning(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Invoice), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			APInvoiceCollectionForDocScanning collection = new APInvoiceCollectionForDocScanning(factory, filter, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Invoice), false));
			return collection;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APTransaction; }
		}
	}
}
