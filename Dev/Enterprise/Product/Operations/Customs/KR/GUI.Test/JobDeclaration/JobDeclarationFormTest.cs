using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	abstract class JobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		static int minScreenHeightSupported => CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(796);
		static int typicalTaskbarHeight => CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

		public override void TestMinimumSizeNotTooBig()
		{
			using (var testForm = GetFormToBash())
			{
				var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);

				var maxSizeWidth = minScreenWidthSupported;
				var maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert(
					"Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(),
testForm.MinimumSize.Width <= maxSizeWidth);
				Assert(
					"Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(),
					testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}
	}

	abstract class JobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		static int minScreenHeightSupported => CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(796);
		static int typicalTaskbarHeight => CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

		public override void TestMinimumSizeNotTooBig()
		{
			using (var testForm = GetFormToBash())
			{
				var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);

				var maxSizeWidth = minScreenWidthSupported;
				var maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert(
					"Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(),
testForm.MinimumSize.Width <= maxSizeWidth);
				Assert(
					"Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(),
					testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}
	}

	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTest : JobDeclarationFormTest
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			JobDeclaration result = base.GetPopulatedDeclarationForFormBashingCore();
			var invoice = result.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice = result.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
			invoice = result.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
			invoice = result.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
			invoice = result.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
			invoice = result.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
			invoice = result.Invoices.AddNew();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodSix;
			return result;
		}
	}

	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : JobDeclarationFormTest_ForWhenDeclarationCancelled
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}

	[TestedType(typeof(JobDeclarationForm))]
	sealed class ExportJobDeclarationFormTest : JobDeclarationFormTest
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Export;
	}

	[TestedType(typeof(JobDeclarationForm))]
	sealed class ExportJobDeclarationFormTest_ForWhenDeclarationCancelled : JobDeclarationFormTest_ForWhenDeclarationCancelled
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Export;
	}
}
