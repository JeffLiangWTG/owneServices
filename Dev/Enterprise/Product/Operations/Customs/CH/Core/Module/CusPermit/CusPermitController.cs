using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.Module;

public class CusPermitController : Customs.Module.CusPermitController
{
	public override Type TypeOfTopLevelBusinessObject
	{
		get { return typeof(CusPermitHeader); }
	}

	protected override IZForm GetForm(IBusiness businessEntity)
	{
		return new CusPermitForm((CusPermitHeader)businessEntity);
	}
}
