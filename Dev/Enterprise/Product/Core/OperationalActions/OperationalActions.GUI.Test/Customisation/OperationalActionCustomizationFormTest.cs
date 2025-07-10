using System.Reflection;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	[TestedType(typeof(OperationalActionCustomizationForm))]
	sealed class OperationalActionCustomizationFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (OperationalActionCustomizationForm form = (OperationalActionCustomizationForm)GetFormToBash())
			{
				form.DisplayMode = ODisplayMode.Browse;
				form.Show();
				AssertEquals("Module Name - Customize Operational Actions", form.Text);
			}
		}

		public void TestDocumentsVisibility()
		{
			actionSupportable = new MockOperationalActionSupportable(typeof(DummyBusinessObjectWithDocumentSupport));

			using (OperationalActionCustomizationForm form = (OperationalActionCustomizationForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Documents tab should be visible", true, DocumentsVisibility(form));
			}

			actionSupportable = new MockOperationalActionSupportable(typeof(DummyBusinessObject));

			using (OperationalActionCustomizationForm form = (OperationalActionCustomizationForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Documents tab should not be visible", false, DocumentsVisibility(form));
			}
		}

		public void TestFieldsVisibility()
		{
			ActionSupportable.supportsBulkUpdate = true;

			using (OperationalActionCustomizationForm form = (OperationalActionCustomizationForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Fields tab should be visible", true, FieldsVisibility(form));
			}

			ActionSupportable.supportsBulkUpdate = false;

			using (OperationalActionCustomizationForm form = (OperationalActionCustomizationForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Fields tab should not be visible", false, FieldsVisibility(form));
			}
		}

		public void TestDisplayMode()
		{
			using (OperationalActionCustomizationForm form = (OperationalActionCustomizationForm)GetFormToBash())
			{
				form.Show();
				ZPostingButtonsUserControl postingButtons = (ZPostingButtonsUserControl)form.Controls.Find("postingButtons", true)[0];

				CombineAssertions(delegate
				{
					AssertEquals("Initial Display Mode", ODisplayMode.NewSaved, form.DisplayMode);
					AssertEquals("Initial: postingButtons.SaveButton.Text", "&Save", postingButtons.SaveButton.Text);
					AssertEquals("Initial: postingButtons.SaveButton.Enabled", false, postingButtons.SaveButton.Enabled);
					AssertEquals("Initial: postingButtons.SaveAndCloseButton.Text", "S&ave && Close", postingButtons.SaveAndCloseButton.Text);
					AssertEquals("Initial: postingButtons.SaveAndCloseButton.Enabled", false, postingButtons.SaveAndCloseButton.Enabled);
					AssertEquals("Initial: postingButtons.CloseButton.Text", "&Close", postingButtons.CloseButton.Text);
					AssertEquals("Initial: postingButtons.CloseButton.Enabled", true, postingButtons.CloseButton.Enabled);
				});

				form.BusinessEntity.HasChanges = true;
				CombineAssertions(delegate
				{
					AssertEquals("Display Mode With Changes", ODisplayMode.Edit, form.DisplayMode);
					AssertEquals("Changed: postingButtons.SaveButton.Text", "&Save", postingButtons.SaveButton.Text);
					AssertEquals("Changed: postingButtons.SaveButton.Enabled", true, postingButtons.SaveButton.Enabled);
					AssertEquals("Changed: postingButtons.SaveAndCloseButton.Text", "S&ave && Close", postingButtons.SaveAndCloseButton.Text);
					AssertEquals("Changed: postingButtons.SaveAndCloseButton.Enabled", true, postingButtons.SaveAndCloseButton.Enabled);
					AssertEquals("Changed: postingButtons.CloseButton.Text", "&Cancel", postingButtons.CloseButton.Text);
					AssertEquals("Changed: postingButtons.CloseButton.Enabled", true, postingButtons.CloseButton.Enabled);
				});

				form.BusinessEntity.Factory.Save();
				CombineAssertions(delegate
				{
					AssertEquals("Saved Display Mode", ODisplayMode.NewSaved, form.DisplayMode);
					AssertEquals("Saved: postingButtons.SaveButton.Text", "&Save", postingButtons.SaveButton.Text);
					AssertEquals("Saved: postingButtons.SaveButton.Enabled", false, postingButtons.SaveButton.Enabled);
					AssertEquals("Saved: postingButtons.SaveAndCloseButton.Text", "S&ave && Close", postingButtons.SaveAndCloseButton.Text);
					AssertEquals("Saved: postingButtons.SaveAndCloseButton.Enabled", false, postingButtons.SaveAndCloseButton.Enabled);
					AssertEquals("Saved: postingButtons.CloseButton.Text", "&Close", postingButtons.CloseButton.Text);
					AssertEquals("Saved: postingButtons.CloseButton.Enabled", true, postingButtons.CloseButton.Enabled);
				});
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ActionSupportable.businessContext = (BusinessContext)(-1);

			OperationalActionSupporter actionSupporter = ActionSupportable.OperationalActionSupporter;
			OperationalActionContext context = new OperationalActionContext(actionSupporter, "Module Name");
			OperationalActionManager manager = new OperationalActionManager(Factory, context);
			return new OperationalActionCustomizationForm(manager);
		}

		MockOperationalActionSupportable ActionSupportable
		{
			get { return actionSupportable ?? (actionSupportable = new MockOperationalActionSupportable()); }
		}
		MockOperationalActionSupportable actionSupportable;

		static bool DocumentsVisibility(OperationalActionCustomizationForm form)
		{
			ZTabPage documentsTabPage = (ZTabPage)typeof(CustomisationControl).InvokeMember(
				"documentsTabPage",
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance,
				null, GetCustomisationControl(form), System.Array.Empty<object>());

			return documentsTabPage.TabVisible;
		}

		static bool FieldsVisibility(OperationalActionCustomizationForm form)
		{
			ZTabPage fieldsTabPage = (ZTabPage)typeof(CustomisationControl).InvokeMember(
				"fieldsTabPage",
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance,
				null, GetCustomisationControl(form), System.Array.Empty<object>());

			return fieldsTabPage.TabVisible;
		}

		static CustomisationControl GetCustomisationControl(OperationalActionCustomizationForm form)
		{
			return (CustomisationControl)typeof(OperationalActionCustomizationForm).InvokeMember(
				"customisationControl",
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance,
				null, form, System.Array.Empty<object>());
		}

		#endregion
	}
}
