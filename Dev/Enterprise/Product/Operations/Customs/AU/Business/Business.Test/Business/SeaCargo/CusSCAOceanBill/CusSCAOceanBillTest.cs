using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBill))]
	public class CusSCAOceanBillTest : EnterpriseBusinessObjectTestCase
	{
		#region Constants
		public const string TestVessel = "ADMIRALENGRACHT";
		public const string TestLloyds = "8811924";
		#endregion

		public virtual void TestFilteredHouseBills()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();

			CusSCAHouse house1 = oceanBill.HouseBills.AddNew();
			house1.CA_ShipmentStatus = "SS1";
			house1.CA_MessageStatus = "MS1";
			CusSCAHouse house2 = oceanBill.HouseBills.AddNew();
			house2.CA_ShipmentStatus = "SS2";
			house2.CA_MessageStatus = "MS2";
			CusSCAHouse house3 = oceanBill.HouseBills.AddNew();
			house3.CA_ShipmentStatus = "SS3";
			house3.CA_MessageStatus = "MS3";

			oceanBill.InvalidHouseBillsOnlyFilter = true;
			oceanBill.FilteredHouseBills.Rebuild();
			AssertEquals("All bills should be invalid", 3, oceanBill.FilteredHouseBills.Count);
			oceanBill.InvalidHouseBillsOnlyFilter = false;

			oceanBill.CustomsShipmentStatusFilter = "SS1";
			oceanBill.FilteredHouseBills.Rebuild();
			AssertEquals("Shipment Status Filter", 1, oceanBill.FilteredHouseBills.Count);

			oceanBill.CustomsMessageStatusFilter = "MS2";
			oceanBill.FilteredHouseBills.Rebuild();
			AssertEquals("Shipment & Message Status Filter", 2, oceanBill.FilteredHouseBills.Count);

			oceanBill = Factory.New<CusSCAOceanBill>();

			int clearCount = 0;
			foreach (CodeDescriptionPair pair in new CMRConsolidatedCargoStatuses())
			{
				var houseBill = oceanBill.HouseBills.AddNew();
				houseBill.CA_ShipmentStatus = pair.Code;
				if (CMRConsolidatedCargoStatuses.AllClearStatus.ContainsCode(pair.Code))
				{
					clearCount++;
				}
			}

			oceanBill.CustomsShipmentStatusFilter = CMRConsolidatedCargoStatuses.Filter.Codes.NotClear;
			oceanBill.FilteredHouseBills.Rebuild();
			AssertEquals("Not clear bills only", oceanBill.HouseBills.Count - clearCount, oceanBill.FilteredHouseBills.Count);
		}

		public void TestFilteredHouseBillsSynchronisation()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();

			var house1 = oceanBill.HouseBills.AddNew();
			AssertEquals("FilteredHouseBills is 'built' on first access", 1, oceanBill.FilteredHouseBills.Count);

			var house2 = oceanBill.HouseBills.AddNew();
			AssertEquals("FilteredHouseBills is updated when a new House is added", 2, oceanBill.FilteredHouseBills.Count);

			house2.Delete();
			AssertEquals("FilteredHouseBills is updated when a House is deleted", 1, oceanBill.FilteredHouseBills.Count);
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var bill = Factory.New<CusSCAOceanBill>();
			bill.CB_OceanBill = "12345";
			var fileNameProvider = bill as IDataExportCSVFileNameProvider;
			AssertEquals("File name suffix", "12345", fileNameProvider.FileNameSuffix);
		}

		public void TestApplicationCodes()
		{
			Assert("Application codes contains CMR", new List<ZString>(CusSCAOceanBill.ApplicationCodes).Contains(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages));
		}

		public void TestIMessageManageableBizObj()
		{
			CusSCAOceanBill oBL = Factory.New<CusSCAOceanBill>();
			Customs.Business.IMessageManageableBizObj bizObj = oBL;

			AssertEquals("MessageManager", typeof(CusSCAOceanBillMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestApplicationCodeReadonly()
		{
			CusSCAOceanBill oBL = Factory.New<CusSCAOceanBill>();
			Assert(oBL.CB_ApplicationCodeInfo.ReadOnly);
		}

		public void TestCMRDefault()
		{
			CusSCAOceanBill oBL = Factory.New<CusSCAOceanBill>();
			AssertEquals(ApplicationCodeDefault, oBL.CB_ApplicationCode);
		}

		protected virtual ZString ApplicationCodeDefault
		{
			get { return Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages; }
		}

		public void TestIsTranshipment()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			AssertEquals("Is transhipment", true, oceanBill.IsTranshipment);

			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("Is transhipment", false, oceanBill.IsTranshipment);
		}

		public void TestHouseBills()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			BusinessObject newHouseBill = oceanBill.HouseBills.AddNew();
			AssertEquals(newHouseBill, oceanBill.HouseBills[0]);
		}

		public void TestValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals("ValidationType", typeof(CusSCAOceanBillValidation), oceanBill.Validation.GetType());
		}

		public void TestMessageCollection()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertNotNull("MessageCollection", oceanBill.MessageCollection);
		}

		public void TestLoad()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("OceanBill", oceanBill, CusSCAOceanBill.Load(consol));
		}

		public void TestReadOnlyFactory()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			BusinessObjectFactory readOnlyFactory = oceanBill.ReadOnlyFactory;
			AssertEquals("ReadOnlyFactory should been the same instance", readOnlyFactory, oceanBill.ReadOnlyFactory);
		}

		public void TestCheckCustomsMessagePreconditions()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			Assert("OceanBill - Unsaved Factory Fails" + oceanBill.CheckCustomsMessagePreconditions(), oceanBill.CheckCustomsMessagePreconditions().Contains("save"));
			Factory.Save();
			Assert("OceanBill - Factory is saved, error message should not contain the word save" + oceanBill.CheckCustomsMessagePreconditions(),
				!oceanBill.CheckCustomsMessagePreconditions().Contains("save"));
		}

		public void TestPivotCollection()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			AssertEquals("Pivot House Bill Collection", 0, house.Pivot.Count);
			var pivot = house.Pivot.AddNew();
			AssertEquals("Pivot House Bill Collection", 1, house.Pivot.Count);

			var container = oceanBill.Containers.AddNew();
			AssertEquals("Pivot Container Bill Collection", 0, container.Pivots.Count);
			pivot.CV_CN = container.PK;
			AssertEquals("Pivot Container Bill Collection", 1, container.Pivots.Count);

			container = oceanBill.Containers.AddNew();
			AssertEquals("Pivot Container Bill Collection", 0, container.Pivots.Count);
			pivot = container.Pivots.AddNew();
			AssertEquals("Pivot Container Bill Collection", 1, container.Pivots.Count);

			house = oceanBill.HouseBills.AddNew();
			AssertEquals("Pivot House Bill Collection", 0, house.Pivot.Count);
			pivot.CV_CA = house.PK;
			AssertEquals("Pivot House Bill Collection", 1, house.Pivot.Count);
		}

		public void TestDefaultValues()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals("Default Port of Discharge", GlbBranch.CurrentBranch.GB_RL_NKHomePort, oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("Default Branch", GlbBranch.CurrentBranch.PK, oceanBill.CB_GB);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			CusSCAOceanBill oceanBill = (CusSCAOceanBill)GetNewBusinessObject();

			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			CusSCAContainer container2 = oceanBill.Containers.AddNew();

			CusSCAHouse house1 = oceanBill.HouseBills.AddNew();
			CusSCAHouse house2 = oceanBill.HouseBills.AddNew();

			CusSCAPivot pivot1 = house1.Pivot.AddNew();
			CusSCAPivot pivot2 = container1.Pivots.AddNew();

			AssertEquals("BusinessObjectWithRelatedLogs should contain Housebills, Containers & Pivots", 6, oceanBill.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains("Container1 should be in BusinessObjectWithRelatedLogs", container1, oceanBill.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Container2 should be in BusinessObjectWithRelatedLogs", container2, oceanBill.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Pivot1 should be in BusinessObjectWithRelatedLogs", pivot1, oceanBill.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Pivot2 should be in BusinessObjectWithRelatedLogs", pivot2, oceanBill.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Housebill1 should be in BusinessObjectWithRelatedLogs", house1, oceanBill.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Housebill2 should be in BusinessObjectWithRelatedLogs", house2, oceanBill.BusinessObjectsWithRelatedEvents);

			TestBusinessObjectsWithRelatedEventsCore(oceanBill);
		}

		public void TestBulkAllocateReferenceNumbersForChildBills()
		{
			var oceanBill = (CusSCAOceanBill)GetNewBusinessObject();

			var house1 = oceanBill.HouseBills.AddNew();

			var shipment = CommonShipment.New(Factory);
			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_JS = shipment.PK;

			oceanBill.BulkAllocateReferenceNumbersForChildBills();

			AssertNotNullOrEmpty(house1.CA_BGMReference);
			AssertNullOrEmpty(house2.CA_BGMReference);
		}

		public void TestUnregisterHouseBillsFromEditableChildren()
		{
			var oceanBill = (CusSCAOceanBill)GetNewBusinessObject();
			IBusiness bizo = oceanBill;
			var count = LoadEditableChildObjects(oceanBill);
			AssertEquals("EditableChildObjects are registered", count, bizo.Children.Length);
			oceanBill.UnregisterHouseBillsFromEditableChildren();
			AssertEquals("All EditableChildObjects were Unregistered", 0, bizo.Children.Length);

			var oceanBill2 = (CusSCAOceanBill)GetNewBusinessObject();
			IBusiness bizo2 = oceanBill2;
			oceanBill2.UnregisterHouseBillsFromEditableChildren();
			LoadEditableChildObjects(oceanBill2);
			AssertEquals("No EditableChildObjects were registered", 0, bizo2.Children.Length);
		}

		public void TestSynchronisedFieldsAreReadOnlyWhenOverrideFreightDefaultsIsEnabled()
		{
			var oceanBill = (CusSCAOceanBill)GetNewBusinessObject();
			AssertEquals(false, oceanBill.OverrideFreightDefaultsVisible);
			AssertEquals(true, oceanBill.OverrideFreightDefaults);

			AssertEquals(false, oceanBill.CB_DateOfArrivalInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_DateOfDepartureInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_LloydsIMOInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_OceanBillInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_OH_ShippingLineInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_RL_NKPortOfDischargeInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_VesselNameInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_VoyageInfo.ReadOnly);

			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			AssertEquals(true, oceanBill.OverrideFreightDefaultsVisible);
			AssertEquals(false, oceanBill.OverrideFreightDefaults);

			AssertEquals(true, oceanBill.CB_DateOfArrivalInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_DateOfDepartureInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_LloydsIMOInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_OceanBillInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_OH_ShippingLineInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_RL_NKPortOfDischargeInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_VesselNameInfo.ReadOnly);
			AssertEquals(true, oceanBill.CB_VoyageInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals(false, oceanBill.CB_DateOfArrivalInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_DateOfDepartureInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_LloydsIMOInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_OceanBillInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_OH_ShippingLineInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_RL_NKPortOfDischargeInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_VesselNameInfo.ReadOnly);
			AssertEquals(false, oceanBill.CB_VoyageInfo.ReadOnly);
		}

		public void TestOverrideFreightDefaultsIsUpdatedByDataRefresh()
		{
			var oceanBill = (CusSCAOceanBill)GetNewBusinessObject();
			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			oceanBill.OverrideFreightDefaults = true;
			Factory.Save();
			Factory.RefreshEnabled = true;

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = true;
			var oceanBillIOF = (CusSCAOceanBill)otherFactory.Load(GetExpectedBusinessObjectType(), oceanBill.PK);
			AssertEquals(true, oceanBillIOF.OverrideFreightDefaults);

			var overrideFreightDefaultsChangedTriggered = false;
			oceanBillIOF.OverrideFreightDefaultsChanged += (sender, value) => overrideFreightDefaultsChangedTriggered = true;

			oceanBill.OverrideFreightDefaults = false;
			Factory.Save();
			AssertEquals("OverrideFreightDefaults is updated by Data Refresh", false, oceanBillIOF.OverrideFreightDefaults);
			Assert("OverrideFreightDefaultsChanged Event Handler was triggered", overrideFreightDefaultsChangedTriggered);
		}

		public void TestPerformanceWhenSettingReadOnlyOnOceanBill()
		{
			var totalHouseBills = 5000;

			Factory.SuspendValidation();
			CMRStatusRecalculationSuspender.SuspendStatusRecalculation(Factory);
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OBL001";
			var container = oceanBill.Containers.AddNew();

			for (var i = 0; i < totalHouseBills; i++)
			{
				var houseBill = oceanBill.HouseBills.AddNew();
				houseBill.CA_HouseBill = "HBL" + oceanBill.HouseBills.Count;
				var packingLine = houseBill.Pivot.AddNew();
				packingLine.CV_CN = container.PK;
			}

			var allPivots = oceanBill.HouseBills.Cast<CusSCAHouse>().SelectMany(houseBill => houseBill.Pivot)
				.Union(oceanBill.Containers.Cast<CusSCAContainer>().SelectMany(cnt => cnt.Pivots));
			Assert(allPivots.All(x => !x.ReadOnly));

			CMRStatusRecalculationSuspender.ResumeStatusRecalculation(Factory);

			var sw = Stopwatch.StartNew();
			oceanBill.SetReadOnlyIncludingChildren(!oceanBill.ReadOnly);
			sw.Stop();
			AssertLessThan("SetReadOnlyIncludingChildren execution time should not exceed 10s", sw.Elapsed.TotalSeconds, 10);

			Assert(allPivots.All(x => x.ReadOnly));
		}

		public void TestDefaultFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "1234";
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			oceanBill.DefaultFromConsol();
			AssertEquals("1234", oceanBill.CB_OceanBill);
		}

		public void TestEffectiveResponsiblePartyOrgHeader()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345", Core.Constants.CountryCodes.Australia);

			Factory.Save();

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ResponsiblePartyID = "12345";

			AssertEquals(org, oceanBill.EffectiveResponsiblePartyOrgHeader);
		}

		public void TestIScanMasterBillProvider()
		{
			var oceanBill1 = Factory.New<CusSCAOceanBill>();
			oceanBill1.CB_OceanBill = "OC1";
			var consol = Factory.New<ForwardingConsol>();
			oceanBill1.CB_ParentId = consol.PK;
			oceanBill1.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			oceanBill1.CB_MasterHouseBill = "MHB1";
			var container1 = oceanBill1.Containers.AddNew();
			container1.CN_ContainerNumber = "CN1";
			var container2 = oceanBill1.Containers.AddNew();
			container2.CN_ContainerNumber = "CN2";
			var container3 = oceanBill1.Containers.AddNew();
			container3.CN_ContainerNumber = "CN3";

			var house1 = oceanBill1.HouseBills.AddNew();
			container1.Pivots.AddNew().CV_CA = house1.PK;
			var house2 = oceanBill1.HouseBills.AddNew();
			container2.Pivots.AddNew().CV_CA = house2.PK;
			var house3 = oceanBill1.HouseBills.AddNew();
			var house5 = oceanBill1.HouseBills.AddNew();
			container1.Pivots.AddNew().CV_CA = house5.PK;
			house5.CA_IsMasterHouse = true;

			var underbond1 = container1.Underbonds.AddNew();
			underbond1.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			var underbond2 = container2.Underbonds.AddNew();
			underbond2.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			var underbond3 = container3.Underbonds.AddNew();
			underbond3.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;

			var iScan = (IScanMasterBillProvider)oceanBill1;
			AssertEquals("OC1", iScan.MasterBill);
			Assert(!iScan.IsStandAlone);
			AssertEquals("MHB1", iScan.MasterHouseBill);
			AssertEquals(2, iScan.Underbonds.Count());
			Assert(iScan.Underbonds.Contains(underbond1));
			Assert(iScan.Underbonds.Contains(underbond2));

			AssertEquals(1, iScan.GetChildBills(underbond1).Count());
			Assert(iScan.GetChildBills(underbond1).Contains(house1));

			var oceanBill2 = Factory.New<CusSCAOceanBill>();
			var house4 = oceanBill2.HouseBills.AddNew();
			var container4 = oceanBill2.Containers.AddNew();
			container4.CN_ContainerNumber = "CN1";
			container4.Pivots.AddNew().CV_CA = house4.PK;

			iScan = oceanBill2;
			Assert(iScan.GetChildBills(underbond1).Contains(house4));
		}

		public void TestIDocManagerSupportIncudingRelatedObjectsMembers()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var underbond = oceanBill.AllUnderbonds.AddNew();
			var supporter = oceanBill as IDocManagerSupportIncudingRelatedObjects;
			AssertEquals("Self reference", oceanBill, supporter.SelfReference);
			AssertEquals(1, supporter.GetRelatedBusinessObjects().Count());
			Assert(supporter.GetRelatedBusinessObjects().Contains(underbond));
			AssertType(typeof(DocManagerIncludingRelatedObjectsInfo), ((IDocManagerSupport)oceanBill).DocManagerInfo);

			var houseBill = oceanBill.HouseBills.AddNew();
			AssertEquals(2, supporter.GetRelatedBusinessObjects().Count());
			Assert(supporter.GetRelatedBusinessObjects().Contains(houseBill));
		}

		public void TestIsForAirCargo()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals(false, ((ICusUnderbondUnionCollectionParent)oceanBill).IsForAirCargo);
		}

		public void TestValidationObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals("Validation object", typeof(CusSCAOceanBillValidation), oceanBill.Validation.GetType());
		}

		public void TestValidationCMR()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals("ValidationType", typeof(CusSCAOceanBillValidation), oceanBill.Validation.GetType());
		}

		public void TestAllUnderbonds()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertNotNull("AllUnderbonds", ((ICusUnderbondUnionCollectionParent)oceanBill).AllUnderbonds);
		}

		public void TestGetAllPossibleCollectionProvidersIncludesAllPivots()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			CusSCAPivot pivot = container.Pivots.AddNew();
			pivot.CV_CA = houseBill.PK;
			ICusUnderbondDependentCollectionParent[] result = ((ICusUnderbondUnionCollectionParent)oceanBill).GetAllPossibleCollectionProviders();
			AssertEquals("Result.Length", 2, result.Length);
			AssertEquals("Result[0]", container, result[0]);
			AssertEquals("Result[1]", pivot, result[1]);
		}

		public void TestTopLevelObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals(oceanBill, ((ISeaCargoConsolInfo)oceanBill).TopLevelObject);
		}

		public void TestDefaultCB_ApplicationCode()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertEquals("Application Code should be CMR", Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, oceanBill.CB_ApplicationCode);
		}

		const string TestValidABNNumber = "74670046403";
		public void TestDefaultCB_ResponsiblePartyId()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = TestValidABNNumber;
			CusSCAOceanBill oceanBill1 = Factory.New<CusSCAOceanBill>();
			AssertEquals("Responsible Party should default to current company ABN", TestValidABNNumber, oceanBill1.CB_ResponsiblePartyID);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			CusSCAOceanBill oceanBill2 = Factory.New<CusSCAOceanBill>();// should not cause exception
		}

		public void TestLloydsNumber()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_VesselName = "ADMIRALENGRACHT";
			AssertEquals("Setting the vessel should set the Lloyds Number", "8811924", oceanBill.CB_LloydsIMO);

			oceanBill.CB_LloydsIMO = "8610033";
			AssertEquals("Setting Lloyds should not set OceanBill Vessel if Vessel Name is not empty", "ADMIRALENGRACHT", oceanBill.CB_VesselName);

			oceanBill.CB_VesselName = "";
			oceanBill.CB_LloydsIMO = "8811924";
			AssertEquals("Setting Lloyds should set OceanBill Vessel if Vessel Name is empty", "ADMIRALENGRACHT", oceanBill.CB_VesselName);
		}

		protected void TestBusinessObjectsWithRelatedEventsCore(CusSCAOceanBill oceanBill)
		{
			CusSCAOceanBill cMROceanBill = oceanBill;
			AssertNotNull("OceanBill should be a CMROceanBill", cMROceanBill);

			int origRelatedBizOCount = cMROceanBill.BusinessObjectsWithRelatedEvents.Length;

			CusSCAContainer container = cMROceanBill.Containers.AddNew();

			CusUnderbond underbond1 = container.Underbonds.AddNew();
			CusUnderbond underbond2 = container.Underbonds.AddNew();

			cMROceanBill.AllUnderbonds.Load();

			AssertEquals("Underbonds should also be included in the BusinessObjectsWithRelatedEvents", 3, cMROceanBill.BusinessObjectsWithRelatedEvents.Length - origRelatedBizOCount);
			AssertCollectionContains("Underbond1 should be in BusinessObjectWithRelatedLogs", underbond1, oceanBill.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Underbond2 should be in BusinessObjectWithRelatedLogs", underbond2, oceanBill.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Container should be in there as well", container, oceanBill.BusinessObjectsWithRelatedEvents);
		}

		protected virtual int LoadEditableChildObjects(CusSCAOceanBill oceanBill)
		{
			_ = oceanBill.HouseBills.Count;
			_ = oceanBill.Containers.Count;
			_ = oceanBill.Pivots.Count;
			return 3;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusSCAOceanBill result = factory.New<CusSCAOceanBill>();
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			return oceanBill;
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		#endregion
	}
}
