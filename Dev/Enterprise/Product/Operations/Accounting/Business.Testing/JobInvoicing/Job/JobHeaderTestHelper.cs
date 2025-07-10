using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class JobHeaderTestHelper
	{
		public JobHeaderTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public JobHeader CreateJobInAnotherCW1(IJobInvoicingPlugIn parent)
		{
			using (Env.SetTemporaryUserContext(TestObjectCreator.GS1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var tempSemaphoreProvide = new SemaphoreProviderWithCurrentUserForTesting())
			using (new DisposableAction(() => TestSemaphoreProviderAttribute.TestProvider = null))
			{
				TestSemaphoreProviderAttribute.TestProvider = tempSemaphoreProvide;
				var facotryInAnotherCW1 = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};
				var parentInAnotherCW1 = facotryInAnotherCW1.Load(parent.TablePrefix(), parent.PK) as IJobInvoicingPlugIn;
				return new JobHeader.Loader(facotryInAnotherCW1, parentInAnotherCW1).TryCreateWithMutex();
			}
		}

		BusinessObjectFactory Factory { get; }

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
