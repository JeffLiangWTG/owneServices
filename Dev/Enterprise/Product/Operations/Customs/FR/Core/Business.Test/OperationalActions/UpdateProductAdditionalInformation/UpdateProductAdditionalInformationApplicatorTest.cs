using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Services.OperationalActions.Support.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(UpdateProductAdditionalInformationApplicator))]
	public class UpdateProductAdditionalInformationApplicatorTest : OperationalActionMethodApplicatorTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateProductAdditionalInformationApplicator(Factory, Mock.Of<IApplicatorValidationSupport>(m => m.IsValid));
		}

		public void TestProductPendingUpdateDataObjectCollection()
		{
			var productPendingUpdateDataForBinding = updateProductAdditionalInformationApplicator.ProductPendingUpdateDataForBinding;
			AssertNotNull(productPendingUpdateDataForBinding);
			AssertType(typeof(ProductPendingUpdateDataObjectCollection), productPendingUpdateDataForBinding);
		}

		public void TestBuild()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_Desc = "Product 1";
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_Desc = "Product 2";
			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_Desc = "Product 3";

			updateProductAdditionalInformationApplicator.Build(new ZGuid[] { product1.PK, product2.PK, product3.PK });

			AssertEquals(3, updateProductAdditionalInformationApplicator.ProductPendingUpdateDataForBinding.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { product1.PK, product2.PK, product3.PK }, updateProductAdditionalInformationApplicator.ProductPendingUpdateDataForBinding.Select(x => x.ProductPk));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Product 1", "Product 2", "Product 3" }, updateProductAdditionalInformationApplicator.ProductPendingUpdateDataForBinding.Select(x => x.Description));
		}

		UpdateProductAdditionalInformationApplicator updateProductAdditionalInformationApplicator => (UpdateProductAdditionalInformationApplicator)Applicator;
	}
}
