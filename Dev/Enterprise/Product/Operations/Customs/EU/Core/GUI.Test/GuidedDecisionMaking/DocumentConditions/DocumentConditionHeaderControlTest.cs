using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class DocumentConditionHeaderControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var condition = new GuidedDecisionMakingCondition();
			condition.ConditionType = "410";
			condition.ConditionTypeDescription = "Veterinary Control";
			condition.ConditionSatisfactionType = "Condition Type Y: Other Conditions";
			var conditionDetail = condition.ConditionDetails.AddNew();
			conditionDetail.Code = "COD1";
			conditionDetail.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
			conditionDetail.IsTicked = true;
			condition.InformationValue = ZString.Empty;

			using (var form = new ZForm())
			using (var control = new DocumentConditionHeaderControl(condition))
			{
				form.Controls.Add(control);
				form.Show();

				var descriptionLabel = control.FindSingle<ZLabel>("ConditionDescriptionLabel");
				var isSatisfiedLabel = control.FindSingle<ZLabel>("IsSatisfiedLabel");
				var isSatisfiedResultLabel = control.FindSingle<ZLabel>("IsSatisfiedResultLabel");

				Assert("Prerequisite: condition.IsSatisfied should be true.", condition.IsSatisfied);
				CombineAssertions(() =>
				{
					AssertEquals("IsSatisfiedLabel and IsSatisfiedResultLabel are horizontally aligned.", isSatisfiedLabel.Location.Y, isSatisfiedResultLabel.Location.Y);
					AssertEquals("The text of ConditionDescriptionLabel should be mapped to GuidedDecisionMakingCondition.ConditionDescription.", "410 - Veterinary Control\r\nDescription: Condition Type Y: Other Conditions", descriptionLabel.Text);
					AssertEquals("The text of IsSatisfiedLabel should be constant 'Is Satisfied:'.", "Condition Satisfied:", isSatisfiedLabel.Text);
					AssertEquals("The text of IsSatisfiedResultLabel should be mapped to GuidedDecisionMakingCondition.IsSatisfiedTextForBinding.", "YES", isSatisfiedResultLabel.Text);
					AssertEquals("The color of IsSatisfiedResultLabel should be Green the text is 'YES'.", Color.Green, isSatisfiedResultLabel.ForeColor);
					Assert("Text in IsSatisfiedResultLabel should be bold.", isSatisfiedResultLabel.IsFontBold);
				});

				conditionDetail.IsTicked = false;
				Assert("Prerequisite: condition.IsSatisfied should be false.", !condition.IsSatisfied);
				CombineAssertions(() =>
				{
					AssertEquals("IsSatisfiedLabel and IsSatisfiedResultLabel are horizontally aligned.", isSatisfiedLabel.Location.Y, isSatisfiedResultLabel.Location.Y);
					AssertEquals("The text of IsSatisfiedResultLabel should be mapped to GuidedDecisionMakingCondition.IsSatisfiedTextForBinding.", "NO", isSatisfiedResultLabel.Text);
					AssertEquals("The color of IsSatisfiedResultLabel should be Red the text is 'NO'.", Color.Red, isSatisfiedResultLabel.ForeColor);
				});

				condition.InformationValue = "INF";
				conditionDetail.IsTicked = true;
				Assert("Prerequisite: condition.IsSatisfied should be true.", condition.IsSatisfied);
				CombineAssertions(() =>
				{
					AssertEquals("IsSatisfiedLabel and IsSatisfiedResultLabel are horizontally aligned.", isSatisfiedLabel.Location.Y, isSatisfiedResultLabel.Location.Y);
					AssertEquals("The text of IsSatisfiedResultLabel should be 'Optional' as the condition is InformationCondition.", "Optional", isSatisfiedResultLabel.Text);
					AssertEquals("The color of IsSatisfiedResultLabel should be Black the text is 'Optional'.", Color.Black, isSatisfiedResultLabel.ForeColor);
				});

				conditionDetail.IsTicked = false;
				Assert("Prerequisite: condition.IsSatisfied should be false.", !condition.IsSatisfied);
				CombineAssertions(() =>
				{
					AssertEquals("IsSatisfiedLabel and IsSatisfiedResultLabel are horizontally aligned.", isSatisfiedLabel.Location.Y, isSatisfiedResultLabel.Location.Y);
					AssertEquals("The text of IsSatisfiedResultLabel should be 'Optional' as the condition is InformationCondition.", "Optional", isSatisfiedResultLabel.Text);
					AssertEquals("The color of IsSatisfiedResultLabel should be Black the text is 'Optional'.", Color.Black, isSatisfiedResultLabel.ForeColor);
				});
			}
		}
	}
}
