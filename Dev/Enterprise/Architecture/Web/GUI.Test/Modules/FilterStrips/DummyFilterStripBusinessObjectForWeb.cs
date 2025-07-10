using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing
{
	sealed class DummyFilterStripBusinessObjectForWeb : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBusinessObjectForWeb()
		{
		}

		public DummyFilterStripBusinessObjectForWeb(OrgContact contact)
		{
			LayoutsHelper = new FilterStripLayoutsHelperForWeb(this, contact);
		}

		public new FilterStripLayoutsHelperForWeb LayoutsHelper
		{
			get { return (FilterStripLayoutsHelperForWeb)base.LayoutsHelper; }
			set { base.LayoutsHelper = value; }
		}
	}
}
