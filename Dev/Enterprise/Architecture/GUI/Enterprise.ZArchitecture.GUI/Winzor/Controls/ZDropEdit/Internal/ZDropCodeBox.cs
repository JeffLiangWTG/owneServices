using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using WinzorFramework.Telemetry;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class ZDropCodeBox
	{
		protected override bool AutoComplete => true;

		protected virtual void OnInputChanged(string value)
		{
			using var trace = TelemetryService.ActivitySource.StartActivity($"{nameof(ZDropCodeBox)}.{nameof(OnInputChanged)}");

			ParentDropEdit.UpdateDropDown();
		}

		protected override void OnInput(string value)
		{
			base.OnInput(value);
			OnInputChanged(value);
		}

		protected override List<string> LoadSuggestions()
		{
			var suggestions = new List<string>();
			if (Parent is ZDropEdit && ParentDropEdit != null)
			{
				using var trace = TelemetryService.ActivitySource.StartActivity($"{nameof(ZDropCodeBox)}.{nameof(LoadSuggestions)}");

				IList codeDescs = null;
				using (ParentDropEdit.PreventErrorReportForMissingListAttribute())
				{
					if (ParentDropEdit.List != null)
					{
						var listCount = ParentDropEdit.List.Count;
						trace?.AddTag("DropEditName", ParentDropEdit.Name);
						trace?.AddTag("ListCount", listCount);
					}
					codeDescs = ParentDropEdit.List;
				}

				if (codeDescs != null)
				{
					foreach (var obj in codeDescs)
					{
						ICodeDescription codeDesc = obj as ICodeDescription;
						if (codeDesc == null)
						{
							continue;
						}
						var busObj = codeDesc as BusinessObject;
						if ((busObj?.IsDeleted ?? false))
						{
							continue;
						}

						var val = ParentDropEdit.GetMultilingualValue(codeDesc).ToString();
						if (IEnumerableExtensions.IsNullOrEmpty(val))
						{
							continue;
						}
						suggestions.Add(val);
					}
				}
			}
			return suggestions;
		}
	}
}
