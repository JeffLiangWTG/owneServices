using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsCusContainer))]
	class WoolworthsCusContainerTest : InvoiceOrderLinkTestCase
	{
		public void TestSyncOrderLineDeliverContainers()
		{
			Factory.Save();
			fCusContainer.CO_ContainerNumber = "CONT1";
			Factory.Save();
			AssertEquals("Container Number", fCusContainer.CO_ContainerNumber, fDeliverContainer.J5_ContainerNum);
			AssertEquals("Container Seal", fCusContainer.CO_Seal.ToUpper(), fDeliverContainer.J5_ContainerSeal.ToUpper());
			AssertEquals("Container Type", fCusContainer.CO_RC, fDeliverContainer.ContainerType.PK);
			AssertEquals("ETA", fDeclaration.JE_DateOfArrival, fDeliverContainer.J5_ETA);
			AssertEquals("Arrival Vessel", fDeclaration.JE_VesselName, fDeliverContainer.J5_RV_NKArrivalVessel);
			fCusContainer.CO_Seal = "seal2";
			Factory.Save();
			AssertEquals("Container Number", fCusContainer.CO_ContainerNumber, fDeliverContainer.J5_ContainerNum);
			AssertEquals("Container Seal", fCusContainer.CO_Seal, fDeliverContainer.J5_ContainerSeal);
		}

		public void TestSyncOrderLineDeliverContainers_DontWhenDeclarationIsInactive()
		{
			fDeclaration.JE_IsCancelled = true;
			Factory.Save();
			AssertEquals("Container Seal not updated", "", fDeliverContainer.J5_ContainerSeal.ToUpper());
			AssertEquals("Container Type not updated", "", fDeliverContainer.J5_RC_NKContainerType);
			AssertEquals("ETA not updated", ZDateTime.Empty, fDeliverContainer.J5_ETA);
			AssertEquals("Arrival Vessel not updated", "", fDeliverContainer.J5_RV_NKArrivalVessel);
		}

		public void TestDontPopulateContainerToOrders_WhenContainerNumberEmpty()
		{
			fCusContainer.CO_ContainerNumber = ZString.Empty;
			Factory.Save();
			AssertEquals("Container Number shouldn't populate when made empty", "12345678", fDeliverContainer.J5_ContainerNum);
		}

		public void TestRenameContainerNumberToOrders()
		{
			Factory.Save();
			AssertEquals("Container Number", fCusContainer.CO_ContainerNumber, fDeliverContainer.J5_ContainerNum);
			fCusContainer.CO_ContainerNumber = "87654321";
			Factory.Save();
			AssertEquals("Container Number", fCusContainer.CO_ContainerNumber, fDeliverContainer.J5_ContainerNum);
		}

		[ExpectNoExceptions]
		public void TestWhenDeleteCusContainer_TwiceDoesntCauseException()
		{
			// to reproduce problem where DataRefreshBus(tm) deletes the row underneath and then the gui somehow calls Delete() again
			fCusContainer.Delete();
			fCusContainer.Delete();
		}

		public void TestWhenDeleteCusContainer_DeleteOrderContainer()
		{
			Factory.Save();
			AssertEquals("Container Number", fCusContainer.CO_ContainerNumber, fDeliverContainer.J5_ContainerNum);
			fCusContainer.Delete();
			AssertEquals("Deleting CusContainer should result in order container deleting", true, fDeliverContainer.IsDeleted);
		}

		public void TestWhenSetContainerNumberToEmpty_DeleteOrderContainer()
		{
			Factory.Save();
			AssertEquals("Container Number", fCusContainer.CO_ContainerNumber, fDeliverContainer.J5_ContainerNum);
			fCusContainer.CO_ContainerNumber = "";
			AssertEquals("Setting ContainerNumber to empty should result in order container deleting", true, fDeliverContainer.IsDeleted);
		}

		#region TestUpdatingCusContainerNumber
		public void TestUpdatingCusContainerNumber()
		{
			Factory.Save(); // set IsInDatabase to true
			fDeclaration.UpdatingCusContainerNumber += new CancelEventHandler(OnDeclaration_UpdatingCusContainerNumber);
			fCusContainer.CO_ContainerNumber = "X";
			AssertEquals("UpdatingCusContainerKey event should have fired", true, fUpdatingCusContainerKeyCalled);
			AssertEquals("Container number set correctly", "X", fCusContainer.CO_ContainerNumber);
			fDeclaration.UpdatingCusContainerNumber -= new CancelEventHandler(OnDeclaration_UpdatingCusContainerNumber);
		}

		public void TestUpdatingCusContainerNumber_CancelEvent()
		{
			Factory.Save(); // set IsInDatabase to true
			fCusContainer.CO_ContainerNumber = "X";
			fDeclaration.UpdatingCusContainerNumber += new CancelEventHandler(OnDeclaration_UpdatingCusContainerNumber_Cancel);
			fCusContainer.CO_ContainerNumber = "Y";
			AssertEquals("Container number NOT set", "X", fCusContainer.CO_ContainerNumber);
			fDeclaration.UpdatingCusContainerNumber -= new CancelEventHandler(OnDeclaration_UpdatingCusContainerNumber_Cancel);
		}

		bool fUpdatingCusContainerKeyCalled;
		void OnDeclaration_UpdatingCusContainerNumber(object sender, CancelEventArgs e)
		{
			fUpdatingCusContainerKeyCalled = true;
		}

		void OnDeclaration_UpdatingCusContainerNumber_Cancel(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			fCusContainer.CO_ContainerNumber = "12345678";
			fDeliverContainer.J5_ContainerNum = "12345678";
			fInvoiceLine.JI_CustomAttrib4 = fCusContainer.CO_ContainerNumber;
			fCusContainer.CO_Seal = "seal";
			fCusContainer.CO_RC = Factory.LoadTop1(typeof(RefContainer), new ZQuery()).PK;
			fDeclaration.JE_DateOfArrival = ZDateTime.Now.AddDays(1);
			fDeclaration.JE_VesselName = "vessel";
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			return declaration.CusContainers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			CusContainer result = declaration.CusContainers.AddNew();
			Customs.Business.Testing.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(result.Factory);
			return result;
		}
		#endregion
	}
}
