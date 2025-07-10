using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Customs.EU.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ExitControlSendToCustomsMenuCreatorBaseOnlyTest : ExitControlSendToCustomsMenuCreatorAbstractTest
{
	public void TestJobDeclarationFormSavingIsCalled()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
		using (ExitControlCustomsDataRegistry.Instance.EnableExitControlPlugin.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "DEC - PreSave";
			var header = Factory.New<CusExitHeader>();
			header.CXH_ParentID = declaration.PK;
			header.CXH_ParentTableCode = declaration.TablePrefix;
			header.CXH_OwnerReference = "REF - PreSave";
			Factory.Save();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.ExitSummaryController);
				plugIn.Enabled = true;
				plugIn.TopLevelMenu.OnPopup(EventArgs.Empty);
				var sendToCustomsMenuItem = plugIn.TopLevelMenu.MenuItems.FindByName(ExitControlSendToCustomsMenuCreator.SendToCustomsMenuItemName);
				declaration.JE_GoodsDescription = "DEC";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("JobDeclaration Not Saved Dialog", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertEquals("JobDeclaration Now Saved", false, declaration.HasChanges);
				header.CXH_OwnerReference = "REF";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("CusExitHeader Not Saved Dialog", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertEquals("CusExitHeader now saved", false, declaration.HasChanges);
			}
		}
	}

	public void TestShipmentFormSavingIsCalled()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
		using (ExitControlCustomsDataRegistry.Instance.EnableExitControlPlugin.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP - PreSave";
			shipment.JS_RL_NKOrigin = "IEDUB";
			shipment.JS_RL_NKDestination = "AUSYD";
			var header = Factory.New<CusExitHeader>();
			header.CXH_ParentID = shipment.PK;
			header.CXH_ParentTableCode = shipment.TablePrefix;
			header.CXH_OwnerReference = "REF - PreSave";
			Factory.Save();
			Freight.Business.ChildEditableService.SetState(Factory, Enterprise.Freight.Integration.ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.ExitSummaryController);
				plugIn.Enabled = true;
				plugIn.TopLevelMenu.OnPopup(EventArgs.Empty);
				var sendToCustomsMenuItem = plugIn.TopLevelMenu.MenuItems.FindByName(ExitControlSendToCustomsMenuCreator.SendToCustomsMenuItemName);
				shipment.JS_GoodsDescription = "SHP";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("JobShipment Not Saved Dialog", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertEquals("JobShipment Now Saved", false, shipment.HasChanges);
				header.CXH_OwnerReference = "REF";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("CusExitHeader Not Saved Dialog", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertEquals("CusExitHeader now saved", false, shipment.HasChanges);
			}
		}
	}

	public void TestSaveParentBusinessObjectBeforeCreate()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "DEC - PreSave";
		var header = Factory.New<CusExitHeader>();
		header.CXH_ParentID = declaration.PK;
		header.CXH_ParentTableCode = declaration.TablePrefix;
		header.CXH_OwnerReference = "REF - PreSave";
		declaration.RegisterEditableChildObject(header);
		Factory.Save();
		declaration.JE_GoodsDescription = "DEC";
		var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreator(header).Create();
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		sendToCustomsMenuItem.PerformClick();
		AssertEquals("JobDeclaration Not Saved Dialog", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("JobDeclaration Now Saved", false, declaration.HasChanges);
		header.CXH_OwnerReference = "REF";
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		sendToCustomsMenuItem.PerformClick();
		AssertEquals("CusExitHeader Not Saved Dialog", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("CusExitHeader now saved", false, declaration.HasChanges);
	}
}
