using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.FR.Module
{
	public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
		}
	}
}
