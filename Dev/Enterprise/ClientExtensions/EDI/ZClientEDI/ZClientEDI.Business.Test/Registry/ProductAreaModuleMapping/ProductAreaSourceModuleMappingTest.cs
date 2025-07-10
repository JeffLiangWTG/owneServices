using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ProductAreaSourceModuleMapping))]
	public class ProductAreaSourceModuleMappingTest : RegistryBusinessObjectTemplateTestCase<ProductAreaSourceModuleMapping>
	{
		#region Validation

		public void TestValidateCode()
		{
			var collection = new ProductAreaSourceModuleMappingCollection();

			var mapping1 = collection.AddNew();
			mapping1.Code = "";
			AssertHasErrorContaining(mapping1.CodeInfo, MandatoryValidation.MustBeEntered);

			var mapping2 = collection.AddNew();
			mapping2.Code = "XXX";
			AssertNoErrors(mapping2.CodeInfo);

			var mapping3 = collection.AddNew();
			mapping3.Code = "XXX";
			AssertHasErrorContaining(mapping3.CodeInfo, "unique");
		}

		public void TestValidateProductArea()
		{
			BizObj.ProductArea = "";
			AssertHasErrorContaining(BizObj.ProductAreaInfo, MandatoryValidation.MustBeEntered);

			BizObj.ProductArea = "AA";
			AssertHasErrorContaining(BizObj.ProductAreaInfo, ListValidation.InvalidCodeError);

			BizObj.ProductArea = "ARC";
			AssertNoErrors(BizObj.ProductAreaInfo);
		}

		#endregion

		public void TestDescription()
		{
			BizObj.Code = "NotACode";
			AssertEquals("", BizObj.Description);

			BizObj.Code = "WorkItem";
			AssertEquals("Work Items", BizObj.Description);
		}

		public void TestModuleTreePath()
		{
			BizObj.Code = "NotACode";
			AssertEquals("", BizObj.ModuleTreePath);

			BizObj.Code = "WorkItem";
			AssertEquals("Operate > Productivity Tools >", BizObj.ModuleTreePath);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ProductAreaSourceModuleMapping GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override ProductAreaSourceModuleMapping GetBusinessObjectToSerialise()
		{
			BizObj.Code = "Orders";
			BizObj.ProductArea = "INT";
			return BizObj;
		}

		#endregion
	}
}
