using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.MX.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrgSupplierPartFormCustomsPluginGlobal((OrgSupplierPart)businessEntity);
	}
}
