using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EDIOpportunityValueAnalysisUserControl : ZUserControl
	{
		public EDIOpportunityValueAnalysisUserControl()
		{
			InitializeComponent();

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(TotalLocalValueCurrencyTextBox);
			MissingResourceStringChecker.ExcludeFromTest(TotalForeignValueCurrencyTextBox);
#endif
		}
	}
}
