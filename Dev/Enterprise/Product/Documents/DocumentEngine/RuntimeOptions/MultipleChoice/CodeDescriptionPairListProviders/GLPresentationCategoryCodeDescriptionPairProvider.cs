using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class GLPresentationCategoryCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			if (fUnion == null)
			{
				ReadOnlyCodeDescriptionPairList validCodes = GetValidCodes();
				fUnion = new CodeDescriptionPairList(validCodes);
				foreach (DynamicBusinessObject code in UsedCodes())
				{
					string testCode = code[AccGLAggregateSchema.AA_TransactionCategory].ToString();
					if (!string.IsNullOrEmpty(testCode) && !validCodes.ContainsCode(testCode))
					{
						fUnion.AddPair(testCode, Res.GetString("5d25ef67-665a-48e9-bc67-0ccf3100323b", "Obsolete code"));
					}
				}
			}
			return fUnion;
		}

		CodeDescriptionPairList fUnion;

		protected virtual ReadOnlyCodeDescriptionPairList GetValidCodes()
		{
			return (ReadOnlyCodeDescriptionPairList)ObjectFactory.Get<IAccounting>().GLPresentationJournalCategoriesList(GlbCompany.CurrentCompany.PK.ToGuid());
		}

		DynamicBusinessObjectCollection UsedCodes()
		{
			DynamicBusinessObjectCollection fUsedCodes = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			string sQL = "SELECT DISTINCT AA_TransactionCategory FROM dbo.AccGLAggregate WHERE AA_GB IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = @CompanyPK) AND AA_TransactionCategory <> ''";
			ZSqlParameterCollection parameter = new ZSqlParameterCollection();
			parameter.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, AccGLAggregateSchema.AA_GB);
			fUsedCodes.Load(sQL, parameter);
			return fUsedCodes;
		}
	}
}
