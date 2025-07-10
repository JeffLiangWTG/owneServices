using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Modules.Base;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public abstract class SimpleModule : ZFilterGridModule
	{
		public SimpleModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "SimpleFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Base");
		}

		public override Type FilterControlType
		{
			get { return typeof(SimpleFilterControl); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			return new FilterBusinessObjectDefaults();
		}
	}
}
