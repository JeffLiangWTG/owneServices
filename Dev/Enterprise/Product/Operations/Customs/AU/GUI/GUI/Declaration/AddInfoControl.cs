using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AddInfoControl : BaseAddInfoControl
	{
		public AddInfoControl()
			: base()
		{
		}

		#region Implementation

		protected virtual bool IsCMR
		{
			get
			{
				if (CurrentAddInfo.JobDeclaration != null)
				{
					return CurrentAddInfo.JobDeclaration.IsImportCMR;
				}
				else
				{
					return new CMRUtilities().AreWeRunningInCMR(ZDateTime.Now);
				}
			}
		}

		protected override void AddInfoButton_Click(object sender, System.EventArgs e)
		{
			ShowAddInfoForm();
		}

		void ShowAddInfoForm()
		{
			if (CurrentAddInfo != null)
			{
				if (IsCMR)
				{
					CMRAddInfoForm.ShowForm(FindForm(), CurrentAddInfo, new AddInfoSavedEventHandler(AddInfoForm_Closed));
				}
				else
				{
					AddInfoForm.ShowForm(FindForm(), CurrentAddInfo, new AddInfoSavedEventHandler(AddInfoForm_Closed));
				}
			}
		}

		internal void AddInfoForm_Closed(object sender, AddInfoEventArgs e)
		{
			//If you add a hidden property to the form it will not be copied back because of this.

			if (CurrentAddInfo != null)
			{
				CurrentAddInfo.ZA_SendZeroDutyOverride_Hidden = e.AddInfo.ZA_SendZeroDutyOverride_Hidden;
				CurrentAddInfo.AddInfoLine = e.AddInfo.AddInfoLine;
			}
		}

		#endregion

	}
}
