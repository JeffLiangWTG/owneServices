using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class DummyControllerwithCancellableWhichCanNotBeDeleted : DummyControllerWithCancellableBizO
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted;

		public override Type TypeOfTopLevelBusinessObject => typeof(DummyCancellableWhichCanNotBeDeleted);

		protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			BusinessObject result = factory.Load<DummyCancellableWhichCanNotBeDeleted>(sourceEntityPK);
			if (result == null)
			{
				var dummyChild = factory.Load<DummyDependantBusinessObject>(sourceEntityPK);
				result = (dummyChild == null) ? null : dummyChild.Parent;
			}
			return result;
		}
	}
}
