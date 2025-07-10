using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class EcsMessagingMenu : ZMenuItem
	{
		public EcsMessagingMenu()
		{
			CaptionResourceString = Res.GetData("E365B950-1709-4276-AE29-539302A63FD6", "Exit Control");
		}

		public CusExitControlHeader ExitHeader
		{
			get { return fExitHeader; }
			set
			{
				if (fExitHeader != value)
				{
					fExitHeader = value;
					SetupMessageMenuProvider();
				}
			}
		}
		CusExitControlHeader fExitHeader;

		void SetupMessageMenuProvider()
		{
			MenuItems.Clear();

			if (ExitHeader != null)
			{
				messageMenuProvider = EcsMessageMenuProvider.New(ExitHeader);
				if (messageMenuProvider != null)
				{
					MenuItems.AddRange(messageMenuProvider.CreateMenuItems().ToArray());
				}
			}
		}
		EcsMessageMenuProvider messageMenuProvider;
	}
}
