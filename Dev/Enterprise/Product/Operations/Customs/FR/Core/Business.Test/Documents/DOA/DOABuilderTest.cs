using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	public class DOABuilderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new DOABuilder(null));
			AssertNoExceptionThrown(() => _ = new DOABuilder(Factory.New<NctsHeader>()));
		}

		public void TestBuildCurrentUser()
		{
			var doa = builder.Build();
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, doa.CurrentUser.Contact);
		}

		public void TestBuildPortSystem()
		{
			var registryCodes = RegistrySetup();
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				nctsHeader.BH_RL_NKImportLoadPort = "FRPAR";
				var doa = builder.Build();
				AssertEquals("PortSystem is inferred from registry entry matching Port Of Dispatch.", "MGI", doa.PortSystem);
				AssertEquals("SICCodeType should be CI5 when PortSystem is MGI.", "CI5", doa.SICCodeType);

				nctsHeader.BH_RL_NKImportLoadPort = "FRABC";
				doa = builder.Build();
				AssertEquals("PortSystem is inferred from registry entry matching Port Of Dispatch.", "SOGET", doa.PortSystem);
				AssertEquals("SICCodeType should be SOA when PortSystem is SOGET.", "SOA", doa.SICCodeType);
			}
		}

		public void TestBuildSendingPartySICCode()
		{
			var branchProxy = GlbBranch.CurrentBranch.OrgProxy;
			var ci5Code = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "CI5_001", Core.Constants.CountryCodes.France);
			var sonCode = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SOA, "SOA_001", Core.Constants.CountryCodes.France);
			var registryCodes = RegistrySetup();
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				nctsHeader.BH_RL_NKImportLoadPort = "FRPAR";
				var doa = builder.Build();
				AssertEquals("Registry + Port Of Dispatch => SICCodeType is CI5, so SendingPartyID should match branch Proxy Customs code of type CI5.", "CI5_001", doa.SendingPartyID);
				AssertEquals("PortOfDispatch is empty", "FORWARDER", doa.SendingPartySICCode);

				nctsHeader.BH_RL_NKImportLoadPort = "FRABC";
				doa = builder.Build();
				AssertEquals("Registry + Port Of Dispatch => SICCodeType is SOA, so SendingPartyID should match branch Proxy Customs code of type SOA.", "SOA_001", doa.SendingPartyID);
				AssertEquals("PortOfDispatch is empty", "FORWARDER2", doa.SendingPartySICCode);
			}
		}

		public void TestBuildRecipient()
		{
			var helper = new UniversalReferenceTestDataHelper(nctsHeader.Factory);
			helper.CreateCusMapType("FRCCS", "OUT", "FRCCS", true);
			helper.CreateCusMap("FRCCS", "FRBOD", "CI5BOD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "FR");
			helper.CreateCusMap("FRCCS", "FRDKK", "CI5DKE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "FR");
			Factory.Save();

			nctsHeader.PortOfDispatch = "FRBOD";
			var doa = builder.Build();
			AssertEquals("CI5BOD", doa.RecipientID);
			AssertEquals("CI5BOD", doa.RecipientSICCode);

			nctsHeader.PortOfDispatch = "FRDKK";
			doa = builder.Build();
			AssertEquals("CI5DKE", doa.RecipientID);
			AssertEquals("CI5DKE", doa.RecipientSICCode);

			nctsHeader.PortOfDispatch = "FRFOS";
			doa = builder.Build();
			AssertEquals("", doa.RecipientID);
			AssertEquals("", doa.RecipientSICCode);
		}

		public void TestCommonAccessRef_Phase4()
		{
			var doa = builder.Build();
			AssertEquals("", doa.CommonAccessRef);

			var goodItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var previousDocument1 = goodItem1.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = PreviousDocumentCodeList.Codes.T2M;
			previousDocument1.CSI_ReferenceNumber = "Reference1";

			var goodItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var previousDocument2 = goodItem2.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = PreviousDocumentCodeList.Codes.ZZZ;
			previousDocument2.CSI_ReferenceNumber = "Reference2";

			doa = builder.Build();
			AssertEquals("Reference2", doa.CommonAccessRef);
		}

		public void TestCommonAccessRef()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var doa = builder.Build();
			AssertEquals("", doa.CommonAccessRef);

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = FRConstants.CAED.CommonAccessDocumentCode;
			supportingDocument1.CSI_ReferenceNumber = "SupportingDocument1";

			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "NABC";
			supportingDocument2.CSI_ReferenceNumber = "SupportingDocument2";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			doa = builder.Build();
			AssertEquals("SupportingDocument1", doa.CommonAccessRef);
		}

		public void TestBuildReferenceNumbers()
		{
			nctsHeader.BH_JobReference = "NCT0004";
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			mrnEntryNumber.CE_EntryNum = "MRN0006";
			nctsHeader.LocalReferenceNumber = "LRN0008";

			var doa = builder.Build();
			AssertEquals("NCT0004", doa.JobNumber);
			AssertEquals("MRN0006", doa.DeclarationNumber);
			AssertEquals("LRN0008", doa.DeclarationReference);
		}

		public void TestBuildTypeAndStatus_Phase4()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = "T1";
			var doa = builder.Build();
			AssertEquals("T1", doa.DeclarationType);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
			doa = builder.Build();
			AssertEquals("", doa.DeclarationStatus);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
			doa = builder.Build();
			AssertEquals("Status is BAE only after released.", "BAE", doa.DeclarationStatus);
		}

		public void TestBuildTypeAndStatus()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			builder = new DOABuilder(nctsHeader);
			nctsHeader.MovementHeader.BM_InBondEntryType = "T1";
			var doa = builder.Build();
			AssertEquals("T1", doa.DeclarationType);

			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled;
			doa = builder.Build();
			AssertEquals("", doa.DeclarationStatus);

			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			doa = builder.Build();
			AssertEquals("Status is BAE only after released.", "BAE", doa.DeclarationStatus);
		}

		public void TestBuildCustomsOffices()
		{
			var desCustomOffice = nctsHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			desCustomOffice.CY_Data = "DEP001";
			var depCustomOffice = nctsHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			depCustomOffice.CY_Data = "DES001";
			nctsHeader.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "EXT001");
			var doa = builder.Build();
			AssertEquals("DEP001", doa.CustomsOfficeCodeOfDeparture.Code);
			AssertEquals("DES001", doa.CustomsOfficeCodeOfDestination.Code);
		}

		public void TestBuildPackages_Phase4()
		{
			var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var packageA = goodsItem1.Packages.AddNew();
			packageA.B5_UnitCount = 10;
			packageA.B5_UnitType = "1A";
			var packageB = goodsItem1.Packages.AddNew();
			packageB.B5_UnitCount = 20;
			packageB.B5_UnitType = "1B";
			var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var packageC = goodsItem2.Packages.AddNew();
			packageC.B5_UnitCount = 30;
			packageC.B5_UnitType = "1C";
			var doa = builder.Build();
			AssertEquals(60, doa.TotalNumberOfPacks);
			AssertEquals("1A", doa.PackageType.Code);
		}

		public void TestBuildPackages()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill1 = nctsHeader.Bills.AddNew();
			var billGoodsItem1 = bill1.GoodsItems.AddNew();
			var package1 = billGoodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 100;
			package1.B5_UnitType = "2A";
			var package2 = billGoodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 200;
			package2.B5_UnitType = "2B";

			var bill2 = nctsHeader.Bills.AddNew();
			var billGoodsItem2 = bill2.GoodsItems.AddNew();
			var package3 = billGoodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 300;
			package3.B5_UnitType = "2C";

			var doa = builder.Build();
			AssertEquals(600, doa.TotalNumberOfPacks);
			AssertEquals("MLT", doa.PackageType.Code);

			package2.B5_UnitType = "2A";
			package3.B5_UnitType = "2A";
			doa = builder.Build();
			AssertEquals("2A", doa.PackageType.Code);
		}

		public void TestBuildWeight_Phase4()
		{
			var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 10;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem1.BY_NetWeight = 8;
			goodsItem1.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 6000;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			goodsItem2.BY_NetWeight = 3000;
			goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			var doa = builder.Build();
			AssertEquals(16m, doa.TotalGrossWeightInKilograms);
			AssertEquals(11m, doa.TotalNetWeightInKilograms);
		}

		public void TestBuildWeight()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.MovementHeader.BM_GrossWeight = 10m;

			var goodsItem1 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem1.BY_NetWeight = 8;
			goodsItem1.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			var goodsItem2 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem2.BY_NetWeight = 3000;
			goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Grams;

			var doa = builder.Build();
			AssertEquals(10m, doa.TotalGrossWeightInKilograms);
			AssertEquals(11m, doa.TotalNetWeightInKilograms);
		}

		public void TestPopulateTariffs()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			builder = new DOABuilder(nctsHeader);
			var goodsItem1 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem1.BY_HarmonisedTariff = "10.100.00";
			var goodsItem2 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem2.BY_HarmonisedTariff = "20.200.00";
			var goodsItem3 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem3.BY_HarmonisedTariff = "";
			var doa = builder.Build();
			AssertContainsExactElementsInExactOrder(new ZString[]
			{
				"10.100.00",
				"20.200.00"
			}, doa.Tariffs.Select(x => x.Code));
		}

		public void TestPopulateTariffs_Phase4()
		{
			var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_HarmonisedTariff = "10.100.00";
			var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_HarmonisedTariff = "20.200.00";
			var goodsItem3 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem3.BY_HarmonisedTariff = "";
			var doa = builder.Build();
			AssertContainsExactElementsInExactOrder(new ZString[]
			{
				"10.100.00",
				"20.200.00"
			}, doa.Tariffs.Select(x => x.Code));
		}

		public void TestBuildPortDues_Phase4()
		{
			nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var fee1 = goodsItem.Fees.AddNew();
			fee1.BFE_ChargeType = HarbourFeeCodes.Codes.V905;
			fee1.BFE_ChargeAmount = 11.11m;
			var fee2 = goodsItem.Fees.AddNew();
			fee2.BFE_ChargeType = HarbourFeeCodes.Codes.P635;
			fee2.BFE_ChargeAmount = 12m;

			var doa = builder.Build();
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, doa.PortDuesCurrency.Code);
			AssertEquals(Core.Constants.CountryCodes.France, doa.Port);
			AssertEquals(23m, doa.PortDuesAmount);
		}

		public void TestBuildPortDues()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
			var bill1 = nctsHeader.Bills.AddNew();
			var billGoodsItem1 = bill1.GoodsItems.AddNew();
			var billFee1 = billGoodsItem1.Fees.AddNew();
			billFee1.BFE_ChargeType = HarbourFeeCodes.Codes.V905;
			billFee1.BFE_ChargeAmount = 1.11m;
			var billFee2 = billGoodsItem1.Fees.AddNew();
			billFee2.BFE_ChargeType = HarbourFeeCodes.Codes.P635;
			billFee2.BFE_ChargeAmount = 1000m;
			var billFee3 = billGoodsItem1.Fees.AddNew();
			billFee3.BFE_ChargeType = "VAT";
			billFee3.BFE_ChargeAmount = 100m;

			var doa = builder.Build();
			AssertEquals(1001m, doa.PortDuesAmount);
		}

		public void TestBuildOtherFields()
		{
			nctsHeader.MovementHeader.PreLodgedForAgreedLocationOfGoodsCode = true;
			var doa = builder.Build();
			AssertEquals(true, doa.IsPrelodged);

			nctsHeader.MovementHeader.PreLodgedForAgreedLocationOfGoodsCode = false;
			doa = builder.Build();
			AssertEquals(false, doa.IsPrelodged);

			nctsHeader.MovementHeader.BM_SealQty = 0;
			doa = builder.Build();
			AssertEquals(false, doa.HasSeal);

			nctsHeader.MovementHeader.BM_SealQty = 5;
			doa = builder.Build();
			AssertEquals(true, doa.HasSeal);
		}

		public void TestValidateSendingPartyID()
		{
			var message = "Sender's ID is required.";
			var doa = builder.Build();

			doa.SendingPartyID = "";
			doa.Validate(nameof(doa.SendingPartyID));
			AssertHasMessageError(doa.SendingPartyIDInfo, message);

			doa.SendingPartyID = "EASYLOG";
			doa.Validate(nameof(doa.SendingPartyID));
			AssertNoMessageError(doa.SendingPartyIDInfo, message);
		}

		public void TestValidateSendingPartySICCode()
		{
			var branchProxy = GlbBranch.CurrentBranch.OrgProxy;
			var ci5Code = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "CI5_001", Core.Constants.CountryCodes.France);
			var sonCode = branchProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SOA, "SOA_001", Core.Constants.CountryCodes.France);
			var registryCodes = RegistrySetup();
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				var message = "Sender's SIC code is required.";
				var doa = builder.Build();

				doa.SendingPartySICCode = "";
				doa.Validate(nameof(doa.SendingPartySICCode));
				AssertHasError(doa.SendingPartySICCodeInfo, "There is no Forwarder code available in Registry \"Port Community System Codes of Forwarder and Agent\".");
				AssertHasMessageError(doa.SendingPartySICCodeInfo, message);

				nctsHeader.BH_RL_NKImportLoadPort = "FRPAR";
				doa.Validate(nameof(doa.SendingPartySICCode));
				AssertNoError(doa.SendingPartySICCodeInfo, "There is no Forwarder code available in Registry \"Port Community System Codes of Forwarder and Agent\".");
				AssertHasMessageError(doa.SendingPartySICCodeInfo, message);

				doa.SendingPartySICCode = "CI5_001";
				doa.Validate(nameof(doa.SendingPartySICCode));
				AssertNoMessageError(doa.SendingPartySICCodeInfo, message);
			}
		}

		public void TestValidateRecipientID()
		{
			var message = "Recipient's ID is required.";
			var doa = builder.Build();

			doa.RecipientID = "";
			doa.Validate(nameof(doa.RecipientID));
			AssertHasMessageError(doa.RecipientIDInfo, message);

			doa.RecipientID = "EASYLOG";
			doa.Validate(nameof(doa.RecipientID));
			AssertNoMessageError(doa.RecipientIDInfo, message);
		}

		public void TestValidateRecipientSICCode()
		{
			var message = "Recipient's SIC code is required.";
			var doa = builder.Build();

			doa.RecipientSICCode = "";
			doa.Validate(nameof(doa.RecipientSICCode));
			AssertHasMessageError(doa.RecipientSICCodeInfo, message);

			doa.RecipientSICCode = "SON_001";
			doa.Validate(nameof(doa.RecipientSICCode));
			AssertNoMessageError(doa.RecipientSICCodeInfo, message);
		}

		public void TestValidateCTOPartyID()
		{
			var message = "CTO address's ID is required.";
			var doa = builder.Build();

			doa.CTOPartyID = "";
			doa.Validate(nameof(doa.CTOPartyID));
			AssertHasMessageError(doa.CTOPartyIDInfo, message);

			doa.CTOPartyID = "EASYLOG";
			doa.Validate(nameof(doa.CTOPartyID));
			AssertNoMessageError(doa.CTOPartyIDInfo, message);
		}

		public void TestValidateCTOPartySICCode()
		{
			var message = "CTO address's SIC code is required.";
			var doa = builder.Build();

			doa.CTOPartySICCode = "";
			doa.Validate(nameof(doa.CTOPartySICCode));
			AssertHasMessageError(doa.CTOPartySICCodeInfo, message);

			doa.CTOPartySICCode = "SON_001";
			doa.Validate(nameof(doa.CTOPartySICCode));
			AssertNoMessageError(doa.CTOPartySICCodeInfo, message);
		}

		public void TestContainers()
		{
			nctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT001";

			var doa = builder.Build();

			AssertEquals("CNT001 should be set on EquipmentReference", "CNT001", doa.EquipmentRef);

			nctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT002";
			nctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT003";

			doa = builder.Build();
			AssertEquals("All 3 containers should be built", 3, doa.Containers.Count);
			AssertEquals("CNT001 should be retrieved", true, doa.Containers.Any(x => x.Number == "CNT001"));
			AssertEquals("CNT002 should be retrieved", true, doa.Containers.Any(x => x.Number == "CNT002"));
			AssertEquals("CNT003 should be retrieved", true, doa.Containers.Any(x => x.Number == "CNT003"));
		}

		public void TestValidateDeclarationType()
		{
			var message = "Declaration type is required.";
			var doa = builder.Build();

			doa.DeclarationType = ZString.Empty;
			AssertHasMessageError(doa.DeclarationTypeInfo, message);

			doa.DeclarationType = "T1";
			AssertNoMessageError(doa.DeclarationTypeInfo, message);
		}

		public void TestValidateJobNumber()
		{
			var message = "Job number is required.";
			var doa = builder.Build();

			doa.JobNumber = ZString.Empty;
			AssertHasMessageError(doa.JobNumberInfo, message);

			doa.JobNumber = "NCT001";
			AssertNoMessageError(doa.JobNumberInfo, message);
		}

		public void TestValidateDeclarationNumber()
		{
			var message = "Declaration number is required.";
			var doa = builder.Build();

			doa.DeclarationNumber = ZString.Empty;
			AssertHasMessageError(doa.DeclarationNumberInfo, message);

			doa.DeclarationNumber = "MRN001";
			AssertNoMessageError(doa.DeclarationNumberInfo, message);
		}

		public void TestValidateDeclarationReference()
		{
			var message = "Declaration reference is required.";
			var doa = builder.Build();

			doa.DeclarationReference = ZString.Empty;
			AssertHasMessageError(doa.DeclarationReferenceInfo, message);

			doa.DeclarationReference = "NCT001";
			AssertNoMessageError(doa.DeclarationReferenceInfo, message);
		}

		public void TestValidateTotalNumberOfPacks()
		{
			var message = "At least one pack should be entered.";
			var doa = builder.Build();

			doa.TotalNumberOfPacks = 0;
			AssertHasMessageError(doa.TotalNumberOfPacksInfo, message);

			doa.TotalNumberOfPacks = 10;
			AssertNoMessageError(doa.TotalNumberOfPacksInfo, message);
		}

		public void TestValidateTotalGrossWeightInKilograms()
		{
			var message = "Gross weight cannot be zero.";
			var doa = builder.Build();

			doa.TotalGrossWeightInKilograms = 0;
			AssertHasMessageError(doa.TotalGrossWeightInKilogramsInfo, message);

			doa.TotalGrossWeightInKilograms = 10;
			AssertNoMessageError(doa.TotalGrossWeightInKilogramsInfo, message);
		}
		static Freight.Business.CommunitySystemCodesOfForwarderAndAgentCollection RegistrySetup()
		{
			var registryCodes = new Freight.Business.CommunitySystemCodesOfForwarderAndAgentCollection();
			var registryCode1 = registryCodes.AddNew();
			registryCode1.AgentCode = OrgCusCode.FranceCodeTypes.CI5;
			registryCode1.ForwarderCode = "FORWARDER";
			registryCode1.Port = "FRPAR";
			registryCode1.PCS = FrenchPortSystemCodeList.Codes.MGI;

			var registryCode2 = registryCodes.AddNew();
			registryCode2.AgentCode = OrgCusCode.FranceCodeTypes.SOA;
			registryCode2.ForwarderCode = "FORWARDER2";
			registryCode2.Port = "FRABC";
			registryCode2.PCS = FrenchPortSystemCodeList.Codes.SOGET;
			return registryCodes;
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			builder = new DOABuilder(nctsHeader);
		}

		NctsHeader nctsHeader;
		DOABuilder builder;
	}
}
