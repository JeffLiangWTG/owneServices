using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyControllerWithCancellableBizO : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWithCancellableBizO;

		public override Type TypeOfTopLevelBusinessObject => typeof(DummyCancellable);

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var businessEntity = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			if (businessEntity is ICancellable businessEntityICancellable)
			{
				businessEntityICancellable.IsCancelled = (sourceEntity as ICancellable).IsCancelled;
			}

			return businessEntity;
		}

		protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			BusinessObject result = factory.Load<DummyCancellable>(sourceEntityPK);
			if (result == null)
			{
				var dummyChild = factory.Load<DummyDependantBusinessObject>(sourceEntityPK);
				result = (dummyChild == null) ? null : dummyChild.Parent;
			}
			return result;
		}
	}
}
