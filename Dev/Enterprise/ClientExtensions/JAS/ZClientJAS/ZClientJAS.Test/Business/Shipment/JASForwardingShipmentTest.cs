using System;
using System.Collections;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	public class JASForwardingShipmentTest : ForwardingShipmentTest
	{
		public void TestIsSuitableForJXC()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Should be suitable", JASForwardingShipment.SuitableForJXC.Suitable, Shipment.IsSuitableForJXC());
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Should be suitable", JASForwardingShipment.SuitableForJXC.Suitable, Shipment.IsSuitableForJXC());
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("JXC only supports Air and Sea shipments", JASForwardingShipment.SuitableForJXC.InvalidShipmentTransportMode, Shipment.IsSuitableForJXC());
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.Consols.AddNew();
			AssertEquals("Shipment is already attached to a consol, not a pre-shipment", JASForwardingShipment.SuitableForJXC.NotAPreShipment, Shipment.IsSuitableForJXC());
		}

		public void TestGrossWeightInKilograms()
		{
			AssertEquals(ZDecimal.Zero, Shipment.GrossWeightInKilograms);
			Shipment.JS_ActualWeight = 87.89M;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			AssertEquals(87.89M, Shipment.GrossWeightInKilograms);
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertEquals(39.866233M, Shipment.GrossWeightInKilograms);
		}

		public void TestMeasurementInCubicMetres()
		{
			AssertEquals(ZDecimal.Zero, Shipment.MeasurementInCubicMetres);
			Shipment.JS_ActualVolume = 100;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			AssertEquals(100M, Shipment.MeasurementInCubicMetres);
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			AssertEquals(2.831685M, Shipment.MeasurementInCubicMetres);
		}

		public void TestJASNoteTypes()
		{
			AssertCollectionContains(JASPredefinedNoteTypes.Instance.JXCExportLog, Shipment.NoteTypes);
		}

		protected override Type GetExpectedLocalConsolType()
		{
			return typeof(JASForwardingConsol);
		}

		#region Shipment Job & Charges
		public void TestJob()
		{
			AssertNull("Pre-condition", Shipment.Job);
			JobHeader jobHeaderForDifferentCompany = Factory.NewJobForTesting<JobHeader>();
			jobHeaderForDifferentCompany.JH_ParentID = Shipment.PK;
			jobHeaderForDifferentCompany.JH_GC = OtherCompany.PK;
			jobHeaderForDifferentCompany.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertNull("Should still have no shipment job for the current company", Shipment.Job);
			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = Shipment.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals(jobHeader.PK, Shipment.Job.PK);
		}

		public void TestTotalFreightRevenue()
		{
			PopulateChargesForTest();
			AssertEquals("Should be accumulating revenues with Freight charge code", 515m, Shipment.TotalFreightRevenue);
		}

		public void TestTotalFreightCost()
		{
			PopulateChargesForTest();
			AssertEquals("Should be accumulating costs with Freight charge code", 1350m, Shipment.TotalFreightCost);
		}

		public void TestTotalCost()
		{
			PopulateChargesForTest();
			AssertEquals("Should be accumulating all costs", 1411m, Shipment.TotalCost);
		}

		public void TestTotalOtherCosts()
		{
			PopulateChargesForTest();
			AssertEquals("Should be accumulating all non freight costs", 61m, Shipment.TotalOtherCosts);
		}

		public void TestAgentDeclaredGrossProfit()
		{
			PopulateChargesForTest();
			AssertEquals("Should be Total Freight Revenue - Total Costs; Should not be a negative value", 896m, Shipment.AgentDeclaredGrossProfit);
		}

		public void TestGetProfitShareDueDestination_NullSellAccount()
		{
			PopulateChargesForTest();
			AssertEquals("Should be zero when SellAccount unspecified", 0m, Shipment.GetProfitShareDueDestination(null));
		}

		public void TestGetProfitShareDueDestination()
		{
			PopulateChargesForTest();
			AssertEquals("Should be accumulating all profit share rebate from the revenue side; Should not be a negative value", 25m, Shipment.GetProfitShareDueDestination(SellAccount));
		}

		public void TestGetTotalCollectChargesWithoutProfitShare_NullSellAccount()
		{
			PopulateChargesForTest();
			AssertEquals("Should be zero when SellAccount unspecified", 0m, Shipment.GetTotalCollectChargesWithoutProfitShare(null));
		}

		public void TestGetTotalCollectChargesWithoutProfitShare()
		{
			PopulateChargesForTest();
			AssertEquals("Should be accumulating all collect charges beside profit share", 340m, Shipment.GetTotalCollectChargesWithoutProfitShare(SellAccount));
		}

		public void TestGetCollectCharges()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(Factory.NewWithValidTestData<JASOrgHeader>());
			JASForwardingShipment shipment1 = (JASForwardingShipment)consol.Shipments.AddNew();
			JASForwardingShipment shipment2 = (JASForwardingShipment)consol.Shipments.AddNew();
			AddJobChargeToShipment(consol, shipment1, "BLAH1", true, true);
			AddJobChargeToShipment(consol, shipment1, "BLAH2", true, false);
			AddJobChargeToShipment(consol, shipment1, "BLAH3", false, true);
			AddJobChargeToShipment(consol, shipment1, "BLAH4", false, false);
			AddJobChargeToShipment(consol, shipment1, "BLAH5", true, true);
			AddJobChargeToShipment(consol, shipment2, "Charge 1", true, true);
			AddJobChargeToShipment(consol, shipment2, "Charge 2", false, true);
			AddJobChargeToShipment(consol, shipment2, "Charge 3", true, false);
			AddJobChargeToShipment(consol, shipment2, "Charge 4", false, false);
			JobCharge[] charges1 = shipment1.GetCollectCharges(consol.ReceivingForwarder);
			AssertEquals(2, charges1.Length);
			SortChargesByDesc(charges1);
			AssertEquals("BLAH1", charges1[0].JR_Desc);
			AssertEquals("BLAH5", charges1[1].JR_Desc);
			JobCharge[] charges2 = shipment2.GetCollectCharges(consol.ReceivingForwarder);
			AssertEquals(1, charges2.Length);
			AssertEquals("Charge 1", charges2[0].JR_Desc);
		}

		public void TestGetCollectChargesWithProfitShareCharges()
		{
			Assert(!JASDataRegistry.Instance.IncludeProfitShareCharges);
			JASDataRegistry.Instance.IncludeProfitShareCharges = true;
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "AC_Desc";
			SetupProfitShareChargeRegistry(chargeCode.PK.ToGuid());
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(Factory.NewWithValidTestData<JASOrgHeader>());
			JASForwardingShipment shipment1 = (JASForwardingShipment)consol.Shipments.AddNew();
			JASForwardingShipment shipment2 = (JASForwardingShipment)consol.Shipments.AddNew();
			AddJobChargeToShipment(consol, shipment1, "BLAH1", true, true, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			AddJobChargeToShipment(consol, shipment1, "BLAH2", true, false);
			AddJobChargeToShipment(consol, shipment1, "BLAH3", false, true);
			AddJobChargeToShipment(consol, shipment1, "BLAH4", false, false);
			AddJobChargeToShipment(consol, shipment1, "BLAH5", true, true);
			AddJobChargeToShipment(consol, shipment2, "Charge 1", true, true);
			AddJobChargeToShipment(consol, shipment2, "Charge 2", false, true);
			AddJobChargeToShipment(consol, shipment2, "Charge 3", true, true, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			AddJobChargeToShipment(consol, shipment2, "Charge 4", false, false);
			JobCharge[] charges1 = shipment1.GetCollectCharges(consol.ReceivingForwarder);
			AssertEquals(2, charges1.Length);
			SortChargesByDesc(charges1);
			AssertEquals("AC_Desc", charges1[0].JR_Desc);
			AssertEquals("BLAH5", charges1[1].JR_Desc);
			JobCharge[] charges2 = shipment2.GetCollectCharges(consol.ReceivingForwarder);
			AssertEquals(2, charges2.Length);
			SortChargesByDesc(charges2);
			AssertEquals("AC_Desc", charges2[0].JR_Desc);
			AssertEquals("Charge 1", charges2[1].JR_Desc);
			JASDataRegistry.Instance.IncludeProfitShareCharges = false;
			charges1 = shipment1.GetCollectCharges(consol.ReceivingForwarder);
			AssertEquals(1, charges1.Length);
			AssertEquals("BLAH5", charges1[0].JR_Desc);
			charges2 = shipment2.GetCollectCharges(consol.ReceivingForwarder);
			AssertEquals(1, charges2.Length);
			AssertEquals("Charge 1", charges2[0].JR_Desc);
		}

		void SetupProfitShareChargeRegistry(Guid chargePK)
		{
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargePK);
		}

		public void TestGetChargesCostAndRevenueWhenNoShipmentJob()
		{
			AssertNull("Sanity check", Shipment.Job);
			AssertEquals(0m, Shipment.TotalFreightRevenue);
			AssertEquals(0m, Shipment.TotalFreightCost);
			AssertEquals(0m, Shipment.TotalCost);
			AssertEquals(0m, Shipment.TotalOtherCosts);
			AssertEquals(0m, Shipment.AgentDeclaredGrossProfit);
			AssertEquals(0m, Shipment.GetProfitShareDueDestination(SellAccount));
			AssertEquals(0m, Shipment.GetTotalCollectChargesWithoutProfitShare(SellAccount));
			AssertEquals(0, Shipment.GetCollectCharges(SellAccount).Length);
		}

		void PopulateChargesForTest()
		{
			AddNewJobHeaderToShipment();
			JASOrgHeader differentSellAccount = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
			differentSellAccount.OH_Code = "XYZ123";
			differentSellAccount.OH_IsDebtor = true;
			differentSellAccount.Factory.Save();
			Charge freightCharge1 = Shipment.Job.Charges.AddNew();
			freightCharge1.JR_AC = Env.Registry.FreightChargeCode;
			freightCharge1.JR_AgentDeclaredCostAmtLocal = 100m;
			freightCharge1.JR_LocalCostAmt = 150m;
			freightCharge1.JR_LocalSellAmt = 200m;
			freightCharge1.JR_OH_SellAccount = SellAccount.PK;
			Charge freightCharge2 = Shipment.Job.Charges.AddNew();
			freightCharge2.JR_AC = Env.Registry.FreightChargeCode;
			freightCharge2.JR_LocalCostAmt = 1250m;
			freightCharge2.JR_LocalSellAmt = 300m;
			freightCharge2.JR_AgentDeclaredSellAmtLocal = 315m;
			freightCharge2.JR_OH_SellAccount = differentSellAccount.PK;
			Charge otherCharge1 = Shipment.Job.Charges.AddNew();
			otherCharge1.JR_LocalCostAmt = 50m;
			otherCharge1.JR_LocalSellAmt = 60m;
			otherCharge1.JR_OH_SellAccount = SellAccount.PK;
			Charge otherCharge2 = Shipment.Job.Charges.AddNew();
			otherCharge2.JR_LocalSellAmt = 70m;
			otherCharge2.JR_AgentDeclaredSellAmtLocal = 80m;
			otherCharge2.JR_OH_SellAccount = SellAccount.PK;
			Charge otherCharge3 = Shipment.Job.Charges.AddNew();
			otherCharge3.JR_LocalCostAmt = 0m;
			otherCharge3.JR_AgentDeclaredCostAmtLocal = 11m;
			otherCharge3.JR_LocalSellAmt = 0m;
			otherCharge3.JR_AgentDeclaredSellAmtLocal = 13m;
			otherCharge3.JR_OH_SellAccount = differentSellAccount.PK;
			Charge profitShareCharge1 = Shipment.Job.Charges.AddNew();
			profitShareCharge1.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			profitShareCharge1.JR_LocalSellAmt = -15m;
			profitShareCharge1.JR_OH_SellAccount = SellAccount.PK;
			Charge profitShareCharge2 = Shipment.Job.Charges.AddNew();
			profitShareCharge2.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			profitShareCharge2.JR_AgentDeclaredSellAmtLocal = -22m;
			profitShareCharge2.JR_OH_SellAccount = SellAccount.PK;
			Charge profitShareCharge3 = Shipment.Job.Charges.AddNew();
			profitShareCharge3.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			profitShareCharge3.JR_AgentDeclaredSellAmtLocal = 12m;
			profitShareCharge3.JR_OH_SellAccount = SellAccount.PK;
			Charge profitShareCharge4 = Shipment.Job.Charges.AddNew();
			profitShareCharge4.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			profitShareCharge4.JR_AgentDeclaredSellAmtLocal = -100m;
			profitShareCharge4.JR_OH_SellAccount = differentSellAccount.PK;
		}

		JASOrgHeader SellAccount
		{
			get
			{
				if (fSellAccount == null)
				{
					fSellAccount = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
					fSellAccount.OH_IsDebtor = true;
					fSellAccount.OH_Code = "ZUBTED123";
					fSellAccount.Factory.Save();
				}

				return fSellAccount;
			}
		}

		JASOrgHeader fSellAccount;
		#endregion
		#region Validation
		public void TestRunPreSaveValidation()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.RunPreSaveValidation();
			Assert("Should not be loaded for sea shipment", !Shipment.IsAWBLoaded);
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.RunPreSaveValidation();
			Assert("Should be loaded for air shipment so that AWB could be validated", Shipment.IsAWBLoaded);
		}

		public void TestJXCValidation()
		{
			JASForwardingShipment localShipment = Factory.New<JASForwardingShipment>();
			AssertEquals("No domain validations registered, should use base type", typeof(JXCForwardingShipmentValidation), localShipment.JXCValidation.GetType());
			Factory.Validation.MainGroup.RegisterValidationType<JASForwardingShipment, JXCForwardingSeaShipmentValidation>();
			AssertEquals("Registered to the MainDomainValidationGroup", typeof(JXCForwardingSeaShipmentValidation), localShipment.JXCValidation.GetType());
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Factory.Validation.MainGroup.UnregisterValidationType<JASForwardingShipment, JXCForwardingSeaShipmentValidation>();
			newFactory.Validation.MainGroup.RegisterValidationType<JASForwardingShipment, JXCForwardingSeaShipmentValidation>();
			Factory.Validation.AddAllFrom(newFactory.Validation);
			AssertEquals("Registered to the AdditionalDomainValidationGroup", typeof(JXCForwardingSeaShipmentValidation), localShipment.JXCValidation.GetType());
			newFactory.Validation.MainGroup.UnregisterValidationType<JASForwardingShipment, JXCForwardingSeaShipmentValidation>();
			Factory.Validation.MainGroup.RegisterValidationType<ForwardingShipment, InvalidDomainValidation>();
			AssertEquals("Domain validation found is not of type JXCForwardingShipmentValidation, should return the base type", typeof(JXCForwardingShipmentValidation), Shipment.JXCValidation.GetType());
		}

		#region class InvalidDomainValidation
		public class InvalidDomainValidation : ZValidation
		{
			public InvalidDomainValidation(ForwardingShipment shipment) : base(shipment)
			{
			}

			public override Type AutoValidationType
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public override void ValidateAll()
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		#endregion
		#endregion
		#region GSUM Messaging
		[TestDate(2006, 1, 1, 20, 20, 20)]
		public void TestGsumMessaging_FileCreatedOnSave()
		{
			JASForwardingShipment shipment = GetShipmentForGsumTesting();
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			shipment.JS_OH_ImportBroker = Factory.NewWithValidTestData<JASOrgHeader>().PK;
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			declaration.LogCustomsClearedIfNeeded();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
			AssertGsumFileContent(ExpectedGsumLines);
		}

		public void TestGsumMessaging_AutoJXCMessagingDisabled()
		{
			JASDataRegistry.Instance.EnableAutoJXCMessaging = false;
			JASForwardingShipment shipment = GetShipmentForGsumTesting();
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			shipment.JS_OH_ImportBroker = Factory.NewWithValidTestData<JASOrgHeader>().PK;
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			declaration.LogCustomsClearedIfNeeded();
			Factory.Save();
			AssertNoExportFilesInExportDir("File should not be created when GSUM messaging is disabled");
		}

		public void TestGsumMessaging_AutoJXCMessagingEnabledButGsumDisabled()
		{
			JASDataRegistry.Instance.DisableGsumMessaging = true;
			JASForwardingShipment shipment = GetShipmentForGsumTesting();
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			shipment.JS_OH_ImportBroker = Factory.NewWithValidTestData<JASOrgHeader>().PK;
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			declaration.LogCustomsClearedIfNeeded();
			Factory.Save();
			AssertNoExportFilesInExportDir("File should not be created when GSUM messaging is disabled");
		}

		public void TestGsumMessaging_SaveNotSucceeded()
		{
			JASForwardingShipment shipment = GetShipmentForGsumTesting();
			shipment.SaveSucceededOverridesForTesting = false;
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			shipment.JS_OH_ImportBroker = Factory.NewWithValidTestData<JASOrgHeader>().PK;
			Factory.Save();
			AssertNoExportFilesInExportDir("File should not be created when save fails.");
		}

		[TestDate(2006, 1, 1, 20, 20, 20)]
		public void TestGsumMessaging_ExistingLinesShouldBeCleared()
		{
			JASForwardingShipment shipment = GetShipmentForGsumTesting();
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2007, 1, 1, 20, 20, 30);
			shipment.JS_OH_ImportBroker = Factory.NewWithValidTestData<JASOrgHeader>().PK;
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.LogCustomsCommencedIfNeeded();
			declaration.LogCustomsClearedIfNeeded();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
			AssertGsumFileContent(ExpectedGsumLines);
			string fullPath = Path.Combine(JASDataRegistry.Instance.JXCOutgoingDirectoryName, "GSUM_GsumTest_20060101202020.txt");
			File.Delete(fullPath);
			Shipment.JS_GoodsDescription = "THIS SHD MK HASCHANGES = TRUE";
			AssertNoExportFilesInExportDir("Existing GSUM lines should be cleared before running subsequent exports.");
		}

		JASForwardingShipment GetShipmentForGsumTesting()
		{
			JASForwardingShipment result = Factory.New<JASForwardingShipment>();
			result.FillWithValidTestData();
			result.JS_TransportMode = Core.Constants.TransportModes.Air;
			result.JS_HouseBill = "GsumTest";
			result.JS_RL_NKOrigin = "SGSIN";
			result.JS_RL_NKDestination = "AUBNE";
			return result;
		}

		void AssertGsumFileContent(string expectedContent)
		{
			string fullPath = Path.Combine(JASDataRegistry.Instance.JXCOutgoingDirectoryName, "GSUM_GsumTest_20060101202020.txt");
			Assert("GSUM file does not exist", File.Exists(fullPath));
			AssertFileSameAsString(fullPath, expectedContent);
		}

		void AssertNoExportFilesInExportDir(string errorMessage)
		{
			AssertEquals(errorMessage + " There should be no files exported", 0, Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName).Length);
		}

		const string ExpectedGsumLines = @"HEAD3100;NONET;NONET;NONET;NONET;AUBNE
