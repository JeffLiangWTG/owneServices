using System.Windows.Forms;

#if !DEBUG
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
#endif

namespace Enterprise.ZArchitecture.DevTools
{
	public class QueryAnalyserTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		public string Name
		{
			get { return "ZQuery Analyser"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		/// <param name="form">Can be null. The Query Analyser is modeless even when a parent form is passed through.</param>
		public void Show(Form form)
		{
			Show(string.Empty);
		}

		/// <param name="sqlText">The SQL script that will show on the Query Analyser form.</param>
		public void Show(string sqlText)
		{
#if !DEBUG // SuppressCodeSmell Reason = On ediProd we want to withold access to the query analyser
			if (Enterprise.ZArchitecture.Modules.ClientHookLoader.Instance.Client == CargoWise.Definitions.Clients.EDI)
			{
				if (string.IsNullOrEmpty(sqlText))
				{
					Globals.Message.Show(Res.GetString("F756AFEE-1015-4657-A3D2-B142A6361686", "There is no SQL Query to show on this form."));
				}
				else
				{
					Globals.Message.Show(sqlText); // SuppressCodeSmell Reason = Developer Only Tool
				}
				return;
			}
#endif
			new ZQueryAnalyzerForm(sqlText).Show();
		}
	}
}
