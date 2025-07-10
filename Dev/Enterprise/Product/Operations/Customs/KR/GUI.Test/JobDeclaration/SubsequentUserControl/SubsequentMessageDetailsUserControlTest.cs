using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SubsequentMessageDetailsUserControl))]
	sealed class SubsequentMessageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestSubsequentMessageDetailsUserControl()
		{
			using (var control = new ImportMessageUserControl())
			{
				var subsequentTabControl = control.Controls.Find("SubsequentTabControl", true)[0];
				AssertNotNull(subsequentTabControl);
				var details = subsequentTabControl.Controls.Find("SubsequentMessageDetailsUserControl", true)[0];
				var valueDeclaration = details.Controls.Find("ValuationDeclarationGroupBox", true)[0];
				AssertNotNull(valueDeclaration.Controls.Find("MessageStatus934DropEdit", true)[0]);
				AssertNotNull(valueDeclaration.Controls.Find("ValuationMethodDropEdit", true)[0]);
				AssertNotNull(valueDeclaration.Controls.Find("NoticeNumberTextBox", true)[0]);
				AssertNotNull(valueDeclaration.Controls.Find("AcceptedDate934DateEdit", true)[0]);
				AssertNotNull(valueDeclaration.Controls.Find("EstimatedDateOfFinalPriceDateEdit", true)[0]);

				var cancellationOfImportDeclaration = details.Controls.Find("CancellationOfImportDeclarationGroupBox", true)[0];
				AssertNotNull(cancellationOfImportDeclaration.Controls.Find("MessageStatus5BFDropEdit", true)[0]);
				AssertNotNull(cancellationOfImportDeclaration.Controls.Find("DecisionDateDateEdit", true)[0]);
				AssertNotNull(cancellationOfImportDeclaration.Controls.Find("CancellationReasonTextBox", true)[0]);
				AssertNotNull(cancellationOfImportDeclaration.Controls.Find("AcceptedDate5BFDateEdit", true)[0]);
				AssertNotNull(cancellationOfImportDeclaration.Controls.Find("ReviewResult5BFDropEdit", true)[0]);

				var goodsRemovalPriorToCustomsRelease = details.Controls.Find("GoodsRemovalPriorToCustomsReleaseGroupBox", true)[0];
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("MessageStatus5BDDropEdit", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("ReviewResult5BDDropEdit", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("SecurityTypeDropEdit", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("AcceptedDate5BDDateEdit", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("RequestReasonTextBox", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("SecurityPeriodStartDateEdit", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("SecurityPeriodEndDateEdit", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("ReviewDateDateEdit", true)[0]);
				AssertNotNull(goodsRemovalPriorToCustomsRelease.Controls.Find("SecurityAmountCalcEdit", true)[0]);

				var goldVATDeclaration = details.Controls.Find("GoldVATDeclarationGroupBox", true)[0];
				AssertNotNull(goldVATDeclaration.Controls.Find("AcceptedDate5TMDateEdit", true)[0]);
				AssertNotNull(goldVATDeclaration.Controls.Find("MessageStatus5TMDropEdit", true)[0]);
			}
		}
		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew();
		}
	}
}
