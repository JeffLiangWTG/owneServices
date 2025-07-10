using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(PayableReceiptData),
	Enterprise.Core.Constants.DocManagerCodes.PayableReceipt)]

namespace Enterprise.Accounting.Business
{
	public class PayableReceiptData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARAP.ReceiptPayment.APReceipt); } }
		protected override Type CollectionType
		{
			get { return typeof(TransactionHeaderCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ccac78d5-f431-46ad-b7e8-b76f6fa31b73", "Payables Receipt"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.APTransaction; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Receipt), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Receipt), false));
			return collection;
		}

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new TransactionHeaderWithOrgEDocsViaUniversalXmlSupport(this);
	}
}
