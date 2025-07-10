using System;
using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
sealed partial class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
{
	protected override EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForImport => EUCommonConstants.TransportModeSource.InlandTransportMode;

	public void TestEUD_AgreedPlaceCodeSupport()
	{
		var declaration = Factory.New<JobDeclaration>();

		AssertEquals("In IT EUD_AgreedPlaceCodeSupport is always true", true, declaration.EUD_AgreedPlaceCodeValidationSupport);
	}

	public void TestZG_AgreedPlaceCodeSupport()
	{
		var declaration = Factory.New<JobDeclaration>();

		AssertEquals("In IT ZG_AgreedPlaceCodeSupport is always false", false, declaration.ZG_AgreedPlaceCodeValidationSupport);
	}

	public void TestOfficeOfExit()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IT017100");

		declaration.JE_MessageType = "EXP";
		AssertEquals("When declaration is EXP, Customs Office with pourpose EXT is expected", "IT017100", declaration.OfficeOfExit);

		declaration.JE_MessageType = "IMP";
		AssertEquals("When declaration is IMP, No customs office is expected", ZString.Empty, declaration.OfficeOfExit);
	}

	public void TestIsOfficeOfExitMeaningfulForDeclaration()
	{
		var declaration = Factory.New<JobDeclarationForOfficeOfExitTest>();

		declaration.JE_MessageType = "IMP";
		AssertEquals("When declaration is IMP office of exit is not meaningful", false, declaration.IsOfficeOfExitMeaningful_Exposed);

		declaration.JE_MessageType = "EXP";
		AssertEquals("When declaration is EXP office of exit is meaningful", true, declaration.IsOfficeOfExitMeaningful_Exposed);
	}

	public void TestImportJE_SubLocationOfGoods()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		declaration.ImportJE_SubLocationOfGoods = "IT";
		AssertEquals("ImportJE_SubLocationOfGoods wraps JE_SubLocationOfGoods so the same value must be found in the latter", "IT", declaration.JE_SubLocationOfGoods);
	}

	public void TestImportJE_LocationOtherInformation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		declaration.JE_LocationOtherInformation = "RRR";
		AssertEquals("ImportJE_LocationOtherInformation wraps JE_LocationOtherInformation so the same value must be found in the latter", "RRR", declaration.JE_LocationOtherInformation);
	}

	public void TestLogEventIfEntryStatusChanged()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		CombineAssertions("Declaration logs", () =>
		{
			declaration.JE_EntryStatus = ITMessageStatusList.Codes.ClearOriginal;
			Factory.Save();
			Assert("JobDeclaration should not have ECC line ", !declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CES" && x.SL_Reference == "ECC"));

			Assert("JobDeclaration should not have AWC line at this stage", !declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CES" && x.SL_Reference == "AWC"));
			declaration.JE_EntryStatus = ITMessageStatusList.Codes.AwaitingChange;
			Factory.Save();
			Assert("JobDeclaration should have AWC line at this stage", declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CES" && x.SL_Reference == "AWC"));
		});
	}

	public void TestEntryCreationStrategy()
	{
		AssertType<EntryCreationStrategy>("Should Be Enterprise.Customs.IT.Business.Declaration.EntryCreationStrategy", declaration.CreateEntryCreationStrategy());
	}

	public void TestGetApplicationReference()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		Factory.New<OrgHeader>().OH_Code = "DEC1";

		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "845A")
			.AppendAccountDetail("8451-DEC1", "DEC1")
			.Build();

		CombineAssertions(() =>
		{
			jobDeclaration.JE_CustomsProfile = "8451-DEC1";
			jobDeclaration.JE_GS_NKCusAgent = "CRR";
			jobDeclaration.JE_CustomsOffice = "IT279100";
			AssertEquals("Filled values", "845A:CRR:IT279100", jobDeclaration.GetApplicationReference());

			jobDeclaration.JE_CustomsProfile = "";
			AssertEquals("Empty JE_CustomsProfile", ":CRR:IT279100", jobDeclaration.GetApplicationReference());

			jobDeclaration.JE_CustomsProfile = "XXXX";
			AssertEquals("Not Valid JE_CustomsProfile", ":CRR:IT279100", jobDeclaration.GetApplicationReference());

			jobDeclaration.JE_GS_NKCusAgent = "";
			AssertEquals("Empty JE_CustomsProfile and JE_GS_NKCusAgent", "::IT279100", jobDeclaration.GetApplicationReference());

			jobDeclaration.JE_CustomsOffice = "";
			AssertEquals("Empty values (edge case)", "::", jobDeclaration.GetApplicationReference());
		});
	}

	public override void TestMergeManagerType()
	{
		AssertType<MergeManager>("Should be a Customs.IT.Business.MergeManager", declaration.MergeManager);
	}

	public void TestReciprocalRates()
	{
		AssertEquals(false, declaration.IsReciprocalRates);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Italy, declaration.LocalCurrencyCode);
	}

	public void TestCustomsEntryHeaders()
	{
		AssertType<CusEntryHeaderCollection>(declaration.CustomsEntryHeaders);
		AssertType<CusEntryHeader>(declaration.CustomsEntryHeaders.AddNew());
	}

	[ExpectNoExceptions]
	public void TestZG_AuthorisationNumber()
	{
		NUnit.Framework.Assert.That(delegate
		{ declaration.ZG_AuthorisationNumber = "12345678"; }, CustomConstraints.InnermostExceptionThrown(typeof(MaxLengthExceededException)));
		ErrorReporter.Clear();
	}

	[ExpectNoExceptions]
	public void TestJE_LocationOfGoods()
	{
		NUnit.Framework.Assert.That(delegate
		{ declaration.JE_LocationOfGoods = "123456789"; }, CustomConstraints.InnermostExceptionThrown(typeof(MaxLengthExceededException)));
		ErrorReporter.Clear();
	}

	public void TestCheckJE_LocationOfGoods_MaxLength()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";

			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("Import declation, authorisationNumber and locationQualifier empty, JE_LocationOfGoods max length", 8, declaration.JE_LocationOfGoodsInfo.MaxLength);

			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			AssertEquals("Import declaration, authorisationNumber empty and locationQualifier is FC, JE_LocationOfGoods max length", 20, declaration.JE_LocationOfGoodsInfo.MaxLength);

			declaration.ZG_AuthorisationNumber = "12345";
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			AssertEquals("Import declaration, authorisationNumber is present and locationQualifier is FC, JE_LocationOfGoods max length", 8, declaration.JE_LocationOfGoodsInfo.MaxLength);

			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit;
			AssertEquals("IMP, authorisationNumber empty and locationQualifier is F, JE_LocationOfGoods max length", 8, declaration.JE_LocationOfGoodsInfo.MaxLength);

			declaration.JE_MessageType = "EXP";
			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			AssertEquals("Export declaration, authorisationNumber empty and locationQualifier is FC, JE_LocationOfGoods max length", 8, declaration.JE_LocationOfGoodsInfo.MaxLength);
		});
	}

	public void TestGoodsLocationAddress()
	{
		AssertType<JobDocAddress>(declaration.GoodsLocationAddress);
	}

	public void TestDocumentSupport()
	{
		AssertType<JobDeclarationDocumentSupporter>(declaration.DocumentSupporter);
	}

	public void TestGetNewLookups()
	{
		AssertType<JobDeclarationLookups>("Enterprise.Customs.IT.Business.Declaration.JobDeclarationLookups", declaration.Lookups);
	}

	public void TestGetNewValidation()
	{
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationValidation>(declaration.Validation);
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationValidation>(declaration.Validation);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertType<Ucc6ExportJobDeclarationValidation>(declaration.Validation);
		}
		declaration.JE_MessageType = "";
		AssertType<CommonJobDeclarationValidation>(declaration.Validation);
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<CommonJobDeclarationValidation>(declaration.Validation);
	}

	public override void TestAutoRating()
	{
		Assert("Italy Autorating works but the entry charge type list defined for Germany does not match the standard EU tax type codes (box 47a)", true);
	}

	public override void TestCustomsOfficeRequirementHelper()
	{
		AssertType<JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
	}

	public void TestZG_PreClearing()
	{
		AssertEquals("ZG_PreClearing default value", ZBool.False, declaration.ZG_PreClearing);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		declaration.ZG_PreClearing = ZBool.True;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("When JE_MessageType is EXP and JE_TransportMode is SEA, ZG_PreClearing", ZBool.False, declaration.ZG_PreClearing);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.ZG_PreClearing = ZBool.True;
		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		AssertEquals("When JE_MessageType is IMP and JE_TransportMode is AIR, ZG_PreClearing", ZBool.False, declaration.ZG_PreClearing);
	}

	public void TestIsPreClearingEditable()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		AssertEquals("When JE_MessageType is IMP and JE_TransportMode is SEA, PreClearingReadOnly", ZBool.True, declaration.IsPreClearingEditable);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("When JE_MessageType is EXP and JE_TransportMode is SEA, PreClearingReadOnly", ZBool.False, declaration.IsPreClearingEditable);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		AssertEquals("When JE_MessageType is IMP and JE_TransportMode is AIR, PreClearingReadOnly", ZBool.False, declaration.IsPreClearingEditable);
	}

	public void TestDefaultNodeFromDeclarant()
	{
		var declarantWithNoNodes = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithNoNodes.OH_Code = "AA";
		var declarantWithOneNode = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithOneNode.OH_Code = "BB";
		var declarantWithTwoNodes = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithTwoNodes.OH_Code = "CC";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "XXXX").AppendAccountDetail("XXXX-BB", "BB")
			.AppendAccount("22222222222-001", "YYYY").AppendAccountDetail("YYYY-CC", "CC")
			.AppendAccount("33333333333-001", "ZZZZ").AppendAccountDetail("ZZZZ-CC", "CC")
			.Build();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = declarantWithNoNodes.MainAddress.PK;
		AssertEquals("Node has not been defaulted", "", declaration.JE_CustomsProfile);

		declaration.JE_OA_DeclarantAddress = declarantWithTwoNodes.MainAddress.PK;
		AssertEquals("Node has not been defaulted", "", declaration.JE_CustomsProfile);

		declaration.JE_OA_DeclarantAddress = declarantWithOneNode.MainAddress.PK;
		AssertEquals("Node has been defaulted", "XXXX-BB", declaration.JE_CustomsProfile);

		declaration.JE_CustomsProfile = "1234";
		declaration.JE_OA_DeclarantAddress = declarantWithNoNodes.MainAddress.PK;
		AssertEquals("Node has been set to empty", "", declaration.JE_CustomsProfile);

		declaration.JE_CustomsProfile = "1234";
		declaration.JE_OA_DeclarantAddress = declarantWithTwoNodes.MainAddress.PK;
		AssertEquals("Node has been set to empty", "", declaration.JE_CustomsProfile);
	}

	public void TestApportionedDocumentCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var apportionedDocuments = declaration.ApportionedDocumentCollection;
		AssertNotNull("Dictionary item for entryLine1 not null", apportionedDocuments[entryLine1.PK.ToString()]);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		apportionedDocuments = declaration.ApportionedDocumentCollection;
		AssertExceptionThrown<ArgumentException>("Valid ZGuid, but not registered to an entry line", () => { _ = apportionedDocuments[entryLine2.PK.ToString()]; });

		declaration.ResetApportionedPreviousDocuments();
		apportionedDocuments = declaration.ApportionedDocumentCollection;
		AssertNotNull("Dictionary item for entryLine2 not null", apportionedDocuments[entryLine2.PK.ToString()]);
	}

	[TestDate(2020, 6, 5)]
	public override void TestResetValuesOnTemplateCopyAfterClone()
	{
		base.TestResetValuesOnTemplateCopyAfterClone();

		var declaration = Factory.New<JobDeclaration>();

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_DateForDuty = new ZDateTime(2016, 09, 13);

		var clonedDeclaration = declaration.TemplateCopy() as JobDeclaration;
		var clonedInstruction = clonedDeclaration.CustomsEntryInstructions[0];

		AssertNotEquals("Reset CEI_DateForDuty to today", instruction.CEI_DateForDuty, clonedInstruction.CEI_DateForDuty);
		AssertEquals("Reset CEI_DateForDuty to today", new ZDateTime(2020, 6, 5), clonedInstruction.CEI_DateForDuty);
	}

	public void TestMeansOfTransportCrossingBorderIdentity()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		declaration.JE_VesselName = "VESSEL";
		declaration.JE_VoyageFlightNo = "1234";
		AssertEquals("MeansOfTransportCrossingBorderIdentity when TransportMode is Sea", "VESSEL1234", declaration.MeansOfTransportCrossingBorderIdentity);

		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		declaration.JE_VesselName = "VESSEL";
		declaration.JE_VoyageFlightNo = "1234";
		AssertEquals("MeansOfTransportCrossingBorderIdentity when TransportMode is Air", "1234", declaration.MeansOfTransportCrossingBorderIdentity);

		declaration.JE_TransportMode = TransportTypeList.Codes.Road;
		declaration.JE_VesselName = "TRUCK";
		declaration.JE_VoyageFlightNo = "1234";
		AssertEquals("MeansOfTransportCrossingBorderIdentity when TransportMode is Road", "TRUCK", declaration.MeansOfTransportCrossingBorderIdentity);
	}

	public void TestZG_BorderTransportMeans_ResStringData()
	{
		CombineAssertions("Res String Data, ZG_BorderTransportMeans", () =>
		{
			var resData = declaration.ZG_BorderTransportMeansInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Caption", "[21] Code", resData.Caption);
			AssertEquals("ShortCaption", "[21] Code", resData.ShortCaption);
			AssertEquals("MediumCaption", "[21] Border Transport Code", resData.MediumCaption);
			AssertEquals("FullDescription", "[21] Border Transport Code 19 08 061 000", resData.FullDescription);
		});
	}

	public void TestSetDefaultTransportMeansIfRequired()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			CombineAssertions(() =>
			{
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "AIR", expectedDefaultTransportMeans: ZString.Empty);
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "RAI", expectedDefaultTransportMeans: ZString.Empty);
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "ROA", expectedDefaultTransportMeans: ZString.Empty);
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "FIX", expectedDefaultTransportMeans: ZString.Empty);
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "IWT", expectedDefaultTransportMeans: "81");
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "OWN", expectedDefaultTransportMeans: ZString.Empty);
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "MAI", expectedDefaultTransportMeans: ZString.Empty);
				AssertTransportMeansDefaultingForTransportModeInland(transportModeInland: "SEA", expectedDefaultTransportMeans: "11");
			});
		}

		void AssertTransportMeansDefaultingForTransportModeInland(ZString transportModeInland, ZString expectedDefaultTransportMeans)
		{
			declaration.JE_TransportMeans = "10";
			declaration.JE_TransportModeInland = transportModeInland;
			AssertEquals($"When JE_TransportModeInland set to {transportModeInland}, JE_TransportMeans", expectedDefaultTransportMeans, declaration.JE_TransportMeans);
		}
	}

	public void TestDefaultSubscriberFromNode()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var staff1 = Factory.NewWithValidTestData<GlbStaff>();
		staff1.GS_Code = "ST1";
		staff1.GS_FullName = "STAFF1 FULL NAME";
		var staff1Wrapper = GlbStaffWrapper.Get(staff1);
		staff1Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";

		var staff2 = Factory.NewWithValidTestData<GlbStaff>();
		staff2.GS_Code = "ST2";
		staff2.GS_FullName = "STAFF2 FULL NAME";
		var staff2Wrapper = GlbStaffWrapper.Get(staff2);
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "5678";

		var staff3 = Factory.NewWithValidTestData<GlbStaff>();
		staff3.GS_Code = "ST3";
		staff3.GS_FullName = "STAFF3 FULL NAME";
		var staff3Wrapper = GlbStaffWrapper.Get(staff3);
		staff3Wrapper.PasswordCollection.AddNew().GP_UserID = "9876";

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("11111111111-002", "5678").AppendAccountDetail("5678-DEC1", "DEC1")
			.AppendAccount("11111111111-003", "9876").AppendAccountDetail("9876-DEC1", "DEC1")
			.Build();

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions("IsUCC6 = true", () =>
		{
			declaration.JE_MessageType = "IMP";
			AssertEquals("Pre-Cond: IsUCC6", true, declaration.IsUCC6);

			declaration.JE_CustomsProfile = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);

			declaration.JE_CustomsProfile = "9999";
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);

			declaration.JE_CustomsProfile = "1234-DEC1";
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);

			declaration.JE_CustomsProfile = "5678-DEC1";
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);

			declaration.JE_CustomsProfile = "8888";
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);

			declaration.JE_CustomsProfile = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);

			declaration.JE_CustomsProfile = "1234-DEC1";
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);

			declaration.JE_CustomsProfile = "9876-DEC1";
			AssertEquals(ZString.Empty, declaration.JE_GS_NKCusAgent);
		});
	}

	public void TestCountryOfDestinationCode()
	{
		var refUNLOCO = Factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "FIN";
		refUNLOCO.RL_RN_NKCountryCode = "IT";

		declaration.JE_RL_NKFinalDestination = "FIN";
		AssertEquals(declaration.JE_GoodsDestination, declaration.CountryOfDestinationCode);

		declaration.JE_RL_NKFinalDestination = "XXX";
		AssertEquals(declaration.JE_GoodsDestination, declaration.CountryOfDestinationCode);

		declaration.JE_RL_NKFinalDestination = "";
		AssertEquals(ZString.Empty, declaration.CountryOfDestinationCode);
	}

	public void TestNeedsPortTax()
	{
		var declaration = Factory.New<JobDeclaration>();

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		declaration.JE_MessageType = "IMP";
		declaration.JE_TransportMode = "SEA";
		declaration.JE_RL_NKPortOfArrival = "ITCEJ";
		AssertEquals("NeedsPortTax", true, declaration.NeedsPortTax);

		declaration.JE_RL_NKPortOfArrival = "XXXXX";
		AssertEquals("NeedsPortTax", false, declaration.NeedsPortTax);

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		AssertEquals("NeedsPortTax", false, declaration.NeedsPortTax);

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		declaration.JE_TransportMode = "AIR";
		AssertEquals("NeedsPortTax", false, declaration.NeedsPortTax);

		declaration.JE_TransportMode = "SEA";
		declaration.JE_RL_NKPortOfArrival = "ITTRS";
		AssertEquals("NeedsPortTax", false, declaration.NeedsPortTax);

		declaration.JE_RL_NKPortOfArrival = "";
		AssertEquals("NeedsPortTax", false, declaration.NeedsPortTax);

		declaration.JE_MessageType = "EXP";
		declaration.JE_RL_NKPortOfArrival = "ITCEJ";
		declaration.JE_RL_NKPortOfLoading = "ITTRS";
		AssertEquals("NeedsPortTax", false, declaration.NeedsPortTax);

		declaration.JE_RL_NKPortOfLoading = "ITCEJ";
		AssertEquals("NeedsPortTax", true, declaration.NeedsPortTax);

		declaration.JE_RL_NKPortOfLoading = "";
		AssertEquals("NeedsPortTax", false, declaration.NeedsPortTax);

		declaration.JE_RL_NKPortOfLoading = "ITCEJ";
		var officeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>().Single(x => x.CY_Code == "EXT");
		officeOfExit.CY_Data = "IT123456";
		declaration.JE_CustomsOffice = "IT123456";
		AssertEquals("When a EXP Declaration has Customs Office of Presentation = Customs Office of Exit, NeedsPortTax", true, declaration.NeedsPortTax);
		declaration.JE_CustomsOffice = "IT654321";
		AssertEquals("When a EXP Declaration has Customs Office of Presentation <> Customs Office of Exit, NeedsPortTax", false, declaration.NeedsPortTax);

		declaration.JE_MessageType = "IMP";
		AssertEquals("When a IMP Declaration has Customs Office of Presentation = Customs Office of Exit, NeedsPortTax", true, declaration.NeedsPortTax);
	}

	public void TestBarrierPortUNLOCO()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_RL_NKPortOfArrival = "ITCEJ";
		AssertNotNull("Actual Port should be not null", declaration.BarrierPortUNLOCO);
		AssertEquals("ActualPort code", "ITCEJ", declaration.BarrierPortUNLOCO.Code);

		declaration.JE_RL_NKPortOfArrival = "XXX";
		AssertNull("ActualPort should be null", declaration.BarrierPortUNLOCO);

		declaration.JE_MessageType = "EXP";
		declaration.JE_RL_NKPortOfLoading = "ITTRS";
		AssertNotNull("Actual Port should be not null", declaration.BarrierPortUNLOCO);
		AssertEquals("ActualPort code", "ITTRS", declaration.BarrierPortUNLOCO.Code);

		declaration.JE_RL_NKPortOfLoading = "XXXXX";
		AssertNull("ActualPort should be null", declaration.BarrierPortUNLOCO);
	}

	public void TestVATDeferStrategy()
	{
		AssertType<VATDeferStrategy>("VATDeferStrategy type", declaration.VATDeferStrategy);
	}

	public void TestApplyEntryLineDefaultLogicForC100SupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var supplierWithRex = Factory.New<OrgHeader>();
		var rexNumber = supplierWithRex.CustomsCodes.AddNew();
		rexNumber.OK_CodeType = "REX";
		rexNumber.OK_CustomsRegNo = "IEREX12345AB";
		declaration.JE_OH_Supplier = supplierWithRex.PK;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_PrimaryPreference = "200";
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_PrimaryPreference = "220";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;

		declaration.ApplyEntryLineDefaultLogicForC100SupportingDocuments();

		CombineAssertions("Assert Entry Lines and Invoice Lines have C100 Sup", () =>
		{
			AssertEquals("C100 Supporting Document in Entry Line 1 is expected", true, entryLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C100"));
			AssertEquals("C100 Supporting Document in Invoice Line 1 is expected", true, invoiceLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C100"));

			AssertEquals("C100 Supporting Document in Entry Line 2 is expected", true, entryLine2.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C100"));
			AssertEquals("C100 Supporting Document in Invoice Line 2 is expected", true, invoiceLine2.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C100"));
		});
	}

	public void TestSupplierRexCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("When Supplier is empty, SupplierRexCode", "", declaration.SupplierRexCode);

		var supplierWithoutRexCode = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplierWithoutRexCode.PK;
		AssertEquals("When Supplier has not the Rex Code, SupplierRexCode", "", declaration.SupplierRexCode);

		var supplirtWithRexCode = Factory.New<OrgHeader>();
		var rexNumber = supplirtWithRexCode.CustomsCodes.AddNew();
		rexNumber.OK_CodeType = "REX";
		rexNumber.OK_CustomsRegNo = "IEREX12345AB";
		declaration.JE_OH_Supplier = supplirtWithRexCode.PK;
		AssertEquals("When Supplier has the Rex Code, SupplierRexCode", "IEREX12345AB", declaration.SupplierRexCode);
	}

	public void TestAddMissingSupportingDocumentsForThoseInvoiceLinesHaveSameCondition()
	{
		MissingSupportingDocumentParentTest.SetupRefCusConditionValueForMissingSupportingDocumentImportTest(Factory);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();

		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		var invoiceLine3 = invoice2.InvoiceLines.AddNew();

		invoiceLine1.JI_Tariff = "800900";
		invoiceLine1.JI_SupplementaryCode1 = "Q001";
		invoiceLine2.JI_Tariff = "800900";
		invoiceLine2.JI_SupplementaryCode2 = "Q001";
		invoiceLine3.JI_Tariff = "800900";
		invoiceLine3.JI_SupplementaryCode1 = "Q051";

		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine2);

		var allMissingSupportingDocuments = missingSupportingDocumentParent.MissingSupportingDocumentGrouped.SelectMany(x => x.MissingSupportingDocuments);
		allMissingSupportingDocuments.SingleOrDefault(x => x.Code == "XB").ShouldImport = true;
		allMissingSupportingDocuments.SingleOrDefault(x => x.Code == "YA").ShouldImport = true;

		declaration.AddMissingSupportingDocumentsForThoseInvoiceLinesHaveSameCondition(missingSupportingDocumentParent.GetMissingSupportingDocumentsToImport(), invoiceLine2.JI_Tariff, invoiceLine2.ConditionSelectionCriterias.FirstOrDefault());

		AssertEquals("Invoice Line 1 Supporting documents count", 2, invoiceLine1.SupportingDocuments.Count);
		CombineAssertions("Check supporting documents in Invoice Lines Line 1", () =>
		{
			var invoiceLine1SupportingDocumentCodes = invoiceLine1.SupportingDocuments.Cast<SupportingDocument>().Select(x => x.CSI_Code).ToArray();
			AssertCollectionContains("XB", invoiceLine1SupportingDocumentCodes);
			AssertCollectionContains("YA", invoiceLine1SupportingDocumentCodes);
		});

		AssertEquals("Invoice Line 2 Supporting documents count", 2, invoiceLine2.SupportingDocuments.Count);
		CombineAssertions("Check supporting documents in Invoice Lines Line 1", () =>
		{
			var invoiceLine2SupportingDocumentCodes = invoiceLine2.SupportingDocuments.Cast<SupportingDocument>().Select(x => x.CSI_Code).ToArray();
			AssertCollectionContains("XB", invoiceLine2SupportingDocumentCodes);
			AssertCollectionContains("YA", invoiceLine2SupportingDocumentCodes);
		});

		AssertEquals("Invoice Line 3 Supporting documents count", 0, invoiceLine3.SupportingDocuments.Count);
	}

	public void TestZG_CTStatusID_GetsClearedIfImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ZG_CTStatusID = "XX";

		declaration.JE_MessageType = ZString.Empty;
		AssertEquals("Setting empty should not clear", "XX", declaration.ZG_CTStatusID);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		AssertEquals("Setting EXP should not clear", "XX", declaration.ZG_CTStatusID);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertEquals("Setting IMP should clear", "", declaration.ZG_CTStatusID);
	}

	public void TestZG_CTStatusIDResourceStringData()
	{
		var resourceStringData = declaration.ZG_CTStatusIDInfo.GetAttribute<ResourceStringDataAttribute>();
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "[1c] CT Status", resourceStringData.Caption);
		});
	}

	public void TestAuthorisation()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "2222222", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisation.PK, "3333333", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertNull("Empty ZG_AuthorisationNumber", declaration.Authorization);

		declaration.ZG_AuthorisationNumber = "123456";
		AssertNull("Invalid ZG_AuthorisationNumber", declaration.Authorization);

		declaration.ZG_AuthorisationNumber = "1111111";
		AssertNull("Valid ZG_AuthorisationNumber, but other needed data not filled", declaration.Authorization);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		declaration.JE_OH_Supplier = organisation.PK;

		AssertNotNull("EXP/ALE Supplier, Authorization", declaration.Authorization);
		AssertEquals("EXP/ALE Supplier, CPH_Number", "1111111", declaration.Authorization.CPH_Number);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisation.PK;

		AssertNotNull("IMP/ALE Importer, Authorization", declaration.Authorization);
		AssertEquals("IMP/ALE Importer, CPH_Number", "1111111", declaration.Authorization.CPH_Number);

		declaration.ZG_AuthorisationNumber = "3333333";
		AssertNull("'3333333' DOI authorisation not valid", declaration.Authorization);

		declaration.ZG_AuthorisationNumber = "2222222";

		AssertNotNull("IMP/ALI Importer, Authorization", declaration.Authorization);
		AssertEquals("IMP/ALI Importer, CPH_Number", "2222222", declaration.Authorization.CPH_Number);
	}

	public void TestDeclarationOfIntentAuthorisation()
	{
		var organisationWithDOI = Factory.New<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisationWithDOI.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("[PRE-CONDITION]", ZGuid.Empty, declaration.JE_OH_Importer);
		AssertNull("Empty Importer", declaration.DeclarationOfIntentAuthorisation);

		declaration.JE_OH_Importer = organisationWithDOI.PK;
		CombineAssertions("Importer with DOI", () =>
		{
			var declarationOfIntent = declaration.DeclarationOfIntentAuthorisation;
			AssertNotNull(declarationOfIntent);
			AssertEquals("1111111", declarationOfIntent.CPH_Number);
		});

		var organisationWithoutDOI = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = organisationWithoutDOI.PK;
		AssertNull("Importer without DOI", declaration.DeclarationOfIntentAuthorisation);
	}

	public void TestDeclarationOfIntentRefresher()
	{
		AssertNotNull(declaration.DeclarationOfIntentRefresher);
	}

	public void TestSetJE_MessageTypeClearZG_UseDeclarationOfIntent()
	{
		var organisationWithDOI = Factory.New<OrgHeader>();
		var doiAuthorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisationWithDOI.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = entryInstruction2.ZG_UseDeclarationOfIntent = true;

		var authorisationRule = doiAuthorisation.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Use;
		authorisationRule.CPR_ValueFrom = CusAuthorisationRuleUseValueList.Codes.Always;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		CombineAssertions("When setting JE_MessageType = 'IMP', ZG_UseDeclarationOfIntent is not cleared", () =>
		{
			Assert(entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		declaration.JE_MessageType = "XXX";
		CombineAssertions("When setting JE_MessageType != 'IMP', ZG_UseDeclarationOfIntent is cleared", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});
	}

	public void TestSetJE_MessageTypeDefaultZG_UseDeclarationOfIntent()
	{
		var organisationWithDOI = Factory.New<OrgHeader>();
		var doiAuthorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisationWithDOI.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		var authorisationRule = doiAuthorisation.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Use;
		authorisationRule.CPR_ValueFrom = CusAuthorisationRuleUseValueList.Codes.Always;
		declaration.JE_OH_Importer = organisationWithDOI.PK;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		CombineAssertions("IMP and 'USE - Always' rule", () =>
		{
			Assert(entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(entryInstruction2.ZG_UseDeclarationOfIntent);
		});
	}

	public void TestSetJE_OH_ImporterDefaultZG_UseDeclarationOfIntent()
	{
		var organisationWithDOI = Factory.New<OrgHeader>();
		var doiAuthorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisationWithDOI.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		CombineAssertions("IMP and no 'USE' rule", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		var authorisationRule = doiAuthorisation.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Use;
		authorisationRule.CPR_ValueFrom = CusAuthorisationRuleUseValueList.Codes.Never;
		EmulatePropertyChange(declaration.JE_OH_ImporterInfo);
		CombineAssertions("IMP and 'USE - Never' rule", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		authorisationRule.CPR_ValueFrom = CusAuthorisationRuleUseValueList.Codes.Always;
		EmulatePropertyChange(declaration.JE_OH_ImporterInfo);
		CombineAssertions("IMP and 'USE - Always' rule", () =>
		{
			Assert(entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		declaration.JE_MessageType = "XXX";
		EmulatePropertyChange(declaration.JE_OH_ImporterInfo);
		CombineAssertions("XXX and 'USE - Always' rule", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});
	}

	void EmulatePropertyChange(ZPropertyInfo propertyInfo)
	{
		var currentValue = propertyInfo.Value;
		propertyInfo.ClearValue();
		propertyInfo.Value = currentValue;
	}

	public void TestSetJE_LocationOfGoodsDefaultJE_CustomsOffice()
	{
		void EmulateGoodsLocationChange(ZPropertyInfo propertyInfo)
		{
			var currentValue = propertyInfo.Value;
			propertyInfo.ClearValue();
			propertyInfo.Value = currentValue;
		}

		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var authorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisation.PK;
		declaration.ZG_AuthorisationNumber = "1111111";
		AssertEquals("Authorisation has no rules", ZString.Empty, declaration.JE_CustomsOffice);

		var authorisationRule = authorisation.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorisationRule.CPR_ValueFrom = "123456A";
		declaration.JE_LocationOfGoods = "123456A";
		AssertEquals("'LOC' rule has no linked rules", ZString.Empty, declaration.JE_CustomsOffice);

		var authorisationLinkedRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
		authorisationLinkedRule.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		authorisationLinkedRule.CPR_ValueFrom = "IT137100";
		EmulateGoodsLocationChange(declaration.JE_LocationOfGoodsInfo);
		AssertEquals("'LOC' rule has 1 'CUS' linked rule", "IT137100", declaration.JE_CustomsOffice);

		declaration.JE_CustomsOffice = "IT000000";
		EmulateGoodsLocationChange(declaration.JE_LocationOfGoodsInfo);
		AssertEquals("'LOC' rule has 1 'CUS' linked rule, but JE_CustomsOffice is already set", "IT000000", declaration.JE_CustomsOffice);
	}

	public void TestSupplierTraderId()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Empty supplier", ZString.Empty, declaration.SupplierTraderId);

		var organization = Factory.New<OrgHeader>();
		organization.OH_Category = "NAT";
		declaration.JE_OH_Supplier = organization.PK;
		var customsCode = organization.CustomsCodes.AddNew();
		customsCode.OK_CodeType = "EOR";
		customsCode.OK_RN_NKCodeCountry = "DE";
		customsCode.OK_CustomsRegNo = "385040449";
		AssertEquals("DE385040449", declaration.SupplierTraderId);
	}

	public void TestImporterTraderId()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Empty importer", ZString.Empty, declaration.ImporterTraderId);

		var organization = Factory.New<OrgHeader>();
		organization.OH_Category = "NAT";
		declaration.JE_OH_Importer = organization.PK;
		var customsCode = organization.CustomsCodes.AddNew();
		customsCode.OK_CodeType = "EOR";
		customsCode.OK_RN_NKCodeCountry = "DE";
		customsCode.OK_CustomsRegNo = "385040449";
		AssertEquals("DE385040449", declaration.ImporterTraderId);
	}

	public void TestSetJE_MessageTypeResetOrDefaultParticipantType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = ZString.Empty;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals($"{nameof(entryInstruction1)}", ZString.Empty, entryInstruction1.ZG_ParticipantType);
			AssertEquals($"{nameof(entryInstruction2)}", ZString.Empty, entryInstruction2.ZG_ParticipantType);
		});

		CombineAssertions("Default for JE_MessageType = EXP", () =>
		{
			entryInstruction1.ZG_ParticipantType = ZString.Empty;
			entryInstruction2.ZG_ParticipantType = ZString.Empty;
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals($"{nameof(entryInstruction1)}", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, entryInstruction1.ZG_ParticipantType);
			AssertEquals($"{nameof(entryInstruction2)}", ParticipantTypeList.Codes.StandardOneSupplierOneImporter, entryInstruction2.ZG_ParticipantType);
		});

		CombineAssertions("Reset for JE_MessageType IS NOT EXP", () =>
		{
			entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.StandardOneSupplierOneImporter;
			entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.StandardOneSupplierOneImporter;
			declaration.JE_MessageType = "ZZZ";
			AssertEquals($"{nameof(entryInstruction1)}", ZString.Empty, entryInstruction1.ZG_ParticipantType);
			AssertEquals($"{nameof(entryInstruction2)}", ZString.Empty, entryInstruction2.ZG_ParticipantType);
		});
	}

	public void TestICustomsProfileDataProviderMembers()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_CustomsProfile = "1234";
		CombineAssertions(() =>
		{
			var declarationAsICustomsProfileDataProvider = (ICustomsProfileDataProvider)declaration;
			AssertEquals("CustomsProfile", declaration.JE_CustomsProfile, declarationAsICustomsProfileDataProvider.CustomsProfile);
		});
	}

	public void TestPerformOnGetEntryToPrintSadHC88()
	{
		var declaration = Factory.New<JobDeclaration>();

		AssertNoExceptionThrown("No exception expected when No delegates are attached", () => declaration.PerformOnGetEntryToPrintSadHC88());

		var onGetEntryToPrintSadHC88HasBeenFired = false;
		declaration.OnGetEntryToPrintSadHC88 += (s, e) =>
		{
			onGetEntryToPrintSadHC88HasBeenFired = true;
			e.Cancel = true;
		};

		var eventArgsResult = declaration.PerformOnGetEntryToPrintSadHC88();

		CombineAssertions("Assert OnGetEntryToPrintSadHC88 has been fired", () =>
		{
			AssertEquals("OnGetEntryToPrintSadHC88 has been fired", true, onGetEntryToPrintSadHC88HasBeenFired);
			AssertEquals("EventArgs Cancel", true, eventArgsResult.Cancel);
		});
	}

	public void TestIDeclarantProviderMembers()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		declaration.JE_DeclarantType = "S";

		CombineAssertions("Assert IDeclarantProviderMembers", () =>
		{
			var declarationAsIDeclarantProvider = (IDeclarantProvider)declaration;
			AssertEquals("RepresentativeType", "S", declarationAsIDeclarantProvider.RepresentativeType);
			AssertNotNull("DeclarantAddress", declarationAsIDeclarantProvider.DeclarantAddress);
			AssertSame("DeclarantAddress", declaration.DeclarantAddress, declarationAsIDeclarantProvider.DeclarantAddress);
		});
	}

	public void TestAeoCertificateSupporter()
	{
		var aeoCertificateSupporter = declaration.AeoCertificateSupporter;
		AssertNotNull("AeoCertificateSupporter must be never null", aeoCertificateSupporter);
		AssertSame("AeoCertificateSupporter must be cached", aeoCertificateSupporter, declaration.AeoCertificateSupporter);
	}

	public void TestIAutHeaderWithCusOfficeProviderMembers()
	{
		var declarationAsIAutHeaderWithCusOfficeProvider = (IAutHeaderWithCusOfficeProvider)declaration;

		AssertEquals("AuthorizationNumber", ZString.Empty, declarationAsIAutHeaderWithCusOfficeProvider.AuthorizationNumber);
		AssertNull("Authorization", declarationAsIAutHeaderWithCusOfficeProvider.Authorization);
		AssertEquals("IsExport is true when MessageType = Export", true, declarationAsIAutHeaderWithCusOfficeProvider.IsExport);

		declaration.ZG_AuthorisationNumber = "123456";
		AssertEquals("AuthorizationNumber", declaration.ZG_AuthorisationNumber, declarationAsIAutHeaderWithCusOfficeProvider.AuthorizationNumber);

		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertEquals("IsExport is false when MessageType = Import", false, declarationAsIAutHeaderWithCusOfficeProvider.IsExport);

		declaration.JE_OH_Importer = organisation.PK;
		declaration.ZG_AuthorisationNumber = "1111111";
		AssertEquals("Authorization", declaration.Authorization, declarationAsIAutHeaderWithCusOfficeProvider.Authorization);

		declaration.JE_CustomsOffice = "IT123456";
		AssertEquals("CustomsOffice", declaration.JE_CustomsOffice, declarationAsIAutHeaderWithCusOfficeProvider.CustomsOffice);
	}

	public void TestCheckJE_LocationQualifierMaxLength()
	{
		AssertEquals(2, declaration.JE_LocationQualifierInfo.MaxLength);
	}

	public void TestCustomsOffices()
	{
		AssertNotNull("CustomsOffice", declaration.CustomsOffices);
	}

	public override void TestGetCusCodeDataType()
	{
		AssertEquals(typeof(OfficeCode), ((ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.OfficeCode]);
	}

	public void TestShimpmentSyncronizer()
	{
		var shipment = Factory.New<ForwardingShipment>();
		declaration.JE_JS = shipment.PK;

		AssertType<JobDeclarationSynchroniser>("ShipmentSynchroniser", declaration.ShipmentSynchroniser);
	}

	public void TestJE_OH_BuyerResourceStringData()
	{
		var resourceStringData = declaration.JE_OH_BuyerInfo.GetAttribute<ResourceStringDataAttribute>();
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Buyer", resourceStringData.Caption);
			AssertEquals("FullDescription", "Buyer's name 3/26", resourceStringData.FullDescription);
		});
	}

	public void TestMessageTypeChangeTriggersClearDeclarationFields()
	{
		var declarationMock = Factory.New<DummyJobDeclaration_GetNewJobDeclarationFieldsCleaner>();
		declarationMock.CleanUpMessageDependentFieldsTriggeredCount = 0;
		declarationMock.JE_MessageType = "XYZ";
		Assert("Must trigger CleanUpMessageDependentFieldsIfNoLongerApplicable at least once.", declarationMock.CleanUpMessageDependentFieldsTriggeredCount > 0);
	}

	public void TestMessageVersionChangeTriggersClearDeclarationFields()
	{
		var declarationMock = Factory.New<DummyJobDeclaration_GetNewJobDeclarationFieldsCleaner>();
		declarationMock.CleanUpMessageDependentFieldsTriggeredCount = 0;
		declarationMock.MessageVersion = "CCC";
		Assert("Must trigger CleanUpMessageDependentFieldsIfNoLongerApplicable at least once.", declarationMock.CleanUpMessageDependentFieldsTriggeredCount > 0);
	}

	public void TestTransportModeInlandChangeTriggersClearDeclarationFields()
	{
		var declarationMock = Factory.New<DummyJobDeclaration_GetNewJobDeclarationFieldsCleaner>();
		declarationMock.CleanUpTransportModeInlandDependentFieldsTriggeredCount = 0;
		declarationMock.JE_TransportModeInland = "ABC";
		Assert("Must trigger CleanUpTransportModeInlandDependentFieldsIfNoLongerApplicable at least once.", declarationMock.CleanUpTransportModeInlandDependentFieldsTriggeredCount > 0);
	}

	public void TestIsDefermentAccountNumberEqualDatCode()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_DefermentAccountNumber = "";
		AssertEquals("When JE_DefermentAccountNumber is Empty, IsDefermentAccountNumberEqualToDatCode()", false, declaration.IsDefermentAccountNumberEqualToDatCode());

		declaration.JE_DefermentAccountNumber = "123A";
		AssertEquals("When JE_DefermentAccountNumber is not Empty, IsDefermentAccountNumberEqualToDatCode()", false, declaration.IsDefermentAccountNumberEqualToDatCode());

		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		var importer = Factory.New<OrgHeader>();
		importer.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "DAT123", declaration.CountryCode);
		declaration.JE_OH_Importer = importer.PK;

		declaration.JE_DefermentAccountNumber = "DAT123";
		AssertEquals("When JE_DefermentAccountNumber is equal to Importer DAT Code, IsDefermentAccountNumberEqualToDatCode()", true, declaration.IsDefermentAccountNumberEqualToDatCode());
	}

	public void TestJE_DefermentAccountNumberMaxLengthNotDefaultForLookupTruncation()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("MaxLength", 35, declaration.JE_DefermentAccountNumberInfo.MaxLength);
	}

	public void TestNode()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		declaration.JE_CustomsProfile = "";
		AssertEquals("Node", "", declaration.Node);

		declaration.JE_CustomsProfile = "1111-DEC1";
		AssertEquals("Node", "1111", declaration.Node);

		declaration.JE_CustomsProfile = "XXXX";
		AssertEquals("Node", "", declaration.Node);
	}

	public void TestMessageVersionMaxLength()
	{
		AssertEquals("Max length", 3, declaration.MessageVersionInfo.MaxLength);
	}

	public void TestMessageVersionReadonly()
	{
		AssertMessageVersionReadonlyWithExportMessageVersionValue("TXT", true);
		AssertMessageVersionReadonlyWithExportMessageVersionValue("XML", true);
		AssertMessageVersionReadonlyWithExportMessageVersionValue("BTX", false);
		AssertMessageVersionReadonlyWithExportMessageVersionValue("BXM", false);

		void AssertMessageVersionReadonlyWithExportMessageVersionValue(string exportMessageVersion, bool expectedValue)
		{
			using (ITCustomsDataRegistry.Instance.ExportMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: exportMessageVersion))
			{
				AssertEquals($"Pre-Condi: For {exportMessageVersion} - {nameof(ITCustomsDataRegistry.Instance.IsExportMessageVersionEnabled)}", !expectedValue, ITCustomsDataRegistry.Instance.IsExportMessageVersionEnabled);
				AssertEquals($"For {exportMessageVersion} - MessageVersionReadonly", expectedValue, declaration.MessageVersionInfo.ReadOnly);
			}
		}
	}

	public void TestMessageVersionResourceStringData()
	{
		var resourceStringData = declaration.MessageVersionInfo.GetAttribute<ResourceStringDataAttribute>();
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Message Version", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Msg. Version", resourceStringData.ShortCaption);
		});
	}

	public void TestMessageVersionDefaulting()
	{
		AssertMessageVersionDefaultingWithExportMessageVersionValue("TXT", "TXT");
		AssertMessageVersionDefaultingWithExportMessageVersionValue("XML", "XML");
		AssertMessageVersionDefaultingWithExportMessageVersionValue("BTX", "TXT");
		AssertMessageVersionDefaultingWithExportMessageVersionValue("BXM", "XML");

		void AssertMessageVersionDefaultingWithExportMessageVersionValue(string exportMessageVersion, string expectedDefaultMessageVersion)
		{
			using (ITCustomsDataRegistry.Instance.ExportMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: exportMessageVersion))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "EXP";
				declaration.JE_ApplicationCode = "BLT";
				AssertEquals("MessageVersionDropEdit visibility", true, declaration.IsMessageVersionApplicable);
				AssertEquals($"For {exportMessageVersion} - MessageVersion", expectedDefaultMessageVersion, declaration.MessageVersion);
			}
		}
	}

	public void TestMessageVersionVisibilityAndClearing()
	{
		using (ITCustomsDataRegistry.Instance.ExportMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: "BXM"))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = "BLT";
			AssertEquals("MessageVersionDropEdit visibility", true, declaration.IsMessageVersionApplicable);
			AssertEquals("For EXP and Application code=BLT - MessageVersion defaults", "XML", declaration.MessageVersion);

			declaration.JE_MessageType = "IMP";
			AssertEquals("MessageVersionDropEdit visibility", false, declaration.IsMessageVersionApplicable);
			AssertEquals("For IMP - MessageVersion clears", "", declaration.MessageVersion);
		}
	}

	public void TestGetValueChangedAnnouncerType()
	{
		var invoicesProvider = declaration as IInvoicesProvider;
		AssertType<DeclarationValueChangedAnnouncer>("Type", invoicesProvider.GetValueChangedAnnouncer());
	}

	public void TestSetMessageVersionDefaultWhileSaving()
	{
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		using (ITCustomsDataRegistry.Instance.ExportMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: "BXM"))
		{
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("Pre: For EXP and Application code=BLT - MessageVersion defaults", "XML", declaration.MessageVersion);

			declaration.MessageVersion = ZString.Empty;
			Factory.Save();
			AssertEquals("For EXP and Application code=BLT - MessageVersion defaults after save", "XML", declaration.MessageVersion);

			declaration.JE_MessageType = "IMP";
			declaration.MessageVersion = ZString.Empty;
			Factory.Save();
			AssertEquals("For IMP, saving should not default Message version", "", declaration.MessageVersion);
		}
	}

	public void TestWipeSpecificCircumstanceIndicatorChangingMessageVersion()
	{
		declaration.JE_MessageType = "EXP";

		declaration.MessageVersion = "TXT";
		declaration.ZG_SpecificCircumstanceIndicator = "C";
		declaration.MessageVersion = "XML";
		AssertEquals(nameof(declaration.ZG_SpecificCircumstanceIndicator), "", declaration.ZG_SpecificCircumstanceIndicator);

		declaration.ZG_SpecificCircumstanceIndicator = "A20";
		declaration.MessageVersion = "";
		AssertEquals(nameof(declaration.ZG_SpecificCircumstanceIndicator), "", declaration.ZG_SpecificCircumstanceIndicator);
	}

	public void TestIsSecurityAllowed()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_MessageType = "EXP";

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			AssertEquals("For EntryStyle = CO, IsSecurityDeclarationCheckBox IsVisible", false, declaration.IsSecurityAllowed());

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			AssertEquals("For EntryStyle = EX, IsSecurityDeclarationCheckBox IsVisible", true, declaration.IsSecurityAllowed());
		}
	}

	public void TestAmendingReadOnlyFields()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions("When there are no entry headers", () =>
		{
			AssertEquals("JE_EntryStyleInfo.ReadOnl", false, declaration.JE_EntryStyleInfo.ReadOnly);
			AssertEquals("JE_CustomsOfficeInfo.ReadOnly", false, declaration.JE_CustomsOfficeInfo.ReadOnly);
		});

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_EntryStatus = "AMG";
		AssertReadOnlyFields(shouldBeReadOnly: true);

		entryHeader.CH_EntryStatus = "";
		AssertReadOnlyFields(shouldBeReadOnly: false);
		entryHeader.CH_EntryStatus = "AMD";
		AssertReadOnlyFields(shouldBeReadOnly: false);

		void AssertReadOnlyFields(bool shouldBeReadOnly)
		{
			var assertionMessage = shouldBeReadOnly
				? "When at least one Entry Header has status AMG"
				: "When no Entry Headers have status AMG";

			CombineAssertions(assertionMessage, () =>
			{
				AssertEquals("JE_EntryStyleInfo.ReadOnl", shouldBeReadOnly, declaration.JE_EntryStyleInfo.ReadOnly);
				AssertEquals("JE_CustomsOfficeInfo.ReadOnly", shouldBeReadOnly, declaration.JE_CustomsOfficeInfo.ReadOnly);
			});
		}
	}

	public void TestHasAtLeastOneEntryInAmendingStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("When Declaration has not entry headers", false, declaration.HasAtLeastOneEntryInAmendingStatus);

		declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("When Declaration has entry headers, but none is in Amending status ", false, declaration.HasAtLeastOneEntryInAmendingStatus);

		declaration.CustomsEntryHeaders.AddNew().CH_EntryStatus = "AMG";
		AssertEquals("When Declaration has entry headers and at least one is in Amending status ", true, declaration.HasAtLeastOneEntryInAmendingStatus);
	}

	public void TestJE_CustomsProfileResourceStringData()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_CustomsProfileInfo);
			AssertEquals("Caption", "Node", resourceStringData.Caption);

			var uccResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_CustomsProfileInfo, JobDeclaration.CaptionKeyUCC);
			AssertEquals("Caption", "Account", uccResourceStringData.Caption);

			var exportUccResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_CustomsProfileInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Account", exportUccResourceStringData.Caption);
		});
	}

	public void TestJE_VoyageFlightNoResourceStringData()
	{
		CombineAssertions(() =>
		{
			var airVoyageFlightNoResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_VoyageFlightNoInfo, JobDeclaration.CaptionKeyAirVoyageFlightNo);
			AssertEquals("Caption", "Flight", airVoyageFlightNoResourceStringData.Caption);

			var seaVoyageFlightNoResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_VoyageFlightNoInfo, JobDeclaration.CaptionKeySeaVoyageFlightNo);
			AssertEquals("Caption", "Voyage", seaVoyageFlightNoResourceStringData.Caption);
		});
	}

	public void TestJE_VesselNameResourceStringData()
	{
		CombineAssertions(() =>
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_VesselNameInfo);
			AssertEquals("Caption", "[21] Transport ID", resourceStringData.Caption);

			var uccResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_VesselNameInfo, JobDeclaration.CaptionKeySeaVesselName);
			AssertEquals("Caption", "[21] Vessel", uccResourceStringData.Caption);
		});
	}

	public void TestMultipleKeysToUse()
	{
		ISupportMultipleResourceStringData multipleResourceStringData = declaration;
		CombineAssertions("For AIR", () =>
		{
			declaration.JE_TransportMode = "AIR";
			AssertCollectionContains(JobDeclaration.CaptionKeyAirVoyageFlightNo, multipleResourceStringData.MultipleKeysToUse);
			AssertCollectionNotContains(JobDeclaration.CaptionKeySeaVoyageFlightNo, multipleResourceStringData.MultipleKeysToUse);
		});

		CombineAssertions("For SEA", () =>
		{
			declaration.JE_TransportMode = "SEA";
			AssertCollectionContains(JobDeclaration.CaptionKeySeaVesselName, multipleResourceStringData.MultipleKeysToUse);
			AssertCollectionContains(JobDeclaration.CaptionKeySeaVoyageFlightNo, multipleResourceStringData.MultipleKeysToUse);
			AssertCollectionNotContains(JobDeclaration.CaptionKeyAirVoyageFlightNo, multipleResourceStringData.MultipleKeysToUse);
		});

		CombineAssertions("For ROA", () =>
		{
			declaration.JE_TransportMode = "ROA";
			AssertCollectionNotContains(JobDeclaration.CaptionKeySeaVesselName, multipleResourceStringData.MultipleKeysToUse);
			AssertCollectionNotContains(JobDeclaration.CaptionKeySeaVoyageFlightNo, multipleResourceStringData.MultipleKeysToUse);
			AssertCollectionNotContains(JobDeclaration.CaptionKeyAirVoyageFlightNo, multipleResourceStringData.MultipleKeysToUse);
		});
	}

	public void TestIsUcc6ExportAndIsShipmentIncoTermOther()
	{
		declaration.JE_ShipmentIncoTerm = "XXX";

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("JE_MessageType=IMP, JE_ShipmentIncoTerm=XXX", false, declaration.IsUcc6ExportAndIsShipmentIncoTermOther);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertEquals("JE_MessageType=EXP, JE_ShipmentIncoTerm=XXX", false, declaration.IsUcc6ExportAndIsShipmentIncoTermOther);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("UCC6, JE_MessageType=EXP, JE_ShipmentIncoTerm=XXX", true, declaration.IsUcc6ExportAndIsShipmentIncoTermOther);

			declaration.JE_ShipmentIncoTerm = "FBO";
			AssertEquals("UCC6, JE_MessageType=EXP, JE_ShipmentIncoTerm=FBO", false, declaration.IsUcc6ExportAndIsShipmentIncoTermOther);
		}
	}

	public void TestZG_DeliveryTermsResourceStringData()
	{
		var declaration = Factory.New<JobDeclaration>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.ZG_AdditionalDeliveryTermsInfo);

		AssertNotNull("Additional Delivery Terms Resource String", resourceStringData);
		AssertEquals("Caption", "Delivery Terms", resourceStringData.Caption);
	}

	public void TestShipmentIncoTermChangeTriggersClearShipmentDeliveryFields()
	{
		var declarationMock = Factory.New<DummyJobDeclaration_GetNewJobDeclarationFieldsCleaner>();
		declarationMock.CleanUpShipmentDeliveryTermsFieldsTriggeredCount = 0;
		declarationMock.JE_ShipmentIncoTerm = "EXW";
		Assert("Must trigger CleanUpShipmentIncoTermFieldsIfNoLongerApplicable at least once.", declarationMock.CleanUpShipmentDeliveryTermsFieldsTriggeredCount > 0);
	}

	public void TestInventorySelectionHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<InventorySelectionHeader>("InventorySelectionHeader Type", declaration.InventorySelectionHeader);
	}

	public void TestDefaultPaymentMethodAndDefermentAccountNumberIfApplicable_ForRepresentative()
	{
		const string expectedPaymentMethod = "4";
		var factory = Factory;

		var representativeWithoutDPO = factory.NewWithValidTestData<OrgHeader>();
		representativeWithoutDPO.OH_Code = "REP0";

		var representativeWithOneDPO = factory.NewWithValidTestData<OrgHeader>();
		representativeWithOneDPO.OH_Code = "REP1";
		representativeWithOneDPO.CustomsCodes.AddNew("DAN", "4444", declaration.CountryCode);
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(factory, type: "DPO", permitHolder: representativeWithOneDPO.PK, "4444444", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var representativeWithMultiDPO = factory.NewWithValidTestData<OrgHeader>();
		representativeWithMultiDPO.OH_Code = "REP2";
		representativeWithMultiDPO.CustomsCodes.AddNew("DAN", "333", declaration.CountryCode);
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(factory, type: "DPO", permitHolder: representativeWithMultiDPO.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(factory, type: "DPO", permitHolder: representativeWithMultiDPO.PK, "2222222", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		factory.Save();

		CombineAssertions("When export and representative With One DPO has been entered", () =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OA_Representative = representativeWithOneDPO.MainAddress.PK;
			AssertNullOrEmpty(declaration.JE_PaymentMethod);
			AssertNullOrEmpty(declaration.JE_DefermentAccountNumber);
		});

		declaration.JE_OA_Representative = ZGuid.Empty;
		declaration.JE_PaymentMethod = ZString.Empty;
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

		CombineAssertions("When import and representative Without DPO has been entered", () =>
		{
			declaration.JE_OA_Representative = representativeWithoutDPO.MainAddress.PK;
			AssertEquals(expectedPaymentMethod, declaration.JE_PaymentMethod);
			AssertNullOrEmpty(declaration.JE_DefermentAccountNumber);
		});

		CombineAssertions("When import and representative With One DPO has been entered", () =>
		{
			declaration.JE_OA_Representative = representativeWithOneDPO.MainAddress.PK;
			AssertEquals(expectedPaymentMethod, declaration.JE_PaymentMethod);
			AssertEquals("4444444", declaration.JE_DefermentAccountNumber);
		});

		CombineAssertions("When import and representative With Multi DPO has been entered", () =>
		{
			declaration.JE_OA_Representative = representativeWithMultiDPO.MainAddress.PK;
			AssertEquals(expectedPaymentMethod, declaration.JE_PaymentMethod);
			AssertNullOrEmpty(declaration.JE_DefermentAccountNumber);
		});

		CombineAssertions("When import and Representative has not entered", () =>
		{
			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNullOrEmpty(declaration.JE_PaymentMethod);
			AssertNullOrEmpty(declaration.JE_DefermentAccountNumber);
		});
	}

	public void TestImporterDocumentaryAddressChanged_ApplyDeclarantAndRepresentativeDefaultingForImporter_WhenImporterIsCleared()
	{
		var factory = Factory;
		var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
		var representative = factory.NewWithValidTestData<OrgHeader>();
		representative.OH_Code = "REP";

		var declarantAddress = factory.NewWithValidTestData<OrgHeader>();
		declarantAddress.OH_Code = "DEC";

		var importer = factory.NewWithValidTestData<OrgHeader>();
		importer.OH_Code = "IMP";

		declaration.JE_OA_Representative = representative.MainAddress.PK;
		declaration.JE_OA_DeclarantAddress = declarantAddress.MainAddress.PK;
		CombineAssertions("When Export, nothing change", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			importerDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.MainAddress.PK;
			importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals(representative.MainAddress.PK, declaration.JE_OA_Representative);
			AssertEquals(declarantAddress.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		});

		CombineAssertions("When Import", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importerDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.MainAddress.PK;
			importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("clear ‘Representative’ field and ‘its address’", true, declaration.JE_OA_Representative_ZAddress.OrgPK.IsEmpty);
			AssertEquals("clear ‘Declarant’ field and ‘its address’", true, declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK.IsEmpty);
		});
	}

	public void TestImporterDocumentaryAddressChanged_ApplyDeclarantAndRepresentativeDefaultingForImporter_WhenImporterIsOrganizationProxy()
	{
		var orgProxyNotMainAddressPK = GlbCompany.CurrentCompany.OrgProxy.Addresses.Cast<OrgAddress>().FirstOrDefault(a => !a.IsMainAddress).PK;
		AssertNotNull("PRE-CONDITION", orgProxyNotMainAddressPK);

		AssertDeclarantAndRepresentativeDefaultingForImporter("Organization Proxy is Not main address", orgProxyNotMainAddressPK);

		AssertDeclarantAndRepresentativeDefaultingForImporter("Organization Proxy is main address", GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK);
	}

	public void TestImporterDocumentaryAddressChanged_ApplyDeclarantAndRepresentativeDefaultingForImporter_WhenImporterIsNotOrganizationProxy()
	{
		var orgProxyMainAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
		var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
		var importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_Code = "IMP";

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		importerDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

		CombineAssertions("When Export, nothing change", () =>
		{
			AssertEquals(true, declaration.JE_OA_Representative_ZAddress.OrgPK.IsEmpty);
			AssertEquals(true, declaration.JE_OA_DeclarantAddress.IsEmpty);
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		importerDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

		CombineAssertions("When Import, Application Code 'BLT'", () =>
		{
			AssertEquals("fill 'Representative' field and address with the 'company proxy' field code and address", orgProxyMainAddressPK, declaration.JE_OA_Representative);
			AssertEquals("fill 'Declarant' field and address with 'Importer' field code and address", importerDocumentaryAddress.E2_OA_Address, declaration.JE_OA_DeclarantAddress);
		});

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;

		importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		importerDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

		CombineAssertions("When Import, Application Code 'ITF'", () =>
		{
			AssertEquals("no change in 'Representative' field and address", true, declaration.JE_OA_Representative.IsEmpty);
			AssertEquals("no change in 'Declarant' field and address", true, declaration.JE_OA_DeclarantAddress.IsEmpty);
		});
	}

	public void TestSupplierDocumentaryAddressChanged_ApplyDeclarantAndRepresentativeDefaultingForSupplier_WhenSupplierIsOrganizationProxy()
	{
		var orgProxyNotMainAddressPK = GlbCompany.CurrentCompany.OrgProxy.Addresses.Cast<OrgAddress>().FirstOrDefault(a => !a.IsMainAddress).PK;
		AssertNotNull("PRE-CONDITION", orgProxyNotMainAddressPK);

		AssertDeclarantAndRepresentativeDefaultingForSupplier("Organization Proxy is Not main address", orgProxyNotMainAddressPK);

		AssertDeclarantAndRepresentativeDefaultingForSupplier("Organization Proxy is main address", GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK);
	}

	public void TestSupplierDocumentaryAddressChanged_ApplyDeclarantAndRepresentativeDefaultingForSupplier_WhenSupplierIsNotOrganizationProxy()
	{
		var orgProxyMainAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
		var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		supplier.OH_Code = "Sup";

		CombineAssertions("When Import, nothing change", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			AssertEquals(true, declaration.JE_OA_Representative_ZAddress.OrgPK.IsEmpty);
			AssertEquals(true, declaration.JE_OA_DeclarantAddress.IsEmpty);
		});

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		supplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

		CombineAssertions("When Export, Application Code 'BLT'", () =>
		{
			AssertEquals("fill 'Representative' field with the 'company proxy' field code and address", orgProxyMainAddressPK, declaration.JE_OA_Representative);
			AssertEquals("fill 'Declarant' field and address with 'Supplier' field code and address", supplierDocumentaryAddress.E2_OA_Address, declaration.JE_OA_DeclarantAddress);
		});

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;

		supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		supplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

		CombineAssertions("When Export, Application Code 'ITF'", () =>
		{
			AssertEquals("no change in 'Representative' field and address", true, declaration.JE_OA_Representative.IsEmpty);
			AssertEquals("no change in 'Declarant' field and address", true, declaration.JE_OA_DeclarantAddress.IsEmpty);
		});
	}

	public void TestSupplierDocumentaryAddressChanged_ApplyDeclarantAndRepresentativeDefaultingForSupplier_WhenSupplierIsCleared()
	{
		var factory = Factory;
		var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
		var supplier = factory.NewWithValidTestData<OrgHeader>();
		supplier.OH_Code = "Sup";
		var representative = factory.NewWithValidTestData<OrgHeader>();
		representative.OH_Code = "REP";
		var declarantAddress = factory.NewWithValidTestData<OrgHeader>();
		declarantAddress.OH_Code = "DEC";

		CombineAssertions("When Import, nothing change", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.MainAddress.PK;
			supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals(representative.MainAddress.PK, declaration.JE_OA_Representative);
			AssertEquals(declarantAddress.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		});

		CombineAssertions("When Export", () =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			supplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.MainAddress.PK;
			supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("clear ‘Representative’ field and ‘its address’", true, declaration.JE_OA_Representative_ZAddress.OrgPK.IsEmpty);
			AssertEquals("clear ‘Declarant’ field and ‘its address’", true, declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK.IsEmpty);
		});
	}

	public void TestSetDefaultValuesForDeclarantAndRepresentativeWithOrgProxy_OptOut()
	{
		AssertEquals("Default Declarant from 'Organization Proxy' is opt-out", true, declaration.JE_OA_DeclarantAddress.IsEmpty);
	}

	protected override bool IsDeclarantTypeExpectedToChangeWhenAssigningImporter => false;

	protected override ZString ExpectedDefaultDeclarantTypeForExport => "";

	protected override string GetLocalPortCode() => "ITMIL";

	protected override Hashtable ExpectedDocAddressTypes
	{
		get
		{
			if (expectedDocAddressTypes == null)
			{
				expectedDocAddressTypes = base.ExpectedDocAddressTypes;
				expectedDocAddressTypes.Add(DocAddressTypes.Codes.Location, DocAddressType.Location);
			}
			return expectedDocAddressTypes;
		}
	}

	Hashtable expectedDocAddressTypes;

	#region Implementation

	void AssertDeclarantAndRepresentativeDefaultingForImporter(string orgProxyType, ZGuid orgProxyNotMainAddressPK)
	{
		var factory = Factory;
		var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
		var representative = factory.NewWithValidTestData<OrgHeader>();
		representative.OH_Code = "REP";
		var importer = factory.NewWithValidTestData<OrgHeader>();
		importer.OH_Code = "IMP";

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = representative.MainAddress.PK;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		importerDocumentaryAddress.E2_OA_Address = orgProxyNotMainAddressPK;

		CombineAssertions($"When Export, and {orgProxyType} then nothing change", () =>
		{
			AssertEquals(true, declaration.JE_OA_DeclarantAddress.IsEmpty);
			AssertEquals(representative.MainAddress.PK, declaration.JE_OA_Representative);
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		importerDocumentaryAddress.E2_OA_Address = orgProxyNotMainAddressPK;

		CombineAssertions($"When Import, Application Code 'BLT' and {orgProxyType}", () =>
		{
			AssertEquals("fill 'Declarant' field and address with 'Importer' field code and address", importerDocumentaryAddress.E2_OA_Address, declaration.JE_OA_DeclarantAddress);
			AssertEquals("clear 'Representative' field and address", true, declaration.JE_OA_Representative_ZAddress.OrgPK.IsEmpty);
		});

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;

		importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		importerDocumentaryAddress.E2_OA_Address = orgProxyNotMainAddressPK;

		CombineAssertions("When Import, Application Code 'ITF'", () =>
		{
			AssertEquals("no change in 'Representative' field and address", true, declaration.JE_OA_Representative.IsEmpty);
			AssertEquals("no change in 'Declarant' field and address", true, declaration.JE_OA_DeclarantAddress.IsEmpty);
		});
	}

	void AssertDeclarantAndRepresentativeDefaultingForSupplier(string orgProxyType, ZGuid orgProxyPK)
	{
		var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		representative.OH_Code = "REP";

		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = representative.MainAddress.PK;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		supplierDocumentaryAddress.E2_OA_Address = orgProxyPK;

		CombineAssertions($"When Import, and {orgProxyType} then nothing change", () =>
		{
			AssertEquals(true, declaration.JE_OA_DeclarantAddress.IsEmpty);
			AssertEquals(representative.MainAddress.PK, declaration.JE_OA_Representative);
		});

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		CombineAssertions($"When Export, and {orgProxyType}", () =>
		{
			using (declaration.SetterSuspender.SuspendSetting(BaseJobDeclaration.Schema.JE_MessageType))
			{
				supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				supplierDocumentaryAddress.E2_OA_Address = orgProxyPK;
				AssertEquals("fill 'Declarant' field and address with 'Supplier' field code and address", supplierDocumentaryAddress.E2_OA_Address, declaration.JE_OA_DeclarantAddress);
				AssertEquals("clear 'Representative' field and address", true, declaration.JE_OA_Representative_ZAddress.OrgPK.IsEmpty);
			}
		});

		var dummyAddress = Factory.NewWithValidTestData<OrgHeader>();

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		declaration.JE_OA_DeclarantAddress = dummyAddress.PK;
		declaration.JE_OA_Representative = dummyAddress.PK;

		supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		supplierDocumentaryAddress.E2_OA_Address = orgProxyPK;

		CombineAssertions("When Export, Application Code 'ITF'", () =>
		{
			using (declaration.SetterSuspender.SuspendSetting(BaseJobDeclaration.Schema.JE_MessageType))
			{
				AssertEquals("no change in 'Representative' field and address", dummyAddress.PK, declaration.JE_OA_Representative);
				AssertEquals("no change in 'Declarant' field and address", dummyAddress.PK, declaration.JE_OA_DeclarantAddress);
			}
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declarationForLightValidationTest = Factory.New<JobDeclaration>();
		var entryInstruction = declarationForLightValidationTest.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "";
		return declarationForLightValidationTest;
	}

	protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.DisableDefaultPackingInformation = true;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
	}

	JobDeclaration declaration;

	#endregion
}

class JobDeclarationForOfficeOfExitTest : JobDeclaration
{
	public JobDeclarationForOfficeOfExitTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public bool IsOfficeOfExitMeaningful_Exposed => base.IsOfficeOfExitMeaningfulForDeclaration;
}

sealed class DummyJobDeclaration_GetNewJobDeclarationFieldsCleaner : JobDeclaration
{
	public DummyJobDeclaration_GetNewJobDeclarationFieldsCleaner(BusinessObjectFactory factory, DataRow row)
	: base(factory, row)
	{
	}

	public int CleanUpMessageDependentFieldsTriggeredCount { get; set; }
	public int CleanUpTransportModeInlandDependentFieldsTriggeredCount { get; set; }
	public int CleanUpShipmentDeliveryTermsFieldsTriggeredCount { get; set; }

	protected override IJobDeclarationFieldsCleaner GetNewJobDeclarationFieldsCleaner()
	{
		jobDeclarationFieldsCleanerMock = new Mock<IJobDeclarationFieldsCleaner>();
		jobDeclarationFieldsCleanerMock.Setup(mock => mock.CleanUpMessageDependentFieldsIfNoLongerApplicable()).Callback(() => CleanUpMessageDependentFieldsTriggeredCount++);
		jobDeclarationFieldsCleanerMock.Setup(mock => mock.CleanUpTransportModeInlandDependentFieldsIfNoLongerApplicable()).Callback(() => CleanUpTransportModeInlandDependentFieldsTriggeredCount++);
		jobDeclarationFieldsCleanerMock.Setup(mock => mock.CleanUpShipmentIncoTermFieldsIfNoLongerApplicable()).Callback(() => CleanUpShipmentDeliveryTermsFieldsTriggeredCount++);
		return jobDeclarationFieldsCleanerMock.Object;
	}

	Mock<IJobDeclarationFieldsCleaner> jobDeclarationFieldsCleanerMock;
}
