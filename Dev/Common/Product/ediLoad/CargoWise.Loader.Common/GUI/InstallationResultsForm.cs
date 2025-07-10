using System;
using System.Linq;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public partial class InstallationResultsForm : System.Windows.Forms.Form
	{
		// TODO: Pull these factory methods out to a factory class when generics are available?
		public static InstallationResultsForm New()
		{
#if DEBUG
			if (fFormToReturn != null)
			{
				return fFormToReturn;
			}
			else
#endif
			{
				return new InstallationResultsForm();
			}
		}

#if DEBUG
		public static IDisposable OverrideFactoryForTest(InstallationResultsForm formToReturn)
		{
			return new OverrideResetter(formToReturn);
		}

		[ThreadStatic]
		static InstallationResultsForm fFormToReturn;

		class OverrideResetter : IDisposable
		{
			public OverrideResetter(InstallationResultsForm formToReturn)
			{
				fFormToReturn = formToReturn;
			}

			public void Dispose()
			{
				fFormToReturn = null;
			}
		}
#endif

		protected InstallationResultsForm()
		{
			InitializeComponent();
			this.Icon = BrandingFactory.Instance.ProductIcon;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "No need to translate")]
#if DEBUG
		virtual // for mocks
#endif
		public void LoadInstallationResults(InstallationResultCollection results)
		{
			Argument.NotNull(results, nameof(results));
			InstallationResultStatus statusToShow;

			if (results.ErrorCount > 0)
			{
				ErrorLabel.Visible = true;
				statusToShow = InstallationResultStatus.Error;
			}
			else
			{
				WarningLabel.Visible = true;
				statusToShow = InstallationResultStatus.Warning;

				if (results.All(u => !u.AffectCargoWiseFunctions))
				{
					WarningLabel.Text = "Warning! CargoWise One detected some problems. You or your IT administrator should address these problems.";
				}
				else
				{
					WarningLabel.Text = "Warning! CargoWise One detected some problems. CargoWise One will still run, but performance may be severely reduced and some functions may not work. You or your IT administrator should address these problems.";
				}
			}

			StringBuilder textBoxContents = new StringBuilder();

			foreach (InstallationResult result in results)
			{
				if (result.Status == statusToShow)
				{
					if (textBoxContents.Length > 0)
					{
						textBoxContents.Append(Environment.NewLine);
						textBoxContents.Append(Environment.NewLine);
					}
					textBoxContents.Append(result.Message);
				}
			}
			WarningErrorTextBox.Text = textBoxContents.ToString();
			WarningErrorTextBox.Select(0, 0);
		}
	}
}

