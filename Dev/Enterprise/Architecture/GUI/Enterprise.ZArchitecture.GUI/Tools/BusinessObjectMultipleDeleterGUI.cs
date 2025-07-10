using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	class BusinessObjectMultipleDeleterGUI
	{
		public BusinessObjectMultipleDeleterGUI(BusinessObject[] selectedBusinessObjects)
		{
			SelectedBusinessObjects = selectedBusinessObjects;
		}

		readonly BusinessObject[] SelectedBusinessObjects;

		public void Process(BusinessObjectMultipleDeleterAction action, bool listBizObj = true)
		{
			var deleter = new BusinessObjectMultipleDeleter(SelectedBusinessObjects);
			if (deleter.CanDelete)
			{
				var caption = string.Empty;
				switch (action)
				{
					case BusinessObjectMultipleDeleterAction.Delete:
						caption = Res.GetString("7d641e50-27c6-4107-ad77-71a92ba2f289", "Delete");
						break;
					case BusinessObjectMultipleDeleterAction.Activate:
						caption = Res.GetString("129c90c9-5873-48c5-93da-409c0dac14f8", "Activate");
						break;
					case BusinessObjectMultipleDeleterAction.Deactivate:
						caption = Res.GetString("26fcd601-93cb-4944-9310-ab4a60a0fc42", "Deactivate");
						break;
				}
				if (Globals.Message.ShowConfirmation(deleter.GetConfirmationMessage(action, listBizObj), caption, Res.GetString("0216fb0b-6f1d-4fd9-8981-2ac3338d1c55", "yes"), MessageBoxIcon.Question) == DialogResult.OK)
				{
					var message = deleter.Process(action);
					if (!string.IsNullOrEmpty(message))
					{
						Globals.Message.ShowError(message);
					}
				}
			}
			else
			{
				Globals.Message.Show(deleter.ReasonForNotAbleToDelete);
			}
		}
	}
}
