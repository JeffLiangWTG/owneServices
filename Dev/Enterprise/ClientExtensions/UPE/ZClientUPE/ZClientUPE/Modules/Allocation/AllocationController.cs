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
	public class AllocationController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ClientControllerRegistration.Allocation; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Allocation); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AllocationForm((Allocation)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new Allocation(new BusinessObjectFactory());
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}
		internal SecurityCheckpoint InternalCheckPointForNew => CheckPointForNew;

#endregion

			}
}
