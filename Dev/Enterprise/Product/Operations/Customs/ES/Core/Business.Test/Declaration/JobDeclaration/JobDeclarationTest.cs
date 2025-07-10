using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
{
	public void TestSupportValidateCustomsMessaging()
	{
		var supporter = (IValidateForCustomsMessagingSupporter)declaration;
		AssertEquals(true, supporter.SupportValidateCustomsMessaging);
	}

	public void TestZG_AgreedPlaceCodeValidationSupport()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = "EXP";
				declaration.JE_ShipmentIncoTerm = "AH";
				AssertEquals("UCC6 EXP and IncoTerm not empty", true, declaration.ZG_AgreedPlaceCodeValidationSupport);

				declaration.JE_ShipmentIncoTerm = ZString.Empty;
				AssertEquals("UCC6 EXP and IncoTerm empty", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				declaration.JE_ShipmentIncoTerm = "APC";
				AssertEquals("UCC6 EXP and IncoTerm not empty", true, declaration.ZG_AgreedPlaceCodeValidationSupport);

				declaration.JE_ShipmentIncoTerm = "XXX";
				AssertEquals("UCC6 EXP and IncoTerm XXX", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				declaration.JE_ShipmentIncoTerm = "AH";
				AssertEquals("UCC6 EXP and IncoTerm not empty", true, declaration.ZG_AgreedPlaceCodeValidationSupport);

				var instruction1 = declaration.CustomsEntryInstructions.AddNew();
				instruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2C;
				AssertEquals("not diff instruction than T2C T2L EXS", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				instruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2L;
				AssertEquals("not diff instruction than T2C T2L EXS", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				instruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				AssertEquals("not diff instruction than T2C T2L EXS", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				instruction1.CEI_SubStyle = "A";
				AssertEquals("diff instruction than T2C T2L EXS", true, declaration.ZG_AgreedPlaceCodeValidationSupport);
			}

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("not UCC6 EXP", true, declaration.ZG_AgreedPlaceCodeValidationSupport);

				declaration.JE_MessageType = "IMP";
				AssertEquals("not UCC6 not EXP", true, declaration.ZG_AgreedPlaceCodeValidationSupport);

				var instruction1 = declaration.CustomsEntryInstructions.AddNew();
				instruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2C;
				AssertEquals("not diff instruction than T2C T2L EXS", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				instruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2L;
				AssertEquals("not diff instruction than T2C T2L EXS", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				instruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				AssertEquals("not diff instruction than T2C T2L EXS", false, declaration.ZG_AgreedPlaceCodeValidationSupport);

				instruction1.CEI_SubStyle = "A";
				AssertEquals("diff instruction than T2C T2L EXS", true, declaration.ZG_AgreedPlaceCodeValidationSupport);
			}
		});
	}

	public void TestHasInvoiceWithEmptyIncoTermPlace()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invHeader = declaration.Invoices.AddNew();
		var invHeader1 = declaration.Invoices.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("All incoTerm Place is empty", true, declaration.HasInvoiceWithEmptyIncoTermPlace);
			invHeader1.JZ_IncoTermPlace = "Barcelona";
			invHeader.JZ_IncoTermPlace = "Madrid";
			AssertEquals("All incoTerm place is not empty", false, declaration.HasInvoiceWithEmptyIncoTermPlace);
			invHeader.JZ_IncoTermPlace = ZString.Empty;
			AssertEquals("Some incoTerm place is empty", true, declaration.HasInvoiceWithEmptyIncoTermPlace);
		});
	}

	public void TestHasAnyCPCStartWithList()
	{
		var declaration = Factory.New<JobDeclaration>();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_FormattedProcedure = "7100";
		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_FormattedProcedure = "7700";
		var invoiceLine3 = invoice1.InvoiceLines.AddNew();
		invoiceLine3.JI_FormattedProcedure = "8100";
		var invoiceLine4 = invoice1.InvoiceLines.AddNew();
		invoiceLine4.JI_FormattedProcedure = "8700";

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine5 = invoice2.InvoiceLines.AddNew();
		invoiceLine5.JI_FormattedProcedure = "9000";

		CombineAssertions(() =>
		{
			AssertEquals("When no exist any in the list", false, declaration.HasAnyCPCStartWithList(new List<ZString> { "61", "62", "63" }));
			AssertEquals("When only one exist in the list", true, declaration.HasAnyCPCStartWithList(new List<ZString> { "61", "77", "63" }));
			AssertEquals("When no exist any in the list with letters", false, declaration.HasAnyCPCStartWithList(new List<ZString> { "AA", "62", "63" }));
			AssertEquals("When only one exist in the list with letters in second invoice", true, declaration.HasAnyCPCStartWithList(new List<ZString> { "AA", "62", "90" }));
		});
	}

	public void TestHasAnyCPCNotStartWithList()
	{
		var declaration = Factory.New<JobDeclaration>();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_FormattedProcedure = "7100";
		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_FormattedProcedure = "7700";

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine5 = invoice2.InvoiceLines.AddNew();
		invoiceLine5.JI_FormattedProcedure = "9000";

		CombineAssertions(() =>
		{
			AssertEquals("When one CPC does not start with a code in the list", true, declaration.HasAnyCPCNotStartWithList(new List<ZString> { "71", "77", "63" }));
			AssertEquals("When all CPC start with a code in the list", false, declaration.HasAnyCPCNotStartWithList(new List<ZString> { "71", "77", "90", "81" }));
			AssertEquals("When no CPC starts with a code in the list", true, declaration.HasAnyCPCNotStartWithList(new List<ZString> { "AA", "62", "63" }));
		});
	}

	public void TestDeclarationNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("DeclarationNumber is empty", ZString.Empty, declaration.DeclarationNumber);

			entryHeader1.MovementReferenceNumber = "21AH30999912345678";

			AssertEquals("DeclarationNumber is equal to the unique MRN in the Declaration", "21AH30999912345678", declaration.DeclarationNumber);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.MovementReferenceNumber = "21AH30999923TEST2";
			AssertEquals("DeclarationNumber is equal to Multiple due to more than 1 EntryHeader", "Multiple", declaration.DeclarationNumber);

			entryHeader1.MovementReferenceNumber = ZString.Empty;
			AssertEquals("DeclarationNumber is equal to the unique MRN in the Declaration", "21AH30999923TEST2", declaration.DeclarationNumber);
		});
	}

	public void TestIsCustomOfficeCanaryIsland()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_CustomsOffice = ZString.Empty;

		CombineAssertions(() =>
		{
			AssertEquals("JE_CustomsOffice is empty", false, declaration.IsCustomOfficeCanaryIsland);
			declaration.JE_CustomsOffice = "ES003500";
			AssertEquals("JE_CustomsOffice ES003500", true, declaration.IsCustomOfficeCanaryIsland);
			declaration.JE_CustomsOffice = "ES003600";
			AssertEquals("JE_CustomsOffice ES003600", false, declaration.IsCustomOfficeCanaryIsland);
			declaration.JE_CustomsOffice = "ES003800";
			AssertEquals("JE_CustomsOffice ES003800", true, declaration.IsCustomOfficeCanaryIsland);
			declaration.JE_CustomsOffice = "ES00";
			AssertEquals("JE_CustomsOffice ES00", false, declaration.IsCustomOfficeCanaryIsland);
			declaration.JE_CustomsOffice = "ES009998";
			AssertEquals("JE_CustomsOffice ES009998", true, declaration.IsCustomOfficeCanaryIsland);
		});
	}

	public void TestLatestEntryAcceptanceDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			AssertEquals(ZDateTime.Empty, declaration.LatestEntryAcceptanceDate);

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.MovementReferenceNumberIssueDate = ZDateTime.BrettsBirthday.AddDays(-1);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.MovementReferenceNumberIssueDate = ZDateTime.BrettsBirthday;

			AssertEquals(ZDateTime.BrettsBirthday, declaration.LatestEntryAcceptanceDate);
		});
	}

	public override void TestDefaultValuesForExport()
	{
		GlbDepartment.CurrentDepartment.GE_Import = false;
		GlbDepartment.CurrentDepartment.GE_Export = true;

		JobDeclaration jobDeclaration = base.Factory.New<JobDeclaration>();

		AssertEquals(ZString.Empty, jobDeclaration.JE_DeclarantType);
		AssertEquals(ZString.Empty, jobDeclaration.ZG_CTStatusID);
	}

	public override void TestCustomsOfficeOfEntry()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.JE_CustomsOffice = "LV001000";
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
		AssertEquals("LV002000", declaration.OfficeOfEntry);
	}

	public override void TestMergeManagerType()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			AssertEquals("Should be a Customs.ES.Business.MergeManager", ExpectedMergeManagerType, declaration.MergeManager.GetType());
		}
	}

	public override void TestMaxSupportingDocuments()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertEquals("EnableMaxCountValidationWithMessageError is disabled", 99, dec.MaxSupportingDocuments);
	}

	public void TestEnableMaxCountValidationWithMessageError()
	{
		var dec = Factory.New<JobDeclaration>();
		for (int i = 0; i < 99; i++)
		{
			var doc = dec.SupportingDocuments.AddNew();
			doc.CSI_Code = $"doc{i}";
		}
		CombineAssertions(() =>
		{
			AssertEquals(99, dec.SupportingDocuments.Count);
			AssertNoRowMessageErrorContaining(dec.SupportingDocuments[98], "Customs will not accept a declaration with more than 99 documents per line");

			var newDoc = dec.SupportingDocuments.AddNew();
			newDoc.CSI_Code = "mydoc";
			AssertEquals(100, dec.SupportingDocuments.Count);
			AssertHasRowMessageErrorContaining(dec.SupportingDocuments[99], "Customs will not accept a declaration with more than 99 documents per line");
		});
	}

	public void TestIsGoodsDestinationEsOrXcOrXlOrEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Spain;
		CombineAssertions(() =>
		{
			AssertEquals("JE_GoodsDestination is ES, Expected true", true, declaration.IsGoodsDestinationESOrXCOrXLOrEmpty);
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			AssertEquals("JE_GoodsDestination is FR, Expected false", false, declaration.IsGoodsDestinationESOrXCOrXLOrEmpty);
			declaration.JE_GoodsDestination = Core.Constants.NonStandardCountryCodes.Codes.XC;
			AssertEquals("JE_GoodsDestination is XC, Expected true", true, declaration.IsGoodsDestinationESOrXCOrXLOrEmpty);
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Germany;
			AssertEquals("JE_GoodsDestination is DE, Expected false", false, declaration.IsGoodsDestinationESOrXCOrXLOrEmpty);
			declaration.JE_GoodsDestination = Core.Constants.NonStandardCountryCodes.Codes.XL;
			AssertEquals("JE_GoodsDestination is XL, Expected true", true, declaration.IsGoodsDestinationESOrXCOrXLOrEmpty);
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Italy;
			AssertEquals("JE_GoodsDestination is IT, Expected false", false, declaration.IsGoodsDestinationESOrXCOrXLOrEmpty);
			declaration.JE_GoodsDestination = ZString.Empty;
			AssertEquals("JE_GoodsDestination is Empty, Expected true", true, declaration.IsGoodsDestinationESOrXCOrXLOrEmpty);
		});
	}

	public void TestEUD_RegionOrTerritoryOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(declaration.EUD_RegionOrTerritoryOfDestinationInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Region of Destination", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Reg. Destination", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Reg. Dest.", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "State or Region of Destination", captionResourceString.FullDescription);
		});
	}

	public void TestZG_DestinationStateList()
	{
		AssertEquals("Lookups.DestinationStateIslandCodeList", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ListAttribute>(declaration.JE_DestinationStateInfo).ListDataSourceMember);
		AssertEquals("AddInfoLookups.DestinationStateIslandCodeList", Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<ListAttribute>(declaration.ZG_DestinationStateInfo).ListDataSourceMember);
	}

	public void TestDestinationStateIsCanaryIsland() => CombineAssertions(() =>
	{
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var canaryIslandCode = "61";
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, canaryIslandCode, "Test 61");

			declaration.ZG_DestinationState = canaryIslandCode;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Destination State is canary island for declaration, ZG_DestinationState is canary island, Import and not UCC6", true, declaration.DestinationStateIsCanaryIsland);

			declaration.ZG_DestinationState = "12";
			AssertEquals("Destination State is not canary island for declaration, ZG_DestinationState is not canary island, Import and not UCC6", false, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES009998";
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is canary island, Import and not UCC6", false, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_DestinationState = canaryIslandCode;
			AssertEquals("Destination State is not canary island for declaration, ZG_DestinationState is canary island, Export and not UCC6", false, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003861";
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is canary island, Export and not UCC6", false, declaration.DestinationStateIsCanaryIsland);
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is canary island, Export and UCC6", false, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Destination State is canary island for declaration, JE_CustomsOffice is canary island, Import and UCC6", true, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is not canary island, Import and UCC6", false, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003541";
			AssertEquals("Destination State is canary island for declaration, JE_CustomsOffice is canary island, Import and UCC6", true, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003712";
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is not canary island, Import and UCC6", false, declaration.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES009998";
			AssertEquals("Destination State is canary island for declaration, JE_CustomsOffice is canary island, Import and UCC6", true, declaration.DestinationStateIsCanaryIsland);
		}
	});

	public void TestCusEntryHeader()
	{
		AssertType(typeof(EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>), declaration.CustomsEntryHeaders);
		var entry = declaration.CustomsEntryHeaders.AddNew();
		AssertType(typeof(CusEntryHeader), entry);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Spain, declaration.LocalCurrencyCode);
	}

	public override void TestAreMultipleEntryInstructionsAllowed()
	{
		Assert(declaration.AreMultipleEntryInstructionsAllowed);
	}

	public void TestZG_PartialWriteoffIsSetToFalseWhenReadOnly()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_CustomsOffice = "ES005511";
		declaration.ZG_PartialWriteoff = true;
		AssertEquals(true, declaration.ZG_PartialWriteoff);
		declaration.JE_CustomsOffice = "ES005611";
		AssertEquals(true, declaration.ZG_PartialWriteoff);
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_CustomsOffice = "ES002800";
		AssertEquals(true, declaration.ZG_PartialWriteoffInfo.ReadOnly);
		AssertEquals(false, declaration.ZG_PartialWriteoff);
	}

	public void TestZG_PartialWriteoffReadOnly()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_CustomsOffice = "ES005511";
		AssertEquals(false, declaration.ZG_PartialWriteoffInfo.ReadOnly);
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_CustomsOffice = "ES002800";
		AssertEquals(true, declaration.ZG_PartialWriteoffInfo.ReadOnly);
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_CustomsOffice = "ES002800";
		AssertEquals(true, declaration.ZG_PartialWriteoffInfo.ReadOnly);
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_CustomsOffice = "ES005511";
		AssertEquals(true, declaration.ZG_PartialWriteoffInfo.ReadOnly);
	}
	public void TestJE_MessageType_NeedToGetNewIncoTermAndChargeFactory()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();

		AssertNotNull(declaration.IncoTermAndChargeFactory);
		AssertNotNull(invoice.IncoTermAndChargeFactory);
		Assert(!declaration.NeedToGetNewIncoTermAndChargeFactory);
		Assert(!invoice.NeedToGetNewIncoTermAndChargeFactory);

		declaration.JE_MessageType = "EXP";
		Assert(declaration.NeedToGetNewIncoTermAndChargeFactory);
		Assert(invoice.NeedToGetNewIncoTermAndChargeFactory);
	}

	public void TestDefaultEmail()
	{
		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = "XAX";
		staffCurrentUser.GS_LoginName = "Current User";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Test no Email by Default when current user has no email", ZString.Empty, declaration.ZG_OtherEmailAddr);

				staffCurrentUser.GS_EmailAddress = ZString.Empty;
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Test no Email by Default when email of current user is empty", ZString.Empty, declaration2.ZG_OtherEmailAddr);

				staffCurrentUser.GS_EmailAddress = "main@test.com";
				var emailAddress1 = staffCurrentUser.EmailAddresses.FindByEmailAddressType(Core.Constants.EmailFromAddressTypes.Codes.Main);
				emailAddress1.GSE_Type = "AAA";
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Test no Email by Default when email of current user is not main email", ZString.Empty, declaration3.ZG_OtherEmailAddr);

				emailAddress1.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
				var declaration4 = Factory.New<JobDeclaration>();
				declaration4.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Test Email by Default when an Import is created", staffCurrentUser.GS_EmailAddress, declaration4.ZG_OtherEmailAddr);

				var declaration5 = Factory.New<JobDeclaration>();
				declaration5.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Test Email by Default when an Export is created", staffCurrentUser.GS_EmailAddress, declaration5.ZG_OtherEmailAddr);
			});
		}
	}

	public void TestDefaultZG_IsTrainingDeclaration()
	{
		var registrationMock = new Mock<IProductRegistration>();
		registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
		using (ObjectFactory.Substitute(registrationMock.Object))
		{
			var declaration2 = Factory.New<JobDeclaration>();
			AssertEquals("ZG_IsTrainingDeclaration is defaulted to false when creating a new declaration in a production env", false, declaration2.ZG_IsTrainingDeclaration);
		}

		registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Training);
		using (ObjectFactory.Substitute(registrationMock.Object))
		{
			var declaration2 = Factory.New<JobDeclaration>();
			AssertEquals("ZG_IsTrainingDeclaration is defaulted to true when creating a new declaration in a non production env", true, declaration.ZG_IsTrainingDeclaration);
		}
	}

	public void TestDeclEmailAddrValue()
	{
		const string clearanceEmail = "mail1.mail@mail.com";
		const string mailboxEmail = "mail2.mail@mail.com";
		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, declaration.DeclEmailAddr);
			}

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, declaration.DeclEmailAddr);
			}

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, declaration.DeclEmailAddr);
			}
		});
	}

	public void TestSupportingDocuments()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<SupportingDocumentCollection>(dec.SupportingDocuments);
	}

	public void TestPreviousDocuments()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<PreviousDocumentCollection>(dec.PreviousDocuments);
	}

	public void TestAdditionalInfos()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<AdditionalInfoCollection>(dec.AdditionalInfos);
	}

	public void TestJobDeclarationDocumentSupporter()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<JobDeclarationDocumentSupporter>(dec.DocumentSupporter);
	}

	public void TestCusEntryheaderDocumentSupporter()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<CusEntryHeader>(entryHeader);
		AssertType<CusEntryHeaderDocumentSupporter>(entryHeader.DocumentSupporter);
	}

	public void TestCustomsEntryInstructions()
	{
		AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(declaration.CustomsEntryInstructions);
	}

	public override void TestGetCustomsEntryInstructionProviderCore()
	{
		AssertType<EntryInstructionProvider>(declaration.CustomsEntryInstructionProvider);
	}

	public void TestCustomsOffices()
	{
		AssertType<OfficeCodeCollection>(declaration.CustomsOffices);
	}

	public override void TestGetCusCodeDataType()
	{
		AssertEquals(typeof(OfficeCode), ((ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.OfficeCode]);
	}

	public override void TestCustomsOfficeRequirementHelper()
	{
		AssertType<JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
	}

	public override void TestGetNewCusEquipmentCollection()
	{
		AssertType<CusEquipmentCollection<CusEquipment>>(declaration.Equipments);
	}

	public void TestFilteredInvoiceLines()
	{
		AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
	}

	public void TestGuarantees()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<ESGuaranteeCollection>(dec.Guarantees);
		var guarantee = dec.Guarantees.AddNew();
		AssertType<ESGuarantee>(dec.Guarantees[0]);
	}

	public void TestSupplierTraderId()
	{
		var declaration = Factory.New<JobDeclaration>();

		var supplier = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty", ZString.Empty, declaration.SupplierTraderId);

			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS with country code when no NIF or EORI declared", "GB333333333", declaration.SupplierTraderId);

			supplier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			supplier.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF", "NIF22222222", declaration.SupplierTraderId);

			supplier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			supplier.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", declaration.SupplierTraderId);

			OrgCusCode eoriCusCode = supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI with country code", "FR22222222", declaration.SupplierTraderId);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI with country code not repeated", "ES22222222", declaration.SupplierTraderId);

			supplier.CustomsCodes.RemoveAndDeleteAll();
			var customCode = supplier.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "XI123456789A";
			customCode.OK_RN_NKCodeCountry = "GB";
			customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Expected GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI, when there are multiple EORIs", "XI123456789A", declaration.SupplierTraderId);

			var customsCode2 = supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			AssertEquals("Expected ES EORI when there are multiple (one EU and one GB), even when the ES one was added after, when there are multiple EORIs", "ESA12345678", declaration.SupplierTraderId);

			customsCode2.OK_RN_NKCodeCountry = "AU";
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
			AssertEquals("Expected GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after, when there are multiple EORIs", "XI123456789A", declaration.SupplierTraderId);

			customCode.OK_CustomsRegNo = "123456789A";
			AssertEquals("Expected the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI, when there are multiple EORIs", "AUA12345678", declaration.SupplierTraderId);
		});
	}

	public void TestImporterTraderId()
	{
		var declaration = Factory.New<JobDeclaration>();

		var importer = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty", ZString.Empty, declaration.ImporterTraderId);

			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS with country code when no NIF or EORI declared", "GB333333333", declaration.ImporterTraderId);

			importer.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF", "NIF22222222", declaration.ImporterTraderId);

			importer.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			importer.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", declaration.ImporterTraderId);

			OrgCusCode eoriCusCode = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI with country code", "FR22222222", declaration.ImporterTraderId);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI with country code not repeated", "ES22222222", declaration.ImporterTraderId);

			importer.CustomsCodes.RemoveAndDeleteAll();
			var customCode = importer.CustomsCodes.AddNew();

			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_CustomsRegNo = "XI123456789A";
			customCode.OK_RN_NKCodeCountry = "GB";
			customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Expected GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI, when there are multiple EORIs", "XI123456789A", declaration.ImporterTraderId);

			var customsCode2 = importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			AssertEquals("Expected ES EORI when there are multiple (one EU and one GB), even when the ES one was added after, when there are multiple EORIs", "ESA12345678", declaration.ImporterTraderId);

			customsCode2.OK_RN_NKCodeCountry = "AU";
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
			AssertEquals("Expected GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after, when there are multiple EORIs", "XI123456789A", declaration.ImporterTraderId);

			customCode.OK_CustomsRegNo = "123456789A";
			AssertEquals("Expected the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI, when there are multiple EORIs", "AUA12345678", declaration.ImporterTraderId);
		});
	}

	public void TestGetNewValidation()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationValidation>(declaration.Validation);
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>(declaration.Validation);
			declaration.JE_MessageType = "";
			AssertType<JobDeclarationValidation>(declaration.Validation);
			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationValidation>(declaration.Validation);
		});
	}

	public void TestJE_MessageTypeChanged_NewValueExport()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_ZZF_NKTaxType = "IV1";
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_ZZF_NKTaxType = "IV2";
		CombineAssertions(() =>
		{
			AssertEquals("All Lines have Tax Type", false, invoice.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_ZZF_NKTaxType.IsEmpty));
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("All Lines empty Tax Type", true, invoice.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.JI_ZZF_NKTaxType.IsEmpty));
		});
	}

	public void TestGenerateExitControl_AcceptedHeader_ExistingExitHeaderWithoutExitDetail()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";
		var expectedNotificationDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
		var expectedNotificationPlace = "Place";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedDeclarantOrgHeader = Factory.New<OrgHeader>();
		expectedDeclarantOrgHeader.OH_Code = "Declarant";
		var expectedDeclarantAddress = expectedDeclarantOrgHeader.MainAddress;
		expectedDeclarantAddress.OA_Address1 = "Declarant Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitDetail);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OA_DeclarantAddress = expectedDeclarantAddress.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = expectedHeaderReference;
			exitHeader.CEH_ArrivalNotificationDate = expectedNotificationDate;
			exitHeader.CEH_ArrivalNotificationPlace = expectedNotificationPlace;

			Factory.Save();

			var generatedDetails = declaration.GenerateExitControlFromEntries(declaration.CustomsEntryHeaders.Select(entry => entry.MovementReferenceNumber), exitHeader);

			CombineAssertions(() =>
			{
				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedDetails);
				var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
				AssertEquals("There is 1 new exitDetail created", 1, exitHeaderAfterGeneration.CusExitDetails.Count);
				AssertExitDetail("New exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ExpectedMRN1ForNewExitDetail, ExpectedCustomsOfficeForNewExitDetail, expectedNotificationDate, expectedNotificationPlace);
			});
		}
	}

	public void TestGenerateExitControl_AcceptedHeader_ExistingExitHeaderWithExitDetailToUpdate()
	{
		var expectedBrokerCode = "XZX";
		var expectedHeaderReference = "Reference";
		var expectedNotificationDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
		var expectedNotificationPlace = "Place";

		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = expectedBrokerCode;
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = false;

		var expectedDeclarantOrgHeader = Factory.New<OrgHeader>();
		expectedDeclarantOrgHeader.OH_Code = "Declarant";
		var expectedDeclarantAddress = expectedDeclarantOrgHeader.MainAddress;
		expectedDeclarantAddress.OA_Address1 = "Declarant Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, ExpectedCustomsOfficeForNewExitDetail);
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OA_DeclarantAddress = expectedDeclarantAddress.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = expectedHeaderReference;
			exitHeader.CEH_ArrivalNotificationDate = expectedNotificationDate;
			exitHeader.CEH_ArrivalNotificationPlace = expectedNotificationPlace;

			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
			exitDetail.CED_CustomsOffice = "AAA";
			exitDetail.CED_ArrivalNotificationPlace = "Detail place";

			Factory.Save();

			var generatedDetails = declaration.GenerateExitControlFromEntries(declaration.CustomsEntryHeaders.Select(entry => entry.MovementReferenceNumber), exitHeader);

			CombineAssertions(() =>
			{
				AssertEquals("GenerateExitControlFromEntries returns 1", 1, generatedDetails);
				var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
				AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
				AssertEquals("There is 1 exitDetail updated", 1, exitHeaderAfterGeneration.CusExitDetails.Count);
				AssertExitDetail("Updated exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ExpectedMRN1ForNewExitDetail, ExpectedCustomsOfficeForNewExitDetail, expectedNotificationDate, expectedNotificationPlace);
			});
		}
	}

	public void TestGenerateExitControl_MultipleHeaders_ExistingExitHeaderWithoutExitDetails()
	{
		var expectedHeaderReference = "Reference";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetail;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CEH_Parent = declaration;
		exitHeader.CEH_ReferenceNumber = expectedHeaderReference;

		Factory.Save();

		var generatedDetails = declaration.GenerateExitControlFromEntries(declaration.CustomsEntryHeaders.Select(entry => entry.MovementReferenceNumber), exitHeader);

		CombineAssertions(() =>
		{
			AssertEquals("GenerateExitControlFromEntries returns 3", 3, generatedDetails);
			var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
			AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
			AssertEquals("There are 3 new exitDetails created", 3, exitHeaderAfterGeneration.CusExitDetails.Count);
			AssertContainsExactElementsInAnyOrder("The new exitDetaisl have the correct mrn codes", new ZString[] { ExpectedMRN1ForNewExitDetail, ExpectedMRN2ForNewExitDetail, ExpectedMRN3ForNewExitDetail }, exitHeaderAfterGeneration.CusExitDetails.Cast<CusExitDetail>().Select(x => x.CED_MovementReferenceNumber).ToArray());
			AssertExitDetail("First new exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ZString.Empty, ExpectedCustomsOfficeForNewExitDetail, ZDateTime.Today, ZString.Empty);
			AssertExitDetail("Second new exit detail", exitHeaderAfterGeneration.CusExitDetails[1], ZString.Empty, ExpectedCustomsOfficeForNewExitDetail, ZDateTime.Today, ZString.Empty);
			AssertExitDetail("Third new exit detail", exitHeaderAfterGeneration.CusExitDetails[2], ZString.Empty, ExpectedCustomsOfficeForNewExitDetail, ZDateTime.Today, ZString.Empty);
		});
	}

	public void TestGenerateExitControl_MultipleHeaders_ExistingExitHeaderWithExitDetailsToUpdate()
	{
		var expectedHeaderReference = "Reference";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true);
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CEH_Parent = declaration;
		exitHeader.CEH_ReferenceNumber = expectedHeaderReference;

		var exitDetail1 = exitHeader.CusExitDetails.AddNew();
		exitDetail1.CED_MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
		exitDetail1.CED_Status = "AWR";
		exitDetail1.CED_CustomsOffice = "AAA";
		exitDetail1.CED_ArrivalNotificationPlace = "Detail Place";
		exitDetail1.CED_ArrivalNotificationDate = new ZDateTime(2020, 10, 20, 15, 50, 00);

		var exitDetail2 = exitHeader.CusExitDetails.AddNew();
		exitDetail2.CED_MovementReferenceNumber = ExpectedMRN2ForNewExitDetail;
		exitDetail2.CED_CustomsOffice = "BBB";
		exitDetail2.CED_ArrivalNotificationPlace = "Detail Place 2";
		exitDetail2.CED_ArrivalNotificationDate = new ZDateTime(2020, 05, 20, 15, 50, 00);

		Factory.Save();

		var generatedDetails = declaration.GenerateExitControlFromEntries(declaration.CustomsEntryHeaders.Select(entry => entry.MovementReferenceNumber), exitHeader);

		CombineAssertions(() =>
		{
			AssertEquals("GenerateExitControlFromEntries returns 3", 3, generatedDetails);
			var exitHeaderAfterGeneration = GetExitHeaderForDeclaration(declaration);
			AssertEquals("There is no new exitHeader created since there was already one", exitHeader, exitHeaderAfterGeneration);
			AssertEquals("There are 3 exitDetails, 2 updated, 1 new created", 3, exitHeaderAfterGeneration.CusExitDetails.Count);
			AssertContainsExactElementsInAnyOrder("The new exitDetaisl have the correct mrn codes", new ZString[] { ExpectedMRN1ForNewExitDetail, ExpectedMRN2ForNewExitDetail, ExpectedMRN3ForNewExitDetail }, exitHeaderAfterGeneration.CusExitDetails.Cast<CusExitDetail>().Select(x => x.CED_MovementReferenceNumber).ToArray());
			AssertExitDetail("First updated exit detail", exitHeaderAfterGeneration.CusExitDetails[0], ZString.Empty, ZString.Empty, ZDateTime.Today, ZString.Empty);
			AssertExitDetail("Second updated exit detail", exitHeaderAfterGeneration.CusExitDetails[1], ZString.Empty, ZString.Empty, ZDateTime.Today, ZString.Empty);
			AssertExitDetail("Third new exit detail", exitHeaderAfterGeneration.CusExitDetails[2], ZString.Empty, ZString.Empty, ZDateTime.Today, ZString.Empty);
		});
	}

	void AssertExitDetail(string message, CusExitDetail exitDetail, ZString expectedMRN, string expectedExitCustomsOffice, ZDateTime expectedNotificationDate, string expectedNotificationPlace)
	{
		if (!expectedMRN.IsEmpty)
		{
			AssertEquals(message + " MRN", expectedMRN, exitDetail.CED_MovementReferenceNumber);
		}
		AssertEquals(message + " CustomsOffice", expectedExitCustomsOffice, exitDetail.CED_CustomsOffice);
		AssertEquals(message + " ArrivalNotificationDate", expectedNotificationDate, exitDetail.CED_ArrivalNotificationDate);
		AssertEquals(message + " ArrivalNotificationPlace", expectedNotificationPlace, exitDetail.CED_ArrivalNotificationPlace);
	}

	public void TestGetEntriesWithMRNInAcceptedExitDetails()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		entry1.MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
		var entry2 = declaration.CustomsEntryHeaders.AddNew();
		entry2.MovementReferenceNumber = ExpectedMRN2ForNewExitDetail;
		var entry3 = declaration.CustomsEntryHeaders.AddNew();
		entry3.MovementReferenceNumber = ExpectedMRN3ForNewExitDetail;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		var exitDetail1 = exitHeader.CusExitDetails.AddNew();
		exitDetail1.CED_MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
		exitDetail1.CED_Status = "AWR";

		var exitDetail2 = exitHeader.CusExitDetails.AddNew();
		exitDetail2.CED_MovementReferenceNumber = ExpectedMRN2ForNewExitDetail;

		var exitDetail3 = exitHeader.CusExitDetails.AddNew();
		exitDetail3.CED_MovementReferenceNumber = ExpectedMRN3ForNewExitDetail;
		exitDetail3.CED_Status = "CLR";

		AssertContainsExactElementsInAnyOrder("Should have only entries 1 and 3", new[] { entry1, entry3 }, declaration.GetEntriesWithMRNInAcceptedExitDetails(declaration.CustomsEntryHeaders, exitHeader.CusExitDetails));
	}

	public void TestGetEntriesWithMRNInNotAcceptedExitDetails()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		entry1.MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
		var entry2 = declaration.CustomsEntryHeaders.AddNew();
		entry2.MovementReferenceNumber = ExpectedMRN2ForNewExitDetail;
		var entry3 = declaration.CustomsEntryHeaders.AddNew();
		entry3.MovementReferenceNumber = ExpectedMRN3ForNewExitDetail;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		var exitDetail1 = exitHeader.CusExitDetails.AddNew();
		exitDetail1.CED_MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
		exitDetail1.CED_Status = "AWR";

		var exitDetail2 = exitHeader.CusExitDetails.AddNew();
		exitDetail2.CED_MovementReferenceNumber = ExpectedMRN2ForNewExitDetail;

		var exitDetail3 = exitHeader.CusExitDetails.AddNew();
		exitDetail3.CED_MovementReferenceNumber = ExpectedMRN3ForNewExitDetail;
		exitDetail3.CED_Status = "CLP";

		AssertContainsExactElementsInAnyOrder("Should have only entries 2 and 3", new[] { entry2, entry3 }, declaration.GetEntriesWithMRNInNotAcceptedExitDetails(declaration.CustomsEntryHeaders, exitHeader.CusExitDetails));
	}

	public void TestGetMRNFromEntries()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		entry1.MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
		var entry2 = declaration.CustomsEntryHeaders.AddNew();
		entry2.MovementReferenceNumber = ExpectedMRN2ForNewExitDetail;
		var entry3 = declaration.CustomsEntryHeaders.AddNew();
		entry3.MovementReferenceNumber = ExpectedMRN3ForNewExitDetail;

		AssertContainsExactElementsInAnyOrder("Should return the mrns", new[] { ExpectedMRN1ForNewExitDetail, ExpectedMRN2ForNewExitDetail, ExpectedMRN3ForNewExitDetail }, declaration.GetMRNFromEntries(declaration.CustomsEntryHeaders));
	}

	public void TestGetMRNAndReferenceFromEntries()
	{
		var expectedRef1 = "refNum1";
		var expectedRef2 = "refNum2";
		var expectedRef3 = "refNum3";
		var declaration = Factory.New<JobDeclaration>();
		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		entry1.MovementReferenceNumber = ExpectedMRN1ForNewExitDetail;
		entry1.CH_BGMReference = expectedRef1;
		var entry2 = declaration.CustomsEntryHeaders.AddNew();
		entry2.MovementReferenceNumber = ExpectedMRN2ForNewExitDetail;
		entry2.CH_BGMReference = expectedRef2;
		var entry3 = declaration.CustomsEntryHeaders.AddNew();
		entry3.MovementReferenceNumber = ExpectedMRN3ForNewExitDetail;
		entry3.CH_BGMReference = expectedRef3;

		AssertContainsExactElementsInAnyOrder("Should return the mrns and entry references",
			new[] { new Tuple<ZString, ZString>(ExpectedMRN1ForNewExitDetail, expectedRef1),
					new Tuple<ZString, ZString>(ExpectedMRN2ForNewExitDetail, expectedRef2),
					new Tuple<ZString, ZString>(ExpectedMRN3ForNewExitDetail, expectedRef3) },
			declaration.GetMRNAndReferenceFromEntries(declaration.CustomsEntryHeaders));
	}

	public void TestGetOrCreateExitHeader_NewExitHeader()
	{
		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = "XZX";
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = true;

		var expectedDeclarantOrgHeader = Factory.New<OrgHeader>();
		expectedDeclarantOrgHeader.OH_Code = "Declarant";
		var expectedDeclarantAddress = expectedDeclarantOrgHeader.MainAddress;
		expectedDeclarantAddress.OA_Address1 = "Declarant Address";

		var expectedSupplierOrgHeader = Factory.New<OrgHeader>();
		expectedSupplierOrgHeader.OH_Code = "Supplier";
		var expectedSupplierAddress = expectedSupplierOrgHeader.MainAddress;
		expectedSupplierAddress.OA_Address1 = "Supplier Address";

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, false);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, ExpectedCustomsOfficeForNewExitDetail);
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "LV009999");
			declaration.JE_CustomsOffice = "LV009998";
			declaration.JE_OA_DeclarantAddress = expectedDeclarantAddress.PK;
			declaration.JE_OH_Supplier = expectedSupplierOrgHeader.PK;

			Factory.Save();

			declaration.GetOrCreateExitControlHeader();

			CombineAssertions(() =>
			{
				var exitHeader = GetExitHeaderForDeclaration(declaration);
				AssertExitHeader("New exit detail", exitHeader, ZDateTime.Today, ZString.Empty, ZString.Empty, expectedDeclarantAddress.PK, expectedSupplierAddress.PK, declaration.PK, ExpectedReferenceForNewExitHeader);
			});
		}
	}

	public void TestGetOrCreateExitHeader_ExistingExitHeader()
	{
		var expectedHeaderReference = "Reference";

		var declaration = Factory.GetNewDeclarationForExitControlGeneration(Staff, true, true);
		declaration.JE_CustomsOffice = ExpectedCustomsOfficeForNewExitDetail;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;

		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CEH_Parent = declaration;
		exitHeader.CEH_ReferenceNumber = expectedHeaderReference;

		Factory.Save();

		declaration.GetOrCreateExitControlHeader();

		CombineAssertions(() =>
		{
			exitHeader = GetExitHeaderForDeclaration(declaration);
			AssertExitHeader("Updated exit detail", exitHeader, ZDateTime.Today, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, declaration.PK, expectedHeaderReference);
		});
	}

	void AssertExitHeader(string message, CusExitControlHeader header, ZDateTime expectedNotificationDate, string expectedNotificationPlace, string expectedBroker, ZGuid expectedAgentID, ZGuid expectedCarrierID, ZGuid expectedParentID, string expectedHeaderReference)
	{
		AssertEquals(message + " Header.ArrivalNotificationDate", expectedNotificationDate, header.CEH_ArrivalNotificationDate);
		AssertEquals(message + " Header.ArrivalNotificationPlace", expectedNotificationPlace, header.CEH_ArrivalNotificationPlace);
		AssertEquals(message + " Header.NKCustomsAgent", expectedBroker, header.CEH_GS_NKCustomsAgent);
		AssertEquals(message + " Header.OA_Agent", expectedAgentID, header.CEH_OA_Agent);
		AssertEquals(message + " Header.OA_Carrier", expectedCarrierID, header.CEH_OA_Carrier);
		AssertEquals(message + " Header.Parent", expectedParentID, header.CEH_ParentID);
		AssertEquals(message + " Header.ParentTableCode", "JE", header.CEH_ParentTableCode);
		AssertEquals(message + " Header.ReferenceNumber", expectedHeaderReference, header.CEH_ReferenceNumber);
	}

	public void TestShouldAddOrCopyHouseBillToTransportDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_HouseBill = ZString.Empty;
		declaration.JE_TransportMode = TransportModes.OwnPropulsion;

		CombineAssertions(() =>
		{
			AssertEquals("Document shouldn't be added to the list or copy reference if House of Bill is Empty", false, declaration.ShouldAddOrCopyHouseBillToTransportDocuments());

			declaration.JE_HouseBill = "Transport";
			AssertEquals("Document shouldn't be added to the list or copy reference if transport mode is not Sea, Rail, Road, Air", false, declaration.ShouldAddOrCopyHouseBillToTransportDocuments());

			declaration.JE_TransportMode = TransportModes.Sea;
			AssertEquals("Document should be added to the list or copy reference if House of Bill is not empty and transport mode is Sea, Rail, Road, Air", true, declaration.ShouldAddOrCopyHouseBillToTransportDocuments());
		});
	}

	public void TestAddHouseBillTransportDocument_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.T2L);
		declaration.Factory.Save();

		AssertAddHouseBillTransportDocument(declaration);
	}

	public void TestAddHouseBillTransportDocument_ImportT2LorT2C()
	{
		var declaration = Factory.New<JobDeclaration>();
		BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, EntrySubStyleList.Codes.T2C);
		declaration.Factory.Save();

		AssertAddHouseBillTransportDocument(declaration);
	}

	public void TestAddHouseBillTransportDocument_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.B);
		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No additional infos in the list", 0, declaration.AdditionalInfos.Count);
			AssertEquals("No supporting documents in the list", 0, declaration.SupportingDocuments.Count);

			declaration.AddHouseBillTransportDocument();
			AssertEquals("New supporting document added to the list", 1, declaration.SupportingDocuments.Count);
			AssertEquals("No new additional info added to the list", 0, declaration.AdditionalInfos.Count);

			var document = declaration.SupportingDocuments.FirstOrDefault();
			AssertEquals("New document Code", "N705", document.CSI_Code);
			AssertEquals("New document Reference", "Transport", document.CSI_ReferenceNumber);
		});
	}

	public void TestAddHouseBillTransportDocument_ImportT2LAndNoT2L()
	{
		var declaration = Factory.New<JobDeclaration>();
		BuilderHelperTest.AddBillDataToDeclaration(declaration, new ZDateTime(2021, 11, 03, 9, 51, 0), "Transport", Common.Shared.SharedJobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.T2L);
		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("No additional infos in the list", 0, declaration.AdditionalInfos.Count);
			AssertEquals("No supporting documents in the list", 0, declaration.SupportingDocuments.Count);

			declaration.AddHouseBillTransportDocument();
			AssertEquals("No additional infos in the list after method", 0, declaration.AdditionalInfos.Count);
			AssertEquals("No supporting documents in the list after method", 0, declaration.SupportingDocuments.Count);
		});
	}

	void AssertAddHouseBillTransportDocument(JobDeclaration declaration)
	{
		CombineAssertions(() =>
		{
			AssertEquals("No additional infos in the list", 0, declaration.AdditionalInfos.Count);
			AssertEquals("No supporting documents in the list", 0, declaration.SupportingDocuments.Count);

			declaration.AddHouseBillTransportDocument();
			AssertEquals("New additional info added to the list", 1, declaration.AdditionalInfos.Count);
			AssertEquals("no new supporting document added to the list", 0, declaration.SupportingDocuments.Count);

			var document = declaration.AdditionalInfos.FirstOrDefault();
			AssertEquals("New document Code", "N705", document.CSI_Code);
			AssertEquals("New document Reference", "Transport", document.CSI_ReferenceNumber);
			AssertEquals("New document SubType", "TRA", document.CSI_SubType);
		});
	}

	public void TestAreAllEntriesT2LorT2C()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

		CombineAssertions(() =>
		{
			AssertEquals("All entries are T2C", true, declaration.AreAllEntriesT2LorT2C());

			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("None entries are T2L or T2C", false, declaration.AreAllEntriesT2LorT2C());

			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("One entry is T2L and other T2C", true, declaration.AreAllEntriesT2LorT2C());

			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("One entry is T2L and other not", false, declaration.AreAllEntriesT2LorT2C());

			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("All entries are T2L", true, declaration.AreAllEntriesT2LorT2C());
		});
	}

	public void TestAreAllEntriesNoT2LNorT2C()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.Invoices.AddNew();

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		CombineAssertions(() =>
		{
			AssertEquals("One entry is T2L and other A", false, declaration.AreAllEntriesNoT2LNorT2C());

			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("None entries are T2L or T2C", true, declaration.AreAllEntriesNoT2LNorT2C());

			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("One entry is A and other T2C", false, declaration.AreAllEntriesNoT2LNorT2C());
		});
	}

	public void TestMethodOfPaymentNotDefaultedInInvoiceLines_NoDestinationState()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = ZString.Empty;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.ZG_MethodOfPayment = "A";
		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.ZG_MethodOfPayment = "A";
		invoiceLine3.ZG_MethodOfPayment2 = "A";

		CombineAssertions("For Import", () =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPayment = "J";
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "S";
			declaration.JE_OH_Importer = orgHeader.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted", invoiceLine1, ZString.Empty, ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 not defaulted", invoiceLine2, "A", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 not defaulted", invoiceLine3, "A", "A");
		});

		CombineAssertions("For Export", () =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPayment = "R";
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "R";
			declaration.JE_OH_Importer = orgHeader2.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted", invoiceLine1, ZString.Empty, ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 not defaulted", invoiceLine2, "A", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 not defaulted", invoiceLine3, "A", "A");
		});
	}

	public void TestMethodOfPaymentDefaultedInInvoiceLines_Mainland()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = "01";

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.ZG_MethodOfPayment = "A";
		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.ZG_MethodOfPayment = "A";
		invoiceLine3.ZG_MethodOfPayment2 = "A";

		CombineAssertions("For Import", () =>
		{
			var orgHeaderWithoutMOP = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = orgHeaderWithoutMOP.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted when no mop in importer", invoiceLine1, ZString.Empty, ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 not defaulted when no mop in importer", invoiceLine2, "A", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 not defaulted when no mop in importer", invoiceLine3, "A", "A");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPayment = "J";
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "S";
			declaration.JE_OH_Importer = orgHeader.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 defaulted when mop in importer", invoiceLine1, "J", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 defaulted when mop in importer", invoiceLine2, "J", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 defaulted when mop in importer", invoiceLine3, "J", "A");

			invoiceLine1.ZG_MethodOfPayment = ZString.Empty;
			invoiceLine2.ZG_MethodOfPayment = "A";
			invoiceLine3.ZG_MethodOfPayment = "A";

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 back to original value", invoiceLine1, ZString.Empty, ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 back to original value", invoiceLine2, "A", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 back to original value", invoiceLine3, "A", "A");

			declaration.ZG_DestinationState = "28";

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 defaulted when mop in importer and destination state changes", invoiceLine1, "J", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 defaulted when mop in importer and destination state changes", invoiceLine2, "J", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 defaulted when mop in importer and destination state changes", invoiceLine3, "J", "A");
		});

		CombineAssertions("For Export", () =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPayment = "R";
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "R";
			declaration.JE_OH_Importer = orgHeader2.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted for export", invoiceLine1, "J", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 not defaulted for export", invoiceLine2, "J", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 not defaulted for export", invoiceLine3, "J", "A");
		});
	}

	public void TestMethodOfPaymentDefaultedInInvoiceLines_CanaryIsland()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");

		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain + "C", parent: grouping);
		helper.CreateCusCodeList("ESC", RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "61", "Test 61", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateCusCodeList("ESC", RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "62", "Test 62", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.ZG_DestinationState = "61";

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.ZG_MethodOfPayment = "A";
		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.ZG_MethodOfPayment = "A";
		invoiceLine3.ZG_MethodOfPayment2 = "A";

		CombineAssertions("For Import", () =>
		{
			var orgHeaderWithoutMOP = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = orgHeaderWithoutMOP.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted when no mop in importer", invoiceLine1, ZString.Empty, ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 not defaulted when no mop in importer", invoiceLine2, "A", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 not defaulted when no mop in importer", invoiceLine3, "A", "A");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPayment = "J";
			((ESOrgImpAddInfo)orgHeader.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "S";
			declaration.JE_OH_Importer = orgHeader.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 defaulted when mop in importer", invoiceLine1, "J", "S");
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 defaulted when mop in importer", invoiceLine2, "J", "S");
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 defaulted when mop in importer", invoiceLine3, "J", "S");

			invoiceLine1.ZG_MethodOfPayment = ZString.Empty;
			invoiceLine1.ZG_MethodOfPayment2 = ZString.Empty;
			invoiceLine2.ZG_MethodOfPayment = "A";
			invoiceLine2.ZG_MethodOfPayment2 = ZString.Empty;
			invoiceLine3.ZG_MethodOfPayment = "A";
			invoiceLine3.ZG_MethodOfPayment2 = "A";

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 back to original value", invoiceLine1, ZString.Empty, ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 back to original value", invoiceLine2, "A", ZString.Empty);
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 back to original value", invoiceLine3, "A", "A");

			declaration.ZG_DestinationState = "62";

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 defaulted when mop in importer and destination state changes", invoiceLine1, "J", "S");
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 defaulted when mop in importer and destination state changes", invoiceLine2, "J", "S");
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 defaulted when mop in importer and destination state changes", invoiceLine3, "J", "S");
		});

		CombineAssertions("For Export", () =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPayment = "R";
			((ESOrgImpAddInfo)orgHeader2.CountryData.ImpAddInfo).ZO_MethodOfPaymentCan = "R";
			declaration.JE_OH_Importer = orgHeader2.PK;

			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 1 not defaulted for export", invoiceLine1, "J", "S");
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 2 not defaulted for export", invoiceLine2, "J", "S");
			AssertMethodOfPaymentsForInvoiceLine("InvoiceLine 3 not defaulted for export", invoiceLine3, "J", "S");
		});
	}

	public void TestJE_CustomsProfileDefaulted()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert2";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var staff2 = Factory.New<GlbStaff>();
		staff2.GS_Code = "AA";
		staff2.GS_LoginName = "aatest";
		var wrapper2 = GlbStaffWrapper.Get(staff2);
		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert3";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert4";
		cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

		var staff3 = Factory.New<GlbStaff>();
		staff3.GS_Code = "AZ";
		staff3.GS_LoginName = "aztest";

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("JE_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, declaration.JE_CustomsProfile);

			declaration.JE_GS_NKCusAgent = staff2.GS_Code;
			AssertEquals("JE_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", declaration.JE_CustomsProfile);

			declaration.JE_GS_NKCusAgent = staff3.GS_Code;
			AssertEquals("JE_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, declaration.JE_CustomsProfile);

			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.JE_CustomsProfile = "TestCert2";
			AssertEquals("JE_CustomsProfile has value when broker not empty", "TestCert2", declaration.JE_CustomsProfile);

			declaration.JE_GS_NKCusAgent = ZString.Empty;
			AssertEquals("JE_CustomsProfile has been cleared when broker is empty", ZString.Empty, declaration.JE_CustomsProfile);

			declaration.JE_CustomsProfile = "TESTCERT1";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("JE_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", declaration.JE_CustomsProfile);

			declaration.JE_GS_NKCusAgent = ZString.Empty;
			declaration.JE_CustomsProfile = "TestCert2";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("JE_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, declaration.JE_CustomsProfile);
		});
	}

	public void TestJE_CustomsProfileEmptyWhenCopy()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_CustomsProfile = "TestCert2";

		var copiedDeclaration = (JobDeclaration)declaration.TemplateCopy();
		AssertEquals("JE_CustomsProfile is empty when copied", ZString.Empty, copiedDeclaration.JE_CustomsProfile);
	}

	public void TestHasAnyH2Entry()
	{
		var declaration = Factory.New<JobDeclaration>();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is only one and is A and style empty", false, declaration.HasAnyH2Entry);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("Entry is only one and is A and style H2", true, declaration.HasAnyH2Entry);

			entryInstruction.CEI_Style = ZString.Empty;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("there are two Entry, one with A-Not H2 and other with B-Not H2", false, declaration.HasAnyH2Entry);

			entryInstruction1.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("there are two Entry, one with A-Not H2 and other with B-H2", true, declaration.HasAnyH2Entry);
		});
	}

	public void TestHasAnyDiffH2Entry()
	{
		var declaration = Factory.New<JobDeclaration>();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is only one and is A and style empty", true, declaration.HasAnyDiffH2Entry);

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("Entry is only one and is A and style H2", false, declaration.HasAnyDiffH2Entry);

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("there are two Entry, one with A-H2 and other with B-Not H2", true, declaration.HasAnyDiffH2Entry);

			entryInstruction1.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("there are two Entry, one with A-H2 and other with B-H2", false, declaration.HasAnyDiffH2Entry);
		});
	}

	public void TestHasAnyDiffT2CEntry()
	{
		var declaration = Factory.New<JobDeclaration>();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is only one and is A", true, declaration.HasAnyDiffT2CEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("Entry is only one and is T2c", false, declaration.HasAnyDiffT2CEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("there are two Entry and all have A", true, declaration.HasAnyDiffT2CEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and one have T2C", true, declaration.HasAnyDiffT2CEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and all have T2C", false, declaration.HasAnyDiffT2CEntry);
		});
	}

	public void TestHasAnyDiffT2CAndT2lEntry()
	{
		var declaration = Factory.New<JobDeclaration>();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is only one and is A", true, declaration.HasAnyDiffT2CAndT2lEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("Entry is only one and is T2c", false, declaration.HasAnyDiffT2CAndT2lEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("there are two Entry and all have A", true, declaration.HasAnyDiffT2CAndT2lEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and one have T2C", true, declaration.HasAnyDiffT2CAndT2lEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and all have T2C", false, declaration.HasAnyDiffT2CAndT2lEntry);
		});
	}

	public void TestHasAnyDiffT2CAndT2lAndEXSEntry()
	{
		var declaration = Factory.New<JobDeclaration>();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is only one and is A", true, declaration.HasAnyDiffT2CAndT2lAndEXSEntry);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("Entry is only one and is EXS", false, declaration.HasAnyDiffT2CAndT2lAndEXSEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("Entry is only one and is T2C", false, declaration.HasAnyDiffT2CAndT2lAndEXSEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("there are two Entry and all have A", true, declaration.HasAnyDiffT2CAndT2lAndEXSEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("there are two Entry and one have EXS", true, declaration.HasAnyDiffT2CAndT2lAndEXSEntry);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and one have EXS and other T2C", false, declaration.HasAnyDiffT2CAndT2lAndEXSEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and one have T2C", true, declaration.HasAnyDiffT2CAndT2lAndEXSEntry);
		});
	}

	public void TestHasAnyDiffT2CAndT2lAndEXSAndBAndCEntry()
	{
		var declaration = Factory.New<JobDeclaration>();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			AssertEquals("Entry is only one and is A", true, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("Entry is only one and is EXS", false, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("Entry is only one and is T2C", false, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("there are two Entry and all have A", true, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("there are two Entry and one have EXS", true, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and one have EXS and other T2C", false, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("there are two Entry and one have T2C", true, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("there are two Entry and one have B", true, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("there are two Entry and one have C and other B", false, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("there are two Entry and one have C", true, declaration.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry);
		});
	}

	public void TestGetExportCustomsOffice()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			AssertEquals("Expected correct value with CustomsOffice office code when no other customsOffice is declared", "ES009999", declaration.GetExportCustomsOffice());

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			customsOffice1.CY_Data = "FR008889";
			AssertEquals("Expected correct value with OfficeOfExport office code when declared alone in CustomsOffice list", "FR008889", declaration.GetExportCustomsOffice());

			customsOffice1.CY_Data = ZString.Empty;
			AssertEquals("Expected correct value with CustomsOffice office code when declared Empty OfficeOfExport", "ES009999", declaration.GetExportCustomsOffice());
		});
	}

	public void TestGetExitCustomsOffice()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			AssertEquals("Expected correct value with CustomsOffice office code when no other customsOffice is declared", "ES009999", declaration.GetExitCustomsOffice());

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			customsOffice1.CY_Data = "FR008889";
			AssertEquals("Expected correct value with OfficeOfExport office code when declared alone in CustomsOffice list", "FR008889", declaration.GetExitCustomsOffice());

			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOffice1.CY_Data = "ES009998";
			AssertEquals("Expected correct value with OfficeOfExit office code when declared alone in CustomsOffice list", "ES009998", declaration.GetExitCustomsOffice());

			customsOffice1.CY_Data = ZString.Empty;
			AssertEquals("Expected correct value with CustomsOffice office code when declared Empty OfficeOfExit an no OfficeOfExport declared", "ES009999", declaration.GetExitCustomsOffice());

			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			AssertEquals("Expected correct value with CustomsOffice office code when declared Empty OfficeOfExport an no OfficeOfExit declared", "ES009999", declaration.GetExitCustomsOffice());

			customsOffice1.CY_Data = "FR008889";
			var customsOffice2 = declaration.CustomsOffices.AddNew();
			customsOffice2.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOffice2.CY_Data = "ES009998";
			AssertEquals("Expected correct value with OfficeOfExit office code when declared all (CustomsOffice, OfficeOfExport and OfficeOfExit)", "ES009998", declaration.GetExitCustomsOffice());

			customsOffice2.CY_Data = ZString.Empty;
			AssertEquals("Expected correct value with OfficeOfExit office code when declared all (CustomsOffice, OfficeOfExport and OfficeOfExit) but OfficeOfExit is empty", "FR008889", declaration.GetExitCustomsOffice());
		});
	}

	public void TestGetPresentationCustomsOffice()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			AssertEquals("Expected correct value with CustomsOffice office code when no other customsOffice is declared", "ES009999", declaration.GetPresentationCustomsOffice());

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOffice1.CY_Data = "FR008889";
			AssertEquals("Expected correct value with OfficeOfPresentation office code when declared alone in CustomsOffice list", "FR008889", declaration.GetPresentationCustomsOffice());

			customsOffice1.CY_Data = ZString.Empty;
			AssertEquals("Expected correct value with CustomsOffice office code when declared Empty OfficeOfPresentation", "ES009999", declaration.GetPresentationCustomsOffice());
		});
	}

	public void TestGetCustomsOfficeFromList()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			AssertEquals("Expected empty when no office is declared in CustomsOffices", ZString.Empty, declaration.GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport));

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			customsOffice1.CY_Data = "FR008889";
			AssertEquals("Expected correct value when EXP office is declared in CustomsOffices", "FR008889", declaration.GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport));

			AssertEquals("Expected empty when EXT office is not declared in CustomsOffices", ZString.Empty, declaration.GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit));

			var customsOffice2 = declaration.CustomsOffices.AddNew();
			customsOffice2.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOffice2.CY_Data = "ES009998";
			AssertEquals("Expected correct value when EXT office is declared in CustomsOffices", "ES009998", declaration.GetCustomsOfficeFromList(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit));
		});
	}

	public void TestLoadAEOCusGuaranteeHeaderFromReference()
	{
		var declaration = Factory.New<JobDeclaration>();

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, holder.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

		CombineAssertions(() =>
		{
			AssertEquals("When holder pk is empty the method returns null", null, declaration.LoadAEOCusGuaranteeHeaderFromReference(ZGuid.Empty));

			AssertEquals("When holder pk is not empty and there is a current AEOC authorisation with that holder it is returned", authorisation, declaration.LoadAEOCusGuaranteeHeaderFromReference(holder.PK));

			authorisation.CPH_Type = ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation;
			Factory.Save();
			AssertEquals("When holder pk is not empty but there is a current not AEOC, AEOF or AEOS authorisation with that holder the method returns null", null, declaration.LoadAEOCusGuaranteeHeaderFromReference(holder.PK));

			authorisation.CPH_Type = ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplificationsSecurityAndSafety;
			Factory.Save();
			AssertEquals("When holder pk is not empty and there is a current AEOF authorisation with that holder it is returned", authorisation, declaration.LoadAEOCusGuaranteeHeaderFromReference(holder.PK));

			authorisation.CPH_StartDate = ZDate.Today.AddDays(1);
			Factory.Save();
			AssertEquals("When holder pk is not empty but there is a not current AEOC, AEOF or AEOS authorisation with that holder the method returns null", null, declaration.LoadAEOCusGuaranteeHeaderFromReference(holder.PK));

			authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
			authorisation.CPH_Type = ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorSecurityAndSafety;
			Factory.Save();
			AssertEquals("When holder pk is not empty and there is a current AEOS authorisation with that holder it is returned", authorisation, declaration.LoadAEOCusGuaranteeHeaderFromReference(holder.PK));

			SupDocTestHelper.AddAuthorisationWithHolder(Factory, holder.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);
			AssertNotNull("When holder pk is not empty and there are multiple current AEOC, AEOF or AEOS authorisation with that holder one is returned", declaration.LoadAEOCusGuaranteeHeaderFromReference(holder.PK));
		});
	}

	public void TestGetEntryFeePaymentPartyUnderstander()
	{
		AssertType<EntryFeePaymentPartyUnderstander>(declaration.GetEntryFeePaymentPartyUnderstander(null));
	}

	public void TestIsSecurityAllowed()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = "EXP";

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			AssertEquals("For EntryStyle = CO, IsSecurityDeclarationCheckBox IsVisible", false, declaration.IsSecurityAllowed());

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			AssertEquals("For EntryStyle = EX, IsSecurityDeclarationCheckBox IsVisible", true, declaration.IsSecurityAllowed());
		}
	}

	public void TestJE_RN_NKTransportNationality_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("[21.2] Nationality", DataBoundResourceStrings.GetDataForProperty(declaration.JE_RN_NKTransportNationalityInfo).Caption);
	}

	public void TestBox18TransportNationality_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("[18.2] Nationality", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_Box18TransportNationalityInfo).Caption);
	}

	public void TestZG_Box18TransportID_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("[18] Transport ID", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_Box18TransportIDInfo).Caption);
	}

	public void TestJE_TransportModeInland_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("[26] Trans. Mode", DataBoundResourceStrings.GetDataForProperty(declaration.JE_TransportModeInlandInfo).Caption);
	}

	public void TestJE_IATALoadPort_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("IATA", DataBoundResourceStrings.GetDataForProperty(declaration.JE_IATALoadPortInfo).Caption);
	}

	public void TestEUD_AgreedPlaceCodeValidationSupport()
	{
		AssertEquals("For ES, EUD_AgreedPlaceCodeValidation is not apply", false, declaration.EUD_AgreedPlaceCodeValidationSupport);
	}
	public void TestDoNotDefaultJE_DeclarantType_OnJobCreation()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Declarant Type", "", declaration.JE_DeclarantType);
	}

	public void TestDefaultJE_DeclarantType_OnImporterChangeWithMatchingDeclarantEori()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("EOR", "0123456789");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
		declaration.JE_OH_Importer = orgHeader.PK;
		AssertEquals("Declarant Type", "1", declaration.JE_DeclarantType);
	}

	public void TestDefaultJE_DeclarantType_OnSupplierChangeWithMatchingDeclarantEori()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("EOR", "0123456789");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
		declaration.JE_OH_Supplier = orgHeader.PK;
		AssertEquals("Declarant Type", "1", declaration.JE_DeclarantType);
	}

	public void TestDoNotDefaultJE_DeclarantType_OnImporterChangeWithNoMatchingDeclarantEori()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew("EOR", "0123456789");
		var importer = Factory.New<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_OH_Importer = importer.PK;
		AssertEquals("Declarant Type", "", declaration.JE_DeclarantType);
	}

	public void TestDoNotDefaultJE_DeclarantType_OnSupplierChangeWithNoMatchingDeclarantEori()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew("EOR", "0123456789");
		var supplier = Factory.New<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_OH_Supplier = supplier.PK;
		AssertEquals("Declarant Type", "", declaration.JE_DeclarantType);
	}

	public void TestEquipmentsType()
	{
		AssertType<CusEquipmentCollection<CusEquipment>>(declaration.Equipments);
	}

	void AssertMethodOfPaymentsForInvoiceLine(ZString message, JobComInvoiceLine line, ZString mop, ZString mop2)
	{
		AssertEquals("MethodOfPayment for " + message, mop, line.ZG_MethodOfPayment);
		AssertEquals("MethodOfPayment2 for " + message, mop2, line.ZG_MethodOfPayment2);
	}

	protected override Type AddInfoChildType => typeof(JobEUDeclaration);

	protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

	protected override Type ExpectedMergeManagerType => typeof(MergeManager);

	protected override bool IsDeclarantTypeExpectedToChangeWhenAssigningImporter => false;

	protected override string GetLocalPortCode() => "ESMAD";

	protected override string GetExpectedBox30LocationOfGoods(string countryCode) => "LHRLHR";

	protected override string GetExpectedBox30LocationOfGoodsWithSubLocation(string countryCode) => "LHRLHRBAC";

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override ZString? NoVariableDefaultDataGroupingCodeCountry => Core.Constants.CountryCodes.Spain;

	protected override ZString DefaultBorderTransportModeForAir => ZString.Empty;

	protected override ZString DefaultBorderTransportModeForSea => ZString.Empty;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		JobDeclaration result = (JobDeclaration)base.GetNewBusinessObjectForDeleteTest(factory);
		result.CustomsOffices.AddNew();
		return result;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	public GlbStaff Staff
	{
		get
		{
			if (staff == null)
			{
				staff = Factory.GetStaffAccount();
			}

			return staff;
		}
	}
	GlbStaff staff;

	const string ExpectedMRN1ForNewExitDetail = "refNum1";
	const string ExpectedMRN2ForNewExitDetail = "refNum2";
	const string ExpectedMRN3ForNewExitDetail = "refNum3";
	const string ExpectedReferenceForNewExitHeader = "ES00001";
	const string ExpectedCustomsOfficeForNewExitDetail = "ES009999";

	CusExitControlHeader GetExitHeaderForDeclaration(JobDeclaration declaration, bool exitHeaderShouldExist = true)
	{
		var query = new ZQuery(CusExitControlHeaderSchema.CEH_ParentID, declaration.PK);
		var exitHeaders = Factory.Load<CusExitControlHeader>(query);

		if (exitHeaderShouldExist)
		{
			AssertEquals("Only one exitHeader associated to the declaration", 1, exitHeaders.Length);
			return exitHeaders[0];
		}
		else
		{
			AssertEquals("No exitHeaders associated to the declaration", 0, exitHeaders.Length);
			return null;
		}
	}

	public void TestGetTemplateCopyStrategy()
	{
		var declaration = Factory.New<JobDeclarationForTest>();
		var stratety = declaration.GetTemplateCopyStrategyExposed(declaration.Factory, CloneType.TemplateCopy);
		AssertType<JobDeclarationDeepCloneStrategy>(stratety);
	}

	public void TestIsIntegrationWithAccountingSupported()
	{
		var declaration = Factory.New<JobDeclarationForTest>();
		Assert("Integration with accounting should be enabled in ES solution (mandatory for autobilling).", declaration.IsIntegrationWithAccountingSupportedExposed);
	}

	public void TestReportSaveExceptionForSupplierOrImporter()
	{
		var factory = declaration.Factory;

		var sqlException = SqlExceptionBuilder.CreateSqlException(547, "The AR/AP Details (OrgCompanyData) cannot be inserted/updated, requires a reference to a valid Organization (OrgHeader).");
		var ex = new ZSaveException(new ZDataException(sqlException, null, TestConnection), factory);

		var supplier = factory.New<OrgHeader>();
		supplier.OH_Code = "AAA";
		supplier.OH_IsConsignor = true;
		supplier.MainAddress.OA_Address1 = "Add1";
		factory.Save();

		supplier.OH_FullName = "AAA Inc.";

		var importer = factory.New<OrgHeader>();
		importer.OH_Code = "BBB";
		importer.OH_IsConsignee = true;
		importer.MainAddress.OA_Address1 = "Add2";

		declaration.JE_OH_Supplier = supplier.PK;
		declaration.JE_OH_Importer = importer.PK;

		declaration.ReportSaveExceptionForSupplierOrImporter(ex);
		CombineAssertions(() =>
		{
			var lastMessage = ErrorReporter.LastMessageReported;
			AssertContains("Declaration.Supplier:", lastMessage);
			AssertContains("JE_OH_Supplier from a new factory:", lastMessage);
			AssertContains("Declaration.Importer:", lastMessage);
			AssertContains("JE_OH_Importer from a new factory:", lastMessage);
			AssertContains("Changed OrgHeader in the same factory:", lastMessage);
			AssertContains("All Field Values:", lastMessage);
			AssertContains("OH_Category: BUS", lastMessage);
			AssertContains("Current registry settings:", lastMessage);
			AssertContains("OrgMatchThreshold: MED", lastMessage);
			AssertContains("UseUnmatchedOrganisationForMatching: UNMATCHED", lastMessage);
			ErrorReporter.Clear();
		});
	}

	public void TestCustomsClearanceStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_EntryStatus = "ACC";

		AssertEquals("ACC", declaration.CustomsClearanceStatus);
	}

	class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategyExposed(BusinessObjectFactory alternateFactory, CloneType cloneType) => base.GetTemplateCopyStrategy(alternateFactory, cloneType);

		public bool IsIntegrationWithAccountingSupportedExposed => IsIntegrationWithAccountingSupported;
	}
}
