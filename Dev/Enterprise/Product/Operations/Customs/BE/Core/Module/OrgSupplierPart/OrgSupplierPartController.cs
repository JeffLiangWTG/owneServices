using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.MasterFiles;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.BE.Module;

public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
	{
		return new EUOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
	}
}
