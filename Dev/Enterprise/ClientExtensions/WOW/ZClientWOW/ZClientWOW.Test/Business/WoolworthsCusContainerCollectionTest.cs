using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsCusContainerCollection))]
	public class WoolworthsCusContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCorrectTypeUsed()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(WoolworthsCusContainerCollection), declaration.CusContainers.GetType());
		}

		#region DeletingCusContainer
		public void TestDeletingCusContainer()
		{
			fDeclaration.DeletingCusContainer += new CancelEventHandler(OnDeclaration_DeletingCusContainer);
			fDeclaration.CusContainers.RemoveAndDelete(fCusContainer);
			AssertEquals("UpdatingCusContainerNumber event should have fired", true, fDeletingCusContainerNumberCalled);
			AssertEquals("Should have deleted ok", true, fCusContainer.IsDeleted);
			fDeclaration.DeletingCusContainer -= new CancelEventHandler(OnDeclaration_DeletingCusContainer);
		}

		public void TestDeletingCusContainer_OnAirDeclaration()
		{
			fDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			fDeclaration.DeletingCusContainer += delegate
			{
				Fail("User should not be queried when deleting a ULD (air) container, because ULDs shouldn't be declared and are deleted by the system");
			};
			AssertEquals("Should be a container on the declaration initially for the test", 1, fDeclaration.CusContainers.Count);
			Factory.Save();
			AssertEquals("Containers should be deleted on save of the dec", 0, fDeclaration.CusContainers.Count);
		}

		public void TestDeletingCusContainer_CancelEvent()
		{
			fDeclaration.DeletingCusContainer += new CancelEventHandler(OnDeclaration_DeletingCusContainer_Cancel);
			fDeclaration.CusContainers.RemoveAndDelete(fCusContainer);
			AssertEquals("Should NOT have deleted", false, fCusContainer.IsDeleted);
			fDeclaration.DeletingCusContainer -= new CancelEventHandler(OnDeclaration_DeletingCusContainer_Cancel);
		}

		bool fDeletingCusContainerNumberCalled;
		void OnDeclaration_DeletingCusContainer(object sender, CancelEventArgs e)
		{
			fDeletingCusContainerNumberCalled = true;
		}

		void OnDeclaration_DeletingCusContainer_Cancel(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion
		#region Implementation
		WoolworthsJobDeclaration fDeclaration;
		CusContainer fCusContainer;
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			WoolworthsJobDeclaration declaration = Factory.New<WoolworthsJobDeclaration>();
			return (WoolworthsCusContainerCollection)declaration.CusContainers;
		}

		protected override void SetUp()
		{
			base.SetUp();
			fDeclaration = (WoolworthsJobDeclaration)Factory.New(typeof(JobDeclaration));
			fCusContainer = fDeclaration.CusContainers.AddNew();
		}
		#endregion
	}
}
