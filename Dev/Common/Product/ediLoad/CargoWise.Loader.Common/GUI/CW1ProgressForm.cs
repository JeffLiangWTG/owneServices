using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.IO;

namespace CargoWise.Loader.Common
{
	public partial class CW1ProgressForm : Form, IProcessStatus
	{
		#region Construction

		public static CW1ProgressForm New()
		{
#if DEBUG
			if (fFormToReturn != null)
			{
				return fFormToReturn;
			}
			else
#endif
			{
				return new CW1ProgressForm();
			}
		}

		#region Testing Only
#if DEBUG

		public static IDisposable OverrideFactoryForTest(CW1ProgressForm formToReturn)
		{
			return new OverrideResetter(formToReturn);
		}

		static CW1ProgressForm fFormToReturn;

		class OverrideResetter : IDisposable
		{
			public OverrideResetter(CW1ProgressForm formToReturn)
			{
				fFormToReturn = formToReturn;
			}

			public void Dispose()
			{
				fFormToReturn = null;
			}
		}

#endif
		#endregion

		public CW1ProgressForm()
		{
			InitializeComponent();
			UpdateBranding();
		}

		#endregion

		public void UpdateBranding()
		{
			Icon = BrandingFactory.Instance.ProductIcon;
			BackgroundImage = BrandingFactory.Instance.SplashScreenImage;
		}

		public void UpdateStatus(string status, int progressValue)
		{
			StatusLabel.Text = status;
			ProgressBar.Value = progressValue;
		}
	}
}
