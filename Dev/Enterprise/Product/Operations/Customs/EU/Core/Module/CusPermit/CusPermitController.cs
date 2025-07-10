using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class CusPermitController : Customs.Module.CusPermitController
	{
		public CusPermitController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusPermitHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusPermitForm((CusPermitHeader)businessEntity);
		}
	}
}
