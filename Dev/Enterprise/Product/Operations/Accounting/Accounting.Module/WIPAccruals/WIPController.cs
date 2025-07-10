using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class WIPController : WIPAccrualController
	{
		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.WIPAccruals;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WIP); }
		}

		#region Implementation

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewWIPs; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewWIPs; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseSingleWipOrAccrual; }
		}

		#endregion

		#region Multiple Reversing

		protected override IZForm GetFormCore(IBusiness businessEntity) => new GUI.WipAccrual.WIPForm((WIP)businessEntity);

		protected override ControllerID IDCore => ControllerIDs.WIP;

		#endregion
	}
}
