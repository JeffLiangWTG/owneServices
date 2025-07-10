using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AirCargoOutturnBillsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AirCargoOutturnBillsController; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.AU.AirCargoOutturnBills; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusUnderbond); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			CusUnderbond underbond = (CusUnderbond)businessEntity;
			if (underbond.LinkedObject is CusMAWB)
			{
				if (((CusMAWB)underbond.LinkedObject).Consol != null)
				{
					IZForm result = new ConsolForm(((CusMAWB)underbond.LinkedObject).Consol);
					((ConsolForm)result).PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.AirCargo;
					return result;
				}
				else
				{
					return new AirCargoMasterForm(underbond.LinkedObject as CusMAWB);
				}
			}
			return new AirCargoOutturnBillsForm(underbond);
		}

		#region CheckPoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ACAOutturnBillsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ACAOutturnBillsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ACAOutturnBillsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ACAOutturnBills; }
		}

		#endregion
	}
}
