using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(APPaymentData),
	Enterprise.Core.Constants.DocManagerCodes.APPayment)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using Enterprise.ZArchitecture.Schema;

	public class APPaymentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARAP.ReceiptPayment.APPayment); } }
		protected override Type CollectionType
		{
			get { return typeof(TransactionHeaderCollection); }
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.APTransaction; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("da5cb077-a7a2-4e1a-97a0-3ee31cc423ab", "Payables Payment"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Payment), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Payment), false));
			return collection;
		}

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new TransactionHeaderWithOrgEDocsViaUniversalXmlSupport(this);
	}
}