GSUM3100;A;Y;OFD;01/01/2007;20:20;;EBM22Q33TU475BXH3P60;GSUMTEST;NONET;A
GSUM3100;A;Y;DTO;01/01/2006;20:20;;EBM22Q33TU475BXH3P60;GSUMTEST;NONET;A
GSUM3100;A;Y;CUS;01/01/2006;20:20;;EBM22Q33TU475BXH3P60;GSUMTEST;NONET;A
GSUM3100;A;Y;CLR;01/01/2006;20:20;;EBM22Q33TU475BXH3P60;GSUMTEST;NONET;A
TRLR3100";
		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			string testDir = Path.Combine(Env.TempPath, "JASShipment_GsumExportTest");
			Directory.CreateDirectory(testDir);
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = testDir;
		}

		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			base.TearDown();
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
				}

				return fShipment;
			}
		}

		GlbCompany OtherCompany
		{
			get
			{
				if (fOtherCompany == null)
				{
					ZQuery filter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
					fOtherCompany = Factory.LoadTop1<GlbCompany>(filter);
				}

				return fOtherCompany;
			}
		}

		void AddJobChargeToShipment(IJXCExportHeader headerData, JASForwardingShipment shipment, ZString desc, bool useCurrentCompany, bool isCollectCharge)
		{
			AddJobChargeToShipment(headerData, shipment, desc, useCurrentCompany, isCollectCharge, ZGuid.Empty);
		}

		void AddJobChargeToShipment(IJXCExportHeader headerData, JASForwardingShipment shipment, ZString desc, bool useCurrentCompany, bool isCollectCharge, ZGuid profitShareChargePK)
		{
			GlbCompany company = (useCurrentCompany) ? GlbCompany.CurrentCompany : OtherCompany;
			JASJob job = AddNewJobHeaderToShipment(shipment, company);
			JobCharge newCharge = job.Charges.AddNew();
			newCharge.JR_OH_SellAccount = (isCollectCharge && headerData.ReceivingForwarder != null) ? headerData.ReceivingForwarder.PK : ZGuid.Empty;
			newCharge.JR_Desc = desc;
			if (!profitShareChargePK.IsEmpty)
			{
				newCharge.JR_AC = profitShareChargePK.ToGuid();
			}
		}

		void AddNewJobHeaderToShipment()
		{
			AddNewJobHeaderToShipment(Shipment, GlbCompany.CurrentCompany);
		}

		JASJob AddNewJobHeaderToShipment(JASForwardingShipment shipment, GlbCompany company)
		{
			ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
			filter.AddToFilter(JobHeaderSchema.JH_GC, company.PK);
			JASJob job = Factory.LoadTop1<JASJob>(filter);
			if (job == null)
			{
				job = Factory.NewJobForTesting<JASJob>();
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.JH_GC = company.PK;
			}

			return job;
		}

		void SortChargesByDesc(JobCharge[] charges)
		{
			Array.Sort(charges, new ChargeDescComparer());
		}

		class ChargeDescComparer : IComparer
		{
			#region IComparer Members
			public int Compare(object x, object y)
			{
				return Compare(x as JobCharge, y as JobCharge);
			}

			int Compare(JobCharge x, JobCharge y)
			{
				return x.JR_Desc.CompareTo(y.JR_Desc);
			}
			#endregion
		}

		JASForwardingShipment fShipment;
		GlbCompany fOtherCompany;
		#endregion
	}
}
