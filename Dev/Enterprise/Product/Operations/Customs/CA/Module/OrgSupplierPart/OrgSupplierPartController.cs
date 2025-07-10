using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CA.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.CAOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
		}
	}
}
