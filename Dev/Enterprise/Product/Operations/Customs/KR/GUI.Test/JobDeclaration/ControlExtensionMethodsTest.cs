using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public sealed class ControlExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestChangeBindingPaths()
		{
			var bindingPath = "testPath.";
			using var refundDeclarationDetailUserControl = new RefundDeclarationDetailUserControl();
			refundDeclarationDetailUserControl.Controls.ChangeBindingPaths(refundDeclarationDetailUserControl.BindingSource, bindingPath);

			AssertEquals("ZTextBox Binding", $"{bindingPath}FormattedRefundDeclarationNumber", refundDeclarationDetailUserControl.FindSingle<ZTextBox>("EntryNumberTextBox").BindTo);
			AssertEquals("ZCalcEdit Binding", $"{bindingPath}TotalRefundAmount", refundDeclarationDetailUserControl.FindSingle<ZCalcEdit>("TotalRefundAmountCalcEdit").BindTo);
			AssertEquals("ZDropEdit Binding", $"{bindingPath}CRD_MessageStatus", refundDeclarationDetailUserControl.FindSingle<ZDropEdit>("MessageStatusDropEdit").BindTo);
			AssertEquals("ZDateEdit Binding", $"{bindingPath}AcceptedDate", refundDeclarationDetailUserControl.FindSingle<ZDateEdit>("AcceptedDateEdit").BindTo);
			AssertEquals("ZCodeFindBox Binding", $"{bindingPath}CRD_CustomsOffice", refundDeclarationDetailUserControl.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox").BindTo);
			AssertEquals("ZGuidFindBox Binding", $"{bindingPath}CRD_GB_Branch", refundDeclarationDetailUserControl.FindSingle<ZGuidFindBox>("BranchCodeGuidFindBox").BindTo);
			AssertEquals("ZAddressControl Binding", $"{bindingPath}CRD_OA_DeclarantAddress", refundDeclarationDetailUserControl.FindSingle<ZAddressControl>("PayerAddressControl").BindTo);

			using var importMethodTwoToThreeUserControl = new ImportMethodTwoToThreeUserControl();
			var calcFindBox = importMethodTwoToThreeUserControl.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox");
			AssertEquals(FindBoxType.Code, calcFindBox.FindBoxType);

			importMethodTwoToThreeUserControl.Controls.ChangeBindingPaths(importMethodTwoToThreeUserControl.BindingSource, bindingPath);
			AssertEquals("ZCalcFindBox Binding", $"{bindingPath}ReplaceAmount", importMethodTwoToThreeUserControl.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox").BindToAmount);
			calcFindBox = importMethodTwoToThreeUserControl.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox");
			AssertEquals("ZCalcFindBox Binding", $"{bindingPath}ReplacementCurrency", calcFindBox.BindToUnit);
			AssertEquals(FindBoxType.Code, calcFindBox.FindBoxType);
		}
	}
}
