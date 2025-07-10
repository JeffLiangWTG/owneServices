using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.CusDec.Testing
{
	public class DeclarationChosererTester : TestCaseWithFactory
	{
		delegate void SetRegistryDelegate(string newValue);

		public void TestSelectionOfSenderUsingRegoOptions()
		{
			how = new CusdecMessageFunction.New();
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);

			RunChooserTest(mcpBadge, "MCP", delegate(string newValue)
			{ GBCustomsDataRegistry.Instance.McpDestin8Url = newValue; });
			RunChooserTest(cnsBadge, "CNS", delegate(string newValue)
			{ GBCustomsDataRegistry.Instance.CnsUploadUrl = newValue; });
			RunChooserTest(ccsukBadge, "Ccsuk", delegate(string newValue)
			{ GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue); });

			var nesDecDiffeentBecauseNoRegoOptionsNeeded = Factory.New<JobDeclaration>();
			nesDecDiffeentBecauseNoRegoOptionsNeeded.JE_CustomsProfile = nesBadge.BadgeCode;
			GbDeclarationSenderChooserForTest chooser = new GbDeclarationSenderChooserForTest();
			chooser.Send(nesDecDiffeentBecauseNoRegoOptionsNeeded, shutUp, how);
			AssertContains("Chooser should have pulled a sender of this approximate Type", "NES", chooser.SelectedSenderType.FullName);
		}

		void RunChooserTest(BadgeCodeSetting badgeCodeSetting, string expectedTypeOfSender, SetRegistryDelegate regoSetter)
		{
			var factoryToEnsureTheRegoValuesAreNotCached = new BusinessObjectFactory();
			var dec1 = factoryToEnsureTheRegoValuesAreNotCached.New<JobDeclaration>();

			dec1.JE_CustomsProfile = badgeCodeSetting.BadgeCode;
			regoSetter("https://WhatBadgersEat.com");
			GbDeclarationSenderChooserForTest chooser = new GbDeclarationSenderChooserForTest();
			chooser.Send(dec1, shutUp, how);
			AssertContains("Chooser should have pulled a sender of this approximate Type", expectedTypeOfSender, chooser.SelectedSenderType.FullName);

			var dec2 = factoryToEnsureTheRegoValuesAreNotCached.New<JobDeclaration>();
			dec2.JE_CustomsProfile = badgeCodeSetting.BadgeCode;
			regoSetter("");
			chooser = new GbDeclarationSenderChooserForTest();
			chooser.Send(dec2, shutUp, how);
			AssertEquals("If Url for CSP is not provided it is impossible to send", "The registry options are not sufficiently configured for this CSP. Cannot send.", shutUp.InvalidOperationText);

			var dec3 = factoryToEnsureTheRegoValuesAreNotCached.New<JobDeclaration>();
			dec3.JE_CustomsProfile = badgeCodeSetting.BadgeCode;
		}

		public void TestEndToEndWithPermitsBTH()
		{
			TestEndToEndWithPermitsBTHAndQTY(PermitQtyValIndicatorList.Codes.BTH);
		}

		public void TestEndToEndWithPermitsQTY()
		{
			TestEndToEndWithPermitsBTHAndQTY(PermitQtyValIndicatorList.Codes.QTY);
		}

		void TestEndToEndWithPermitsBTHAndQTY(ZString permitIndicator)
		{
			var declarationHelper = new DeclarationTestHelper(Factory);

			declarationHelper.CreateRefDataForPermits(Core.Constants.CountryCodes.UnitedKingdom);

			var mcpDec = CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declarationHelper.SetupPermits(mcpDec.Declarant.Header, permitIndicator);
			var invoice = mcpDec.Invoices[0];
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invLine = mcpDec.InvoiceLines[0];
			invLine.JI_LinePrice = 100;
			var suppDoc = invLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = "9001";
			suppDoc.CSI_ReferenceNumber = "12345";
			suppDoc.CSI_Value = 100.0;

			var suppDoc2 = invLine.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "9001";
			suppDoc2.CSI_ReferenceNumber = "12345";
			suppDoc2.CSI_Quantity = 75;
			suppDoc2.CSI_UnitOfQuantity = "KGM";
			mcpDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			SendsMessagesToCustomsShutterUpperer sendGui = new SendsMessagesToCustomsShutterUpperer(false);
			sendGui.AnswerToContinueWithAction = true;
			sendGui.ContinueWithActionMessage = null;
			sendGui.ContinueWithActionCaption = null;
			Factory.Save();
			GbDeclarationSenderChooser sender = new GbDeclarationSenderChooser();

			//MCP
			sender.Send(mcpDec, sendGui, new CusdecMessageFunction.New());
			var entry = mcpDec.CustomsEntryHeaders[0];

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals((ZDecimal)(-75.0), transactions[0].CPL_TranQty);

			suppDoc.CSI_Value = 50;
			suppDoc2.CSI_Quantity = 100;

			mcpDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entry = mcpDec.CustomsEntryHeaders[0];
			entry.CH_CustomsMessageRemarks = "Updating Permits";

			sender = new GbDeclarationSenderChooser();
			sender.Send(mcpDec, sendGui, new CusdecMessageFunction.Amended());

			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(2, transactions.Length);
			AssertEquals((ZDecimal)(-75.00), transactions[0].CPL_TranQty);
			AssertEquals((ZDecimal)(-25.0), transactions[1].CPL_TranQty);
		}

		public void TestEndToEndWithPermitsVAL()
		{
			var declarationHelper = new DeclarationTestHelper(Factory);

			declarationHelper.CreateRefDataForPermits(Core.Constants.CountryCodes.UnitedKingdom);

			var mcpDec = CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declarationHelper.SetupPermits(mcpDec.Declarant.Header, PermitQtyValIndicatorList.Codes.VAL);
			var invoice = mcpDec.Invoices[0];
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invLine = mcpDec.InvoiceLines[0];
			invLine.JI_LinePrice = 100;
			var suppDoc = invLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = "9001";
			suppDoc.CSI_ReferenceNumber = "12345";
			suppDoc.CSI_Value = 100.0;

			var suppDoc2 = invLine.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "9001";
			suppDoc2.CSI_ReferenceNumber = "12345";
			suppDoc2.CSI_Quantity = 75;
			suppDoc2.CSI_UnitOfQuantity = "KGM";
			mcpDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			SendsMessagesToCustomsShutterUpperer sendGui = new SendsMessagesToCustomsShutterUpperer(false);
			sendGui.AnswerToContinueWithAction = true;
			sendGui.ContinueWithActionMessage = null;
			sendGui.ContinueWithActionCaption = null;
			Factory.Save();
			GbDeclarationSenderChooser sender = new GbDeclarationSenderChooser();

			//MCP
			sender.Send(mcpDec, sendGui, new CusdecMessageFunction.New());
			var entry = mcpDec.CustomsEntryHeaders[0];

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals("CHIEF declarations should use CSI_Quatnity", (ZDecimal)(-75), transactions[0].CPL_TranValue);

			suppDoc.CSI_Value = 50;
			suppDoc2.CSI_Quantity = 100;

			mcpDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entry = mcpDec.CustomsEntryHeaders[0];
			entry.CH_CustomsMessageRemarks = "Updating Permits";

			sender = new GbDeclarationSenderChooser();
			sender.Send(mcpDec, sendGui, new CusdecMessageFunction.Amended());

			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(2, transactions.Length);
			AssertEquals("CHIEF declarations should use CSI_Quatnity", (ZDecimal)(-75.0), transactions[0].CPL_TranValue);
			AssertEquals("CHIEF declarations should use CSI_Quatnity", (ZDecimal)(-25.0), transactions[1].CPL_TranValue);
		}

		public void TestEndToEndWithPermitsWithError()
		{
			var declarationHelper = new DeclarationTestHelper(Factory);

			declarationHelper.CreateRefDataForPermits(Core.Constants.CountryCodes.UnitedKingdom);

			var mcpDec = CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declarationHelper.SetupPermits(mcpDec.Declarant.Header, PermitQtyValIndicatorList.Codes.VAL);
			var invoice = mcpDec.Invoices[0];
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invLine = mcpDec.InvoiceLines[0];
			invLine.JI_LinePrice = 11000;
			var suppDoc = invLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = "9001";
			suppDoc.CSI_ReferenceNumber = "12345";
			suppDoc.CSI_Quantity = 11000.0;
			suppDoc.CSI_UnitOfQuantity = "KGM";

			mcpDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			SendsMessagesToCustomsShutterUpperer sendGui = new SendsMessagesToCustomsShutterUpperer(false);
			sendGui.AnswerToContinueWithAction = true;
			sendGui.ContinueWithActionMessage = null;
			sendGui.ContinueWithActionCaption = null;

			Factory.Save();
			GbDeclarationSenderChooser sender = new GbDeclarationSenderChooser();

			//MCP
			sender.Send(mcpDec, sendGui, new CusdecMessageFunction.New());
			var entry = mcpDec.CustomsEntryHeaders[0];

			AssertEquals("Checking that the MCP sender actually created an outgoing message.  If not, it may be that there is additional validation that means the sender decided not to send.  Try setting/initialising more properties to CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission() in this test class.",
					1, entry.Messages.Count);
			var message = entry.Messages[0];
			AssertEquals("The MCP sender should make a message with the MCP app code", ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly, message.EM_ApplicationCode);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(0, transactions.Length);
			AssertContains("The declared customs value, 11000.00, for permit 12345 is greater than the permit value balance of 10000.00", sendGui.ContinueWithActionMessage);
		}

		public void TestEndToEndNewDeclarationSenderTester()
		{
			var mcpDec = CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			mcpDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ccsukDec = CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			ccsukDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			ccsukDec.JE_CustomsProfile = "ZPE";

			var cnsDec = CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			cnsDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			// Use the same method:
			var nesDec = CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			nesDec.JE_CustomsProfile = "DES";

			var gemsDec = CreateGemsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			gemsDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			SendsMessagesToCustomsShutterUpperer sendGui = new SendsMessagesToCustomsShutterUpperer(false);
			sendGui.AnswerToContinueWithAction = true;
			sendGui.ContinueWithActionMessage = null;
			sendGui.ContinueWithActionCaption = null;
			Factory.Save();
			GbDeclarationSenderChooser sender = new GbDeclarationSenderChooser();

			//MCP
			sender.Send(mcpDec, sendGui, new CusdecMessageFunction.New());
			var entry = mcpDec.CustomsEntryHeaders[0];
			AssertEquals("Checking that the MCP sender actually created an outgoing message.  If not, it may be that there is additional validation that means the sender decided not to send.  Try setting/initialising more properties to CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission() in this test class.",
					1, entry.Messages.Count);
			var message = entry.Messages[0];
			AssertEquals("The MCP sender should make a message with the MCP app code", ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly, message.EM_ApplicationCode);
			AssertContains("MCP sender should make a message with MCP's charset", @"#CUSDEC\D\04A\UN\109730#", message.EM_MessageText);

			// NES
			sender.Send(nesDec, sendGui, new CusdecMessageFunction.New());
			entry = nesDec.CustomsEntryHeaders[0];
			AssertEquals("Checking that the NES sender actually created an outgoing message.  If not, it may be that there is additional validation that means the sender decided not to send.  Try setting/initialising more properties to CreateNesDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission() in this test class.",
						1, entry.Messages.Count);
			message = entry.Messages[0];
			AssertEquals("The Nes sender should make a message with the Nes app code", NES.NesConstants.ApplicationCode, message.EM_ApplicationCode);

			// CNS
			sender.Send(cnsDec, sendGui, new CusdecMessageFunction.New());
			entry = cnsDec.CustomsEntryHeaders[0];
			AssertEquals("Checking that the CNS sender actually created an outgoing message.  If not, it may be that there is additional validation that means the sender decided not to send.  Try setting/initialising more properties to CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission() in this test class.",
					1, entry.Messages.Count);
			message = entry.Messages[0];
			AssertEquals("The CNS sender should make a message with the CNS app code", ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly, message.EM_ApplicationCode);
			AssertContains("CNS sender should make a message with UNA charset", @"+CUSDEC:D:04A:UN:109730", message.EM_MessageText);

			// CCSUK
			sender.Send(ccsukDec, sendGui, new CusdecMessageFunction.New());
			entry = ccsukDec.CustomsEntryHeaders[0];
			AssertEquals("Checking that the CUK sender actually created an outgoing message.  If not, it may be that there is additional validation that means the sender decided not to send.  Try setting/initialising more properties to CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission() in this test class.",
					1, entry.Messages.Count);
			message = entry.Messages[0];
			AssertEquals("The CCSUK sender should make a message with the CUK app code", ApplicationCodeList.Codes.GbCcsuk, message.EM_ApplicationCode);
			AssertContains("CUK sender should make a message with UNA charset", @"+CUSDEC:D:04A:UN:109730", message.EM_MessageText);

			// NO sender at all
			sender.Send(gemsDec, sendGui, new CusdecMessageFunction.New());
			entry = gemsDec.CustomsEntryHeaders[0];
			AssertEquals("No message was created", 0, entry.Messages.Count);
		}

		public void TestEndToEndThatFactorySaveIsOnlyCalledOnce()
		{
			var mcpDec = CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			mcpDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			SendsMessagesToCustomsShutterUpperer sendGui = new SendsMessagesToCustomsShutterUpperer(false);
			sendGui.AnswerToContinueWithAction = true;
			sendGui.ContinueWithActionMessage = null;
			sendGui.ContinueWithActionCaption = null;
			Factory.Save();
			GbDeclarationSenderChooser sender = new GbDeclarationSenderChooser();

			int saveCount = Factory.SaveCount;
			sender.Send(mcpDec, sendGui, new CusdecMessageFunction.New());

			AssertEquals("Factory.Save should only be called once when sending a message", 1, Factory.SaveCount - saveCount);
		}

		public static JobDeclaration CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(BusinessObjectFactory factory)
		{
			var mcpDec = factory.New<JobDeclaration>();
			var inv = mcpDec.Invoices.AddNew();
			inv.JZ_InvoiceNumber = "Love is in the air";
			var line = (Eu.JobComInvoiceLine)inv.InvoiceLines.AddNew();
			line.JI_Procedure = "4000000";
			line.JI_Description = "Something from China";
			mcpDec.JE_CustomsProfile = "AMY";
			mcpDec.JE_TotalNoOfPacks = 69;
			mcpDec.JE_MasterBill = "Master";
			mcpDec.JE_DeclarationType = "EFD";
			mcpDec.JE_EntrySubStyle = "D";
			var cw = mcpDec.Bills[0].PackingGroups[0].Packages.AddNew();
			cw.CW_PackQty = 6;
			var box31Pack = (InvoiceLinePackagePivot)mcpDec.InvoiceLines[0].PackagesPivot.AddPivotFor(cw);
			box31Pack.CHC_NumberOfPacks = 6;
			box31Pack.Package.CW_PackType = "CT";
			mcpDec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AddEoriIfNotExits(mcpDec.Declarant.Header, "EORI0000001");
			factory.Save();
			mcpDec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			return mcpDec;
		}

		public static JobDeclaration CreateCnsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(BusinessObjectFactory factory)
		{
			// Same as MCP but for badge code
			var sharedDec = CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(factory);
			sharedDec.JE_CustomsProfile = "AGI";
			sharedDec.JE_TotalNoOfPacks = 69;
			sharedDec.JE_DeclarationType = "EFD";
			sharedDec.JE_EntrySubStyle = "D";
			return sharedDec;
		}

		public static JobDeclaration CreateGemsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(BusinessObjectFactory fact)
		{
			JobDeclaration gemsDec = fact.New<JobDeclaration>();
			OrgHeader supplier = fact.New<OrgHeader>();
			supplier.OH_Code = "AGI";
			OrgHeader buyer = fact.New<OrgHeader>();
			buyer.OH_Code = "DAN";
			gemsDec.JE_OH_Supplier = supplier.PK;
			gemsDec.JE_OH_Importer = buyer.PK;
			var inv2 = gemsDec.Invoices.AddNew();
			inv2.JZ_InvoiceNumber = "Love is in the air";
			var line2 = inv2.InvoiceLines.AddNew();
			gemsDec.JE_CustomsProfile = "DAN";
			gemsDec.JE_TotalNoOfPacks = 69;
			line2.JI_Procedure = "4000000";

			gemsDec.JE_MasterBill = "X";
			gemsDec.Bills[0].PackingGroups[0].Packages.AddNew();
			var box31Pack = line2.PackagesForInvoiceLinesForBindingOnly[0];
			box31Pack.IsLinked = true;
			box31Pack.PackQty = 69;
			box31Pack.Package.CW_PackType = "CT";

			AddEoriIfNotExits(gemsDec.Declarant.Header, "EORI0000001");
			return gemsDec;
		}

		public static void AddEoriIfNotExits(OrgHeader org, string eoriCode)
		{
			if (!org.CustomsCodes.OfType<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori))
			{
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			badges = new BadgeCodeSettingCollection();
			mcpBadge = new BadgeCodeSetting();
			mcpBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.MCP_CUSDECOnly;
			mcpBadge.BadgeCode = "AMY";
			ccsukBadge = new BadgeCodeSetting();
			ccsukBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW;
			ccsukBadge.BadgeCode = "ZPE";
			cnsBadge = new BadgeCodeSetting();
			cnsBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CNS_CUSDECOnly;
			cnsBadge.BadgeCode = "AGI";
			gemsBadge = new BadgeCodeSetting();
			gemsBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW;
			gemsBadge.BadgeCode = "DAN";
			nesBadge = new BadgeCodeSetting();
			nesBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.NES;
			nesBadge.BadgeCode = "DES";
			badges.Add(mcpBadge);
			badges.Add(gemsBadge);
			badges.Add(cnsBadge);
			badges.Add(nesBadge);
			badges.Add(ccsukBadge);

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);

			CredentialsSetting mcpCred = new CredentialsSetting();
			mcpCred.BadgeCode = mcpBadge.BadgeCode;
			mcpCred.Username = "CAW8";
			mcpCred.Printer = "X";
			mcpCred.Company = "Y";
			mcpCred.Password = "foo";
			CredentialsSetting cnsCred = new CredentialsSetting();
			cnsCred.BadgeCode = cnsBadge.BadgeCode;
			cnsCred.Username = "AAW";
			cnsCred.Password = "xxx";
			cnsCred.Printer = "X";
			cnsCred.Company = "Y";
			var nesCred = new CredentialsSetting();
			nesCred.BadgeCode = nesBadge.BadgeCode;
			nesCred.Company = "THS1ZEG";
			nesCred.Printer = "LOCEDC1ZEG";
			var ccsukCred = new CredentialsSetting();
			ccsukCred.BadgeCode = ccsukBadge.BadgeCode;
			ccsukCred.Company = ccsukBadge.BadgeCode;
			ccsukCred.PIMA = "CUKFFW98000" + ccsukBadge.BadgeCode;
			CredentialsSettingCollection allCreds = new CredentialsSettingCollection();
			allCreds.Add(cnsCred);
			allCreds.Add(mcpCred);
			allCreds.Add(nesCred);
			allCreds.Add(ccsukCred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);

			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FOO");
			GB.Registry.GBCustomsDataRegistry.Instance.McpDestin8Url = "https://CatsLikePlainCrisps.com";
			GB.Registry.GBCustomsDataRegistry.Instance.CnsUploadUrl = "http://whatBadgersEat.com";

			ServiceManagerQuerierHelperForTesting faker1 = new ServiceManagerQuerierHelperForTesting("GBG");
			ServiceManagerQuerierHelperForTesting faker2 = new ServiceManagerQuerierHelperForTesting("GBM");
			ServiceManagerQuerierHelperForTesting faker3 = new ServiceManagerQuerierHelperForTesting("GBF");
			faker1.ActivateServiceForTesting();
			faker2.ActivateServiceForTesting();
			faker3.ActivateServiceForTesting();
		}

		class GbDeclarationSenderChooserForTest : GbDeclarationSenderChooser
		{
			protected override void DoFinalSend(BaseJobDeclaration declaration, ISendsMessagesToCustoms sendToCustoms, CusdecMessageFunction how, IDeclarationMessageSender sender)
			{
				SelectedSenderType = sender.GetType();
			}
			public Type SelectedSenderType { get; private set; }
		}

		BadgeCodeSettingCollection badges;
		SendsMessagesToCustomsShutterUpperer shutUp;
		CusdecMessageFunction how;
		BadgeCodeSetting mcpBadge;
		BadgeCodeSetting ccsukBadge;
		BadgeCodeSetting nesBadge;
		BadgeCodeSetting gemsBadge;
		BadgeCodeSetting cnsBadge;
	}
}
