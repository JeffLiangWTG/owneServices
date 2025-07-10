using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	sealed class ZRulingFindBox : ZCodeFindBox
	{
		public ZRulingFindBox() : base()
		{
		}

		protected override void ShowEditOrViewForm()
		{
			using (var showForm = new ShowRulingInEditOrViewForm(this))
			{
				var subscribers = OnCreatingRuling?.GetInvocationList();
				if (subscribers != null)
				{
					foreach (var subscriber in subscribers)
					{
						var realSubscriber = (EventHandler)subscriber;
						showForm.OnCreatingRuling += realSubscriber;
					}
				}

				showForm.ShowEditOrViewForm();
			}
		}

		public event EventHandler OnCreatingRuling;
	}
}
