using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DeclarationExportPermit))]
	sealed class ExportPermitTest : Customs.Business.Testing.CusCodeDataTest<DeclarationExportPermit>
	{
		public void TestValidation()
		{
			DeclarationExportPermit permit = Factory.New<DeclarationExportPermit>();
			AssertEquals("Validation", typeof(ExportPermitValidation), permit.Validation.GetType());
		}

		public void TestLookups()
		{
			DeclarationExportPermit permit = Factory.New<DeclarationExportPermit>();
			AssertEquals("Validation", typeof(CusCodeDataLookups), permit.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			DeclarationExportPermit permit = Factory.New<DeclarationExportPermit>();
			AssertEquals(CusCodeDataTypeList.Codes.Permit, permit.CY_Code);
		}

		public void TestParent()
		{
			DeclarationExportPermit permit = Factory.New<DeclarationExportPermit>();
			permit.CY_ParentID = Declaration.PK;
			permit.CY_ParentTableCode = Declaration.TablePrefix;
			AssertEquals(Declaration, permit.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().Permits.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Declaration.Permits.AddNew();
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}
