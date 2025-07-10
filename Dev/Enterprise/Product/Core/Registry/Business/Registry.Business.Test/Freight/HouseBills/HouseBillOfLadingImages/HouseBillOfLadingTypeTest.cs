using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillOfLadingType))]
	sealed class HouseBillOfLadingTypeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestGetTermsAndConditionsImage()
		{
			SetHouseBillOfLadingTermsAndConditionsImagesValue();

			BizObj.TermsAndConditionsCode = "";
			AssertEquals("GetTermsAndConditionsImage() should return null if TermsAndConditionsCode is invalid.", null, BizObj.GetTermsAndConditionsImage(nameof(PrintCopyType.ALL)));

			BizObj.TermsAndConditionsCode = "XYZ";
			HouseBillOfLadingTermsAndConditions termsAndConditions = BizObj.GetTermsAndConditionsImage(nameof(PrintCopyType.EML));
			AssertEquals("TermsAndConditionsImage.Description", "RDY Description", termsAndConditions.Description);
		}

		public void TestSettingCurrentFallbackLevelClearsImageCodeLists()
		{
			CodeDescriptionPairList initialLogoImageCodeList = BizObj.LogoImageCodeList;
			CodeDescriptionPairList initialTermsAndConditionImageCodeList = BizObj.TermsAndConditionImageCodeList;

			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			Assert("LogoImageCodeList should have changed.", BizObj.LogoImageCodeList != initialLogoImageCodeList);
			Assert("TermsAndConditionImageCodeList should have changed.", BizObj.TermsAndConditionImageCodeList != initialTermsAndConditionImageCodeList);
		}

		public void TestValidateDescription()
		{
			AssertNoErrors("Precondition: Description should not have errors.", BizObj.DescriptionInfo);

			BizObj.Description = (NoResString)"";
			AssertHasError(BizObj.DescriptionInfo, "Please enter a Description.");

			BizObj.Description = (NoResString)"123";
			AssertNoErrors(BizObj.DescriptionInfo);
		}

		public void TestLogoImageAndCodeList()
		{
			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			SetHouseBillOfLadingLogoImagesProposedValue();

			AssertEquals("LogoImageCodeList.GetDescriptionFromCode(\"ABC\")", "ABC Description", BizObj.LogoImageCodeList.GetDescriptionFromCode("ABC"));

			BizObj.LogoCode = "ABC";
			AssertEquals("LogoImage", null, BizObj.LogoImage);

			FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK,
				(RegistryImageCollection)((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingLogoImages).GetProposedValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			AssertEquals("LogoImage.Description", "ABC Description", BizObj.LogoImage.Description);

			BizObj.LogoCode = "!@#";
			AssertEquals("LogoImage", null, BizObj.LogoImage);
		}

		public void TestTermsAndConditionsImageAndCodeList()
		{
			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			SetHouseBillOfLadingTermsAndConditionsImagesProposedValue();

			AssertEquals("TermsAndConditionImageCodeList.GetDescriptionFromCode(\"XYZ\")", "XYZ Description", BizObj.TermsAndConditionImageCodeList.GetDescriptionFromCode("XYZ"));

			BizObj.TermsAndConditionsCode = "XYZ";
			AssertEquals("TermsAndConditionsImage", null, BizObj.TermsAndConditionsImage);

			FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK,
				(HouseBillOfLadingTermsAndConditionsCollection)((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages).GetProposedValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			AssertEquals("TermsAndConditionsImage.Description", "XYZ Description", BizObj.TermsAndConditionsImage.Description);

			BizObj.TermsAndConditionsCode = "!@#";
			AssertEquals("TermsAndConditionsImage", null, BizObj.TermsAndConditionsImage);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

			BizObj.TermsAndConditionsCode = "!@#";
			BizObj.LogoCode = "!@#";
			BizObj.PrintLogoInFormBuilder = "!@#";

			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.TermsAndConditionsCodeInfo);
			AssertHasErrors(BizObj.PrintLogoInFormBuilderInfo);

			BizObj.TermsAndConditionsCode = ZString.Empty;
			BizObj.LogoCode = ZString.Empty;
			BizObj.PrintLogoInFormBuilder = PrintLogoOptions.Codes.All;

			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.TermsAndConditionsCodeInfo);
			AssertNoErrors(BizObj.LogoCodeInfo);
			AssertNoErrors(BizObj.PrintLogoInFormBuilderInfo);
		}

		public void TestValidateTermsAndConditionsCode()
		{
			BizObj.TermsAndConditionsCode = "!@#";
			AssertNoErrors(BizObj.TermsAndConditionsCodeInfo);

			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			SetHouseBillOfLadingTermsAndConditionsImagesProposedValue();

			BizObj.TermsAndConditionsCode = "!@#";
			AssertHasError(BizObj.TermsAndConditionsCodeInfo, "Enter a valid selection.");

			BizObj.TermsAndConditionsCode = "XYZ";
			AssertNoErrors(BizObj.TermsAndConditionsCodeInfo);

			BizObj.TermsAndConditionsCode = "";
			AssertNoErrors(BizObj.TermsAndConditionsCodeInfo);
		}

		public void TestValidateLogoCode()
		{
			BizObj.LogoCode = "!@#";
			AssertNoErrors(BizObj.LogoCodeInfo);

			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			SetHouseBillOfLadingLogoImagesProposedValue();

			BizObj.LogoCode = "!@#";
			AssertHasError(BizObj.LogoCodeInfo, "Enter a valid selection.");

			BizObj.LogoCode = "ABC";
			AssertNoErrors(BizObj.LogoCodeInfo);

			BizObj.LogoCode = "";
			AssertNoErrors(BizObj.LogoCodeInfo);
		}

		public void TestValidatePrintLogoInFormBuilder()
		{
			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

			BizObj.PrintLogoInFormBuilder = PrintLogoOptions.Codes.None;
			AssertNoErrors(BizObj.PrintLogoInFormBuilderInfo);

			BizObj.PrintLogoInFormBuilder = "ZZZ";
			AssertHasError(BizObj.PrintLogoInFormBuilderInfo, "Enter a valid selection.");

			BizObj.PrintLogoInFormBuilder = PrintLogoOptions.Codes.All;
			AssertNoErrors(BizObj.PrintLogoInFormBuilderInfo);

			BizObj.PrintLogoInFormBuilder = PrintLogoOptions.Codes.Original;
			AssertNoErrors(BizObj.PrintLogoInFormBuilderInfo);

			BizObj.PrintLogoInFormBuilder = PrintLogoOptions.Codes.Copy;
			AssertNoErrors(BizObj.PrintLogoInFormBuilderInfo);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();

			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingLogoImages).ClearCurrentValueToUseCache();
			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingLogoImages).ClearProposedCache();
			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingLogoImages).ClearCache();

			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages).ClearCurrentValueToUseCache();
			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages).ClearProposedCache();
			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages).ClearCache();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new HouseBillOfLadingType(new FallbackLevel(Guid.NewGuid(), Guid.Empty, Guid.Empty))
			{
				Code = "ABC",
				Description = (NoResString)"ABC Description",
				LogoCode = "XYZ",
				TermsAndConditionsCode = "POP",
				PrePrinted = true,
				PrintLogo = false,
				PrintLogoInFormBuilder = PrintLogoOptions.Codes.None
			};
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		new HouseBillOfLadingType BizObj
		{
			get { return (HouseBillOfLadingType)base.BizObj; }
		}

		void SetHouseBillOfLadingLogoImagesProposedValue()
		{
			RegistryImageCollection collection = new RegistryImageCollection();

			RegistryImage houseBillOfLadingImage = collection.AddNew();
			houseBillOfLadingImage.Code = "ABC";
			houseBillOfLadingImage.Description = (NoResString)"ABC Description";
			houseBillOfLadingImage.Image = new Bitmap(1, 1);

			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingLogoImages).SetProposedValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, collection);
			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingLogoImages).SetCurrentValueToUse(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, ValueToUse.ProposedValue);
		}

		void SetHouseBillOfLadingTermsAndConditionsImagesValue()
		{
			FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, GetHouseBillOfLadingTermsAndConditionsImagesTestValue());
		}

		void SetHouseBillOfLadingTermsAndConditionsImagesProposedValue()
		{
			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages).SetProposedValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, GetHouseBillOfLadingTermsAndConditionsImagesTestValue());
			((IRegistryItemInternals)FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages).SetCurrentValueToUse(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, ValueToUse.ProposedValue);
		}

		HouseBillOfLadingTermsAndConditionsCollection GetHouseBillOfLadingTermsAndConditionsImagesTestValue()
		{
			HouseBillOfLadingTermsAndConditionsCollection result = new HouseBillOfLadingTermsAndConditionsCollection();

			HouseBillOfLadingTermsAndConditions termsAndConditions1 = result.AddNew();
			HouseBillOfLadingTermsAndConditions termsAndConditions2 = result.AddNew();

			termsAndConditions1.Code = "XYZ";
			termsAndConditions1.Description = (NoResString)"XYZ Description";
			termsAndConditions1.DeliveryMode = nameof(PrintCopyType.ALL);
			termsAndConditions1.Image = new Bitmap(1, 1);

			termsAndConditions2.Code = "XYZ";
			termsAndConditions2.Description = (NoResString)"RDY Description";
			termsAndConditions2.DeliveryMode = nameof(PrintCopyType.EML);
			termsAndConditions2.Image = new Bitmap(1, 1);

			return result;
		}

		#endregion
	}
}
