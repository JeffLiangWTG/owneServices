using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class AddressExtensionsTest : TestCaseWithFactory
	{
		public void TestNotInput()
		{
			var address = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company Full Name", "SupplierCus1", "SupplierSocial1", "SupplierCIQ1", "Supplier Company");
			var declaration = Factory.New<JobDeclaration>();
			var docAddress = declaration.ImporterDocumentaryAddress;
			Assert("HasAddress Should return false when Address not input and not overriden.", !docAddress.HasAddress());
			docAddress.E2_OA_Address = address.PK;
			Assert("HasAddress Should return true when Organisation input.", docAddress.HasAddress());
			docAddress.E2_OA_Address = ZGuid.Empty;
			docAddress.E2_AddressOverride = true;
			Assert("HasAddress Should return false when overriden but not input.", !docAddress.HasAddress());
			docAddress.E2_Address1 = "ADDRESS";
			Assert("HasAddress Should return true when overriden input.", docAddress.HasAddress());
		}
	}
}
