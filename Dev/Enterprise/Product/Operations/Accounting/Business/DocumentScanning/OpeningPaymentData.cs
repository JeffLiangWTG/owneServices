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
	typeof(OpeningPaymentData),
	Enterprise.Core.Constants.DocManagerCodes.OpeningPayment)]

namespace Enterprise.Accounting.Business
{
	public class OpeningPaymentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CashBook.OpeningPayment.OpeningPayment); } }
		protected override Type CollectionType
		{
			get { return typeof(TransactionHeaderCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ca0ec144-9fdb-4d71-b287-c62e30a0d7d3", "Opening Payment"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CashbookTransaction; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.OpeningPayment);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.OpeningPayment), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.OpeningPayment);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.OpeningPayment), false));
			return collection;
		}
	}
}
