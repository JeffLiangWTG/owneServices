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
	typeof(DirectReceiptData),
	Enterprise.Core.Constants.DocManagerCodes.DirectReceipt)]

namespace Enterprise.Accounting.Business
{
	public class DirectReceiptData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CashBook.DirectReceipt.DirectReceipt); } }
		protected override Type CollectionType
		{
			get { return typeof(TransactionHeaderCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("f138ee20-eb51-4fe7-9bb3-83aa0be622d8", "Direct Receipt"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CashbookTransaction; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DirectReceipt);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.DirectReceipt), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DirectReceipt);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.DirectReceipt), false));
			return collection;
		}
	}
}
