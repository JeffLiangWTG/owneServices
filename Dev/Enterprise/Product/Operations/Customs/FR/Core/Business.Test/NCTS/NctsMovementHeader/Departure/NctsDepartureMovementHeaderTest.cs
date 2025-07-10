using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs.FR;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsDepartureMovementHeader))]
	class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => departureMovement;

		protected override BusinessObject GetNewBusinessObject() => departureMovement;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;

		public void TestMessages()
		{
			var messages = departureMovement.Messages;
			AssertEquals(departureMovement, messages.Master);
			AssertType<FREDIMessageCollection>(messages);
		}

		public void TestCorrelationID()
		{
			var nctsHeader1 = Factory.New<NctsHeader>();
			nctsHeader1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement1 = nctsHeader1.MovementHeader;

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement2 = nctsHeader2.MovementHeader;

			Factory.Save();
			Assert(departureMovement1.CorrelationID != departureMovement2.CorrelationID);
			AssertType<ZString>(departureMovement1.CorrelationID);
			AssertEquals("The CorrelationID of departure movement header is the same as the CorrelationID of its NctsHeader.", departureMovement1.CorrelationID, nctsHeader1.CorrelationID);
		}

		public void TestCorrelationIDPrefix()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement1 = nctsHeader.MovementHeader;
			Factory.Save();
			AssertEquals(ZString.Empty, departureMovement.CorrelationIDPrefix);
		}

		public void TestEntryAndValuationDatesExcludedFromCloning()
		{
			departureMovement.BM_EntryDate = ZDateTime.Today;
			departureMovement.BM_ValuationDate = ZDateTime.Today;

			NctsDepartureMovementHeader clone = (NctsDepartureMovementHeader)departureMovement.Clone();

			AssertEquals("BM_EntryDate should have been cleared.", ZDateTime.Empty, clone.BM_EntryDate);
			AssertEquals("BM_ValuationDate should have been cleared.", ZDateTime.Empty, clone.BM_ValuationDate);
		}

		public void TestValidation()
		{
			AssertType<NctsDepartureMovementHeaderPhase4Validation>(departureMovement.Validation);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsDepartureMovementHeaderPhase5Validation>(departureMovement.Validation);
		}

		public void TestLookups()
		{
			AssertType<NctsDepartureMovementHeaderPhase4Lookups>(departureMovement.Lookups);

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement2 = nctsHeader2.MovementHeader;
			AssertType<NctsDepartureMovementHeaderPhase5Lookups>(departureMovement2.Lookups);
		}

		#region IHarbourJob Properties

		public void TestIsDCN()
		{
			Assert((departureMovement as IHarbourJob).IsDCN);
		}

		public void TestContainerMode()
		{
			AssertEquals("ContainerMode should be empty when there is no container set against the transit declaration.", ZString.Empty, departureMovement.ContainerMode);

			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			container.BC_Mode = Core.Constants.ContainerModes.FCL;
			AssertEquals("ContainerMode should reflect the first container mode, if any.", Core.Constants.ContainerModes.FCL, departureMovement.ContainerMode);
		}

		public void TestBarrierPort()
		{
			AssertEquals("BarrierPort should be empty when no port of dispatch is not set against the transit declaration.", ZString.Empty, departureMovement.BarrierPort);
			nctsHeader.BH_RL_NKImportLoadPort = "FRLEH";
			nctsHeader.MovementHeader.BM_RL_NKPortOfPresentation = "FRCDG";
			AssertEquals("In Ncts4 BarrierPort should reflect the transit declaration port of dispatch.", "FRLEH", departureMovement.BarrierPort);

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_RL_NKImportLoadPort = "FRLEH";
			header.MovementHeader.BM_RL_NKPortOfPresentation = "FRCDG";
			AssertEquals("In Ncts5 BarrierPort should reflect the movement header Port of presentation.", "FRCDG", header.MovementHeader.BarrierPort);
		}

		public void TestCustomsOffice()
		{
			AssertEquals("CustomsOffice should be empty when no departure office is set against the transit declaration.", ZString.Empty, departureMovement.CustomsOffice);
			var departureOffice = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			departureOffice.CY_Data = "FR002300";
			AssertEquals("CustomsOffice should reflect the transit Customs Office of Departure.", "FR002300", departureMovement.CustomsOffice);
		}

		public void TestDataGrouping()
		{
			AssertEquals("DataGrouping should reflect the transit declaration default data grouping.", "FR", departureMovement.DataGrouping);
		}

		public void TestHarbourType()
		{
			AssertEquals("HarbourType should always return 'IMP'.", Customs.Common.EU.EUJobMessageTypeList.Codes.Import, departureMovement.HarbourType);
		}

		public void TestValuationDate()
		{
			AssertEquals("ValuationDate should always return today.", ZDateTime.Today, departureMovement.ValuationDate);
		}

		#endregion

		public void TestCloneChargePaymentOrDestinationID()
		{
			departureMovement.ChargePaymentOrDestinationID = "395";
			var clonedNctsHeader = (NctsHeader)nctsHeader.Clone();
			AssertEquals("395", clonedNctsHeader.MovementHeader.ChargePaymentOrDestinationID);
		}

		public void TestSettingChargePaymentOrDestinationID()
		{
			var movementHeader = PrepareCalculationData();
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);

			var goodItem1 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 1);
			var goodItem2 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 2);

			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);

			var fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(2m, fee.BFE_ChargeAmount);

			movementHeader.ChargePaymentOrDestinationID = "395";
			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);
			fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(4m, fee.BFE_ChargeAmount);
		}

		public void TestIsContainerised_Phase4()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Goods Items", false, departureMovement.IsContainerised);
				var goodsItem = departureMovement.GoodsItems.AddNew();
				AssertEquals("No Containers", false, departureMovement.IsContainerised);
				var container = nctsHeader.DepartureHeaderContainers.AddNew();
				container.BC_Seal1 = "SEAL1";
				container.BC_Seal2 = "SEAL2";
				AssertEquals("Container Not linked to Goods Item", false, departureMovement.IsContainerised);
				goodsItem.ContainersPivots[0].ContainerSelected = true;
				AssertEquals("Linked no Container Number", false, departureMovement.IsContainerised);
				container.BC_ContainerNum = "CONTAINER1";
				AssertEquals("Is Containerised", true, departureMovement.IsContainerised);
			});
		}

		public void TestIsContainerised_Phase5()
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				AssertEquals("IsContainerised should be false when no goods items are present.", false, departureMovement.IsContainerised);

				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();
				AssertEquals("IsContainerised should be false when no containers are created.", false, departureMovement.IsContainerised);

				var container = nctsHeader.DepartureHeaderContainers.AddNew();
				var package = goodsItem.Packages.AddNew();
				container.BC_Seal1 = "SEAL1";
				container.BC_Seal2 = "SEAL2";
				AssertEquals("IsContainerised should be false when container is not selected.", false, departureMovement.IsContainerised);

				var containerPivot = package.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>().ToArray()[0];
				containerPivot.ContainerSelected = true;
				AssertEquals("IsContainerised should be false when a container is selected but has no container number.", false, departureMovement.IsContainerised);

				container.BC_ContainerNum = "CONTAINER1";
				AssertEquals("IsContainerised should be true when a container is selected and has a container number.", true, departureMovement.IsContainerised);

				containerPivot.ContainerSelected = false;
				AssertEquals("IsContainerised should be false when container has a number but is not selected.", false, departureMovement.IsContainerised);

				var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
				container2.BC_Seal1 = "SEAL3";
				container2.BC_Seal2 = "SEAL4";
				var containerPivot2 = package.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>().ToArray()[1];
				containerPivot2.ContainerSelected = true;
				containerPivot.ContainerSelected = true;
				AssertEquals("IsContainerised should be true when any one of the containers has both 1) Number and 2) Is selected.", true, departureMovement.IsContainerised);
			});
		}

		public void TestGuarantees()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			AssertType<NctsGuaranteeCollection<FRNctsGuarantee>>(movementHeader.Guarantees);
		}

		NctsDepartureMovementHeader PrepareCalculationData()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 1.1341", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 2.1341", "FR");
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var headerContainer1 = header.DepartureHeaderContainers.AddNew();
			headerContainer1.BC_Mode = "FCL";
			var headerContainer2 = header.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_Mode = "LCL";

			var movementHeader = header.MovementHeader;
			var goodItem1 = movementHeader.GoodsItems.AddNew();
			goodItem1.BY_LineNo = 1;
			goodItem1.BY_GrossWeight = 2000m;
			goodItem1.BY_GrossWeightUnit = "KG";
			var containersPivot = goodItem1.ContainersPivots.AddNew();
			containersPivot.Container = headerContainer1;
			containersPivot.ContainerSelected = true;
			containersPivot.ContainerNumber = "1";
			var goodItem2 = movementHeader.GoodsItems.AddNew();
			goodItem2.BY_LineNo = 2;
			goodItem2.BY_GrossWeight = 3000m;
			goodItem2.BY_GrossWeightUnit = "KG";
			var containersPivot2 = goodItem1.ContainersPivots.AddNew();
			containersPivot2.Container = headerContainer2;
			containersPivot2.ContainerSelected = true;
			containersPivot2.ContainerNumber = "2";

			movementHeader.ChargePaymentOrDestinationID = "108";

			Factory.Save();
			return movementHeader;
		}

		public void TestChargePaymentOrDestinationID()
		{
			AssertEquals(3, departureMovement.ChargePaymentOrDestinationIDInfo.MaxLength);
			AssertEquals("Payment/Destination", departureMovement.ChargePaymentOrDestinationIDInfo.HumanReadableName);
		}

		public void TestDefaultValues()
		{
			AssertEquals("BM_InBondEntryType should be defaulted to T1.", CusEntryNumberTypes.EU.T1, departureMovement.BM_InBondEntryType);
			AssertEquals("BM_GS_NKCusAgent should be defaulted to Current user code.", GlbStaff.CurrentUser.GS_Code, departureMovement.BM_GS_NKCusAgent);
		}

		public void TestLastNonIntermediateStatus()
		{
			AssertEquals(ZString.Empty, departureMovement.LastNonIntermediateStatus);

			var logEntryForR4A = nctsHeader.Logs.AddNew();
			using (logEntryForR4A.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForR4A.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForR4A.SL_Reference = NctsTransitStatusList.Codes.ReadyForAmendment;
				logEntryForR4A.SL_EventTime = ZDateTime.Now.AddDays(-2);
			}

			var logEntryForDRJ = nctsHeader.Logs.AddNew();
			using (logEntryForDRJ.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForDRJ.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForDRJ.SL_Reference = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
				logEntryForDRJ.SL_EventTime = ZDateTime.Now.AddDays(-1);
			}

			var logEntryForFUR = nctsHeader.Logs.AddNew();
			using (logEntryForFUR.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForFUR.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForFUR.SL_Reference = NctsTransitStatusList.Codes.FunctionalRejection;
				logEntryForFUR.SL_EventTime = ZDateTime.Now;
			}

			var furStatusTime = logEntryForFUR.SL_EventTime;
			var drjStatusTime = logEntryForDRJ.SL_EventTime;
			var r4AStatusTime = logEntryForR4A.SL_EventTime;

			Assert("Last set status was FUR", furStatusTime > drjStatusTime && furStatusTime > r4AStatusTime);
			Assert("Last set non intermediate status was DRJ ", drjStatusTime > r4AStatusTime);
			AssertEquals(NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, departureMovement.LastNonIntermediateStatus);
		}

		public void TestIsDepartureTabReadOnly_DepartureStatus()
		{
			var departureMovement = nctsHeader.MovementHeader;

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			AssertEquals(false, nctsHeader.IsDepartureTabReadOnly);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			AssertEquals(true, nctsHeader.IsDepartureTabReadOnly);

			nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.ResearchProcedureNotification;
			nctsHeader.IsQueried = false;
			AssertEquals(false, nctsHeader.IsDepartureTabReadOnly);

			nctsHeader.DetailedDepartureStatusCode = ZString.Empty;
			nctsHeader.IsQueried = true;
			AssertEquals(false, nctsHeader.IsDepartureTabReadOnly);
		}

		public void TestPreLodgedForAgreedLocationOfGoodsCode()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = true;
			AssertEquals("BM_LocationOfGoodsCode should not be populated", "", departureMovement.BM_LocationOfGoodsCode);

			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = false;
			departureMovement.BM_LocationOfGoodsCode = "PRE-LODGED";
			AssertEquals("PreLodgedForAgreedLocationOfGoodsCode should be false", false, departureMovement.PreLodgedForAgreedLocationOfGoodsCode);
		}

		public void TestGetValueSetStrategy()
		{
			var nctsDepartureMovementHeader = Factory.New<NctsDepartureMovementHeaderForTest>();
			AssertType<NctsDepartureHeaderMovementHeaderValueSetStrategy>("GetValueSetStrategy()", nctsDepartureMovementHeader.GetValueSetStrategyExposed());
		}

		public void TestErrorShouldBeReported_WhenBM_LocationOfGoodsCodeChangedDuringSaving()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovementHeader = Factory.New<NctsDepartureMovementHeaderForTest>();
			departureMovementHeader.BM_BH = nctsHeader.PK;
			departureMovementHeader.BM_LocationOfGoodsCode = "Value";
			AssertEquals("No exception reported when BM_LocationOfGoodsCode changed out of saving process.", 0, ExceptionReporterTestListener.Instance.Count);

			Factory.Save();
			AssertEquals("One exception reported if BM_LocationOfGoodsCode changed during saving process.", 1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Exception message: ", "Value of BM_LocationOfGoodsCode unexpectedly changed during saving, IsInDataBase: False.", ExceptionReporterTestListener.Instance.GetExceptionMessage(0));

			departureMovementHeader.BM_LocationOfGoodsCode = "Value";
			Factory.Save();
			AssertEquals("One exception reported if BM_LocationOfGoodsCode changed during saving process.", 2, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Exception message: ", "Value of BM_LocationOfGoodsCode unexpectedly changed during saving, IsInDataBase: True.", ExceptionReporterTestListener.Instance.GetExceptionMessage(1));

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestIsDepartureCancellationAllowed()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = header.MovementHeader;

			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			Assert("IsDepartureCancellationAllowed must return true when BM_CustomsStatus = 'DAC'.", movementHeader.IsDepartureCancellationAllowed);

			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
			Assert("IsDepartureCancellationAllowed must return true when BM_CustomsStatus = 'DGN'.", movementHeader.IsDepartureCancellationAllowed);

			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			Assert("IsDepartureCancellationAllowed must return true when BM_CustomsStatus = 'DMA'.", movementHeader.IsDepartureCancellationAllowed);

			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
			Assert("IsDepartureCancellationAllowed must return true when BM_CustomsStatus = 'DNR'.", movementHeader.IsDepartureCancellationAllowed);

			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
			Assert("IsDepartureCancellationAllowed must return true when BM_CustomsStatus = 'DRL'.", movementHeader.IsDepartureCancellationAllowed);

			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			Assert("IsDepartureCancellationAllowed must return true when BM_CustomsStatus = 'ART'.", movementHeader.IsDepartureCancellationAllowed);

			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			Assert("IsDepartureCancellationAllowed must return true when BM_CustomsStatus = 'DCC'.", movementHeader.IsDepartureCancellationAllowed);
		}

		public void TestProcedureHolder()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			var principalOrgHeader = Factory.New<OrgHeader>();
			nctsHeader.Principal.OrganisationPK = principalOrgHeader.PK;
			AssertEquals("ProcedureHolder should be equal to Principal when there is only Principal", principalOrgHeader, departureMovement.ProcedureHolder);

			var representativeOrgHeader = Factory.New<OrgHeader>();
			nctsHeader.MovementHeader.Representative.OrganisationPK = representativeOrgHeader.PK;
			AssertEquals("ProcedureHolder should be equal to Representative when Principal is different from Representative", representativeOrgHeader, departureMovement.ProcedureHolder);
		}

		public void TestCusAuthorizationUsageType()
		{
			var movementHeader = Factory.New<NctsDepartureMovementHeader>();
			var cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
			AssertType<NctsCusAuthorizationUsage>(cusAuthorizationUsage);
		}

		public void TestAllowPermitProcessingMembers()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "BM_1234";

			AssertEquals("Permit value decimal place count of movementHeader should be 2", 2, movementHeader.PermitValueDecimalPlaceCount);
			AssertEquals("Permit quantity decimal place count of movementHeader should be 5", 5, movementHeader.PermitQuantityDecimalPlaceCount);
			AssertPackageCount(nctsHeader);
			AssertEquals("Permit reference of movementHeader should be equal to BM_PaperlessInbondNum", "BM_1234", movementHeader.GetPermitReference());
			AssertEquals("Permit reference number line of movementHeader should be 0", 0, movementHeader.GetPermitReferenceNumberLine());
			AssertGetPermitRecords(nctsHeader);
			AssertEquals("Permit comment of movementHeader should be 'NCTS departure BM_PaperlessInbondNum'", "NCTS departure BM_1234", movementHeader.GetPermitComment(null));
		}

		void AssertPackageCount(NctsHeader nctsHeader)
		{
			var bulkType = Factory.SetupBulkCusCode();
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem1 = bill.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 20;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 30;

			var goodsItem2 = bill.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;

			var goodsItem3 = bill.GoodsItems.AddNew();
			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_UnitCount = 0;
			package4.B5_UnitType = bulkType;

			var goodsItem4 = bill.GoodsItems.AddNew();
			var package5 = goodsItem4.Packages.AddNew();
			package5.B5_UnitCount = 10;
			package5.B5_UnitType = bulkType;

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("Total package count should be correctly calculated", 102, movementHeader.PackageCount);
			AssertEquals("Package count of nctsHeader should match with movementHeader", (nctsHeader as IAllowPermitProcessing).PackageCount, movementHeader.PackageCount);
		}

		void AssertGetPermitRecords(NctsHeader nctsHeader)
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var eoriCode = "123456789000";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "GUA";
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_SubType = "1";
			Factory.Save();

			nctsHeader.Principal.E2_OA_Address = guaranteeHeader.PermitHolder.MainAddress.PK;

			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondAmount = 150m;
			guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("Prerequisite: Guarantee should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees().First().CusGuarantee.PK, guarantee.CusGuarantee.PK);
			AssertEquals("Permit Records count should be 1", 1, movementHeader.GetPermitRecords().Count);
			AssertEquals("Permit Records of nctsHeader should match with movementHeader", nctsHeader.GetPermitRecords().Count, movementHeader.GetPermitRecords().Count);
		}
	}

	class NctsDepartureMovementHeaderForTest : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IValueSetStrategy GetValueSetStrategyExposed() => base.GetValueSetStrategy();

		protected override void ActionDuringSaving()
		{
			BM_LocationOfGoodsCode = "NewValue";
		}
	}
}
