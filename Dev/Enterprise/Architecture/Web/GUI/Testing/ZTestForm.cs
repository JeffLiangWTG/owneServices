#if DEBUG
using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZTestForm : HtmlForm
	{
		public ZTestForm() : base()
		{
		}

		protected override void RenderAttributes(HtmlTextWriter writer)
		{
			// do nothing for the moment
		}

		public void Initialise()
		{
			OnInit(EventArgs.Empty);
		}

		public NameValueCollection FormDataForTesting
		{
			get
			{
				if (fFormDataForTesting == null)
				{
					fFormDataForTesting = new NameValueCollection();
				}
				return fFormDataForTesting;
			}
		}
		NameValueCollection fFormDataForTesting;
	}
}
#endif