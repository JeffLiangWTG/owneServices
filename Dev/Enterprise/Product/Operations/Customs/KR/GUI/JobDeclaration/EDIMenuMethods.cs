using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public static class EDIMenuMethods
	{
		public static bool TryFactorySave(JobDeclaration declaration)
		{
			var res = true;
			try
			{
				declaration.Factory.Save();
			}
			catch (ZSaveException ex)
			{
				res = false;
				ZExceptionReporting.HandleSaveException(ex);
			}
			return res;
		}

		public static bool DeclarationHasEntry(JobDeclaration declaration, ZForm form)
		{
			bool result = true;
			if (!declaration.IsMergeDone || declaration.MergeManager.RequiresMerge)
			{
				if (Globals.Message.Show(GenerateEntriesMessageText, Res.GetString("572D6466-1ADC-4F21-9386-58E815EB1F1C", "Generate Entries (Merge)"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					result = declaration.DoMerge() && (!Globals.IsTest ? form.FireSaveButton() == ContinueWithSave.Yes : TryFactorySave(declaration));
				}
				else
				{
					result = false;
				}
			}
			return result;
		}
		static string GenerateEntriesMessageText => ResString.GetMultilingualString("4A6C3060-8553-467B-99E7-A00159C4752D", "Entries for this job have not been generated. Do you want to generate entries and proceed?");
	}
}
