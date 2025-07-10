using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManTranHead))]
	public class CusSeaManTranHeadTest : Customs.Business.Testing.CusSeaManTranHeadTest
	{
		public void TestIDocManagerSupportIncudingRelatedObjectsMembers()
		{
			var seaman = Factory.New<CusSeaManTranHead>();
			var underbond = seaman.AllUnderbonds.AddNew();
			var supporter = seaman as IDocManagerSupportIncudingRelatedObjects;
			AssertEquals("Self reference", seaman, supporter.SelfReference);
			AssertEquals(1, supporter.GetRelatedBusinessObjects().Count());
			Assert(supporter.GetRelatedBusinessObjects().Contains(underbond));
			AssertType(typeof(DocManagerIncludingRelatedObjectsInfo), ((IDocManagerSupport)seaman).DocManagerInfo);
		}

		public void TestIDocManagerSupportMembers()
		{
			var header = Factory.New<CusSeaManTranHead>();
			var docManagerSupporter = header as IDocManagerSupport;
			AssertEquals(typeof(DocManagerIncludingRelatedObjectsInfo), docManagerSupporter.DocManagerInfo.GetType());
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var header = Factory.New<CusSeaManTranHead>();
			var fileNameProvider = header as IDataExportCSVFileNameProvider;
			header.BT_VoyageNum = "123456";
			header.BT_VesselName = "Afri can";
			header.BT_RL_NKPortOfLastForeignPort = "USAB";
			header.BT_PortOfLastForeignPortATD = new ZDateTime(2012, 02, 13);
			AssertEquals("File name suffix", "Afri can_123456_USAB_120213", fileNameProvider.FileNameSuffix);
		}

		public override void TestBusinessObjectsWithRelatedLogs()
		{
			base.TestBusinessObjectsWithRelatedLogs();

			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();

			AssertEquals(0, header.BusinessObjectsWithRelatedEvents.Length);

			CusUnderbond underbond = header.AllUnderbonds.AddNew();
			AssertEquals(1, header.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains(underbond, header.BusinessObjectsWithRelatedEvents);

			CusSeaManOBLHeaderCargoLine line = header.Arrivals.AddNew().CargoLines.AddNew();
			AssertEquals(3, header.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains(line.Port, header.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains(line, header.BusinessObjectsWithRelatedEvents);
		}

		public void TestIsForAirCargo()
		{
			CusSeaManTranHead head = Factory.New<CusSeaManTranHead>();
			AssertEquals(false, ((ICusUnderbondUnionCollectionParent)head).IsForAirCargo);
		}

		public override void TestOceanBills()
		{
			base.TestOceanBills();

			CusSeaManTranHead head = Factory.New<CusSeaManTranHead>();
			AssertEquals(typeof(CusSeaManOBLHeaderCollection), head.OceanBills.GetType());
		}

		public override void TestArrivals()
		{
			base.TestArrivals();

			CusSeaManTranHead head = Factory.New<CusSeaManTranHead>();
			AssertEquals(typeof(CusSeaManArrivalPortCollection), head.Arrivals.GetType());
		}

		public override void TestSlotCharterers()
		{
			base.TestSlotCharterers();

			CusSeaManTranHead head = Factory.New<CusSeaManTranHead>();
			AssertEquals(typeof(CusSeaManSlotOrgCollection), head.SlotCharterers.GetType());
		}

		public void TestValidation()
		{
			CusSeaManTranHead head = Factory.New<CusSeaManTranHead>();
			AssertEquals(typeof(CusSeaManTranHeadValidation), head.Validation.GetType());
		}

		public void TestImpendingArrivalResponseStatus()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();

			header.ImpendingArrivalResponseStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			AssertEquals("ImpendingArrivalResponseStatus.StatusCode should be same as CMRAcceptedRejectedList.Codes.Accepted", CMRBaseStatuses.Codes.OriginalAccepted, header.ImpendingArrivalResponseStatus.Code);
			AssertEquals("", header.Lookups.ImpendingArrivalStatusList.GetDescriptionFromCode(CMRBaseStatuses.Codes.OriginalAccepted), header.ImpendingArrivalResponseStatus.Description);
		}

		public void TestCalculator()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			AssertNotNull("Calculator", header.Calculator);
		}

		public void TestAllUnderbonds()
		{
			var header = Factory.New<CusSeaManTranHead>();
			AssertNotNull(header.AllUnderbonds);
		}

		public void TestGetAllPossibleCollectionProviders()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			ICusUnderbondUnionCollectionParent headerUnion = header;

			AssertEquals("Length of AllPossibleCollectionProviders is 0", 0, headerUnion.GetAllPossibleCollectionProviders().Length);

			CusSeaManOBLHeader oBLHeader = header.OceanBills.AddNew();
			CusSeaManOBLDetail detail = oBLHeader.Details.AddNew();

			AssertNotNull("GetAllPossibleCollectionProviders should not return null", headerUnion.GetAllPossibleCollectionProviders());
			AssertEquals("Length of AllPossibleCollectionProviders is 1", 1, headerUnion.GetAllPossibleCollectionProviders().Length);
			AssertEquals("The only element in AllPossibleCollectionProviders should be the OceanBillDetail", detail, headerUnion.GetAllPossibleCollectionProviders()[0]);
		}

		public void TestImpendingArrivalStatusStartsAsNotSent()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			AssertEquals("Status Code", CMRBaseStatuses.Codes.NotSent, header.ImpendingArrivalResponseStatus.Code);
		}

		public void TestICMRMessageRespondeeDetails()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			header.BT_VoyageNum = "12345";
			header.BT_VesselName = "ADMIRALENGRACHT";
			AssertEquals("Details", "Vessel: ADMIRALENGRACHT\r\nVoyage: 12345\r\n", ((ICMRMessageRespondee)header).Details);
		}

		public void TestICMRMessageRespondeeShortDescription()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();
			transportHeader.BT_VoyageNum = "12345";
			oceanBill.BO_OceanBill = "12345";
			AssertEquals("ShortDescription", "Voyage: 12345 Ocean Bill: 12345", ((ICMRMessageRespondee)oceanBill).ShortDescription);
		}

		public new void TestOceanBillsView()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			AssertNotNull(header.OceanBillsView);
			AssertEquals("OceanBillsView.GetType()", typeof(CusSeaManOBLHeaderCollectionView), header.OceanBillsView.GetType());
		}

		public void TestLoadByVoyageNumberVessel()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			header.BT_VesselName = "ADMIRALENGRACHT";
			header.BT_VoyageNum = "4321";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManTranHead loadedHeader = CusSeaManTranHead.LoadByVoyageNumberVessel(factory2, "4321", "ADMIRALENGRACHT");
			AssertEquals("PK of original and loaded matches", header.PK, loadedHeader.PK);
		}

		protected const string ABN = "36103224237";
		public void TestSetDefaultValues()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ABN;
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			AssertEquals("Responsible Party ID Default", ABN, header.BT_ResponsiblePartyID);
		}

		public void TestVesselAndLloydsDefaulting()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			header.BT_VesselName = "ADMIRALENGRACHT";
			AssertEquals("Setting the vessel should set the Lloyds Number", "8811924", header.BT_LloydsIMO);

			header.BT_LloydsIMO = "8610033";
			AssertEquals("Setting Lloyds should set OceanBill Vessel", "SOUTHERN CROSS MARU", header.BT_VesselName);
		}
	}
}
