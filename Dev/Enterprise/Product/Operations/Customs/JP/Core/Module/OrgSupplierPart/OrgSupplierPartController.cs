using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.JP.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
		}
	}
}
