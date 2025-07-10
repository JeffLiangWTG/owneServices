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
	typeof(CurrencyAdjustmentData),
	Enterprise.Core.Constants.DocManagerCodes.CurrencyAdjustment)]

namespace Enterprise.Accounting.Business
{
	public class CurrencyAdjustmentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CashBook.ExchangeDifference.CashbookExchangeDiff); } }
		protected override Type CollectionType
		{
			get { return typeof(CashbookTransactionCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ca399258-9db5-46f8-9bf0-ebeca36aa67a", "Currency Adjustment"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CashbookTransaction; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			CashbookTransactionCollection collection = new CashbookTransactionCollection(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.ExchangeDifference), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode).PK);
			CashbookTransactionCollection collection = new CashbookTransactionCollection(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.ExchangeDifference), false));
			return collection;
		}
	}
}
