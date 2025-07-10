using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CA.GUI
{
	public partial class OrganisationB3SendingUserControl : ZUserControl
	{
		public OrganisationB3SendingUserControl()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(DetailsGroupBox);
			MissingResourceStringChecker.ExcludeFromTest(AutoSaveCONDelayIntervalTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(AutoSaveHVSDelayIntervalTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(FailSafeCONDelayIntervalTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(FailSafeHVSDelayIntervalTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(DeferredB3SendingGroupBox);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
