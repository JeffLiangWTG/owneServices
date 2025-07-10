using System;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class InterchangeEventUserControl : ZUserControl
	{
		public InterchangeEventUserControl()
		{
			InitializeComponent();
		}

		void xtMsgEventRefreshButton_Click(object sender, EventArgs e)
		{
			var interchange = (EDIInterchange)base.DataSource;
			interchange.ReloadXtMessageEvents(true);
		}

		internal void EventsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (EventsGrid.ListManager.GetCurrent() is XtMessageEvent eventInfo)
			{
				var lineInterchangeGuid = eventInfo.InterchangeGuid;
				if (lineInterchangeGuid.IsValid)
				{
					var currentInterchange = (EDIInterchange)base.DataSource;
					if (currentInterchange.PK != lineInterchangeGuid)
					{
						var interchange = currentInterchange.Factory.Load<EDIInterchange>(lineInterchangeGuid);
						if (interchange != null)
						{
							var form = new EDIInterchangeForm(interchange);
							form.DisplayMode = ODisplayMode.ReadOnly;
							form.Show();
						}
					}
				}
			}
		}
	}
}
