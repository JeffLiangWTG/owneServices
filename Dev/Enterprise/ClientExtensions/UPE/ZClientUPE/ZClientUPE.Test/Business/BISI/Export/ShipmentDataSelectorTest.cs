using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentDataSelectorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		[TestDate]
		public void TestGetShipmentsDataToBeExported()
		{
			InsertCusHAWBsForTest();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			Factory.Save();
			fMasterBill = null;
			CreateCusHAWB("999", CargoReportQueueCodeDescriptionPairList.Codes.Hold, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(6));
			MasterBill.CM_GB = branch.PK;
			MasterBill.ChildBills[0].Declaration.JE_GB = branch.PK;
			Factory.Save();
			var selector = new ShipmentDataSelector(Factory, Env.Time.GetLocalTimeFromUtc(StartDate.ToDateTime()), Env.Time.GetLocalTimeFromUtc(StartDate.ToDateTime()).AddSeconds(25));
			IShipmentData[] shipmentDataArray = selector.GetShipmentDataToBeExported();
			Sort(shipmentDataArray);
			AssertEquals(11, shipmentDataArray.Length);
			for (int i = 0; i < shipmentDataArray.Length; i++)
			{
				AssertEquals((100 + i * 2).ToString(), shipmentDataArray[i].ShipmentRef);
			}
		}

		public void TestGetShipmentDataForTradeNet()
		{
			var declaration0 = Factory.New<Customs.SG.V4.Business.JobDeclaration>();
			declaration0.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration0.JE_DeclarationReference = "~B000000001";
			var declaration1 = Factory.New<Customs.SG.V4.Business.JobDeclaration>();
			declaration1.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration1.JE_DeclarationReference = "~B000000002";
			declaration1.Logs.AddNew(Events.DeclarationQueued, "EXTRA|BISI|EXTRA");
			var declaration2 = Factory.New<Customs.SG.V4.Business.JobDeclaration>();
			declaration2.JE_DeclarationReference = "~B000000003";
			declaration2.Logs.AddNew(Events.DataExport, "BISI");
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var selector = new ShipmentDataSelector(Factory, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				var shipmentDataArray = selector.GetShipmentDataToBeExported();
				AssertEquals(1, shipmentDataArray.Length);
				AssertEquals("~B000000002", shipmentDataArray[0].ShipmentRef);
				shipmentDataArray[0].LogBISIEventAfterUploaded();
				Factory.Save();
				selector = new ShipmentDataSelector(Factory, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				shipmentDataArray = selector.GetShipmentDataToBeExported();
				AssertEquals(0, shipmentDataArray.Length);
			}
		}

		public void TestNotNoLockOnStmALogForFilterString()
		{
			var selector = new TestShipmentDataSelector(Factory, Env.Time.GetLocalTimeFromUtc(StartDate.ToDateTime()), Env.Time.GetLocalTimeFromUtc(StartDate.ToDateTime()).AddSeconds(25));
			AssertEquals("StmALog must not be with (nolock) otherwise it will miss records", false, selector.FilterString.ToLower().IndexOf("StmALog with (nolock)".ToLower()) != -1);
		}

		public void TestExportManifestIsExcluded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var importManifest = Factory.New<AsycudaManifestHeader>();
				importManifest.AMA_TransportMode = "AIR";
				importManifest.AMA_MasterBill = "1111111";
				importManifest.AMA_ManifestType = "MGI";
				var importBill = importManifest.Bills.AddNew();
				importBill.ABL_BillNumber = "123456";
				var importPack = importBill.Packs.AddNew();
				var importPackedItem = importPack.PackedItem;
				importBill.Logs.AddNew(Events.StatusChange, "CR");
				var exportManifest = Factory.New<AsycudaManifestHeader>();
				exportManifest.AMA_TransportMode = "AIR";
				exportManifest.AMA_MasterBill = "22222222";
				exportManifest.AMA_ManifestType = "MGE";
				var exportBill = exportManifest.Bills.AddNew();
				exportBill.ABL_BillNumber = "654321";
				var exportPack = exportBill.Packs.AddNew();
				var asycudaPackedItem = exportPack.PackedItem;
				exportBill.Logs.AddNew(Events.StatusChange, "IP");
				Factory.Save();
				var selector = new ShipmentDataSelector(Factory, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				IShipmentData[] shipmentDataArray = selector.GetShipmentDataToBeExported();
				AssertEquals(1, shipmentDataArray.Length);
				AssertEquals("123456", shipmentDataArray[0].ShipmentRef);
				shipmentDataArray[0].LogBISIEventAfterUploaded();
				Factory.Save();
				selector = new ShipmentDataSelector(Factory, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				shipmentDataArray = selector.GetShipmentDataToBeExported();
				AssertEquals(0, shipmentDataArray.Length);
			}
		}

		#region Test Classes
		class TestShipmentDataSelector : ShipmentDataSelector
		{
			public TestShipmentDataSelector(BusinessObjectFactory factory, ZDateTime startDate, ZDateTime endDate) : base(factory, startDate, endDate)
			{
			}

			public new string FilterString
			{
				get
				{
					return base.FilterString;
				}
			}
		}

		#endregion
		#region Implementation
		void Sort(IShipmentData[] shipmentDataArray)
		{
			Array.Sort(shipmentDataArray, new ComparerForTest());
		}

		void InsertCusHAWBsForTest()
		{
			InsertIncludedCusHAWBs();
			InsertNotIncludedCusHAWBs();
		}

		void InsertIncludedCusHAWBs()
		{
			CreateCusHAWB("100", CargoReportQueueCodeDescriptionPairList.Codes.Completed, StartDate);
			CreateCusHAWB("102", CargoReportQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(2));
			CreateCusHAWB("104", CargoReportQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(4));
			CreateCusHAWB("106", CargoReportQueueCodeDescriptionPairList.Codes.Hold, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(6));
			CreateCusHAWB("108", CargoReportQueueCodeDescriptionPairList.Codes.EIR, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(8));
			CreateCusHAWB("110", CargoReportQueueCodeDescriptionPairList.Codes.Pending, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, StartDate.AddSeconds(10));
			CreateCusHAWB("112", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(12));
			CreateCusHAWB("114", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, StartDate.AddSeconds(14));
			CreateCusHAWB("116", CargoReportQueueCodeDescriptionPairList.Codes.Hold, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(16));
			CreateCusHAWB("118", CargoReportQueueCodeDescriptionPairList.Codes.EIR, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, StartDate.AddSeconds(18));
			CreateCusHAWB("120", CargoReportQueueCodeDescriptionPairList.Codes.Hold, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(20));
			CreateCusHAWB("122", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.BCA, StartDate.AddSeconds(21), StartDate.AddSeconds(-50));
			CreateCusHAWB("124", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(22), StartDate.AddSeconds(-2));
		}

		void InsertNotIncludedCusHAWBs()
		{
			CreateCusHAWB("101", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, StartDate);
			CreateCusHAWB("103", CargoReportQueueCodeDescriptionPairList.Codes.Pending, StartDate);
			CreateCusHAWB("105", CargoReportQueueCodeDescriptionPairList.Codes.Hold, StartDate);
			CreateCusHAWB("107", CargoReportQueueCodeDescriptionPairList.Codes.EIR, StartDate);
			CreateCusHAWB("109", CargoReportQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(-1));
			CreateCusHAWB("111", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.BCA, StartDate);
			CreateCusHAWB("113", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Pending, StartDate);
			CreateCusHAWB("115", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.EIR, StartDate);
			CreateCusHAWB("117", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(25));
			CreateCusHAWB("119", CargoReportQueueCodeDescriptionPairList.Codes.Completed, DeclarationQueueCodeDescriptionPairList.Codes.Completed, StartDate.AddSeconds(26));
		}

		void CreateCusHAWB(ZString hAWB, ZString customsQueue, ZString declarationQueue, ZDateTime postedTime)
		{
			CreateCusHAWB(hAWB, customsQueue, declarationQueue, postedTime, postedTime);
		}

		void CreateCusHAWB(ZString hAWB, ZString customsQueue, ZDateTime postedTime)
		{
			CreateCusHAWB(hAWB, customsQueue, null, postedTime);
		}

		void CreateCusHAWB(ZString hAWB, ZString customsQueue, ZString declarationQueue, ZDateTime cusHAWBPostedTime, ZDateTime decPostedTime)
		{
			DateTime testDate = TestDateAttribute.Date;
			try
			{
				TestDateAttribute.Date = cusHAWBPostedTime.ToDateTime();
				UPECusHAWB uPECusHAWB = (UPECusHAWB)MasterBill.ChildBills.AddNew();
				uPECusHAWB.CS_HAWB = hAWB;
				ProcessQueueLog log = uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(customsQueue, "", "", "", "");
				Factory.Save();
				if (!declarationQueue.IsEmpty)
				{
					TestDateAttribute.Date = decPostedTime.ToDateTime();
					uPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
					log = uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew(declarationQueue, "", "", "", "");
					Factory.Save();
				}
			}
			finally
			{
				TestDateAttribute.Date = testDate;
			}
		}

		CusMAWB MasterBill
		{
			get
			{
				if (fMasterBill == null)
				{
					fMasterBill = Factory.New<CusMAWB>();
				}

				return fMasterBill;
			}
		}

		ZDateTime StartDate
		{
			get
			{
				if (fStartDate.IsEmpty)
				{
					fStartDate = new ZDateTime(2005, 10, 10, 10, 10, 10);
				}

				return fStartDate;
			}
		}

		CusMAWB fMasterBill;
		ZDateTime fStartDate;
		#region ComparerForTest
		class ComparerForTest : IComparer
		{
			#region IComparer Members
			public int Compare(object x, object y)
			{
				return Compare(x as IShipmentData, y as IShipmentData);
			}

			int Compare(IShipmentData shipmentData1, IShipmentData shipmentData2)
			{
				return shipmentData1.ShipmentRef.CompareTo(shipmentData2.ShipmentRef);
			}
			#endregion
		}
		#endregion
		#endregion
	}
}
