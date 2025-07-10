using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.WebLauncher
{
	public interface IWebUrlLaunchValidator
	{
		void ValidateUrl(string urlString);
	}

	public class WebUrlLaunchValidator : IWebUrlLaunchValidator
	{
		public WebUrlLaunchValidator(IWebUrlValidationUserPrompter userPrompter = null)
		{
			webUrlValidationUserPrompter = userPrompter ?? new WebUrlValidationUserPrompter();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Uri schema validation")]
		public void ValidateUrl(string urlString)
		{
			var uriSchema = string.Empty;
			try
			{
				var uri = new UriBuilder(urlString);
				uriSchema = uri.Scheme;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleUnsafeUrlByUserConfirmationIfApplicable();
				return;
			}

			if (!IsUriSchemaSafe(uriSchema))
			{
				HandleUnsafeUrlByUserConfirmationIfApplicable();
			}

			bool IsUriSchemaSafe(string schema)
			{
				switch (schema)
				{
					case "http":
					case "https":
					case "edient":
					case "mailto":
					case "callto":
					case "tel":
					case "vsnet":
						// Pass
						return true;
					case "file":
					default:
						return false;
				}
			}

			void HandleUnsafeUrlByUserConfirmationIfApplicable()
			{
				if (RawDataRegistry.Instance.ShowWarningPopupBeforeLaunchingURL.Value && !webUrlValidationUserPrompter.GetUserConfirmation(urlString))
				{
					throw new WebUrlValidationException("user chose not to open url.");
				}
			}
		}

		readonly IWebUrlValidationUserPrompter webUrlValidationUserPrompter;
	}
}
