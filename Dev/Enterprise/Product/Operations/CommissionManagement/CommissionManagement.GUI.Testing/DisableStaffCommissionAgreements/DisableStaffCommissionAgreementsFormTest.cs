using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(DisableStaffCommissionAgreementsForm))]
	class DisableStaffCommissionAgreementsFormTest : ZFormBasherTest
	{
		#region Form Caption

		public void TestFormVerb()
		{
			using (var form = (DisableStaffCommissionAgreementsForm)GetFormToBash())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		#endregion

		#region Buttons

		[TestDate(2002, 2, 2)]
		public void TestYesButton()
		{
			var staff = Factory.New<GlbStaff>();
			var action = DisableStaffCommissionAgreementsAction.New(staff);
			var strings = new DisableStaffCommissionAgreementsFormStrings(ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty);

			using (var form = new DisableStaffCommissionAgreementsForm(action, strings))
			{
				form.Show();

				var formClosed = false;
				form.FormClosed += (sender, e) =>
					{
						formClosed = true;
					};

				action.Date = ZDateTime.Empty;

				form.YesButton.PerformClick();
				AssertEquals("Errors!", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(false, formClosed);

				action.Date = new ZDateTime(2002, 2, 2);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.YesButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(true, formClosed);
			}
		}

		[TestDate(2002, 2, 2)]
		public void TestYesAndApproveButton()
		{
			var staff = Factory.New<GlbStaff>();
			var action = DisableStaffCommissionAgreementsAction.New(staff);
			var strings = new DisableStaffCommissionAgreementsFormStrings(ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty);

			using (var form = new DisableStaffCommissionAgreementsForm(action, strings))
			{
				form.Show();

				var formClosed = false;
				form.FormClosed += (sender, e) =>
				{
					formClosed = true;
				};

				Env.Security.CommissionAgreementApproval.IsAllowed = false;
				form.YesAndApproveButton.PerformClick();
				AssertEquals(Env.Security.CommissionAgreementApproval.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, formClosed);

				Env.Security.CommissionAgreementApproval.IsAllowed = true;
				action.Date = ZDateTime.Empty;

				form.YesAndApproveButton.PerformClick();
				AssertEquals("Errors!", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(false, formClosed);

				action.Date = new ZDateTime(2002, 2, 2);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.YesAndApproveButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(true, formClosed);
			}
		}

		[TestDate(2002, 2, 2)]
		public void TestYesAndApproveButton_ConcurrencyException()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_LastApprovedDateUtc = new ZDateTime(2001, 1, 1);
			var recipient = OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement, staff, 5);
			Factory.Save();

			var action = DisableStaffCommissionAgreementsAction.New(staff);
			var strings = new DisableStaffCommissionAgreementsFormStrings(ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty);

			using (var form = new DisableStaffCommissionAgreementsForm(action, strings))
			{
				form.Show();

				var otherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(otherFactory))
				{
					var agreementInOtherFactory = otherFactory.Load<OrgCommissionAgreement>(agreement.PK);
					agreementInOtherFactory.Approve();
					otherFactory.Save();
				}

				var formClosed = false;
				form.FormClosed += (sender, e) =>
				{
					formClosed = true;
				};

				action.Date = new ZDateTime(2002, 2, 2);

				form.YesAndApproveButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "WARNING", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("formClosed", true, formClosed);
				});
			}
		}

		public void TestNoButton()
		{
			var staff = Factory.New<GlbStaff>();
			var action = DisableStaffCommissionAgreementsAction.New(staff);
			var strings = new DisableStaffCommissionAgreementsFormStrings(ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty, ResourceStringData.Empty);

			using (var form = new DisableStaffCommissionAgreementsForm(action, strings))
			{
				form.Show();

				var formClosed = false;
				form.FormClosed += (sender, e) =>
				{
					formClosed = true;
				};

				form.NoButton.PerformClick();
				AssertEquals(true, formClosed);
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var staff = Factory.New<GlbStaff>();
			var action = DisableStaffCommissionAgreementsAction.New(staff);
			var strings = new DisableStaffCommissionAgreementsFormStrings(
				new ResourceStringData("Message", "Message"),
				new ResourceStringData("Yes", "Yes"),
				new ResourceStringData("Yes && Approve", "Yes && Approve"),
				new ResourceStringData("No", "No"));
			return new DisableStaffCommissionAgreementsForm(action, strings);
		}

		#endregion
	}
}
