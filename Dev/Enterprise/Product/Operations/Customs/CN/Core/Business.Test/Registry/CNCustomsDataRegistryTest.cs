using System.Drawing;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNCustomsDataRegistry))]
	class CNCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<CNCustomsDataRegistry>
	{
		public void TestGetColorByRemainingDays()
		{
			CombineAssertions(() =>
			{
				AssertEquals("<0", Color.Empty, CNCustomsDataRegistry.GetColorByRemainingDays(-1, CNDeclarationDeadlineWarningThreshold.ALL));
				AssertEquals("=0", Color.Empty, CNCustomsDataRegistry.GetColorByRemainingDays(0, CNDeclarationDeadlineWarningThreshold.ALL));
				AssertEquals("=1", Color.Red, CNCustomsDataRegistry.GetColorByRemainingDays(1, CNDeclarationDeadlineWarningThreshold.ALL));
				AssertEquals("=2", Color.LightSalmon, CNCustomsDataRegistry.GetColorByRemainingDays(2, CNDeclarationDeadlineWarningThreshold.ALL));
				AssertEquals("=3", Color.LightSalmon, CNCustomsDataRegistry.GetColorByRemainingDays(3, CNDeclarationDeadlineWarningThreshold.ALL));
				AssertEquals("(3, 7)", Color.LightYellow, CNCustomsDataRegistry.GetColorByRemainingDays(5, CNDeclarationDeadlineWarningThreshold.ALL));
				AssertEquals("=7", Color.LightYellow, CNCustomsDataRegistry.GetColorByRemainingDays(7, CNDeclarationDeadlineWarningThreshold.ALL));
				AssertEquals(">7", Color.Empty, CNCustomsDataRegistry.GetColorByRemainingDays(8, CNDeclarationDeadlineWarningThreshold.ALL));
			});
		}

		public void TestActivateTwoStepDeclaration()
		{
			TestGenericRegistryItem(
				ItemSet.TwoStepDeclarationActive,
				"TwoStepDeclarationActive",
				CNCustomsDataRegistry.Categories.Customs_China,
				"Enable Two-step Declaration",
				"Turning this on will show the Two-step Declaration option on CN Customs Declaration.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false
			);
		}

		public void TestEnableGenerateImportAndExportEntryOnOneDeclaration()
		{
			TestGenericRegistryItem(
				ItemSet.CNBTHFunctionActive,
				"CNBTHFunctionActive",
				CNCustomsDataRegistry.Categories.Customs_China,
				"Enable generate Import & Export entry on one Declaration",
				"If set to ‘Yes’, the system will show option BTH for Declaration Type on Declaration.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false
			);
		}

		public void TestCNSWClientSetting()
		{
			TestGenericRegistryItem(
				ItemSet.CNSWClientSetting,
				"CNSWClientApplicationSetting",
				CNCustomsDataRegistry.Categories.Customs_China,
				"Single Window Client Application Settings",
				"The settings are for Single Window Client Application which is a standalone tool installed on the client’s local machine. The tool sends and receives messages through the China Customs Single Window interface.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);
		}

		public void TestCNDocTemplateForAttachment()
		{
			TestGenericRegistryItem(
				ItemSet.CNDocTemplateForAttachment,
				"CNDocTemplateForAttachment",
				CNCustomsDataRegistry.Categories.Customs_China,
				"Document Templates for Generating Attachments",
				"This setting specifies which document templates will be used for generating attachments for entry. You can find the feature on the context menu of Entries grid on Declaration form.",
				RegistryStorageFlags.Company
			);
		}

		public void TestCNDeclarationDeadlineWarningThreshold()
		{
			TestGenericRegistryItem(
				ItemSet.CNDeclarationDeadlineWarningThresholdSetting,
				"CNDeclarationDeadlineWarningThreshold",
				CNCustomsDataRegistry.Categories.Customs_China,
				"Declaration Deadline Warning Threshold",
				"Use this registry setting to set three level thresholds for declaration deadline warning. The jobs on Customs Declarations and Customs Entries module will be displayed in different row colors according to the thresholds (the number of days before its declaration deadline). And those jobs have already been delayed for declaration will be displayed in Delayed Warning Color.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company
			);
		}

		public void TestDefaultTradeUnitPriceOnProduct()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultTradeUnitPriceOnProduct,
				"DefaultTradeUnitPriceOnProduct",
				CNCustomsDataRegistry.Categories.Customs_China,
				"Default Trade Unit Price on Product",
				"Should copy Trade Unit Price when creating a new Product from Invoice Line?",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true
			);
		}
	}
}
