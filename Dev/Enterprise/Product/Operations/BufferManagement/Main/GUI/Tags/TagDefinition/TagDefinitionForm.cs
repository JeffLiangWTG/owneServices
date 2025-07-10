using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.Tags;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TagDefinitionForm : ZTemplateForm, INavigableTagForm
	{
		public TagDefinitionForm(TagDefinition businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;

		protected override ContinueWithSave ValidateAndSave()
		{
			var definition = CurrentDataItem as TagDefinition;

			if ((!definition.CanRuleUseTags && !definition.ApplicableToTasks) && definition.HasTagRules)
			{
				var result = Globals.Message.Show(Res.GetString("76d03867-86a2-4eba-8aef-0241ef212a9f", "The definition [{0}] has Tag Rules defined. Saving with current scope will cause these rules to stop running.", definition.DisplayText), Res.GetString("5f89068a-9cc9-4406-96bb-5c9ebd4ba024", "Invalidating Tag Rules"), MessageBoxButtons.OKCancel, DialogResult.OK);

				if (result != DialogResult.OK)
				{
					return ContinueWithSave.No;
				}
			}

			return base.ValidateAndSave();
		}

		public void NavigateToTagMagnitude(BusinessObject objectToNavigateTo)
		{
			tagDefinitionControl1.NavigateToTagMagnitude(objectToNavigateTo);
		}
	}
}
