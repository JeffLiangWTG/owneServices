using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPIndicatorDeclarationsUserControlTest : TestCaseWithFactory
	{
		public void TestIndicatorText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var testForm = new ZForm(declaration))
			using (var ctr = new RFPIndicatorDeclarationsUserControl())
			{
				testForm.Controls.Add(ctr);
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(ctr, "Invoices");
				testForm.Show();
				Application.DoEvents();
				var trueAndCompleteLabel = ctr.FindSingle<ZLabel>("TrueAndCompleteLabel");
				Assert(trueAndCompleteLabel.Text.StartsWith("Do you have effective measures in place to ensure that the information contained"));
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
				Assert(trueAndCompleteLabel.Text.StartsWith("Is all the information given in this application for an export permit true and complete?"));
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
				Assert(trueAndCompleteLabel.Text.StartsWith("Is all the information given in this application for an export permit true and complete?"));
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert(trueAndCompleteLabel.Text.StartsWith("Is all the information given in this application for an export permit true and complete?"));
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
				Assert(trueAndCompleteLabel.Text.StartsWith("Is all the information given in this application for an export permit true and complete?"));
				invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				Assert(trueAndCompleteLabel.Text.StartsWith("Is all the information given in this application for an export permit true and complete?"));
			}
		}

		public void TestDeclarationOfComplianceLabelText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var header = invoiceHeader.QuarantineExDocHeader;
			using (var testForm = new ZForm(declaration))
			using (var ctrl = new RFPIndicatorDeclarationsUserControl())
			{
				testForm.Controls.Add(ctrl);
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(ctrl, "Invoices");
				testForm.Show();
				Application.DoEvents();
				var label = ctrl.FindSingle<ZLabel>("DeclarationOfComplianceLabel");
				void AssertLabelText(string produceType, string expectedTextPropertyName)
				{
					var expectedText = typeof(RFPIndicatorDeclarationsUserControl).GetProperty(expectedTextPropertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(ctrl);
					header.QH_ProduceType = string.Empty;
					header.QH_ProduceType = produceType;
					AssertEquals($"Should equal to {expectedTextPropertyName} when the produce type is {produceType}.", expectedText, label.Text);
				}

				CombineAssertions(() =>
				{
					AssertLabelText(EXDOCCommodityCodes.Codes.Dairy, "ExDocDairyComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.Eggs, "ExDocDairyComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.Fish, "ExDocDairyComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.Meat, "ExDocMeatComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.GrainsAndPlants, "ExDocGrainsAndHorticultureComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.Horticulture, "ExDocGrainsAndHorticultureComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.OtherGoods, "DefaultComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.Wool, "DefaultComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.SkinsAndHides, "DefaultComplianceMessage");
					AssertLabelText(EXDOCCommodityCodes.Codes.InedibleMeat, "DefaultComplianceMessage");
				});
			}
		}
	}
}
