using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class AccHotChequeControllerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShowEditForm()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			Factory.Save();

			AccHotChequeController testController = new AccHotChequeController();

			cheque.AQ_Cancelled = true;
			using (IZForm form = testController.ShowEditForm(cheque))
			{
				AssertEquals("DisplayMode", ODisplayMode.ReadOnly, form.DisplayMode);
			}

			cheque.AQ_Cancelled = false;
			cheque.AQ_AH = ZGuid.NewZGuid();
			using (IZForm form = testController.ShowEditForm(cheque))
			{
				AssertEquals("DisplayMode", ODisplayMode.ReadOnly, form.DisplayMode);
			}

			cheque.AQ_AH = ZGuid.Empty;
			using (IZForm form = testController.ShowEditForm(cheque))
			{
				AssertEquals("DisplayMode", ODisplayMode.Browse, form.DisplayMode);
			}
		}

		[ExpectNoExceptions]
		public void TestShowViewForm()
		{
			Env.Security.ViewHotCheque.IsAllowed = true;
			Env.Security.EditHotCheque.IsAllowed = false;

			AccHotCheque cheque = Factory.New<AccHotCheque>();
			cheque.AQ_Cancelled = false;
			cheque.AQ_AH = ZGuid.Empty;
			Factory.Save();

			AccHotChequeController testController = new AccHotChequeController();
			using (IZForm form = testController.ShowViewForm(cheque))
			{
				Assert("No statck overflow if we got to here", true);
				AssertEquals(form.DisplayMode, ODisplayMode.ReadOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestShowDeleteForm()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			Factory.Save();

			AccHotChequeController testController = new AccHotChequeController();

			cheque.AQ_Cancelled = true;
			using (IZForm form = testController.ShowDeleteForm(cheque))
			{
				AssertNull("Form", form);
				AssertEquals("LastMessage.Text", "This cheque is already canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			cheque.AQ_Cancelled = false;
			cheque.AQ_AH = ZGuid.NewZGuid();
			using (IZForm form = testController.ShowDeleteForm(cheque))
			{
				AssertNull("Form", form);
				AssertEquals("LastMessage.Text", "This cheque is posted and cannot be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			cheque.AQ_AH = ZGuid.Empty;
			using (IZForm form = testController.ShowDeleteForm(cheque))
			{
				AssertNotNull("Form", form);
				AssertEquals("DisplayMode", ODisplayMode.Delete, form.DisplayMode);
				AssertNull("LastMessage.Text", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
