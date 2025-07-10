using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DeclarationExportPermitCollection))]
	sealed class DeclarationExportPermitCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<DeclarationExportPermit>
	{
		public void TestAddNewWithOneParameter()
		{
			DeclarationExportPermitCollection collection = Declaration.Permits;
			AssertEquals("Count", 0, collection.Count);
			DeclarationExportPermit permit = collection.AddNew("PERM12");
			AssertEquals("CY_Data", "PERM12", permit.CY_Data);
		}

		protected override Customs.Business.CusCodeDataCollection<DeclarationExportPermit> GetCusCodeDataCollection()
		{
			return new DeclarationExportPermitCollection(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			DeclarationExportPermit result = Factory.New<DeclarationExportPermit>();
			result.CY_ParentID = Declaration.PK;
			result.CY_ParentTableCode = Declaration.TablePrefix;
			return result;
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}
