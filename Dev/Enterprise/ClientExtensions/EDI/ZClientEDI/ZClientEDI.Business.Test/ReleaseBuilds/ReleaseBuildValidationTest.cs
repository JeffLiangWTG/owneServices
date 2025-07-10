using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business.Test
{
	class ReleaseBuildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHL_ReleaseStatus()
		{
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_ReleaseStatus = "";
			AssertHasError(build.HL_ReleaseStatusInfo, "Please enter a Status.");

			build.HL_ReleaseStatus = "ABC";
			AssertHasError(build.HL_ReleaseStatusInfo, "Enter a valid Status.");

			build.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			AssertNoErrors(build.HL_ReleaseStatusInfo);

			ReleaseBuild anotherReleaseBuild = ReleaseBuild.NewForTesting(Factory, "X", true);
			anotherReleaseBuild.HL_Product = ZString.Empty;
			anotherReleaseBuild.HL_ReleaseStatus = "";
			AssertNoErrors(anotherReleaseBuild.HL_ReleaseStatusInfo);

			anotherReleaseBuild.HL_ReleaseStatus = "";
			AssertNoErrors(anotherReleaseBuild.HL_ReleaseStatusInfo);
		}

		public void TestCheckHL_Superceded()
		{
			var baseBuild = ReleaseBuild.NewForTesting(Factory, "x", false);
			baseBuild.HL_Product = ProductTypes.Codes.Enterprise;

			baseBuild.Validation.ValidateAll();
			AssertNoErrors(baseBuild.HL_SupercededInfo);

			var firstSuperseded = ReleaseBuild.NewForTesting(Factory, "x", false);
			firstSuperseded.HL_Product = ProductTypes.Codes.Enterprise;
			firstSuperseded.HL_MajorVersion = 1;

			baseBuild.Validation.ValidateAll();
			AssertNoErrors(baseBuild.HL_SupercededInfo);

			var secondSuperseded = ReleaseBuild.NewForTesting(Factory, "x", false);
			secondSuperseded.HL_Product = ProductTypes.Codes.Enterprise;
			secondSuperseded.HL_MajorVersion = 2;

			baseBuild.Validation.ValidateAll();
			AssertHasError(baseBuild.HL_SupercededInfo, "There are already two other non-superseded builds (1.0.0.0 & 2.0.0.0) on the same release ring. Please mark one of these builds as superseded first if you wish to make this a new current build.");

			secondSuperseded.HL_Superceded = true;
			baseBuild.Validation.ValidateAll();
			AssertNoErrors(baseBuild.HL_SupercededInfo);

			baseBuild.HL_Superceded = true;
			secondSuperseded.HL_Superceded = false;
			baseBuild.Validation.ValidateAll();
			AssertNoErrors(baseBuild.HL_SupercededInfo);

			baseBuild.HL_Superceded = false;
			secondSuperseded.HL_ReleaseStatus = "y";
			baseBuild.Validation.ValidateAll();
			AssertNoErrors(build.HL_SupercededInfo);
		}

		public void TestCheckExeVersion()
		{
			var list = new SystemProductCollection();

			list.AddNew("PRK", "Parker", true);
			list.AddNew("ALX", "Alexandar", true);
			list.AddNew("ACH", "Achilles", true);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, list);

			ReleaseBuild build1 = ReleaseBuild.NewForTesting(Factory, "a", true);
			build1.ExeVersion = "1.2.3.4";
			AssertNoErrors(build1.ExeVersionInfo);

			ReleaseBuild build2 = ReleaseBuild.NewForTesting(Factory, "x", true);
			build2.ExeVersion = "0.0.0.0";
			AssertHasErrors(build2.ExeVersionInfo);

			ReleaseBuild build3 = ReleaseBuild.NewForTesting(Factory, "x", true);
			build3.ExeVersion = "1.2.3133312d.0";
			AssertHasErrors(build3.ExeVersionInfo);

			ReleaseBuild build4 = Factory.NewWithValidTestData<ReleaseBuild>();
			build4.HL_Product = "PRK";
			build4.ExeVersion = "2.3.4.5";
			AssertNoErrors(build4.ExeVersionInfo);

			Factory.Save();

			ReleaseBuild build5 = Factory.NewWithValidTestData<ReleaseBuild>();
			build5.HL_Product = "PRK";
			build5.ExeVersion = "2.3.4.5";

			AssertHasErrors(build5.ExeVersionInfo);
		}

		public void TestCheck_CheckHL_Product()
		{
			var list = new SystemProductCollection();

			list.AddNew("PRK", "Parker", true);
			list.AddNew("ALX", "Alexandar", true);
			list.AddNew("ACH", "Achilles", true);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, list);

			ReleaseBuild anotherReleaseBuild = ReleaseBuild.NewForTesting(Factory, "x", true);
			anotherReleaseBuild.HL_Product = "PRK";
			AssertNoErrors(anotherReleaseBuild.HL_ProductInfo);

			ReleaseBuild yetanotherReleaseBuild = ReleaseBuild.NewForTesting(Factory, "a", true);
			yetanotherReleaseBuild.HL_Product = "";
			AssertHasErrors(yetanotherReleaseBuild.HL_ProductInfo);

			ReleaseBuild notanotherReleaseBuild = ReleaseBuild.NewForTesting(Factory, "c", true);
			notanotherReleaseBuild.HL_Product = ProductTypes.Codes.Enterprise;
			AssertNoErrors(notanotherReleaseBuild.HL_ProductInfo);

			notanotherReleaseBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;
			AssertNoErrors(notanotherReleaseBuild.HL_ProductInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			build = Factory.New<ReleaseBuild>();
		}

		ReleaseBuild build;
	}
}
