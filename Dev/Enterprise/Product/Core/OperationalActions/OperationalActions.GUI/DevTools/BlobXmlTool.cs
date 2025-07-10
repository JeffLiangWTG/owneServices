using System.Text;
using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Services.OperationalActions.GUI.DevTools
{
	internal abstract class BlobXmlTool : IDevTool
	{
		protected abstract byte[] GetBlob(OperationalAction action);

		#region IDevTool Members

		public abstract string Name { get; }

		bool IDevTool.AddAsButton
		{
			get { return false; }
		}

		public void Show(Form form)
		{
			OperationalActionCustomizationForm customisationForm = form as OperationalActionCustomizationForm;
			OperationalAction action;
			string message;
			byte[] blob;

			if (customisationForm == null)
			{
				message = (NoResString)"unable to find the form";
			}
			else if ((action = customisationForm.SelectedAction) == null)
			{
				message = (NoResString)"unable to find the selected action";
			}
			else if ((blob = GetBlob(action)) == null || blob.Length == 0)
			{
				message = (NoResString)"blob field empty";
			}
			else
			{
				message = Encoding.UTF8.GetString(blob).Replace("><", ">\n<");
			}

			Globals.Message.Show(message, Name, MessageBoxButtons.OK, DialogResult.OK);
		}

		#endregion
	}
}
