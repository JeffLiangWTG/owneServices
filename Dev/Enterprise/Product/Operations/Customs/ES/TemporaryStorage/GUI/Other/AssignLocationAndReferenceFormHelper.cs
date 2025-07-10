using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public static class AssignLocationAndReferenceFormHelper
	{
		public static DataToUpdateLocationAndReference GetDataFromUpdateLocationAndReferenceForm()
		{
			using (var dialog = new AssignLocationAndReferenceForm())
			{
				var answer = ZFormModaliser.ShowDialogWithoutDispose(dialog);
				return answer == DialogResult.OK ? GetDataToUpdateLocationAndReferenceFromDialog(dialog) : null;
			}
		}

		static DataToUpdateLocationAndReference GetDataToUpdateLocationAndReferenceFromDialog(AssignLocationAndReferenceForm dialog)
		{
			return new DataToUpdateLocationAndReference()
			{
				Location = dialog.LocationTextBox.Text,
				EmptyLocation = dialog.EmptyLocationCheckBox.Checked,
				Reference = dialog.ReferenceTextBox.Text,
				EmptyReference = dialog.EmptyReferenceCheckBox.Checked,
			};
		}
	}
}
