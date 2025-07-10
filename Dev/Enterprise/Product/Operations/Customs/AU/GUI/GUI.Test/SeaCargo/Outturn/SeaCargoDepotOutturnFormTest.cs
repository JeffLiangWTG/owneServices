using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	[TestedType(typeof(SeaCargoDepotOutturnForm))]
	sealed class SeaCargoDepotOutturnFormTest : ZFormBasherTest
	{
		public void TestShowMessageWhenRescind()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn = header.Outturns.AddNew();

			outturn.C5_MasterBill = "Master";
			outturn.C5_HouseBill = "House";
			outturn.C5_ContainerNumber = "DFDF1111116";

			string text = "The outturn [DFDF1111116] - [House/Master] has been rescinded!";

			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				form.Show();

				outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;

				outturn.C5_CargoReceiptDate = DateTime.Now;
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;

				AssertEquals(text, userNotification);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				outturn.C5_CargoUnpackDate = DateTime.Now;
				userNotification = UnitTestUserNotification.Instance.LastMessage.Text;

				AssertEquals(text, userNotification);
			}
		}

		public void TestSelectOutturn()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn1 = header.Outturns.AddNew();
			DepotCusOutturn outturn2 = header.Outturns.AddNew();
			DepotCusOutturn outturn3 = header.Outturns.AddNew();
			DepotCusOutturn outturn4 = header.Outturns.AddNew();
			DepotCusOutturn outturn5 = header.Outturns.AddNew();
			DepotCusOutturn outturn6 = Factory.New<DepotCusOutturn>();

			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				AssertEquals("No rows selected as list manager is null", -1, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);

				form.Show();

				AssertEquals("First row selected", 0, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);

				form.SelectOutturn(outturn4);
				AssertEquals("Correct row selected", outturn4.PK, ((BusinessObject)form.seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.GetCurrent()).PK);

				form.SelectOutturn(outturn5);
				AssertEquals("Correct row selected", outturn5.PK, ((BusinessObject)form.seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.GetCurrent()).PK);

				form.SelectOutturn(outturn6);
				AssertEquals("Same row still selected", outturn5.PK, ((BusinessObject)form.seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.GetCurrent()).PK);
			}
		}

		public void TestCreateLoadListFromContainer()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn1 = AddContainerLine(header, underbond, TestContainerNumber1);
			DepotCusOutturn outturn2 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);

			Factory.Save();

			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				AssertEquals("No rows selected as list manager is null", -1, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);
				form.Show();

				form.SelectOutturn(outturn1);
				form.CreateLoadListFromOutturn(null, EventArgs.Empty);

				var container = Factory.LoadTop1<CFSContainer>(new ZQuery(JobContainerSchema.JC_ContainerNum, TestContainerNumber1));
				AssertNotNull("Failed to find created container", container);
				AssertNotNull("Failed to find created load list", container.Consol);
				AssertEquals("Consols Load List Vessel", VesselFromLloydsNum(TestLloydsNum), container.Consol.JK_JX_JV_NKVessel);
				AssertEquals("Consols Voyage", TestVoyageNum, container.Consol.JK_JX_JV_VoyageFlight);
				AssertEquals("Outturns Parent ID", container.PK, underbond.C4_ParentID);
				AssertEquals("Outturns Parent ID", JobContainerSchema.Constants.Prefix, underbond.C4_ParentTableCode);
			}
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: newConsolFactory")]
#else
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'newConsolFactory')")]
#endif
		public void TestCreateLoadListWithNullFactory()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				form.Show();
				form.CreateOrAttachLoadListFromOutturn(null, null);
			}
		}

		[ExpectNoExceptions()]
		public void TestCreateLoadListConcurrencyError()
		{
			Factory.RefreshEnabled = false;
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn1 = AddContainerLine(header, underbond, TestContainerNumber1);
			DepotCusOutturn outturn2 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			DepotCusOutturn containerOutturn = factory2.Load<DepotCusOutturn>(outturn1.PK);
			DepotCusOutturn shipmentOutturn = factory2.Load<DepotCusOutturn>(outturn2.PK);

			CFSContainer container = new CFSContainerCreator(containerOutturn, factory2).Container;
			CFSShipment shipment = new CFSShipmentCreator(shipmentOutturn, factory2).Shipment;

			CusOutturnHeader otherHeader = factory2.Load<CusOutturnHeader>(header.PK);
			otherHeader.C6_CommercialStatus = "WNG";

			factory2.Save();

			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				form.Show();

				form.SelectOutturn(outturn2);
				form.CreateOrAttachLoadListFromOutturn(Factory, null);
			}

			ErrorReporter.Clear();
		}

		public void TestCreateLoadListFromShipment()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn1 = AddContainerLine(header, underbond, TestContainerNumber1);
			DepotCusOutturn outturn2 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);

			Factory.Save();

			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				AssertEquals("No rows selected as list manager is null", -1, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);
				form.Show();

				form.SelectOutturn(outturn2);
				form.CreateLoadListFromOutturn(null, EventArgs.Empty);

				var consol = Factory.LoadTop1<CFSLoadListConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, TestOceanBill1));
				AssertNotNull("Failed to find created load list", consol);
				AssertEquals("Consol should have 1 shipment", 1, consol.Shipments.Count);
				AssertEquals("Consol should have 1 Container", 1, consol.Containers.Count);
				AssertEquals("Consol should have 1 Container", consol.Containers[0], consol.Shipments[0].OuterPackLines[0].GetContainer(consol));
				AssertEquals("Consol Ocean Bill", TestOceanBill1, consol.JK_MasterBillNum);
				AssertEquals("Consols Load List Vessel", VesselFromLloydsNum(TestLloydsNum), consol.JK_JX_JV_NKVessel);
				AssertEquals("Consols Voyage", TestVoyageNum, consol.JK_JX_JV_VoyageFlight);

				CFSShipment shipment = consol.Shipments[0];
				CFSContainer container = consol.Containers[0];
				AssertEquals("Container Number", TestContainerNumber1, container.JC_ContainerNum);
				AssertEquals("Shipment Housebill", TestHouseBill1, shipment.JS_HouseBill);

				AssertEquals("Outturns Parent ID", outturn2.C5_ParentID, shipment.PK);
			}
		}

		public void TestCreateLoadListFromContainerAndShipment()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn1 = AddContainerLine(header, underbond, TestContainerNumber1);
			DepotCusOutturn outturn2 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);
			Factory.Save();
			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				AssertEquals("No rows selected as list manager is null", -1, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);
				form.Show();

				form.SelectOutturn(outturn1);
				form.AddSelectedOutturn(outturn2);
				AssertEquals("Selected Grid Items - Should have both lines", 2, form.seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements.Length);
				form.CreateLoadListFromOutturn(null, EventArgs.Empty);

				var consol = Factory.LoadTop1<CFSLoadListConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, TestOceanBill1));
				AssertNotNull("Failed to find created load list", consol);
				AssertEquals("Consol should have 1 shipment", 1, consol.Shipments.Count);
				AssertEquals("Consol should have 1 Container", 1, consol.Containers.Count);
				AssertEquals("Consol should have 1 Container", consol.Containers[0], consol.Shipments[0].OuterPackLines[0].GetContainer(consol));
				AssertEquals("Consol Ocean Bill", TestOceanBill1, consol.JK_MasterBillNum);
				AssertEquals("Consols Load List Vessel", VesselFromLloydsNum(TestLloydsNum), consol.JK_JX_JV_NKVessel);
				AssertEquals("Consols Voyage", TestVoyageNum, consol.JK_JX_JV_VoyageFlight);

				CFSShipment shipment = consol.Shipments[0];
				CFSContainer container = consol.Containers[0];
				AssertEquals("Container Number", TestContainerNumber1, container.JC_ContainerNum);
				AssertEquals("Shipment Housebill", TestHouseBill1, shipment.JS_HouseBill);

				AssertEquals("Outturns Parent ID", outturn1.C5_ParentID, container.PK);
				AssertEquals("Outturns Parent ID", outturn2.C5_ParentID, shipment.PK);
			}
		}

		public void TestCreateLoadListFromShipmentAndContainer()
		{
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn2 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);
			DepotCusOutturn outturn1 = AddContainerLine(header, underbond, TestContainerNumber1);
			Factory.Save();
			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				AssertEquals("No rows selected as list manager is null", -1, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);
				form.Show();

				form.SelectOutturn(outturn1);
				form.AddSelectedOutturn(outturn2);
				AssertEquals("Selected Grid Items - Should have both lines", 2, form.seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements.Length);
				form.CreateLoadListFromOutturn(null, EventArgs.Empty);

				var consol = Factory.LoadTop1<CFSLoadListConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, TestOceanBill1));
				AssertNotNull("Failed to find created load list", consol);
				AssertEquals("Consol should have 1 shipment", 1, consol.Shipments.Count);
				AssertEquals("Consol should have 1 Container", 1, consol.Containers.Count);
				AssertEquals("Consol should have 1 Container", consol.Containers[0], consol.Shipments[0].OuterPackLines[0].GetContainer(consol));
				AssertEquals("Consol Ocean Bill", TestOceanBill1, consol.JK_MasterBillNum);
				AssertEquals("Consols Load List Vessel", VesselFromLloydsNum(TestLloydsNum), consol.JK_JX_JV_NKVessel);
				AssertEquals("Consols Voyage", TestVoyageNum, consol.JK_JX_JV_VoyageFlight);

				CFSShipment shipment = consol.Shipments[0];
				CFSContainer container = consol.Containers[0];
				AssertEquals("Container Number", TestContainerNumber1, container.JC_ContainerNum);
				AssertEquals("Shipment Housebill", TestHouseBill1, shipment.JS_HouseBill);

				AssertEquals("Outturns Parent ID", outturn1.C5_ParentID, container.PK);
				AssertEquals("Outturns Parent ID", outturn2.C5_ParentID, shipment.PK);
			}
		}

		public void TestInvalidSelectionErrorMessages()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn1 = AddContainerLine(header, underbond, TestContainerNumber1);
			DepotCusOutturn outturn2 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);
			DepotCusOutturn outturn3 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill2);
			Factory.Save();
			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				AssertEquals("No rows selected as list manager is null", -1, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);
				form.Show();
				form.CreateLoadListFromOutturn(null, EventArgs.Empty);
				AssertEquals("Error should have been reported", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				form.SelectOutturn(outturn2);
				form.AddSelectedOutturn(outturn3);
				AssertEquals("Selected Grid Items - Should have both lines", 2, form.seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CreateLoadListFromOutturn(null, EventArgs.Empty);
				AssertEquals("Error should have been reported", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestDetachOutturnFromCFSObject()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CusOutturnHeader header = CreateOutturnHeader();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_C6 = header.PK;
			DepotCusOutturn outturn1 = AddContainerLine(header, underbond, TestContainerNumber1);
			outturn1.C5_ParentID = ZGuid.NewZGuid();
			outturn1.C5_ParentTableCode = JobContainerSchema.Constants.Prefix;
			DepotCusOutturn outturn2 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill1);
			outturn2.C5_ParentID = ZGuid.NewZGuid();
			outturn2.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			DepotCusOutturn outturn3 = AddLCLLine(header, underbond, TestContainerNumber1, TestHouseBill1, TestOceanBill2);
			outturn3.C5_ParentID = ZGuid.NewZGuid();
			outturn3.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();
			using (SeaCargoDepotOutturnForm form = new SeaCargoDepotOutturnForm(header))
			{
				AssertEquals("No rows selected as list manager is null", -1, form.seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex);
				form.Show();
				form.DetachOutturnFromLoadList(null, EventArgs.Empty);
				AssertEquals("Error should have been reported", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				form.SelectOutturn(outturn2);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.DetachOutturnFromLoadList(null, EventArgs.Empty);
				Assert("Value should not have changed", !outturn2.C5_ParentID.IsEmpty);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.DetachOutturnFromLoadList(null, EventArgs.Empty);
				AssertEquals("Outturn parent should be cleared", ZGuid.Empty, outturn2.C5_ParentID);
				AssertEquals("Outturn Parent table should be cleared", ZString.Empty, outturn2.C5_ParentTableCode);
				form.DeselectAllOutturns();
				form.SelectOutturn(outturn1);
				form.AddSelectedOutturn(outturn3);
				AssertEquals("Selected Grid Items - Should have both lines", 2, form.seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.DetachOutturnFromLoadList(null, EventArgs.Empty);
				AssertEquals("Outturn parent should be cleared", ZGuid.Empty, outturn1.C5_ParentID);
				AssertEquals("Outturn parent should be cleared", ZGuid.Empty, outturn3.C5_ParentID);
			}
		}

		public void TestBindingSeaCargoOutturnMessages()
		{
			using (var testForm = new SeaCargoDepotOutturnForm())
			{
				testForm.Show();
				var ediMgeControl = testForm.Controls.Find("MessageUserControl", true)[0] as EDIMessageUserControl;
				AssertEquals("SeaCargoOutturnMessages", ediMgeControl.MessagesGrid.BindTo);
			}
		}

		public void TestFormHeading()
		{
			var cusOutturnHeader = Factory.New<CusOutturnHeader>();
			cusOutturnHeader.C6_SendersMessageReference = "O00000017";
			using (var testForm = new SeaCargoDepotOutturnForm(cusOutturnHeader))
			{
				testForm.Show();
				AssertEquals("Sea Cargo Outturn O00000017", testForm.FormHeading);
			}

			using (var testForm = new SeaCargoDepotOutturnForm())
			{
				testForm.Show();
				AssertEquals("Simple constructor loads without exception", "Sea Cargo Outturn", testForm.FormHeading);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new SeaCargoDepotOutturnForm(CusOutturnHeader.New(Factory));
			result.ControllerID = ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController;
			return result;
		}

		CusOutturnHeader CreateOutturnHeader()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_LloydsIMO = TestLloydsNum;
			header.C6_VoyageNum = TestVoyageNum;
			header.C6_OutturningPremiseID = TestPremiseID;
			return header;
		}

		DepotCusOutturn AddContainerLine(CusOutturnHeader header, CusUnderbond underbond, ZString containerNumber)
		{
			DepotCusOutturn result = header.Outturns.AddNew();
			result.C5_ContainerNumber = containerNumber;
			result.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			if (underbond != null)
			{
				result.C5_C4_Underbond = underbond.PK;
			}
			return result;
		}

		DepotCusOutturn AddLCLLine(CusOutturnHeader header, CusUnderbond underbond, ZString containerNumber, ZString houseBill, ZString oceanBillNum)
		{
			DepotCusOutturn result = header.Outturns.AddNew();
			result.C5_ContainerNumber = containerNumber;
			result.C5_HouseBill = houseBill;
			result.C5_MasterBill = oceanBillNum;
			result.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			if (underbond != null)
			{
				result.C5_C4_Underbond = underbond.PK;
			}
			return result;
		}

		ZString VesselFromLloydsNum(ZString lloydsNum)
		{
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNum));
			return vessel.RV_Code;
		}

		const string TestLloydsNum = "8610033";
		const string TestPremiseID = "9914N";
		const string TestVoyageNum = "421S";

		const string TestContainerNumber1 = "CTRL0000011";
		const string TestHouseBill1 = "HB393029";
		const string TestOceanBill1 = "OBL20934802";
		const string TestOceanBill2 = "OBL20481987";
	}
}
