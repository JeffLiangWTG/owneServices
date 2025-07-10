using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TagDefinitionControl : ZUserControl
	{
		public TagDefinitionControl()
		{
			InitializeComponent();
		}

		void TagMagnitudeGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			var magnitudes = e.Objects.Cast<TagMagnitude>().ToArray();
			if (!CanDeleteMagnitude(magnitudes))
			{
				e.Cancel = true;
				return;
			}
		}

		bool CanDeleteMagnitude(TagMagnitude[] magnitudes)
		{
			if (magnitudes.Length == 0)
			{
				return true;
			}

			var tagCodesExist = CheckMagnitudeLinksExist(magnitudes);
			if (!tagCodesExist)
			{
				return true;
			}

			var message = Res.GetString("2975f65a-44fa-4d1f-9fa1-f4d4d1ef7a91", "Tag(s) have been applied to some items. Please remove the links to those items or deactivate the tag(s) instead.");

			var caption = Res.GetString("037e6daf-93e3-4407-8efc-a8ae039b8dc6", "Error deleting Tags");
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.None);
			return false;
		}

		bool CheckMagnitudeLinksExist(TagMagnitude[] magnitudes)
		{
			var query = new ZQuery(TagLinkSchema.TGL_TGM_Magnitude, magnitudes.Select(x => x.PK));
			var factory = magnitudes[0].Factory;

			return factory.Exists(typeof(TagLink), query);
		}

		public void NavigateToTagMagnitude(BusinessObject selectedTagMagnitude)
		{
			TagMagnitudeGrid.SelectSingleElement(selectedTagMagnitude);
		}
	}
}
