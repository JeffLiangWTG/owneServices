using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.MY.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.MY.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrgSupplierPartFormCustomsPluginGlobal((OrgSupplierPart)businessEntity);
	}
}
