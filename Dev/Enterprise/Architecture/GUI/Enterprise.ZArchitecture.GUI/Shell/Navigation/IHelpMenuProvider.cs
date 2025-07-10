using System.Windows.Forms;

namespace Enterprise.Core.Modules
{
	public interface IHelpMenuProvider
	{
		void ShowUserPortal();
		void ShowWiseTechAcademy(string path = null, string target = null);
		void ShowBorderWiseWebApp();
		void ShowERequestPortal();
		bool IsCargoWiseWebPortalsConfigured();
		void ShowCargoWiseWebPortals();
		void ShowAbout();
		void ShowAboutForCWNext();
		void ShowHotKeyHelp(Control sender);
		void ShowDeveloperFeatureControlOverride();
		bool ShowDisclaimerConfirmation();
		void ShowSecurityOverrideToken();
	}
}
