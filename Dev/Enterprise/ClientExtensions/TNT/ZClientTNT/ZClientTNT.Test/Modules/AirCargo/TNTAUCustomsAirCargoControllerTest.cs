using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module.AirCargo.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(TNTAUCustomsAirCargoController))]
	public class TNTAUCustomsAirCargoControllerTest : AUCustomsAirCargoControllerTest
	{
		public void TestShowImportedNewForm()
		{
			TNTAUCustomsAirCargoController controller = (TNTAUCustomsAirCargoController)ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargo);
			AirCargo.PopulateDataCoreWasCalled = false;
			using (ZForm form = (ZForm)controller.ShowImportedNewForm(AirCargo))
			{
				CusMAWB masterBill = form.BusinessEntity as CusMAWB;
				AssertNotNull("MasterBill should not be null", masterBill);
				AssertEquals("MasterBill should has changes", true, masterBill.HasChanges);
				AssertEquals("PopulateDataCoreWasCalled is true", true, AirCargo.PopulateDataCoreWasCalled);
				form.Close();
			}
		}

		#region TestShowImportedEditForm
		public void TestShowImportedEditForm()
		{
			TNTAUCustomsAirCargoController controller = (TNTAUCustomsAirCargoController)ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargo);
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "MasterBill";
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = "HouseBill";
			Factory.Save();
			AirCargo.PopulateDataCoreWasCalled = false;
			using (ZForm form = (ZForm)controller.ShowImportedEditForm(masterBill.PK, AirCargo))
			{
				CusMAWB formMasterBill = form.BusinessEntity as CusMAWB;
				AssertNotNull("FormMasterBill should not be null", formMasterBill);
				AssertEquals("FormMasterBill should be the same as MasterBill", masterBill.PK, formMasterBill.PK);
				AssertEquals("FormMasterBill should has changes", true, formMasterBill.HasChanges);
				AssertEquals("PopulateDataCoreWasCalled is true", true, AirCargo.PopulateDataCoreWasCalled);
				form.Close();
			}
		}

		public void TestShowImportedEditForm_MissingHousebills()
		{
			TNTAUCustomsAirCargoController controller = (TNTAUCustomsAirCargoController)ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargo);
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "081323423";
			Factory.Save();
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			using (ZForm form = (ZForm)controller.ShowImportedEditForm(masterBill.PK, AirCargo))
			{
				string expectedErrorMessage = string.Format("{0} hasn't got any Housebill.{1}System cannot load it.", masterBill.UnderbondHumanReadableName, System.Environment.NewLine);
				var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
				AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
				AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, expectedErrorMessage, lastMessage);
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestShowImportedEditForm_ArgumentException()
		{
			TNTAUCustomsAirCargoController controller = (TNTAUCustomsAirCargoController)ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargo);
			using (ZForm form = (ZForm)controller.ShowImportedEditForm(ZGuid.Empty, AirCargo))
			{
			}
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			FlightRec = new FlightRecord(FlightDetailLine);
			AirCargo = new ImportAirCargoTest.ImportAirCargoTestClass(Factory, FlightRec, BranchCode);
		}

		FlightRecord FlightRec;
		ImportAirCargoTest.ImportAirCargoTestClass AirCargo;
		const string BranchCode = "SYD";
		const string FlightDetailLine = "01BA0151SINSYD150705A12527013486 M0000151  SINSYDTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		#endregion
	}
}
