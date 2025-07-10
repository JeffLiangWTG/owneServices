using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	public class NctsCAEDBuilderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new NctsCAEDBuilder(null));
			AssertNoExceptionThrown(() => _ = new NctsCAEDBuilder(Factory.New<NctsHeader>()));
		}

		public void TestBuildCurrentUser()
		{
			var caed = builder.Build();
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, caed.CurrentUser.Contact);
		}

		public void TestBuildPortSystem()
		{
			var registryCodes = RegistrySetup();
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				nctsHeader.BH_RL_NKImportLoadPort = "FRPAR";
				var caed = builder.Build();
				AssertEquals("PortSystem is inferred from registry entry matching Port Of Dispatch.", "MGI", caed.PortSystem);
				AssertEquals("SICCodeType should be CI5 when PortSystem is MGI.", "CI5", caed.SICCodeType);

				nctsHeader.BH_RL_NKImportLoadPort = "FRABC";
				caed = builder.Build();
				AssertEquals("PortSystem is inferred from registry entry matching Port Of Dispatch.", "SOGET", caed.PortSystem);
				AssertEquals("SICCodeType should be SOA when PortSystem is SOGET.", "SOA", caed.SICCodeType);
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
				var caed = builder.Build();
				AssertEquals("Registry + Port Of Dispatch => SICCodeType is CI5, so SendingPartyID should match branch Proxy Customs code of type CI5.", "CI5_001", caed.SendingPartyID);
				AssertEquals("PortOfDispatch is empty", "FORWARDER", caed.SendingPartySICCode);

				nctsHeader.BH_RL_NKImportLoadPort = "FRABC";
				caed = builder.Build();
				AssertEquals("Registry + Port Of Dispatch => SICCodeType is SOA, so SendingPartyID should match branch Proxy Customs code of type SOA.", "SOA_001", caed.SendingPartyID);
				AssertEquals("PortOfDispatch is empty", "FORWARDER2", caed.SendingPartySICCode);
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
			var caed = builder.Build();
			AssertEquals("CI5BOD", caed.RecipientID);
			AssertEquals("CI5BOD", caed.RecipientSICCode);

			nctsHeader.PortOfDispatch = "FRDKK";
			caed = builder.Build();
			AssertEquals("CI5DKE", caed.RecipientID);
			AssertEquals("CI5DKE", caed.RecipientSICCode);

			nctsHeader.PortOfDispatch = "FRFOS";
			caed = builder.Build();
			AssertEquals("", caed.RecipientID);
			AssertEquals("", caed.RecipientSICCode);
		}

		public void TestCommonAccessRef_Phase4()
		{
			var caed = builder.Build();
			AssertEquals("", caed.CommonAccessRef);

			var goodItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var previousDocument1 = goodItem1.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = PreviousDocumentCodeList.Codes.T2M;
			previousDocument1.CSI_ReferenceNumber = "Reference1";

			var goodItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var previousDocument2 = goodItem2.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = PreviousDocumentCodeList.Codes.ZZZ;
			previousDocument2.CSI_ReferenceNumber = "Reference2";

			caed = builder.Build();
			AssertEquals("Reference2", caed.CommonAccessRef);
		}

		public void TestCommonAccessRef()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var caed = builder.Build();
			AssertEquals("", caed.CommonAccessRef);

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = FRConstants.CAED.CommonAccessDocumentCode;
			supportingDocument1.CSI_ReferenceNumber = "SupportingDocument1";

			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "NABC";
			supportingDocument2.CSI_ReferenceNumber = "SupportingDocument2";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			caed = builder.Build();
			AssertEquals("SupportingDocument1", caed.CommonAccessRef);
		}

		public void TestCTOPartySICCode()
		{
			var caed = builder.Build();
			AssertEquals("", caed.CTOPartySICCode);
			AssertEquals("", caed.CTOPartyID);
		}

		public void TestBuildReferenceNumbers()
		{
			nctsHeader.BH_JobReference = "NCT0004";
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			mrnEntryNumber.CE_EntryNum = "MRN0006";
			nctsHeader.LocalReferenceNumber = "LRN0008";

			var caed = builder.Build();
			AssertEquals("NCT0004", caed.JobNumber);
		}

		public void TestDeclarantsSIRETNumber_Phase4()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SIRET", Core.Constants.CountryCodes.France);

			nctsHeader.BH_JobReference = "NCT0004";
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			mrnEntryNumber.CE_EntryNum = "MRN0006";
			nctsHeader.LocalReferenceNumber = "LRN0008";

			nctsHeader.Declarant.E2_OA_Address = declarant.MainAddress.PK;

			var caed = builder.Build();
			AssertEquals("In Phase 4, SIRETNumber should be from Declarant", "FRSIRET", caed.DeclarantsSIRETNumber);
		}

		public void TestDeclarantsSIRETNumber()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SIRET1", Core.Constants.CountryCodes.France);
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SIRET2", Core.Constants.CountryCodes.France);
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SIRET3", Core.Constants.CountryCodes.France);

			nctsHeader.BH_JobReference = "NCT0004";
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			mrnEntryNumber.CE_EntryNum = "MRN0006";
			nctsHeader.LocalReferenceNumber = "LRN0008";

			nctsHeader.Declarant.E2_OA_Address = orgHeader1.MainAddress.PK;
			nctsHeader.Principal.E2_OA_Address = orgHeader2.MainAddress.PK;

			var caed = builder.Build();
			AssertEquals("In Phase 5, SIRETNumber should be from Principal if no Representative", "FRSIRET2", caed.DeclarantsSIRETNumber);

			nctsHeader.MovementHeader.Representative.E2_OA_Address = orgHeader3.MainAddress.PK;
			caed = builder.Build();
			AssertEquals("In Phase 5, SIRETNumber should be from Representative", "FRSIRET3", caed.DeclarantsSIRETNumber);
		}

		public void TestBuildContainer()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var caed = new NctsCAEDBuilder(nctsHeader);

			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "container1";

			var build = caed.Build();
			AssertEquals("container1", build.ContainerNumbers);

			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "container2";

			build = caed.Build();
			AssertEquals("container1 / container2", build.ContainerNumbers);
		}

		public void TestBuildTypeAndStatus()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = "T1";
			var caed = builder.Build();
			AssertEquals("T1", caed.DeclarationType);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
			caed = builder.Build();
			AssertEquals("", caed.DeclarationStatus);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
			caed = builder.Build();
			AssertEquals("Status is BAE only after released.", "BAE", caed.DeclarationStatus);
		}

		public void TestBuildCustomsOffices()
		{
			var desCustomOffice = nctsHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			desCustomOffice.CY_Data = "DEP001";
			var depCustomOffice = nctsHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			depCustomOffice.CY_Data = "DES001";
			nctsHeader.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "EXT001");
			var caed = builder.Build();
			AssertEquals("DEP001", caed.CustomsOfficeCodeOfDeparture.Code);
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
			var caed = builder.Build();
			AssertEquals(60, caed.TotalNumberOfPacks);
			AssertEquals("1A", caed.PackageType.Code);
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

			var caed = builder.Build();
			AssertEquals(600, caed.TotalNumberOfPacks);
			AssertEquals("MLT", caed.PackageType.Code);

			package2.B5_UnitType = "2A";
			package3.B5_UnitType = "2A";
			caed = builder.Build();
			AssertEquals("2A", caed.PackageType.Code);
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

			var caed = builder.Build();
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, caed.PortDuesCurrency.Code);
			AssertEquals(Core.Constants.CountryCodes.France, caed.Port);
			AssertEquals(23m, caed.PortDuesAmount);
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

			var caed = builder.Build();
			AssertEquals(1001m, caed.PortDuesAmount);
		}

		public void TestValidateContainer()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var caed = new NctsCAEDBuilder(nctsHeader);
			var build = caed.Build();
			AssertHasMessageError(build.ContainerNumbersInfo, "At least one container is required.");

			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "container1";

			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "container2";

			build = caed.Build();
			AssertNoMessageError(build.ContainerNumbersInfo, "At least one container is required.");
		}

		public void TestValidateSendingPartyID()
		{
			var message = "Sender's ID is required.";
			var caed = builder.Build();

			caed.SendingPartyID = "";
			caed.Validate(nameof(caed.SendingPartyID));
			AssertHasMessageError(caed.SendingPartyIDInfo, message);

			caed.SendingPartyID = "EASYLOG";
			caed.Validate(nameof(caed.SendingPartyID));
			AssertNoMessageError(caed.SendingPartyIDInfo, message);
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
				var caed = builder.Build();

				caed.SendingPartySICCode = "";
				caed.Validate(nameof(caed.SendingPartySICCode));
				AssertHasError(caed.SendingPartySICCodeInfo, "There is no Forwarder code available in Registry \"Port Community System Codes of Forwarder and Agent\".");
				AssertHasMessageError(caed.SendingPartySICCodeInfo, message);

				nctsHeader.BH_RL_NKImportLoadPort = "FRPAR";
				caed.Validate(nameof(caed.SendingPartySICCode));
				AssertNoError(caed.SendingPartySICCodeInfo, "There is no Forwarder code available in Registry \"Port Community System Codes of Forwarder and Agent\".");
				AssertHasMessageError(caed.SendingPartySICCodeInfo, message);

				caed.SendingPartySICCode = "CI5_001";
				caed.Validate(nameof(caed.SendingPartySICCode));
				AssertNoMessageError(caed.SendingPartySICCodeInfo, message);
			}
		}

		public void TestValidateRecipientID()
		{
			var message = "Recipient's ID is required.";
			var caed = builder.Build();

			caed.RecipientID = "";
			caed.Validate(nameof(caed.RecipientID));
			AssertHasMessageError(caed.RecipientIDInfo, message);

			caed.RecipientID = "EASYLOG";
			caed.Validate(nameof(caed.RecipientID));
			AssertNoMessageError(caed.RecipientIDInfo, message);
		}

		public void TestValidateRecipientSICCode()
		{
			var message = "Recipient's SIC code is required.";
			var caed = builder.Build();

			caed.RecipientSICCode = "";
			caed.Validate(nameof(caed.RecipientSICCode));
			AssertHasMessageError(caed.RecipientSICCodeInfo, message);

			caed.RecipientSICCode = "SON_001";
			caed.Validate(nameof(caed.RecipientSICCode));
			AssertNoMessageError(caed.RecipientSICCodeInfo, message);
		}

		public void TestValidateDeclarantsSIRETNumber()
		{
			var message = "Declarant SIRET Number is required.";
			var caed = builder.Build();

			caed.DeclarantsSIRETNumber = "";
			caed.Validate(nameof(caed.DeclarantsSIRETNumber));
			AssertHasMessageError(caed.DeclarantsSIRETNumberInfo, message);

			caed.DeclarantsSIRETNumber = "SRT_001";
			caed.Validate(nameof(caed.DeclarantsSIRETNumber));
			AssertNoMessageError(caed.DeclarantsSIRETNumberInfo, message);
		}

		public void TestValidateCommonAccessRef_Mandatory()
		{
			var message = "Common Access Reference is required.";
			var caed = builder.Build();

			caed.CommonAccessRef = "";
			AssertHasMessageError(caed.CommonAccessRefInfo, message);

			caed.CommonAccessRef = "ABC";
			AssertNoMessageError(caed.CommonAccessRefInfo, message);
		}

		public void TestValidateDeclarationType()
		{
			var message = "Declaration type is required.";
			var caed = builder.Build();

			caed.DeclarationType = ZString.Empty;
			AssertHasMessageError(caed.DeclarationTypeInfo, message);

			caed.DeclarationType = "T1";
			AssertNoMessageError(caed.DeclarationTypeInfo, message);
		}

		public void TestValidateJobNumber()
		{
			var message = "Job number is required.";
			var caed = builder.Build();

			caed.JobNumber = ZString.Empty;
			AssertHasMessageError(caed.JobNumberInfo, message);

			caed.JobNumber = "NCT001";
			AssertNoMessageError(caed.JobNumberInfo, message);
		}

		public void TestValidateTotalNumberOfPacks()
		{
			var message = "At least one pack should be entered.";
			var caed = builder.Build();

			caed.TotalNumberOfPacks = 0;
			AssertHasMessageError(caed.TotalNumberOfPacksInfo, message);

			caed.TotalNumberOfPacks = 10;
			AssertNoMessageError(caed.TotalNumberOfPacksInfo, message);
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
			builder = new NctsCAEDBuilder(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsCAEDBuilder builder;
	}
}
