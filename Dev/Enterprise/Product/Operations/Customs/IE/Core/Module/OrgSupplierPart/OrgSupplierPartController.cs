using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.MasterFiles;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IE.Module
{
	public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
	}
}
