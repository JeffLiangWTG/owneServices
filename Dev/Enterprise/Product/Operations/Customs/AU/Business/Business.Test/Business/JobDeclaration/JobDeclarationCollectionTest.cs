using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobDeclarationCollection))]
	sealed class JobDeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypedIndexer()
		{
			JobDeclarationCollection collection = new JobDeclarationCollection(Factory);
			JobDeclaration declaration = collection.AddNew();
			AssertEquals(declaration, collection[0]);
		}

		public void TestMergeAdditionalFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration1.JE_DeclarationReference = "B001";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration2.JE_DeclarationReference = "B002";

			var nZCompany = Factory.New<GlbCompany>();
			nZCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nZBranch = nZCompany.Branches.AddNew();
			nZBranch.GB_RL_NKHomePort = "NZAKL";

			var nzDeclaration = Factory.New<JobDeclaration>();
			nzDeclaration.JE_GB = nZBranch.PK;
			nzDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			nzDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			nzDeclaration.JE_DeclarationReference = "B003";

			Factory.Save();

			var collection1 = new JobDeclarationCollection(Factory);
			collection1.Load();
			AssertNull("NZ Declaration is filtered out by default constructor", collection1.FindByPK(nzDeclaration.PK));
			AssertEquals(2, collection1.Count);

			var additionalFilter = new ZQuery(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration1.PK);

			var collection2 = new JobDeclarationCollection(Factory, additionalFilter);
			collection2.Load();
			AssertNull("NZ Declaration is filtered out by merge constructor", collection2.FindByPK(nzDeclaration.PK));
			AssertNull("Declaration is filtered out by additional filter", collection2.FindByPK(declaration1.PK));
			AssertEquals(1, collection2.Count);

			var dbOnlyFilter = new ZDBOnlyQuery(typeof(Customs.Business.BaseJobDeclaration));
			dbOnlyFilter.AddToFilter(JoinCondition.And, JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration2.PK);

			var collection3 = new JobDeclarationCollection(Factory, dbOnlyFilter);
			collection3.Load();
			AssertNull("NZ Declaration is filtered out by merge constructor", collection3.FindByPK(nzDeclaration.PK));
			AssertNull("Declaration is filtered out by additional filter", collection3.FindByPK(declaration2.PK));
			AssertEquals(1, collection3.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobDeclarationCollection(Factory);
		}

		#endregion

	}
}
