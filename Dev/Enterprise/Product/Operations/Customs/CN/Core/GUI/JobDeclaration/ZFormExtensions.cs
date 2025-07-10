using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public static class ZFormExtensions
	{
		static string JobNotSavedMessageText => Res.GetString("7060011C-5FC0-4A01-823D-C92C8F202474", "The Job has not yet been saved. Do you want to save and proceed?");

		static string SaveJobMessageText => Res.GetString("EA56576C-2D45-4F32-AE99-4D92A7E14367", "Save Job");

		public static bool PreSaveDeclaration(this ZForm form, BaseJobDeclaration declaration, string message = null, string caption = null)
		{
			var result = true;
			if (declaration.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(message ?? JobNotSavedMessageText, caption ?? SaveJobMessageText, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);
				if (messageBoxResult == DialogResult.Yes)
				{
					result = form.FireSaveButton() == ContinueWithSave.Yes;
				}
			}
			return result && !declaration.HasChanges;
		}
	}
}
