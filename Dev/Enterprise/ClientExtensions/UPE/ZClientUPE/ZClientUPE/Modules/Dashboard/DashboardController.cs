using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class DashboardController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ClientControllerRegistration.Dashboard; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Dashboard); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DashboardForm((Dashboard)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new Dashboard(new BusinessObjectFactory());
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion

		internal SecurityCheckpoint InternalCheckPointForView => CheckPointForView;
		internal SecurityCheckpoint InternalCheckPointForNew => CheckPointForNew;
		internal SecurityCheckpoint InternalCheckPointForEdit => CheckPointForEdit;
		internal SecurityCheckpoint InternalCheckPointForDelete => CheckPointForDelete;
	}
}
