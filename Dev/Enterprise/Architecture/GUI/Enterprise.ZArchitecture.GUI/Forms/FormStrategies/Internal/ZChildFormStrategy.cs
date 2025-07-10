using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class ZChildFormStrategy
	{
		public static void AddAdornments(Form form)
		{
			EnterpriseFormLookStrategy.AddAdornments(form);
			ZFormStatusBarStrategy.AddAdornments(form);
		}
	}
}
