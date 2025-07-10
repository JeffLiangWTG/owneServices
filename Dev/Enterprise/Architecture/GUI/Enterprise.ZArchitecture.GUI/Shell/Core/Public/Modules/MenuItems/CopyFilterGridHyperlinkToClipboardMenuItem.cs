using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	class CopyFilterGridHyperlinkToClipboardMenuItem : ZMenuItem
	{
		internal CopyFilterGridHyperlinkToClipboardMenuItem(ZFilterGridModule module)
			: base(CopyHyperlinksToClipboardText, CreateWorkflowHyperlinkEventHandler(module))
		{
			Name = CopySelectedHyperlinksToClipboard;
		}

		internal const string CopySelectedHyperlinksToClipboard = "CopySelectedHyperlinksToClipboard";

		static EventHandler CreateWorkflowHyperlinkEventHandler(ZFilterGridModule module)
		{
			return (s, e) =>
			{
				var selectedBizos = module.GetSelectedBusinessObjects();
				if (selectedBizos.Length == 0)
				{
					Globals.Message.Show(Res.GetString("6063778a-d3c8-418b-8a75-54113feef79d", "No row selected."));
				}
				else
				{
					var urlTuples = GetUrlsFromBizos(module, selectedBizos).ToArray();

					if (urlTuples.Length > 0)
					{
						ZMenuStrategyHelper.ShortcutCreator.CopyHyperlinksToClipboard(urlTuples);
					}
					else
					{
						Globals.Message.Show(Res.GetString("9ab5d22f-e112-4224-9f37-eb0465d92aea", "No selected rows could convert to hyperlinks."));
					}
				}
			};
		}

		static IEnumerable<Tuple<ZString, ZString>> GetUrlsFromBizos(ZFilterGridModule module, params BusinessObject[] bizos)
		{
			foreach (var bizo in bizos)
			{
				var controller = module.GetNewController(bizo);
				if (controller != null)
				{
					yield return GetUrlFromBizo(controller.ID, bizo);
				}
				else
				{
					ErrorReporter.ReportOnce(string.Format("Could not find controller for bizo [{0}]", bizo.HumanReadableName));
				}
			}
		}

		internal static Tuple<ZString, ZString> GetUrlFromBizo(ControllerID controllerID, BusinessObject bizo)
		{
			return Tuple.Create(
				bizo.HumanReadableName,
				ZFormUtilities.WebHyperlinksEnabled
					? (ZString)ShowEditFormUrlHandler.Instance.CreateWebTrampolineUri(controllerID, bizo.PK)
					: (ZString)ShowEditFormUrlHandler.Instance.Create(controllerID, bizo.PK));
		}

		public static Core.MultilingualString CopyHyperlinksToClipboardText => ResString.GetMultilingualString("35037140-565a-40f2-aee8-a67f00b180e3", "Copy Hyperlinks to Clipboard");
	}
}
