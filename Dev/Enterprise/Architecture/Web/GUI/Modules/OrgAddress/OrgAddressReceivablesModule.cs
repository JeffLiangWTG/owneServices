using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class OrgAddressReceivablesModule : OrgAddressModule
	{
		public OrgAddressReceivablesModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.OrgAddressReceivablesTracking;

		public override Type FilterBusinessObjectType => typeof(OrgAddressReceivablesFilterBusinessObject);
	}
}
