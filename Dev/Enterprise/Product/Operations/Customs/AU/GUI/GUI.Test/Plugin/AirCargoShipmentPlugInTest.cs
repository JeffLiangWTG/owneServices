using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AirCargoShipmentPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestOnSaveCompletedOrAborted()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			var message = hawb.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				testPlugIn.OnSaveCompletedOrAborted(true);
				Assert(!message.IsDeleted);
				testPlugIn.OnSaveCompletedOrAborted(false);
				Assert(message.IsDeleted);
			}
		}

		public void TestDontCreateCusMAWB_HAWBWhenMutexLockedForConsol()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testPlugIn.MutexForConsol.Lock();

				var anotherFactory = new BusinessObjectFactory();
				var shipmentLoaded = anotherFactory.Load<ForwardingShipment>(shipment.PK);

				shipmentLoaded.Consols[0].JK_RL_NKDischargePort = "AUSYD";

				using (var plugIn2 = new AirCargoShipmentPlugIn(shipmentLoaded))
				{
					testForm.Controls.Add(plugIn2.UserControl);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugIn2.OnGUIShown();
					AssertEquals("Didnt create a master bill for the consol", null, plugIn2.MasterBill);
					AssertEquals("Didnt create a house bill for the shipment", null, plugIn2.HouseBill);
				}
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			using (var plugIn = new AirCargoShipmentPlugIn(shipment))
			{
				AssertEquals("Master/House bill not created yet", false, ShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("PlugIn can be shown when master/house bill has been created", true, QueryUserShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
				AssertEquals("PlugIn can be shown when master/house bill has been created", true, ShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
			}
		}

		public void TestNZCusHAWBDoesNotCauseException()
		{
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var cusMAWB = factory2.New<Integration.Customs.NZ.ICusMAWB>();
			var cusHAWB = factory2.New<Integration.Customs.NZ.ICusHAWB>();
			cusHAWB.CS_CM = cusMAWB.PK;
			cusHAWB.CS_JS = shipment.PK;
			factory2.Save();

			using (var plugIn = new AirCargoShipmentPlugIn(shipment))
			{
				AssertEquals("Master/House bill not created yet", false, ShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("PlugIn can be shown when master/house bill has been created", true, QueryUserShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
				AssertEquals("PlugIn can be shown when master/house bill has been created", true, ShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));

				var entryNumbers = shipment.CusEntryNumbers; // internally loads CusHAWBs to local cache
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, plugIn.ShowPreSaveDialogs()); // uses HouseBillFromLocalCache 
				Factory.Save();
			}
		}

		public void TestConcurrency()
		{
			using (var form = new ZForm())
			using (var testPlugin = new AirCargoShipmentPlugInForTest(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Controls.Add(testPlugin.UserControl);
				testPlugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();

				using (var form2 = new ZForm())
				using (var testPlugin2 = new AirCargoShipmentPlugInForTest(new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK)))
				{
					form2.Controls.Add(testPlugin2.UserControl);
					testPlugin2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
					AssertEquals(AirCargoShipmentPlugIn.SomeoneIsEditingTheShipment, testPlugin2.PlugInNotDisplayedMessage);
					AssertEquals("Should only have created 1 CusMAWB", 1, Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, consol.PK)).Length);
				}
			}
		}

		public void TestRegisterMAWBAsEditableChild()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_JS = shipment.PK;

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull("MAWB is created", testPlugIn.MasterBill);
				AssertNotNull("HAWB is created", testPlugIn.HouseBill);
				AssertEquals("MAWB is registered as Editable child", true, testPlugIn.MasterBill.IsRegisteredEditableChildObject(testPlugIn.MasterBill.ChildBills));
			}
		}

		public void TestDontCreateCusHAWBWhenMutexLockedForShipment()
		{
			var mAWB = CusMAWB.CreateNew(consol);
			Factory.Save();

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();

				AssertEquals("Mutex is not locked:Consol", false, testPlugIn.MutexForConsol.IsLocked);

				var anotherFactory = new BusinessObjectFactory();
				var shipmentLoaded = anotherFactory.Load<ForwardingShipment>(shipment.PK);

				shipmentLoaded.Consols[0].JK_RL_NKDischargePort = "AUSYD";

				using (var plugIn2 = new AirCargoShipmentPlugIn(shipmentLoaded))
				{
					testForm.Controls.Add(plugIn2.UserControl);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugIn2.OnGUIShown();
					AssertEquals("Didnt create a master bill for the consol", mAWB.PK, plugIn2.MasterBill.PK);
					AssertNotNull("Create a house bill for the shipment if not exist.", plugIn2.HouseBill);
				}
			}
		}

		public void TestReleaseLockWhenSaved()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();

				AssertNotNull(testPlugIn.HouseBill);
				Factory.Save();

				using (var testPlugIn2 = new AirCargoShipmentPlugIn(shipment))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					testPlugIn2.OnGUIShown();
					AssertEquals("Mutex should have been released and the second plugin should retrieve house bill saved", "", testPlugIn.PlugInNotDisplayedMessage);
					AssertEquals(testPlugIn.HouseBill, testPlugIn2.HouseBill);
				}
			}
		}

		public void TestReleaseLockWhenDisposed()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull(testPlugIn.HouseBill);
				//did not save, should release mutex when plugin is disposed
			}

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull("previous plugin should release mutex when plugin is disposed", testPlugIn.HouseBill);
			}
		}

		public void TestGetCusHAWBFromShipmentWhenLockIsReleasedAndHAWBIsSaved()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();

				var anotherFactory = new BusinessObjectFactory();
				var shipmentLoaded = anotherFactory.Load<ForwardingShipment>(shipment.PK);

				shipmentLoaded.Consols[0].JK_RL_NKDischargePort = "AUSYD";

				using (var plugIn2 = new AirCargoShipmentPlugIn(shipmentLoaded))
				{
					testForm.Controls.Add(plugIn2.UserControl);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugIn2.OnGUIShown();
					AssertEquals("Get a mawb created for plugin1", testPlugIn.MasterBill.PK, plugIn2.MasterBill.PK);
					AssertEquals("Get a hawb created for plugin1", testPlugIn.HouseBill.PK, plugIn2.HouseBill.PK);
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestShipmentFormSavedOKWithoutClickingAirCargoTab()
		{
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var testForm = new ShipmentForm(shipment))
			{
				foreach (var plugIn in testForm.PlugIns.Instances)
				{
					plugIn.ShowPreSaveDialogs();
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestDeleteHAWB()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				testPlugIn.HouseBill.Delete();
				Factory.Save();
			}
		}

		[ExpectNoExceptions()]
		public void TestGetHouseBillFromShipment()
		{
			var houseBill = Factory.New<CusHAWB>();
			houseBill.CS_JS = shipment.PK;

			var standAloneHAWB1 = Factory.New<CusHAWB>();
			var standAloneHAWB2 = Factory.New<CusHAWB>();//CS_JS is not a unique key any more

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				testPlugIn.OnGUIShown();
			}
		}

		[ExpectNoExceptions()]
		public void TestGetMasterBillFromConsol()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;

			var standAloneMAWB1 = Factory.New<CusMAWB>();
			var standAloneMAWB2 = Factory.New<CusMAWB>();//CM_JK is not a unique key any more

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				testPlugIn.OnGUIShown();
			}
		}

		public void TestLoadZPlugIn()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				testPlugIn.OnGUIShown();
				AssertNotNull(testPlugIn.UserControl);
			}
		}

		[TestDate(2005, 12, 12)]
		public void TestDefaultHouseBillFromShipment_CMR()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Master123456";
			masterShipment.CoLoadShipments.Add(shipment);

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "AUSYD";

			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_GoodsValue = 1000m;
			shipment.JS_HouseBill = "ABC123456";
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true).AddToFilter(OrgHeaderSchema.OH_Code, "UNMATCHED"));
			consignee.OH_RL_NKClosestPort = "AUSYD";
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true)).PK;
			shipment.JS_INCO = Constants.IncoTerms.DefaultIncoFromPaymentType("PPD");
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RX_NKGoodsValueCurr = "NZD";

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();

				var houseBill = testPlugIn.HouseBill;
				var consignorAddress = houseBill.ConsignorAddressForPortOfOrigin;
				var consigneeAddress = houseBill.ConsigneeAddressForPortOfDestination;

				AssertEquals("Pieces Manifested", shipment.JS_OuterPacks, houseBill.CS_PiecesManifested);
				AssertEquals("Weight", shipment.JS_ActualWeight, houseBill.CS_Weight);
				AssertEquals("Weight UQ", shipment.JS_UnitOfWeight, houseBill.CS_WeightUQ);
				AssertEquals("Goods Description", shipment.JS_GoodsDescription, houseBill.CS_GoodsDescription);
				AssertEquals("Goods Value", shipment.JS_GoodsValue, houseBill.CS_GoodsValue);
				AssertEquals("Goods Currency", shipment.JS_RX_NKGoodsValueCurr, houseBill.CS_RX_NKGoodsCurrency);
				AssertEquals("HouseBill Number", shipment.JS_HouseBill, houseBill.CS_HAWB);
				AssertEquals("Master HouseBill Number", masterShipment.JS_HouseBill, houseBill.CS_MasterHouseBill);
				AssertEquals("PrePaid Collect", new CMRUtilities().ConvertOldAirCargoPaymentType(shipment.JS_PaymentTerm), houseBill.CS_FreightPrepaidCollect);
				AssertEquals("Destination", shipment.JS_RL_NKDestination, houseBill.CS_RL_NKDestination);
				AssertEquals("Origin", shipment.JS_RL_NKOrigin, houseBill.CS_RL_NKOrigin);

				AssertNotNull("Consignee", shipment.Consignee);
				Assert("Consingee Name", shipment.Consignee.OH_FullName.StartsWith(houseBill.CS_ConsigneeName));
				Assert("Consignee Address", shipment.ConsigneeDocumentaryAddress.E2_Address1.StartsWith(houseBill.CS_ConsigneeStreet));
				Assert("Consignee City", shipment.ConsigneeDocumentaryAddress.E2_City.StartsWith(houseBill.CS_ConsigneeCity));
				Assert("Consignee State", shipment.ConsigneeDocumentaryAddress.E2_State.StartsWith(houseBill.CS_ConsigneeState));
				Assert("Consignee PostCode", shipment.ConsigneeDocumentaryAddress.E2_Postcode.StartsWith(houseBill.CS_ConsigneePostcode));
				Assert("Consignee Phone", shipment.ConsigneeDocumentaryAddress.E2_Phone.StartsWith(houseBill.CS_ConsigneePhone));
				AssertEquals("Consignee Ctry/Rgn.", shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode, houseBill.CS_RN_NKConsigneeCountry);

				AssertNotNull("Consignor", shipment.Consignor);
				Assert("Consingor Name", shipment.Consignor.OH_FullName.StartsWith(houseBill.CS_ConsignorName));
				Assert("Consignor Address", shipment.ConsignorDocumentaryAddress.E2_Address1.StartsWith(houseBill.CS_ConsignorStreet));
				Assert("Consignor City", shipment.ConsignorDocumentaryAddress.E2_City.StartsWith(houseBill.CS_ConsignorCity));
				Assert("Consignor State", shipment.ConsignorDocumentaryAddress.E2_State.StartsWith(houseBill.CS_ConsignorState));
				Assert("Consignor PostCode", shipment.ConsignorDocumentaryAddress.E2_Postcode.StartsWith(houseBill.CS_ConsignorPostcode));
				Assert("Consignor Phone", shipment.ConsignorDocumentaryAddress.E2_Phone.StartsWith(houseBill.CS_ConsignorPhone));
				AssertEquals("Consignor Ctry/Rgn.", shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode, houseBill.CS_RN_NKConsignorCountry);

				//References
				AssertEquals("Reference to Shipment", shipment.PK, houseBill.CS_JS);
			}
		}

		public void TestDefaultMasterBillFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();

			var transport = consol.Transports[0];
			transport.JW_ATA = ZDateTime.Today;
			consol.JK_MasterBillNum = "08100000000";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKL";
			transport.JW_VoyageFlight = "QF123";

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.Consols.Load();

			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				var masterBill = testPlugIn.MasterBill;

				AssertNotNull(masterBill);
				AssertEquals("Reference to Consol", consol.PK, masterBill.CM_JK);
				AssertEquals("MasterBill Number", consol.JK_MasterBillNum, masterBill.CM_MAWB);
				AssertEquals("Actual Arriving", consol.JK_JX_JB_A_ARV, masterBill.CM_ArrivalDate);
				AssertEquals("Discharge", consol.JK_RL_NKDischargePort, masterBill.CM_RL_NKDischargePort);
				AssertEquals("Load", consol.JK_RL_NKLoadPort, masterBill.CM_RL_NKLoadPort);
				AssertEquals("Flight No", consol.JK_JX_JV_VoyageFlight, masterBill.CM_FlightNo);
			}
		}

		/// <summary>
		/// AirCargo only uses "KG" and "LB". If a shipment's weight is in other units, it should be translated into a KG
		/// </summary>
		public void TestDefaultWeightFromDifferentUnit()
		{
			shipment.JS_ActualWeight = 1m;
			shipment.JS_UnitOfWeight = "T";

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "AUSYD";

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();

				var houseBill = testPlugIn.HouseBill;
				AssertEquals("Weight in T is translated into KG", "KG", houseBill.CS_WeightUQ);
				AssertEquals("Weight", 1000m, houseBill.CS_Weight);
			}
		}

		/// <summary>
		/// MasterHouseBill should have CS_IsMasterHouse as 'Y'
		/// </summary>
		public void TestIsMasterFlag()
		{
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.CoLoadShipments.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "AUSYD";

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				var houseBill = testPlugIn.HouseBill;

				AssertEquals("IsMaster", true, houseBill.CS_IsMasterHouse);
			}
		}

		/// <summary>
		/// If there is more than one consol, choose the one with AU Discharge Port.
		/// </summary>
		public void TestGetConsolFromShipment()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKDischargePort = "NZAKL";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();

				AssertEquals("Reference to Consol", consol1.PK, testPlugIn.HouseBill.MAWB.CM_JK);
			}
		}

		public void TestGetMAWB()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;

			shipment1.Consols.Load();
			shipment2.Consols.Load();

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();

				using (var testPlugIn2 = new AirCargoShipmentPlugIn(shipment))
				{
					testForm.Controls.Add(testPlugIn2.UserControl);
					testPlugIn2.OnGUIShown();

					Assert("MasterBill should be the same one", testPlugIn.MasterBill.PK == testPlugIn2.MasterBill.PK);
				}
			}
		}

		public void TestSeaTransportMakeAirCargoPlugInInvisible()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var tabControl = new ZTabControl())
			{
				var plugInTabPage = new ZTabPagePlugIn(testPlugIn);
				tabControl.TabPages.Add(plugInTabPage);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;

				AssertEquals("User Control Visibility", false, !tabControl.Contains(plugInTabPage));

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("User Control Visibility", true, tabControl.Contains(plugInTabPage));
			}
		}

		public void TestAirCargoPlugAlwaysReturnsAValidBusinessObjectToBindTo()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnMenuShown();
				AssertNotNull("We need a business object to bind to when saving", testPlugIn.HouseBill);
			}
		}

		public void TestCreatingCustomsJobDoesntAffectShipmentHasChanges()
		{
			Assert("Shipment hasn't been changed", !shipment.HasChanges);

			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull("PreCondition : Customs Job Created", testPlugIn.HouseBill);
				Assert("Shipment Hasn't been changed after creating customs job", !shipment.HasChanges);
				Assert("House Bill hasn't been changed", !testPlugIn.HouseBill.HasChanges);
			}
		}

		public void TestDontCreateNewCustomsJobWhenLoaded()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				AssertNull("Customs Job is null", testPlugIn.HouseBill);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull("Customs Job Created", testPlugIn.HouseBill);
			}
		}

		public void TestSetCurrentShipmentToCusMAWB()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull("CurrentShipment has been set to load only one CusHAWB for plugin to Shipment", testPlugIn.MasterBill.CurrentHouseBill);
			}
		}

		public void TestAirCargoShipmentPlugInVisibility()
		{
			consol.Delete(); // delete this extra consol
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			using (var testForm = new ZForm())
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				testPlugIn.OnGUIShown();

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				var aUConsol = shipment.Consols.AddNew();
				aUConsol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("AirCargo Tab Visible", true, testPlugIn.Enabled);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("AirCargo Tab Visible", false, testPlugIn.Enabled);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("AirCargo Tab Visible", true, testPlugIn.Enabled);

				aUConsol.JK_RL_NKDischargePort = "NZAKL";//Changed to NZ one
				AssertEquals("AirCargo Tab Visible", false, testPlugIn.Enabled);

				aUConsol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("AirCargo Tab Visible", true, testPlugIn.Enabled);

				shipment.Consols.Remove(aUConsol);
				AssertEquals("AirCargo Tab Visible", false, testPlugIn.Enabled);
			}
		}

		public void TestInitialVisibility()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				using (var testForm = new ZForm())
				{
					testForm.Controls.Add(testPlugIn.UserControl);
					testPlugIn.OnGUIShown();
					AssertEquals("Plug In Enabled", true, testPlugIn.Enabled);
				}
			}
		}

		public void TestShowPreSaveDialogs()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				AssertEquals("ShowPreSaveDialogs", ContinueWithSave.Yes, testPlugIn.ShowPreSaveDialogs());
			}
		}

		public void TestManager()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				AssertEquals("Manager type", typeof(CusHAWBMessageManager), testPlugIn.Manager.GetType());
			}
		}

		[ExpectNoExceptions]
		public void TestCanDeleteDoesNotThrowException()
		{
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				var houseBill = Factory.New<CusHAWB>();
				testPlugIn.HouseBill = houseBill;
				houseBill.Delete();

				var dummyBool = testPlugIn.CanDelete;
			}
		}

		public void TestReloadHouseBillIfCS_CMIsEmpty()
		{
			using (var testForm = new ZForm())
			using (var testPlugIn = new AirCargoShipmentPlugInForTest(shipment))
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				if (testPlugIn.HouseBill != null)
				{
					shipment.Consols.Remove(testPlugIn.ConsolInternal);
					shipment.Consols.Add(testPlugIn.ConsolInternal);
					AssertEquals("CS_CM is not null", testPlugIn.MasterBill.PK, testPlugIn.HouseBill.CS_CM);
				}
			}
		}

		public void TestQueryUserToCreateAirCargoJob()
		{
			using (var testForm = new ZForm())
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				testForm.Controls.Add(testPlugIn.UserControl);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testPlugIn.OnGUIShown();
				AssertEquals(AirCargoShipmentPlugIn.YouHaveChosenNotToCreateAnAirCargo, testPlugIn.PlugInNotDisplayedMessage);
				AssertNull("Refused to create an air cargo.", testPlugIn.MasterBill);
				AssertNull(testPlugIn.HouseBill);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull("Accepted to create an air cargo.", testPlugIn.MasterBill);
				AssertNotNull(testPlugIn.HouseBill);
				Assert("Saved to database.", testPlugIn.MasterBill.IsInDatabase);
				Assert(testPlugIn.HouseBill.IsInDatabase);
			}
		}

		public void TestQueryUserToCreateAirCargoJob_Legacy()
		{
			var mawb = Factory.New<Business.CusMAWB>();
			mawb.CM_ApplicationCode = "";
			mawb.CM_JK = consol.PK;

			using (var testForm = new ZForm())
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				testForm.Controls.Add(testPlugIn.UserControl);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertEquals("Air Cargo Data is Invalid.", testPlugIn.PlugInNotDisplayedMessage);
				AssertNull("Refused to create an air cargo.", testPlugIn.MasterBill);
				AssertNull(testPlugIn.HouseBill);
			}
		}

		public void TestAskUserToSaveBeforeCreateAirCargoJob()
		{
			using (var testForm = new ZForm())
			using (var testPlugIn = new AirCargoShipmentPlugIn(shipment))
			{
				testForm.Controls.Add(testPlugIn.UserControl);
				shipment.HasChanges = true;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testPlugIn.OnGUIShown();
				AssertEquals(AirCargoShipmentPlugIn.YouHaveToSaveBeforeCreateAnAirCargo, testPlugIn.PlugInNotDisplayedMessage);
				AssertNull("Accepted to create an air cargo, but refused to save the shipment.", testPlugIn.MasterBill);
				AssertNull(testPlugIn.HouseBill);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.OnGUIShown();
				AssertNotNull("Accepted to create an air cargo and save the shipment.", testPlugIn.MasterBill);
				AssertNotNull(testPlugIn.HouseBill);
				Assert("Saved to database.", testPlugIn.MasterBill.IsInDatabase);
				Assert(testPlugIn.HouseBill.IsInDatabase);
			}
		}

		public void TestRaceConditionAndOtherCountryRowsIgnored()
		{
			Factory.RefreshEnabled = false;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			mawb.CM_ApplicationCode = Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var shipment = consol.Shipments.AddNew();
			var nzCusMawb = Factory.New<Business.CusMAWB>();
			nzCusMawb.CM_ApplicationCode = Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			nzCusMawb.CM_JK = consol.PK;
			var nzCusHAWB = Factory.New<CusHAWBInOtherCountry>();
			nzCusMawb.ChildBills.Add(nzCusHAWB);
			nzCusHAWB.CS_JS = shipment.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;
			var shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);
			var shipmentInFactory3 = factory3.Load<ForwardingShipment>(shipment.PK);
			using (var plugin2 = new AirCargoShipmentPlugIn(shipmentInFactory2))
			{
				plugin2.CreateAirCargoJobIfRequired();
			}

			factory2.Save();
			using (var plugin3 = new AirCargoShipmentPlugIn(shipmentInFactory3))
			{
				plugin3.CreateAirCargoJobIfRequired();
			}

			factory3.Save();
			var query = new ZQuery(CusHAWBSchema.CS_JS, shipment.PK);
			query.ReLoadExistingRows = true;
			AssertEquals("Only one new created", 1, Factory.Load<Business.CusHAWB>(query).OfType<CusHAWB>().Count());
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_JS = shipment.PK;
			return new AirCargoShipmentPlugIn(shipment);
		}

		ForwardingShipment shipment;
		ForwardingConsol consol;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";

			shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			//Consol Collection in Shipment needs this to get loaded correctly(to create a pivot object)
			Factory.Save();
			shipment.Consols.Load();
		}

		bool ShouldPlugInGUIAndBusinessEntityBeCreated(ZPlugIn plugIn)
		{
			MethodInfo method = typeof(ZPlugIn).GetMethod("ShouldPlugInGUIAndBusinessEntityBeCreated", BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
			return (bool)method.Invoke(plugIn, null);
		}

		bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated(ZPlugIn plugIn)
		{
			MethodInfo method = typeof(ZPlugIn).GetMethod("QueryUserShouldPlugInGUIAndBusinessEntityBeCreated", BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
			return (bool)method.Invoke(plugIn, null);
		}

		sealed class CusHAWBInOtherCountry : Business.CusHAWB
		{
			public CusHAWBInOtherCountry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		sealed class AirCargoShipmentPlugInForTest : AirCargoShipmentPlugIn
		{
			public AirCargoShipmentPlugInForTest(CommonShipment shipment)
				: base(shipment)
			{
			}

			internal bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();

			internal ForwardingConsol ConsolInternal => Consol;
		}
	}
}
