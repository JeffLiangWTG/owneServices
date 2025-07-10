using System;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class ZTimeFilterControl : ZTextBoxBase
	{
		protected override IZType SelectedValue
		{
			get => GetDateFromText();
			set => Text = GetTextFromDate(value);
		}

		ZDateTime GetDateFromText()
		{
			var timeString = Text.Trim();
			if (!string.IsNullOrEmpty(timeString))
			{
				var timeParts = timeString.Split(':').Select(s => int.TryParse(s, out var n) ? n : 0);
				var hours = timeParts.FirstOrDefault();
				var minutes = timeParts.Skip(1)?.FirstOrDefault() ?? 0;

				if (hours > 0 || minutes > 0)
				{
					return new ZDateTime(ZDateTime.Now.Year, 1, 1)
						.AddHours(hours)
						.AddMinutes(minutes);
				}
			}

			return ZDateTime.Empty;
		}

		string GetTextFromDate(IZType value)
		{
			if (value is ZDateTime date)
			{
				var minutes = date.GetMinutesFromDateTimeSpan();
				var hours = (int)minutes / 60;
				minutes %= 60;

				if (hours > 0 || minutes > 0)
				{
					return $"{hours:##0}:{minutes:00}"; // Format string
				}
			}

			return ":";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "JavaScript")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			Style.Add("text-align", $"{HorizontalAlign.Center}");

			AddControlFunctionsScript();
			Attributes.Add("onfocus", "OnFocusUserTimeInput(this)");
		}

		void AddControlFunctionsScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), ControlFunctionsScriptKey))
			{
				var script = @"
				<script>

				function OnFocusUserTimeInput(control) {
					var value = control.value.trim();
					if (value === ':') {
						control.value = '';
					}
				}

				</script>";
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), ControlFunctionsScriptKey, script);
			}
		}

		protected const string ControlFunctionsScriptKey = "ZTimeFilterControlFunctionsScript";

		protected override string ValidationMessage => Res.GetString("84e059b9-2a6f-4fe7-865d-57db605f8a84", "Please enter hours and minutes, max 999:59");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Pattern string")]
		protected override string ValidationPattern => "^\\\\d{1,3}:[0-5][0-9]$";

		protected override ZWebResource GetNewValidationScriptFile() => new ZWebResource(typeof(ZTimeFilterControl), "ZTimeFilterControlValidation.js", Page);

		protected override string ValidationScriptKey => "ZTimeFilterControl_ValidationScript";

		protected override string ValidationFunctionName => "ValidateUserTimeInput";
	}
}
