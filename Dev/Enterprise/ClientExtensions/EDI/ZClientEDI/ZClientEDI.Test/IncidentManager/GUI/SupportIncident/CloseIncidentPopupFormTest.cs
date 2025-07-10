using System.Windows.Forms;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Tools.SpellCheck.TestFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(CloseIncidentPopupForm))]
	public class CloseIncidentPopupFormTest : BaseIncidentPopupFormTest
	{
		public void TestInvalidXmlCharactersAreRemovedFromResolutionCommentTextBox()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			using (var form = new CloseIncidentPopupFormForTest(action))
			{
				form.Show();
				form.GetResolutionCommentTextBoxForTesting().Text = string.Format("Resolution\t\r\n closed by {0} and {1}", (char)0x02, (char)0x1F);
				AssertEquals("Resolution\t\r\n closed by  and ", form.GetResolutionCommentTextBoxForTesting().Text);
				form.GetResolutionCommentTextBoxForTesting().AppendText((char)0xDFFF + " Blah");
				AssertEquals("Resolution\t\r\n closed by  and  Blah", form.GetResolutionCommentTextBoxForTesting().Text);
				AssertEquals("Resolution\t\r\n closed by  and  Blah".Length, form.GetResolutionCommentTextBoxForTesting().SelectionStart);
			}
		}

		public void TestCheckSpelling()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			action.Comment = "helloo world";
			using (var form = new CloseIncidentPopupFormForTest(action))
			{
				form.Show();
				using (var spellCheckTester = new SpellCheckFormTestHelper((x) => new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, null)))
				{
					form.CloseButton_Exposed.PerformClick();
				}

				AssertEquals("hello world", form.GetResolutionCommentTextBoxForTesting().Text);
				AssertEquals("hello world", action.Comment);
			}
		}

		public void TestCheckSpellingIgnoreKnownNames()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Brruce";

			var incident = Factory.New<SupportIncident>();
			incident.EConversation.Conversation.Staff.AddNewParticipant(staff);

			var action = new SupportIncidentCloseAction(incident);

			var text = $"hello {staff.GS_FullName}";

			using (var form = new CloseIncidentPopupFormForTest(action))
			{
				form.Show();
				action.Comment = text;
				using (var spellCheckTester = new SpellCheckFormTestHelper((x) => new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, null)))
				{
					form.CloseButton_Exposed.PerformClick();
				}

				CombineAssertions(() =>
				{
					AssertEquals(text, form.GetResolutionCommentTextBoxForTesting().Text);
					AssertEquals(text, action.Comment);
				});
			}
		}

		public void TestHideModuleSelectionShouldClearSomeValue()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR3";
			var action = new SupportIncidentCloseAction(incident);
			action.Criticality = "CR8";
			action.SectionRequirementService = "CEC";
			action.ProductArea = "GEO";

			using (var form = new CloseIncidentPopupFormForTest(action))
			{
				form.Show();

				action.Criticality = "CR3";
				AssertNullOrEmpty(action.SectionRequirementService);
				AssertNullOrEmpty(action.ProductArea);
			}
		}

		public void TestResolutionMessageText()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR3";
			var action = new SupportIncidentCloseAction(incident);

			using (var form = new CloseIncidentPopupFormForTest(action))
			{
				form.Show();
				AssertEquals("Customer-facing resolution message:", form.ZLabel_Exposed.Text);
			}
		}

		public void TestPrePopulateResolutionMethod()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR4";
			incident.IM_Category = "SUP";
			var action = new SupportIncidentCloseAction(incident);
			action.PrePopulateResolutionMethod = DispositionList.Constants.Closed.SelfResolved;
			Assert(action.ActiveCloseStatusDispositionList.ContainsCode(action.PrePopulateResolutionMethod));

			using (var form = new CloseIncidentPopupFormForTest(action))
			{
				form.Show();
				AssertEquals(DispositionList.Constants.Closed.SelfResolved, form.ResolutionMethodDropEdit.CodeBox.Text);
			}

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_Priority = "CR4";
			incident2.IM_Category = "SUP";
			var action2 = new SupportIncidentCloseAction(incident2);
			action2.PrePopulateResolutionMethod = DispositionList.Constants.FeatureAccepted;
			Assert(!action2.ActiveCloseStatusDispositionList.ContainsCode(action2.PrePopulateResolutionMethod));

			using (var form = new CloseIncidentPopupFormForTest(action2))
			{
				form.Show();
				AssertNullOrEmpty(form.ResolutionMethodDropEdit.CodeBox.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			return new CloseIncidentPopupForm(action);
		}

		class CloseIncidentPopupFormForTest : CloseIncidentPopupForm
		{
			public CloseIncidentPopupFormForTest(SupportIncidentCloseAction action) : base(action)
			{
			}

			public ZTextBox GetResolutionCommentTextBoxForTesting()
			{
				return resolutionCommentTextBox;
			}

			public ZButton CloseButton_Exposed => CloseButton;
			public ZLabel ZLabel_Exposed => zLabel2;
			public ZDropEdit ResolutionMethodDropEdit => zDropEdit1;
		}
	}
}
