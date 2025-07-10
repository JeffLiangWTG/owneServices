using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ETailCusUnderbondDataObjectReaderTest : DataTransfer.Universal.Testing.DataObjectReaderTestHelper
	{
		[TestDate(2018, 2, 23)]
		public void TestCOARecipientRole()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			consol.JK_MasterBillNum = "OB1";

			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S001";
			forwardingShipment.JS_HouseBill = "02";

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			cusMAWB.CM_MasterHouseBill = "02";
			cusMAWB.CM_MAWB = "OB1";

			var evt = Factory.New<StmALog>();
			using (evt.LockForUpdatingKeyFieldsForTesting())
			{
				evt.SL_Table = cusMAWB.TableName;
				evt.SL_Parent = cusMAWB.PK;
				evt.SL_SE_NKEvent = AutoEvents.HVLVReadyCode;
				evt.SL_Reference = "RES=Outturn";
				evt.SL_EventTime = ZDateTime.Now;
			}

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S001");
			var recipient = new RecipientRoleDetail();
			recipient.Type = RecipientRoleType.COA;
			((DataContext)dataContext).RecipientRoleCollection = new List<RecipientRole>
			{
				RecipientRole.New(recipient)
			};

			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
			};
			var subShipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>());
			shipment.SetSubShipmentCollection(() => new DataObjectList<UShipment> { subShipment });

			Factory.SaveForTesting();

			var reader = new ETailCusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals("The original underbond should be retrieved", underbond.PK, testUnderbond.PK);
			AssertEquals("The date should be set against the event", ZDateTime.Now, underbond.C4_Outurned);
		}

		public void TestLogOutturnsReadyForSendingEvent_OutturnsExist()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			consol.JK_MasterBillNum = "OB1";

			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S001";
			forwardingShipment.JS_HouseBill = "02";

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";
			mawb.CM_MasterHouseBill = "02";
			var houseBill = mawb.FilteredChildBills.AddNew();

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = mawb.TablePrefix;

			var outturn = underbond.Outturns.AddNew();
			outturn.C5_HouseBill = "OB1";
			outturn.C5_ParentID = houseBill.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S001");
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>
			{
				new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ShipmentType = new CodeDescriptionPair
					{
						Code = "HVL",
						Description = "HVLV"
					},
					WayBillNumber = "HB1",
					WayBillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					}
				}
			});

			var reader = new ETailCusUnderbondDataObjectReader(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Error message is empty, event AUT added", true, mawb.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == Events.AUOutturnReadyForSendingCode));
			AssertContains("Log information", "Information - Added event 'AUT' for Air Cargo OB1.", logger.Logs);
		}

		public void TestLogOutturnsReadyForSendingEvent_OutturnsDontExist()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			consol.JK_MasterBillNum = "OB1";

			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S001";
			forwardingShipment.JS_HouseBill = "02";

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";
			mawb.CM_MasterHouseBill = "02";
			var houseBill = mawb.FilteredChildBills.AddNew();
			houseBill.CS_HAWB = "HB1";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = mawb.TablePrefix;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S001");
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>
			{
				new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ShipmentType = new CodeDescriptionPair
					{
						Code = "HVL",
						Description = "HVLV"
					},
					WayBillNumber = "HB1",
					WayBillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					}
				}
			});
			var reader = new ETailCusUnderbondDataObjectReader(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Error message is not empty, hence event AUT is not added", false, mawb.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == Events.AUOutturnReadyForSendingCode));
			AssertNotContains("Log information", "Information - Added event 'AUT' for Air Cargo OB1.", logger.Logs);
		}
	}
}
