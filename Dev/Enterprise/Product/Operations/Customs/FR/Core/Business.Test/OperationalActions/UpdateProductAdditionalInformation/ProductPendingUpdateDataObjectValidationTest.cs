using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(ProductPendingUpdateDataObjectValidation))]
	public class ProductPendingUpdateDataObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckProductPk()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			dataObject.ProductPk = ZGuid.Empty;
			AssertHasError(dataObject.ProductPkInfo, "Please enter a Product.");

			dataObject.ProductPk = ZGuid.Invalid;
			AssertHasError(dataObject.ProductPkInfo, "Enter a valid Product.");

			dataObject.ProductPk = ZGuid.NewZGuid();
			AssertNoErrors(dataObject.ProductPkInfo);
		}

		public void TestCheckOrganizationPk()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			dataObject.OrganizationPk = ZGuid.Empty;
			AssertNoErrors(dataObject.OrganizationPkInfo);

			dataObject.OrganizationPk = ZGuid.Invalid;
			AssertHasError(dataObject.OrganizationPkInfo, "Enter a valid Organization.");

			dataObject.OrganizationPk = ZGuid.NewZGuid();
			AssertNoErrors(dataObject.OrganizationPkInfo);
		}

		public void TestCheckCustomsType()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			dataObject.CustomsType = ZString.Empty;
			AssertHasError(dataObject.CustomsTypeInfo, "Please enter a Customs Type.");

			dataObject.CustomsType = "IMP";
			AssertNoErrors(dataObject.CustomsTypeInfo);

			dataObject.CustomsType = "EXP";
			AssertNoErrors(dataObject.CustomsTypeInfo);

			dataObject.CustomsType = "BTH";
			AssertHasError(dataObject.CustomsTypeInfo, "Enter a valid Customs Type.");
		}
	}
}
