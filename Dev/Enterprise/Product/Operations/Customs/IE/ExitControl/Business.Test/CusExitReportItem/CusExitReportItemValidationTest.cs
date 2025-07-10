using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class CusExitReportItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckERI_GrossMass()
		{
			reportItem.Validation.ValidateERI_GrossMass();
			AssertHasMessageErrorContaining("GrossMass validation for package items.", reportItem.ERI_GrossMassInfo, MandatoryValidation.ValueCannotBeZero);

			reportItem.ERI_NetMass = 100;
			reportItem.ERI_GrossMass = 90;
			var shouldBeGreaterThanOrEqualToMessage = "should be greater than or at least equal to";
			AssertHasMessageErrorContaining("GrossMass ShouldBeGreaterThanOrEqualTo NetMass validation.", reportItem.ERI_GrossMassInfo, shouldBeGreaterThanOrEqualToMessage);
			reportItem.ERI_GrossMass = 100;
			AssertNoMessageErrorContaining("GrossMass ShouldBeGreaterThanOrEqualTo NetMass validation(passes).", reportItem.ERI_GrossMassInfo, shouldBeGreaterThanOrEqualToMessage);
		}

		public void TestCheckERI_NetMass()
		{
			reportItem.Validation.ValidateERI_NetMass();
			AssertHasMessageErrorContaining("GrossMass validation for package items.", reportItem.ERI_NetMassInfo, MandatoryValidation.ValueCannotBeZero);

			reportItem.ERI_GrossMass = 90;
			reportItem.ERI_NetMass = 100;
			var shouldBeGreaterThanOrEqualToMessage = "should be greater than or at least equal to";
			AssertHasMessageErrorContaining("GrossMass ShouldBeGreaterThanOrEqualTo NetMass validation.", reportItem.ERI_NetMassInfo, shouldBeGreaterThanOrEqualToMessage);
			reportItem.ERI_NetMass = 90;
			AssertNoMessageErrorContaining("GrossMass ShouldBeGreaterThanOrEqualTo NetMass validation(passes).", reportItem.ERI_NetMassInfo, shouldBeGreaterThanOrEqualToMessage);
		}

		public void TestCheckERI_Quantity()
		{
			var targetInfo = reportItem.ERI_QuantityInfo;
			Factory.SetupBreakBulkCusCode();
			Factory.SetupBulkCusCode();

			CombineAssertions(() =>
			{
				reportItem.Package.CXP_PackageType = "NE";
				reportItem.ERI_Quantity = 0;
				reportItem.Validation.ValidateERI_Quantity();
				AssertHasMessageErrorContaining("Check for ZERO", targetInfo, MandatoryValidation.ValueCannotBeZero);

				reportItem.Package.CXP_PackageType = "VG";
				reportItem.ERI_Quantity = 0;
				reportItem.Validation.ValidateERI_Quantity();
				AssertNoMessageErrorContaining("Check for ZERO", targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining("Mandatory ZERO", targetInfo, "Package quantity must be zero when package type is bulk");
				reportItem.ERI_Quantity = 1;
				AssertHasMessageErrorContaining("Mandatory ZERO", targetInfo, "Package quantity must be zero when package type is bulk");

				reportItem.ERI_Quantity = 0;
				reportItem.Package.CXP_PackageType = "BX";
				reportItem.Validation.ValidateERI_Quantity();
				AssertNoMessageErrors("Validation passes for Positive", targetInfo);
				reportItem.ERI_Quantity = -1;
				AssertHasErrorContaining("Check for Negative", targetInfo, MandatoryValidation.ValueCannotBeNegative);
				reportItem.ERI_Quantity = 1;
				AssertNoMessageErrors("Validation passes for Positive", targetInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			(reportItem, _, consignmentItem, _, _) = CusExitReportItemTest.GetNewBusinessObject(Factory);
			var package = consignmentItem.CusExitConsignmentPackagePivots.AddNew().Package;
			reportItem.ERI_CXP_Package = package.PK;
		}
		CusExitConsignmentItem consignmentItem;
		CusExitReportItem reportItem;
	}
}
