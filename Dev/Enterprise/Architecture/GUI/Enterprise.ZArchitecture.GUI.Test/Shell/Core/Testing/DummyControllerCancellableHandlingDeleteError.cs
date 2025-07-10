using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerCancellableHandlingDeleteError : DummyControllerwithCancellableWhichCanNotBeDeleted
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted;

		public override Type TypeOfTopLevelBusinessObject => typeof(DummyCancellableHandlingDeleteError);

		protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			BusinessObject result = factory.Load<DummyCancellableHandlingDeleteError>(sourceEntityPK);
			if (result == null)
			{
				var dummyChild = factory.Load<DummyDependantBusinessObject>(sourceEntityPK);
				result = (dummyChild == null) ? null : dummyChild.Parent;
			}
			return result;
		}
	}
}
