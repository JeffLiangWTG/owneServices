using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	public class EntryCAEDBuilderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new EntryCAEDBuilder(null));
			AssertNoExceptionThrown(() => _ = new EntryCAEDBuilder(Factory.New<CusEntryHeader>()));
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
				declaration.JE_RL_NKPortOfArrival = "FRPAR";
				var caed = builder.Build();
				AssertEquals("PortSystem is inferred from registry entry matching JE_RL_NKPortOfArrival.", "MGI", caed.PortSystem);
				AssertEquals("SICCodeType should be CI5 when PortSystem is MGI.", "CI5", caed.SICCodeType);

				declaration.JE_RL_NKPortOfArrival = "FRABC";
				caed = builder.Build();
				AssertEquals("PortSystem is inferred from registry entry matching JE_RL_NKPortOfArrival.", "SOGET", caed.PortSystem);
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
				declaration.JE_RL_NKPortOfArrival = "FRPAR";
				var caed = builder.Build();
				AssertEquals("Registry + JE_RL_NKPortOfArrival => SICCodeType is CI5, so SendingPartyID should match branch Proxy Customs code of type CI5.", "CI5_001", caed.SendingPartyID);
				AssertEquals("PortOfDispatch is empty", "", caed.SendingPartySICCode);

				declaration.JE_RL_NKPortOfArrival = "FRABC";
				caed = builder.Build();
				AssertEquals("Registry + JE_RL_NKPortOfArrival => SICCodeType is SOA, so SendingPartyID should match branch Proxy Customs code of type SOA.", "SOA_001", caed.SendingPartyID);
				AssertEquals("PortOfDispatch is empty", "", caed.SendingPartySICCode);

				declaration.JE_RL_NKPortOfLoading = "FRPAR";
				caed = builder.Build();
				AssertEquals("The forwarder of FRPAR in registry is FORWARDER", "FORWARDER", caed.SendingPartySICCode);

				declaration.JE_RL_NKPortOfLoading = "FRABC";
				caed = builder.Build();
				AssertEquals("The forwarder of FRABC in registry is FORWARDER2", "FORWARDER2", caed.SendingPartySICCode);
			}
		}

		public void TestBuildRecipient()
		{
			var helper = new UniversalReferenceTestDataHelper(entryHeader.Factory);
			helper.CreateCusMapType("FRCCS", "OUT", "FRCCS", true);
			helper.CreateCusMap("FRCCS", "FRBOD", "CI5BOD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "FR");
			helper.CreateCusMap("FRCCS", "FRDKK", "CI5DKE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "FR");
			Factory.Save();

			declaration.JE_RL_NKPortOfLoading = "FRBOD";
			var caed = builder.Build();
			AssertEquals("CI5BOD", caed.RecipientID);
			AssertEquals("CI5BOD", caed.RecipientSICCode);

			declaration.JE_RL_NKPortOfLoading = "FRDKK";
			caed = builder.Build();
			AssertEquals("CI5DKE", caed.RecipientID);
			AssertEquals("CI5DKE", caed.RecipientSICCode);

			declaration.JE_RL_NKPortOfLoading = "FRFOS";
			caed = builder.Build();
			AssertEquals("", caed.RecipientID);
			AssertEquals("", caed.RecipientSICCode);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = "FRBOD";
			caed = builder.Build();
			AssertEquals("CI5BOD", caed.RecipientID);
			AssertEquals("CI5BOD", caed.RecipientSICCode);

			declaration.JE_RL_NKPortOfArrival = "FRDKK";
			caed = builder.Build();
			AssertEquals("CI5DKE", caed.RecipientID);
			AssertEquals("CI5DKE", caed.RecipientSICCode);

			declaration.JE_RL_NKPortOfArrival = "FRFOS";
			caed = builder.Build();
			AssertEquals("", caed.RecipientID);
			AssertEquals("", caed.RecipientSICCode);
		}

		public void TestBuildReferenceNumbers()
		{
			entryHeader.CH_BGMReference = "entryRef";
			var caed = builder.Build();
			AssertEquals("entryRef", caed.JobNumber);
		}

		public void TestDeclarantsSIRETNumber()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SIRET", Core.Constants.CountryCodes.France);
			var caed = builder.Build();
			AssertEquals("", caed.DeclarantsSIRETNumber);

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			caed = builder.Build();
			AssertEquals("FRSIRET", caed.DeclarantsSIRETNumber);
		}

		public void TestBuildContainer()
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "container1";
			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entryHeader.PK;
			pivot.CCE_CO_Container = container1.PK;

			var build = builder.Build();
			AssertEquals("CONTAINER1", build.ContainerNumbers);

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONTAINER2";

			var pivot2 = Factory.New<CusContainerEntryHeaderPivot>();
			pivot2.CCE_CH_EntryHeader = entryHeader.PK;
			pivot2.CCE_CO_Container = container2.PK;

			build = builder.Build();
			AssertEquals("CONTAINER1 / CONTAINER2", build.ContainerNumbers);
		}

		public void TestBuildTypeAndStatus()
		{
			var caed = builder.Build();
			AssertEquals("FR", caed.DeclarationType);
			AssertEquals("", caed.DeclarationStatus);

			declaration.JE_EntryStyle = "I";
			caed = builder.Build();
			AssertEquals("I", caed.DeclarationType);

			entryInstruction.CEI_SubStyle = "MA";
			caed = builder.Build();
			AssertEquals("IMA", caed.DeclarationType);

			declaration.JE_EntryStyle = "";
			caed = builder.Build();
			AssertEquals("MA", caed.DeclarationType);

			entryHeader.CH_EntryStatus = "OK";
			caed = builder.Build();
			AssertEquals("OK", caed.DeclarationStatus);
		}

		public void TestBuildCustomsOffices()
		{
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var caed = builder.Build();
			AssertEquals("", caed.CustomsOfficeCodeOfDeparture.Code);

			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "ENT001");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "EXT001");

			caed = builder.Build();
			AssertEquals("ENT001", caed.CustomsOfficeCodeOfDeparture.Code);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			caed = builder.Build();
			AssertEquals("EXT001", caed.CustomsOfficeCodeOfDeparture.Code);
		}

		public void TestBuildPackages()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "PK";

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 3;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 7;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			AssertEquals(10m, entryHeader.CustomsPackageCount);

			var caed = builder.Build();
			AssertEquals(10, caed.TotalNumberOfPacks);
			AssertEquals("PK", caed.PackageType.Code);
		}

		public void TestBuildPortDues()
		{
			var caed = builder.Build();
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, caed.PortDuesCurrency.Code);
		}

		public void TestCommonAccessRef()
		{
			var caed = builder.Build();
			AssertEquals("", caed.CommonAccessRef);

			var previousDocument = declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.ZZZ;
			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			previousDocument.CSI_ReferenceNumber = "BBB";
			caed = builder.Build();
			AssertEquals("BBB", caed.CommonAccessRef);

			var additionalReferenceNumber = declaration.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.CommonAccessReference;
			additionalReferenceNumber.CE_EntryNum = "AAA";
			caed = builder.Build();
			AssertEquals("AAA", caed.CommonAccessRef);
		}

		public void TestPort()
		{
			var caed = builder.Build();
			AssertEquals("", caed.Port);

			declaration.JE_RL_NKPortOfArrival = "port";

			caed = builder.Build();
			AssertEquals("PORT", caed.Port);
		}

		public void TestPortDuesAmount()
		{
			var caed = builder.Build();
			AssertEquals(0m, caed.PortDuesAmount);

			var charge1 = entryHeader.Charges.AddNew();
			var charge2 = entryHeader.Charges.AddNew();

			charge1.C1_ChargeAmount = 1;
			charge2.C1_ChargeAmount = 2;

			caed = builder.Build();
			AssertEquals(3m, caed.PortDuesAmount);
		}

		public void TestCTOPartySICCode()
		{
			declaration.JE_SubLocationOfGoods = "location";
			var caed = builder.Build();
			AssertEquals("location", caed.CTOPartySICCode);
			AssertEquals("location", caed.CTOPartyID);
		}

		public void TestValidateContainer()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();

			var builder = new EntryCAEDBuilder(entryHeader);
			var build = builder.Build();
			AssertHasMessageError(build.ContainerNumbersInfo, "At least one container is required.");

			var container1 = dec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "container1";
			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entryHeader.PK;
			pivot.CCE_CO_Container = container1.PK;

			build = builder.Build();
			AssertEquals("CONTAINER1", build.ContainerNumbers);
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

				declaration.JE_RL_NKOrigin = "FRPAR";
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

		public void TestValidateCTOPartyID()
		{
			var message = "CTO address's ID is required.";
			var caed = builder.Build();

			caed.CTOPartyID = "";
			caed.Validate(nameof(caed.CTOPartyID));
			AssertHasMessageError(caed.CTOPartyIDInfo, message);

			caed.CTOPartyID = "EASYLOG";
			caed.Validate(nameof(caed.CTOPartyID));
			AssertNoMessageError(caed.CTOPartyIDInfo, message);
		}

		public void TestValidateCTOPartySICCode()
		{
			var message = "CTO address's SIC code is required.";
			var caed = builder.Build();

			caed.CTOPartySICCode = "";
			caed.Validate(nameof(caed.CTOPartySICCode));
			AssertHasMessageError(caed.CTOPartySICCodeInfo, message);

			caed.CTOPartySICCode = "SON_001";
			caed.Validate(nameof(caed.CTOPartySICCode));
			AssertNoMessageError(caed.CTOPartySICCodeInfo, message);
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

			caed.JobNumber = "entry001";
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
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var declarationsuppDoc = declaration.SupportingDocuments.AddNew();
			declarationsuppDoc.CSI_Description = "dec";
			declarationsuppDoc.CSI_Code = "ZZZ";
			declarationsuppDoc.CSI_SubType = "Z";
			declarationsuppDoc.CSI_ReferenceNumber = "ICT12313312";
			declarationsuppDoc.CSI_IsDTP = false;

			var suppDoc = invoice.SupportingDocuments.AddNew();
			suppDoc.CSI_Description = "inv";
			suppDoc.CSI_Code = "ZZZ";
			suppDoc.CSI_Code = "Z";
			suppDoc.CSI_ReferenceNumber = "ICT12313312";
			suppDoc.CSI_IsDTP = false;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryline = entryHeader.MergedLines.AddNew();
			entryline.CL_LineNumber = 1;
			entryline.InvoiceLines.Add(invoiceLine);
			builder = new EntryCAEDBuilder(entryHeader);
		}

		CusEntryHeader entryHeader;
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		EntryCAEDBuilder builder;
	}
}
