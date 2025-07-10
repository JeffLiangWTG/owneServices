using System.Windows.Forms;
#if !DEBUG
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
#endif
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.DevTools
{
	public class IndexSearchAnalyzerTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		public string Name
		{
			get { return "Index Search Analyzer"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		public void Show(Form form)
		{
			Show(null as GlowIndexQueryParam);
		}

		public void Show(GlowIndexQueryParam glowIndexQueryParam)
		{
#if !DEBUG
			if (Enterprise.ZArchitecture.Modules.ClientHookLoader.Instance.Client == CargoWise.Definitions.Clients.EDI)
			{
				if (glowIndexQueryParam == null)
				{
					Globals.Message.Show(Res.GetString("9362164F-2D7F-4035-B2D2-FDBFB81D8091", "There is no search index link to show on this form."));
				}
				else
				{
					Globals.Message.Show(GlowRegistry.Instance.GlowServiceUri.TrimEnd('/') + '/' + glowIndexQueryParam.GetQueryUri(glowIndexQueryParam.MaxQueryResults));
				}
				return;
			}
#endif
			new IndexSearchAnalyzerForm(glowIndexQueryParam).Show();
		}
	}
}
