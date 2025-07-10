using System.Linq;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class ReportsGridUserControlMenuProvider : EU.ExitControl.GUI.ReportsGridUserControlMenuProvider
	{
		public ReportsGridUserControlMenuProvider(IReportsGridUserControlProvider provider) : base(provider)
		{
		}

		protected override ZMenuItem[] GetAdditionalReportsGridMenuItems()
		{
			var baseMenuItems = base.GetAdditionalReportsGridMenuItems().ToList();

			var requestInboxNotificationsMenuItem = new RequestInboxNotificationsMenuItemCreator(provider).Create();
			baseMenuItems.Add(requestInboxNotificationsMenuItem);

			var viewOnCustomsWebsiteMenuItemCreator = new ViewOnCustomsWebsiteMenuItemCreator(provider).Create();
			baseMenuItems.Add(viewOnCustomsWebsiteMenuItemCreator);

			var setAsFailedFromTransmissionMenuItem = new SetAsFailedFromTransmissionMenuItemCreator(provider).Create();
			baseMenuItems.Add(setAsFailedFromTransmissionMenuItem);

			return baseMenuItems.ToArray();
		}
	}
}
