using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public abstract class CMRModule : ZFilterGridModule
	{
		protected override Control GetNewEmbeddedControl()
		{
			Control result;

			if (IsCMR)
			{
				result = base.GetNewEmbeddedControl();
			}
			else
			{
				ZLabel label = new ZLabel();
				label.Text = "This is a CMR-only module, and is not yet activated.";
				label.TextAlign = ContentAlignment.MiddleCenter;

				result = label;
			}

			return result;
		}

		public override MenuItem[] FormActionMenu
		{
			get { return IsCMR ? base.FormActionMenu : null; }
		}

		protected internal bool IsCMR
		{
			get
			{
				if (!isCMRCached)
				{
					isCMRCached = true;
					CMRUtilities cMR = new CMRUtilities();
					fIsCMR = cMR.AreWeRunningInCMR(ZDateTime.Now);
				}
				return fIsCMR;
			}
		}
		bool fIsCMR;
		bool isCMRCached;
	}
}
