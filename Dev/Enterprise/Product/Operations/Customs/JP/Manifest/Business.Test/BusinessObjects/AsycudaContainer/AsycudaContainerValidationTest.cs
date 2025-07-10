using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.JP.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerValidation))]
	public class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckVanningLocationCode()
		{
			var validCode = Factory.CreateJapanBondedAreaCode();
			ValidationTestHelper.AssertInvalidCodeMessageError(Container.VanningLocationCodeInfo, "XXX", validCode, "The entered Move-In Destination is invalid. Please select a value from the list.");
		}

		[TestDate(2024, 10, 16)]
		public void TestCheckACN_MoveOutDate()
		{
			var messageError1 = "Please enter Move Out Date.";
			var messageError2 = "Please enter Move Out Date within 2 days of the system date.";
			var targetInfo = Container.ACN_MoveOutDateInfo;
			Container.ACN_MoveOutDate = new ZDateTime(2024, 10, 18);
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, messageError1);
				AssertNoMessageError(targetInfo, messageError2);
			});

			Container.ACN_MoveOutDate = new ZDateTime(2024, 10, 19);
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, messageError1);
				AssertHasMessageError(targetInfo, messageError2);
			});

			Container.ACN_MoveOutDate = ZDateTime.Empty;
			CombineAssertions(() =>
			{
				AssertHasMessageError(targetInfo, messageError1);
				AssertNoMessageError(targetInfo, messageError2);
			});
		}

		public void TestCheckACN_Seal1()
		{
			var messageError = "At least one seal number is required when the via location code is empty.";
			var targetInfo = Container.ACN_Seal1Info;
			var containerValidation = Container.Validation;
			Header.ViaLocationCode = "XXX";
			containerValidation.ValidateACN_Seal1();
			AssertNoMessageError(targetInfo, messageError);

			Header.ViaLocationCode = ZString.Empty;
			containerValidation.ValidateACN_Seal1();
			AssertHasMessageError(targetInfo, messageError);

			container.ACN_Seal1 = "123";
			AssertNoMessageError(targetInfo, messageError);

			container.ACN_Seal2 = "123";
			container.ACN_Seal1 = ZString.Empty;
			AssertNoMessageError(targetInfo, messageError);

			container.ACN_Seal2 = ZString.Empty;
			container.ACN_Seal3 = "123";
			containerValidation.ValidateACN_Seal1();
			AssertNoMessageError(targetInfo, messageError);

			container.ACN_Seal3 = ZString.Empty;
			container.AdditionalSeals.AddNew().BK_SealNumber = "123";
			containerValidation.ValidateACN_Seal1();
			AssertNoMessageError(targetInfo, messageError);
		}

		AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
		AsycudaManifestHeader header;

		AsycudaContainer Container => container ??= Header.Containers.AddNew();
		AsycudaContainer container;
	}
}
