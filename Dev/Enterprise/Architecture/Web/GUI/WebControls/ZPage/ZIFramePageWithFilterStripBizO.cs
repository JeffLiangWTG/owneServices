using System;
using System.Web;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public abstract class ZIFramePageWithFilterStripBizO : ZIFramePage
	{
		protected override void OnInit(EventArgs e)
		{
			if (FilterStripBizO == null)
			{
				HttpContext.Current.Response.Redirect($"{AppInstance.ErrorPage}?invalidQuery=true"); // redirect path
				return;
			}

			base.OnInit(e);
		}

		public abstract string QueryStringKey { get; }

		#region FilterStripBizO

		protected FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					ZGuid dataSourceIndexer = GetGuidFromParameter(QueryStringKey);
					fFilterStripBizO = GetFilterBusinessObjectFromSession(dataSourceIndexer, this);
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		#endregion

		public FilterStripBusinessObject GetFilterBusinessObjectFromSession(ZGuid dataSourceIndexer, ZPage page)
		{
			FilterStripBusinessObject result = null;
			if (page.Session != null)
			{
				result = (FilterStripBusinessObject)page.Session[dataSourceIndexer.ToString()];
			}
			return result;
		}
	}
}
