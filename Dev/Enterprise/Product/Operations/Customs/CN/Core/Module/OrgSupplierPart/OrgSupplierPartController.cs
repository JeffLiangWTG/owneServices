using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CN.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
	}
}
