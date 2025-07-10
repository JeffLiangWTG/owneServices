using System.Windows.Forms;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DevTools
{
	public class DataMagicTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is the name of form, derr")]
		public string Name
		{
			get { return "DataSet Form"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		public void Show(Form form)
		{
			var zWinForm = form as ZForm;

			if (zWinForm != null)
			{
				zWinForm.ShowDataMagicForm();
			}
		}
	}
}
