using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public abstract class SupportIncidentActionTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected IBMTestHelper bmTestHelper;
		protected IBMSystem system;

		protected override void SetUp()
		{
			base.SetUp();

			bmTestHelper = ObjectFactory.Get<IBMTestHelper>();

			bmTestHelper.EnableBMSInRegistry();
			system = bmTestHelper.CreateSystem(Factory, "INC");
		}

		#endregion
	}
}