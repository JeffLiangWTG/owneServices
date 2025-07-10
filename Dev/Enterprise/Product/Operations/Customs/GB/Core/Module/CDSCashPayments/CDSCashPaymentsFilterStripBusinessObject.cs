using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Module
{
	public class CDSCashPaymentsFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Filters
		{
			public const string Ducr = "DUCR";
			public const string Importer = "Importer";
			public const string MRN = "MRN";
			public const string PaymentDate = "Payment Date";
			public const string PaymentReference = "Payment Reference";
			public const string PaymentStatus = "Payment Status";
			public const string ReceiptDate = "Receipt Date";
			public const string TransactionType = "Transaction Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(Filters.Ducr, new GetTextQueryWithOperator(GetDUCRQuery)).WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_BGMReference)
				.MultilingualDescription = ResString.GetMultilingualString("95AADD20-6593-4E36-A119-5BB2AFD93DA6", Filters.Ducr);
			result.AddGuidFilter(Filters.Importer, ModuleIDs.Organisation, new GetGuidQueryWithOperator(GetImporterQuery), ImporterList)
				.MultilingualDescription = ResString.GetMultilingualString("47149F6F-1DEA-4923-9477-32C29267D225", Filters.Importer);
			result.AddTextFilter(Filters.MRN, new GetTextQueryWithOperator(GetMRNQuery)).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum)
				.MultilingualDescription = ResString.GetMultilingualString("CA2A7B62-32A7-4E92-AAF5-5C13473399C2", Filters.MRN);
			result.AddDateFilter(Filters.PaymentDate, CusEntryPayInfoSchema.C9_PaymentDate)
				.MultilingualDescription = ResString.GetMultilingualString("238122D1-DD88-48CA-9459-56FA59E61CCA", Filters.PaymentDate);
			result.AddTextFilter(Filters.PaymentReference, CusEntryPayInfoSchema.C9_PaymentReference).WithMaxLengthOf<ModuleTextFilter>(CusEntryPayInfoSchema.C9_PaymentReference)
				.MultilingualDescription = ResString.GetMultilingualString("DAFDF079-128B-442D-B793-AD707B1D4F60", Filters.PaymentReference);
			result.AddTextFilter(Filters.PaymentStatus, CusEntryPayInfoSchema.C9_PaymentStatus).WithMaxLengthOf<ModuleTextFilter>(CusEntryPayInfoSchema.C9_PaymentStatus)
				.MultilingualDescription = ResString.GetMultilingualString("DB5403A9-DD50-4016-A49D-7F4AD91B61AC", Filters.PaymentStatus);
			result.AddDateFilter(Filters.ReceiptDate, CusEntryPayInfoSchema.C9_ReceiptDate)
				.MultilingualDescription = ResString.GetMultilingualString("2DE4495F-C7FD-486B-92FD-63C390FE22A5", Filters.ReceiptDate);
			result.AddTextFilter(Filters.TransactionType, CusEntryPayInfoSchema.C9_TransactionType, TransactionTypeList).WithMaxLengthOf<ModuleTextFilter>(CusEntryPayInfoSchema.C9_TransactionType)
				.MultilingualDescription = ResString.GetMultilingualString("57A14707-501E-403D-8F3A-3225CFD6F42A", Filters.TransactionType);
			return result;
		}

		IBusinessObjectCollection importerList;
		public IBusinessObjectCollection ImporterList
		{
			get { return importerList ?? (importerList = new OrganisationsFindBoxCollection(Factory)); }
		}

		ZQuery GetImporterQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			var sub1 = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.JE_ClusterKey);
			var sub2 = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			sub2.AddToFilter(OrgHeaderSchema.PK, comparisonOperator, value);
			sub2.AddToFilter(OrgHeaderSchema.OH_IsConsignee, ZBool.True);
			sub1.AddSubQuery(JobDeclarationSchema.JE_OH_Importer, sub2, JoinCondition.And);
			query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, sub1, JoinCondition.And);
			return query;
		}

		ZQuery GetMRNQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			var sub1 = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			var sub2 = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			sub2.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			sub2.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			sub1.AddSubQuery(CusEntryHeaderSchema.PK, sub2, JoinCondition.And);
			query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, sub1, JoinCondition.And);
			return query;
		}

		ZQuery GetDUCRQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			var sub1 = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			sub1.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, value);
			query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, sub1, JoinCondition.And);
			return query;
		}

		CodeDescriptionPairList transactionTypeList;
		public CodeDescriptionPairList TransactionTypeList
		{
			get { return transactionTypeList ?? (transactionTypeList = new PaymentTransactionTypeList()); }
		}
	}
}
