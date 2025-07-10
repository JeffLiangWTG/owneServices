using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidation_WhenBillDetachedFromHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			header.Bills.Remove(bill);

			AssertNoExceptionThrown(() =>
			{
				pack.Validation.ValidateAll();
			});
		}

		public void TestCheckAPA_PackUQ()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("By default APA_PackUQ is not mandatory", pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F22;
				pack.Validation.ValidateAPA_PackUQ();
				AssertHasMessageErrorContaining("APA_PackUQ is mandatory for F22", pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F26;
				pack.Validation.ValidateAPA_PackUQ();
				AssertHasMessageErrorContaining("APA_PackUQ is mandatory for F26", pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				pack.Validation.ValidateAPA_PackUQ();
				AssertHasMessageErrorContaining("APA_PackUQ is mandatory for F50", pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckAPA_MarksAndNumbers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			const string mandatoryErrorMessage = "You have not entered Marks and Numbers (on Pack)";
			var maxlength = 512;
			var maxLengthErrorMessage = $"Length of {pack.APA_MarksAndNumbersInfo.HumanReadableName} must not exceed {maxlength} characters.";

			CombineAssertions(() =>
			{
				pack.APA_PackUQ = "XX";
				AssertHasMessageError("APA_MarksAndNumbers is mandatory if APA_PackUQ NOT IN (VQ, VG, VL, VY, VR, VS, VO, NE, NF, NG)", pack.APA_MarksAndNumbersInfo, mandatoryErrorMessage);

				var validValues = new ZString[] { "VQ", "VG", "VL", "VY", "VR", "VS", "VO", "NE", "NF", "NG", ZString.Empty };
				foreach (var validValue in validValues)
				{
					pack.APA_PackUQ = validValue;
					AssertNoMessageError("APA_MarksAndNumbers is not mandatory if APA_PackUQ IN (VQ, VG, VL, VY, VR, VS, VO, NE, NF, NG)", pack.APA_MarksAndNumbersInfo, mandatoryErrorMessage);
				}

				pack.APA_MarksAndNumbers = new string('X', maxlength);
				AssertNoMessageError($"Shorter {maxlength} characters", pack.APA_MarksAndNumbersInfo, maxLengthErrorMessage);

				pack.APA_MarksAndNumbers = pack.APA_MarksAndNumbers + "X";
				AssertHasMessageError($"Longer {maxlength} characters", pack.APA_MarksAndNumbersInfo, maxLengthErrorMessage);
			});
		}

		public void TestCheckAPA_Weight()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 0;
			pack.APA_Weight = 0;
			AssertNoMessageErrorContaining(pack.APA_WeightInfo, "cannot be zero");

			pack.APA_PackQty = 2;
			pack.APA_Weight = 0;
			AssertHasMessageErrorContaining(pack.APA_WeightInfo, "cannot be zero");
		}

		public void TestCheckAPA_PackQty()
		{
			const string messageError = "Quantity (on Pack) can only be 0, if another pack record with the same 'Marks and Numbers (on Pack)' and 'Quantity (on Pack) > 0 has been entered.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.APA_PackQty = 0;
			pack1.APA_MarksAndNumbers = "TEST";
			AssertHasMessageError("No other pack with Qty and MarksAndNumbers found", pack1.APA_PackQtyInfo, messageError);

			pack1.APA_PackQty = 2;
			AssertNoMessageError("PackQty entered", pack1.APA_PackQtyInfo, messageError);

			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 5;
			pack2.APA_MarksAndNumbers = "TEST";
			pack1.APA_PackQty = 0;
			AssertNoMessageError("Another pack with Qty and MarksAndNumbers found", pack1.APA_PackQtyInfo, messageError);

			pack2.APA_MarksAndNumbers = "TEST2";
			pack1.APA_PackQty = 0;
			AssertHasMessageError("Another pack with Qty exist, but MarksAndNumbers is not same", pack1.APA_PackQtyInfo, messageError);

			pack2.APA_MarksAndNumbers = "TEST2";
			pack1.APA_PackQty = 0;
			AssertHasMessageError("Another pack with Qty exist, but MarksAndNumbers is not same", pack1.APA_PackQtyInfo, messageError);

			pack2.APA_MarksAndNumbers = "TEST2";
			pack2.APA_PackQty = 5;
			pack1.APA_MarksAndNumbers = ZString.Empty;
			pack1.APA_PackQty = 0;
			AssertHasMessageError("MarksAndNumbers should be not empty", pack1.APA_PackQtyInfo, messageError);
		}

		public void TestCheckF50HasATransportMeans_ShowMessageError()
		{
			const string expectedError = "You have not entered Passive Border Transport Information.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			CombineAssertions(() =>
			{
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F21;
				pack.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F21 and no transport means", pack, expectedError);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				pack.Validation.ValidateAll();
				AssertHasRowMessageError("Show error when F50 and no transport means", pack, expectedError);

				pack.AsycudaTransportMeans.AddNew();
				pack.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F50 and has transport means", pack, expectedError);
			});
		}

		public void TestVolumeIsNotRequired()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = Factory.New<PackForTest>();
			bill.Packs.Add(pack);

			AssertEquals(expected: false, ((PackValidationForTest)pack.Validation).IsVolumeRequiredExposed());
		}

		sealed class PackForTest : AsycudaPack
		{
			public PackForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new PackValidationForTest(this);
		}

		sealed class PackValidationForTest : AsycudaPackValidation
		{
			public PackValidationForTest(AsycudaPack pack) : base(pack)
			{
			}

			public bool IsVolumeRequiredExposed() => IsVolumeRequired();
		}
	}
}
