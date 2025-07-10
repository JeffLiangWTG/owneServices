using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public OrgSupplierPartController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AUOrgSupplierPart); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new AUOrgSupplierPartFormCustomsPlugin((AUOrgSupplierPart)businessEntity);
		}
	}
}
