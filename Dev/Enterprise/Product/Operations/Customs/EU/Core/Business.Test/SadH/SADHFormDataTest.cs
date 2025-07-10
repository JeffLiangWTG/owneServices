using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.SADH;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(SADHFormData))]
	public class SADHFormDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeclarationPropertyGetsSet()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			SADHFormData data = new SADHFormData(Factory, declaration);
			AssertEquals("data.Declaration", declaration, data.Declaration);
		}

		public void TestValidationObjectIsRightType()
		{
			SADHFormData data = new SADHFormData(Factory, Factory.New<JobDeclaration>());
			AssertEquals("data.Validation.GetType()", typeof(SADHFormDataValidation), data.Validation.GetType());
		}

		public void TestLookupsObjectIsRightType()
		{
			SADHFormData data = new SADHFormData(Factory, Factory.New<JobDeclaration>());
			AssertEquals("data.Lookups.GetType()", typeof(SADHFormDataLookups), data.Lookups.GetType());
		}

		public void TestICanBeImportOrExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			SADHFormData data = new SADHFormData(Factory, declaration);
			data.D1_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var impl = data as ICanBeImportOrExport;

			AssertEquals("IsImport", true, impl.IsImport);
			AssertEquals("IsExport", false, impl.IsExport);

			AssertEquals("Level", "Both", impl.Level);
			AssertEquals("Country Code", declaration.CountryCode, impl.TrueCountryCode);
			AssertEquals("Data Grouping", declaration.GetDefaultDataGroupingCode(), impl.DataGroupingCode);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SADHFormData(Factory, Factory.New<JobDeclaration>());
		}
		#endregion
	}
}
