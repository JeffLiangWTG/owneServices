using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(ProfessionalServicesQuoteForm))]
	public class ProfessionalServicesQuoteFormTest : ZFormBasherTest
	{
		public void TestRelatedItemsTabShouldContainUnifiedControlsForRelatedItems()
		{
			using (var form = (ProfessionalServicesQuoteForm)GetFormToBashCore())
			{
				form.Show();
				var relatedTab = form.TopLevelTabControl_Exposed.GetTabPageByNameOrText("Related Items");
				AssertNotNull(relatedTab);
				form.TopLevelTabControl_Exposed.SelectedTab = relatedTab;
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("RelatedItemGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("NetworkDiagramGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ParentWorkflowGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ChildWorkflowGroupBox"));
			}
		}

		#region ChangeClientLabelColor
		public void TestChangeClientLabelColor()
		{
			ProfessionalServicesQuote incident = Factory.New<ProfessionalServicesQuote>();
			using (ProfessionalServicesQuoteForm form = new ProfessionalServicesQuoteForm(incident))
			{
				form.Show();
				Control[] controls = form.Controls.Find("ClientLabel", true);
				AssertEquals(1, controls.Length);
				OrgAddress address1 = Factory.NewWithValidTestData<OrgAddress>();
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoicingPreferences.Description, "");
				address1.OA_OH = org.PK;
				incident.IM_OA_BranchAddress = address1.PK;
				AssertEquals(Color.Red, ((ZLabel)controls[0]).ForeColor);
				OrgAddress address2 = Factory.NewWithValidTestData<OrgAddress>();
				address2.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				incident.IM_OA_BranchAddress = address2.PK;
				AssertEquals(Color.Black, ((ZLabel)controls[0]).ForeColor);
			}
		}

		#endregion
		#region ReadOnly
		public void TestReadOnly()
		{
			ProfessionalServicesQuote incident = Factory.New<ProfessionalServicesQuote>();
			using (ProfessionalServicesQuoteForm form = new ProfessionalServicesQuoteForm(incident))
			{
				form.Show();
				Control[] email = form.Controls.Find("ContactEmailTextBox", true);
				AssertEquals(true, email[0].GetReadOnly());
				Control[] phone = form.Controls.Find("ContactNumberTextBox", true);
				AssertEquals(true, phone[0].GetReadOnly());
			}
		}

		#endregion
		#region Convert to Work Item
		public void TestConvertToWorkItemButton()
		{
			ProfessionalServicesQuote quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			using (DummyProfessionalServicesQuoteForm form = new DummyProfessionalServicesQuoteForm(quote))
			{
				form.Show();
				Factory.Save();
				AssertNoExceptionThrown("No need to have accessed the Related Items Tab Page to convert to WI", () => form.ConvertToWorkItemButton.PerformClick());
				using (NewWorkItemForm workItemForm = (NewWorkItemForm)form.RelatedItemsUserControl.LastController.LastShownForm)
				{
					AssertEquals("workItemForm.BusinessEntity.HasChanges", true, workItemForm.DataSource.HasChanges);
				}

				form.RelatedItemsUserControl.LastController.LastShownForm.Dispose();
			}
		}

		#endregion
		public void TestBusinessEntityIsNull()
		{
			var quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			var job = new JobHeader.Loader(quote).TryCreate();
			using (var form = new DummyProfessionalServicesQuoteForm(quote))
			{
				form.Show();
				Factory.Save();
				form.SetDataBinding(null, null);
				job.JH_OA_LocalChargesAddr = Guid.NewGuid();
				AssertNull("BusinessEntity is null", form.BusinessEntity);
			}
		}

		#region Edit Contact
		public void TestEditContact()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			ProfessionalServicesQuote incident = Factory.New<ProfessionalServicesQuote>();
			using (ProfessionalServicesQuoteForm form = new ProfessionalServicesQuoteForm(incident))
			{
				form.Show();
				AssertEquals("Precondition: LastShownForm should be null.", null, form.LastShownForm);
				form.EditContactButton.PerformClick();
				AssertEquals("LastShownForm should be null.", null, form.LastShownForm);
				incident.IM_OC_Contact = contact.PK;
				form.EditContactButton.PerformClick();
				AssertNotNull("LastShownForm should not be null.", form.LastShownForm);
				using (ZOrganisationsForm contactForm = (ZOrganisationsForm)form.LastShownForm)
				{
					AssertNotNull("ContactController.LastShownForm should not be null", contactForm);
				}
			}
		}

		#endregion
		#region Defaults
		public void TestFormDefaults()
		{
			using (DummyProfessionalServicesQuoteForm form = GetNewForm())
			{
				form.Show();
				AssertEquals("FormCaption", "Professional Services Quote", form.FormCaption);
				AssertEquals("NotesTabPage.RemoveDescriptionColumn", false, form.NotesTabPage.RemoveDescriptionColumn);
			}
		}

		#endregion
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			ProfessionalServicesQuoteForm form = new ProfessionalServicesQuoteForm(Factory.New<ProfessionalServicesQuote>());
			form.ControllerID = ClientControllerRegistration.ProfessionalServicesQuote;
			return form;
		}

		DummyProfessionalServicesQuoteForm GetNewForm()
		{
			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();
			return new DummyProfessionalServicesQuoteForm(quote);
		}

		#region class DummyProfessionalServicesQuoteForm
		class DummyProfessionalServicesQuoteForm : ProfessionalServicesQuoteForm
		{
			public DummyProfessionalServicesQuoteForm(ProfessionalServicesQuote professionalServicesQuote) : base(professionalServicesQuote)
			{
			}

			public new ZTemplateTabControl MainTabControl
			{
				get
				{
					return base.MainTabControl;
				}
			}

			public new ZTabPage RelatedItemsTabPage
			{
				get
				{
					return base.RelatedItemsTabPage;
				}
			}

			public new ZStmNoteTabPage NotesTabPage
			{
				get
				{
					return base.NotesTabPage;
				}
			}

			public new ProfessionalServicesQuote BusinessEntity
			{
				get
				{
					return base.BusinessEntity;
				}
			}

			public new void ConvertToWorkItem()
			{
				base.ConvertToWorkItem();
			}

			public new EDIWorkTaskRelatedItemUserControl RelatedItemsUserControl
			{
				get
				{
					return base.RelatedItemsUserControl;
				}
			}
		}
		#endregion
		#endregion
	}
}
