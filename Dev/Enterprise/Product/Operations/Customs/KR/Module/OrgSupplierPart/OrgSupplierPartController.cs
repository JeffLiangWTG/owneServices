using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.KR.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormCustomsPluginGlobal((OrgSupplierPart)businessEntity);
		}
	}
}
