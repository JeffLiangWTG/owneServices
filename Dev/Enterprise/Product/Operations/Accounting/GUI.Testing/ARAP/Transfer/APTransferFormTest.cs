using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(TransferForm))]
	class APTransferFormTest : TransferFormTest
	{
		public override Type transferType => typeof(APTransfer);

		public void TestHideControls()
		{
			var testTransfer = (APTransfer)Transfer.New(transferType, Factory);
			using (var testForm = new TransferForm(testTransfer))
			{
				testForm.Show();
				Assert("should be visible since not in DB", testForm.AH_Calc_FromBeforeTransferBoundCurrencyControl_ForTestOnly.GetExtension<LabelCaptionRenderer>().Visible);
				Assert("should be visible since not in DB", testForm.AH_Calc_FromAfterTransferBoundCurrencyControl_ForTestOnly.GetExtension<LabelCaptionRenderer>().Visible);
				Assert("should be visible since not in DB", testForm.AH_Calc_ToBeforeTransferBoundCurrencyControl_ForTestOnly.GetExtension<LabelCaptionRenderer>().Visible);
				Assert("should be visible since not in DB", testForm.AH_Calc_ToAfterTransferBoundCurrencyControl_ForTestOnly.GetExtension<LabelCaptionRenderer>().Visible);
			}
		}

		public void TestShowSetsFactoryContext()
		{
			var testTransfer = (APTransfer)Transfer.New(transferType, Factory);
			testTransfer.IsReverseTransaction = true;
			using (var testForm = new TransferForm(testTransfer))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				testForm.OnShown_ForTestOnly(new EventArgs());
				Assert("The Factory should contain BusinessContext.ReverseDateForm", testForm.BusinessEntity.Factory.HasContext(BusinessContext.ReverseDateForm));
			}
		}
	}
}
