using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(CLBillsSelectionDialog))]
	sealed class CLBillsSelectionDialogBasherTest : ZFormBasherTest
	{
		public void TestIsValidToSend()
		{
			var expectedMessage = @"It is likely that your message(s) will be rejected by Customs, please check these below message errors and fix them before sending.
Message Error - Reason: You have not entered a value.";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var bills = header.Bills.OfType<AsycudaBill>().Where(x => x != null).ToArray();
			var chooser = new CLMessageChooser(header, bills, MessageSubTypeCodes.Codes.Cancellation);

			using (var dlg = new CLBillsSelectionDialogForTest(chooser, "Bills"))
			{
				dlg.SelectAll();

				chooser.Reason = ZString.Empty;
				UnitTestUserNotification.Instance.AddOKAnswer();

				Assert("Should is not valid for sending message as the cancel reason is empty.", !dlg.CheckIsValidToSend());
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				chooser.Reason = "Reason";
				Assert("Should is valid for sending message.", dlg.CheckIsValidToSend());
			}
		}

		public void TestIsValidToResend()
		{
			var expectedMessage = @"It is likely that your message(s) will be rejected by Customs, please check these below message errors and fix them before sending.
Message Error - Reason: You have not entered a value.
Message Error - AmendReason: You have not entered a value.
Message Error - AmendType: You have not entered a value.";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var bills = header.Bills.OfType<AsycudaBill>().Where(x => x != null).ToArray();
			var chooser = new CLMessageChooser(header, bills, MessageSubTypeCodes.Codes.Change);

			using (var dlg = new CLBillsSelectionDialogForTest(chooser, "Bills"))
			{
				dlg.SelectAll();

				chooser.Reason = ZString.Empty;
				chooser.AmendType = ZString.Empty;
				chooser.AmendReason = ZString.Empty;
				UnitTestUserNotification.Instance.AddOKAnswer();

				Assert("Should is not valid for sending message as the cancel reason is empty.", !dlg.CheckIsValidToSend());
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				chooser.Reason = "Reason";
				chooser.AmendReason = "GRAL";
				chooser.AmendType = "M";
				Assert("Should is valid for sending message.", dlg.CheckIsValidToSend());
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";

			Factory.Save();

			var result = new CLBillsSelectionDialog(new CLMessageChooser(header, new[] { bill1, bill2 }, MessageSubTypeCodes.Codes.Original), "Bills");
			((IBusinessObjectState)result.BusinessEntity).ClearHasChangesIncludingChildren();

			return result;
		}

		sealed class CLBillsSelectionDialogForTest : CLBillsSelectionDialog
		{
			public CLBillsSelectionDialogForTest(CLMessageChooser messageChooser, string itemsType)
				: base(messageChooser, itemsType)
			{
			}

			public bool CheckIsValidToSend() => base.IsValidToSend();
		}
	}
}

