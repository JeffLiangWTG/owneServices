using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBQ_Seal1()
		{
			container.BQ_Seal1 = "12345678901234567890";
			AssertHasMessageError(container.BQ_Seal1Info, "Seal Number cannot be more than 15 characters.");
			container.BQ_Seal1 = "123456789012345";
			AssertNoNotifications(container.BQ_Seal1Info);
		}

		public void TestCheckBQ_Seal2()
		{
			container.BQ_Seal2 = "12345678901234567890";
			AssertHasMessageError(container.BQ_Seal2Info, "Seal Number cannot be more than 15 characters.");
			container.BQ_Seal2 = "123456789012345";
			AssertNoNotifications(container.BQ_Seal2Info);
		}

		public void TestCheckBQ_ContainerNumber()
		{
			container.BQ_ContainerNumber = "INVALID";
			AssertHasWarning(container.BQ_ContainerNumberInfo, ContainerNumberValidation.GetContainerNumberError("INVALID"));
			container.BQ_ContainerNumber = ZString.Empty;
			AssertHasMessageErrorContaining(container.BQ_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);

			container.BQ_ContainerNumber = "1234";
			AssertHasMessageErrorContaining(container.BQ_ContainerNumberInfo, CusCAeMHContainerizedContainerValidation.InvalidContainerNumberLength);

			container.BQ_ContainerNumber = "12345678901234567";
			AssertHasMessageErrorContaining(container.BQ_ContainerNumberInfo, CusCAeMHContainerizedContainerValidation.InvalidContainerNumberLength);

			container.BQ_ContainerNumber = "1234567890123456";
			AssertNoMessageErrorContaining(container.BQ_ContainerNumberInfo, CusCAeMHContainerizedContainerValidation.InvalidContainerNumberLength);

			container.BQ_ContainerNumber = "123456789012~56";
			AssertHasMessageErrorContaining(container.BQ_ContainerNumberInfo, CusCAeMHContainerizedContainerValidation.InvalidContainerNumberLength);

			container.BQ_ContainerNumber = "123456789012356";
			AssertNoMessageErrorContaining(container.BQ_ContainerNumberInfo, CusCAeMHContainerizedContainerValidation.InvalidContainerNumberLength);
		}

		public void TestCheckBQ_RC_NKContainerType()
		{
			container.BQ_RC_NKContainerType = "XX";
			AssertHasMessageErrorContaining(container.BQ_RC_NKContainerTypeInfo, ListValidation.InvalidCodeMessageError);

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40C";
			container.BQ_RC_NKContainerType = refContainer.RC_Code;
			AssertNoMessageErrorContaining(container.BQ_RC_NKContainerTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		CusCAeMHContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			var master = Factory.New<CusCAeMHMaster>();
			container = master.Containers.AddNew();
		}
	}
}
