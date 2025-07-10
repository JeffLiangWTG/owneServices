using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AE.Module;

public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
	{
		return new OrgSupplierPartFormCustomsPluginGlobal((OrgSupplierPart)businessEntity);
	}
}
