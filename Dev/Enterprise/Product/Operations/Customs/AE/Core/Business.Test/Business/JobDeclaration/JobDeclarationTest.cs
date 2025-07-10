using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AE;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : TypeSafeJobDeclarationTest
{
	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(
			Factory.New<JobDeclaration>(),
			"AEJobDeclaration",
			schemaTypeName: nameof(AutoAEJobDeclaration.Schema));
	}

	public override void TestAreMultipleEntryInstructionsAllowed()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.UnitedArabEmirates, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
	}

	public void TestReciprocalRates()
	{
		Assert(Factory.New<JobDeclaration>().IsReciprocalRates);
	}

	public override void TestMergeMethodThatTakesISendsMessageToCustoms()
	{
		Assert(true);
	}

	public override void TestPackingGroupCollectionRegistration()
	{
		InitialiseDefaultLists();
		Factory.Save();

		base.TestPackingGroupCollectionRegistration();
	}

	public void TestSavingInTwoFactories()
	{
		var declaration = Factory.New<JobDeclaration>();
		Factory.Save();
		Db.Connection.ExecuteNonQuery(@"update dbo.jobdeclaration
set JE_AddInfo = 'ClearanceLocation=21*ImporterFZ=N*ExitPoint=AFZ*RestrictedGoods=N*TypeOfGoods=2*PlaceOfDischarge=F',
    JE_SystemLastEditTimeUtc = GetUtcDate(),
    JE_SystemLastEditUser = '~BP'
where JE_PK = '" + declaration.PK.ToString() + "'"); // Must change data directly
		var factory2 = new BusinessObjectFactory();
		var factory3 = new BusinessObjectFactory();
		var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
		AssertEquals("ClearanceLocation=21*ImporterFZ=N*ExitPoint=AFZ*RestrictedGoods=N*TypeOfGoods=2*PlaceOfDischarge=F", declaration2.JE_AddInfo);
		var declaration3 = factory3.Load<JobDeclaration>(declaration.PK);
		BusinessObjectFactory.SaveTogether(new BusinessObjectFactory[] { factory2, factory3 });
	}

	public void TestJE_Calc_LegacyCode()
	{
		TestDeclaration.JE_OH_Importer = ZGuid.Empty;
		NUnit.Framework.Assert.That(TestDeclaration.Consignee, Is.Null, "Precondition");
		AssertEquals(ZString.Empty, TestDeclaration.JE_Calc_LegacyCode);
		var customsCode = ConsigneeOrg.CustomsCodes.AddNew();
		customsCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
		customsCode.OK_CustomsRegNo = "123456";
		TestDeclaration.JE_OH_Importer = ConsigneeOrg.PK;
		AssertEquals("123456", TestDeclaration.JE_Calc_LegacyCode);
	}

	public void TestJE_Calc_ImporterBankAccount()
	{
		TestDeclaration.JE_OH_Importer = ZGuid.Empty;
		NUnit.Framework.Assert.That(TestDeclaration.Importer, Is.Null, "Precondition");
		AssertEquals(ZString.Empty, TestDeclaration.JE_Calc_ImporterBankAccount);
		ConsigneeOrg.MiscServ.OM_IMEFTBankAccount = "123456789";
		TestDeclaration.JE_OH_Importer = ConsigneeOrg.PK;
		AssertEquals("123456789", TestDeclaration.JE_Calc_ImporterBankAccount);
	}

	public void TestJE_Calc_CustomsCode()
	{
		TestDeclaration.JE_OH_Importer = ZGuid.Empty;
		NUnit.Framework.Assert.That(TestDeclaration.Consignee, Is.Null, "Precondition");
		AssertEquals(ZString.Empty, TestDeclaration.JE_Calc_CustomsCode);
		var customsCode = ConsigneeOrg.CustomsCodes.AddNew();
		customsCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
		customsCode.OK_CustomsRegNo = "11111";
		TestDeclaration.JE_OH_Importer = ConsigneeOrg.PK;
		AssertEquals("11111", TestDeclaration.JE_Calc_CustomsCode);
	}

	public void TestJE_MarksAndNumbersShortCaption()
	{
		AssertEquals("JE_MarksAndNumbers: Caption", "Marks & Numbers", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(JobDeclaration.JE_MarksAndNumbersShort)).Caption);
	}

	public void TestTypeOfGoods()
	{
		AssertEquals("Low ", TestDeclaration.TypeOfGoods);
		TestDeclaration.JE_TypeOfGoods = TypeOfGoodsList.Codes.HighValueAboveDeminimis;
		AssertEquals("High", TestDeclaration.TypeOfGoods);
	}

	public void TestPortFirstArrivalGetsSetToPortOfArrival()
	{
		TestDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
		AssertEquals("AUSYD", TestDeclaration.JE_RL_NKPortOfFirstArrival);
		TestDeclaration.JE_RL_NKPortOfArrival = ZString.Empty;
		AssertEquals(ZString.Empty, TestDeclaration.JE_RL_NKPortOfFirstArrival);
	}

	#region TestCusDecHouseBills
	public void TestCusDecHouseBills()
	{
		var declaration = JobDeclaration.New(Factory);
		var houseBill = declaration.Bills.AddNew();
		AssertNotNull(houseBill);
	}

	#endregion

	#region TestIsAEDeclarationApplicationCodeDubai

	public void TestIsAEDeclarationApplicationCodeDubai()
	{
		CombineAssertions(() =>
		{
			TestDeclaration.JE_ApplicationCode = "AED";
			Assert("When JE_ApplicationCode = AED, IsAEDeclarationApplicationCodeDubai Should Be true", TestDeclaration.IsApplicationCodeDubai);
			TestDeclaration.JE_ApplicationCode = "ITF";
			Assert("When JE_ApplicationCode != AED, IsAEDeclarationApplicationCodeDubai Should Be false", !TestDeclaration.IsApplicationCodeDubai);
		});
	}

	#endregion

	#region TestIsTranshipment

	public void TestIsTranshipment()
	{
		CombineAssertions(() =>
		{
			TestDeclaration.JE_MessageType = AEJobMessageTypeList.Codes.Transfer;
			Assert("When JE_MessageType = TRF, IsTranshipment should be true.", TestDeclaration.IsTranshipment);
			TestDeclaration.JE_MessageType = AEJobMessageTypeList.Codes.Import;
			Assert("When JE_MessageType != TRF, IsTranshipment should be false.", !TestDeclaration.IsTranshipment);
		});
	}

	#endregion

	#region TestIsTransit

	public void TestIsTransit()
	{
		CombineAssertions(() =>
		{
			TestDeclaration.JE_MessageType = AEJobMessageTypeList.Codes.Transit;
			Assert("When JE_MessageType = TRS, IsTransit should be true.", TestDeclaration.IsTransit);
			TestDeclaration.JE_MessageType = AEJobMessageTypeList.Codes.Import;
			Assert("When JE_MessageType != TRS, IsTransit should be false.", !TestDeclaration.IsTransit);
		});
	}

	#endregion

	#region TestDefaultValues
	public void TestDefaultValues()
	{
		AssertEquals("Default Message Sub Type should be Empty String", ZString.Empty, TestDeclaration.JE_MessageSubType);
		AssertEquals("Default Message Type should be 'IMP'", Common.Shared.SharedJobMessageTypeList.Codes.Import, TestDeclaration.JE_MessageType);
		AssertEquals("Default Type of Goods should be '2'", TypeOfGoodsList.Codes.LowValueBelowDeminimis, TestDeclaration.JE_TypeOfGoods);
		AssertEquals("Defult Payment Method should be 'CA'", PaymentByList.Codes.CouriersAccount, TestDeclaration.JE_PaymentMethod);
	}

	public void TestDefaultApplicationCode() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Default Application Code", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
		var uAEManifestFeatureControlData = new UAECustomsModuleFeatureControlData() { EnableUAESeaExportManifest = true };
		var featureDataMock = new Mock<IFeatureData>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out uAEManifestFeatureControlData)).Returns(true);

		var featureControlMock = new Mock<IFeatureControlManager>();
		featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UAEDubaiCustomsModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		using (ObjectFactory.Substitute(featureControlMock.Object))
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals("Default Application Code for Builtin type", AEDeclarationApplicationCodeList.Codes.Dubai, declaration.JE_ApplicationCode);
			}
		}
	});

	public override void TestDefaultDataGroupingCode() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertEquals("Data Grouping for Interface type", Core.Constants.CountryCodes.UnitedArabEmirates, declaration.GetDefaultDataGroupingCode());

		declaration.JE_ApplicationCode = "XYZ";
		AssertEquals("Default Data Grouping", "XYZ", declaration.GetDefaultDataGroupingCode());
	});

	#endregion
	public void TestSupportJE_PaymentMethodUsage()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(true, declaration.SupportJE_PaymentMethodUsage);
	}

	public override void TestDefaultDataGroupingCodeForTariffs()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Data grouping", AEConstants.DefaultDataGroupingForTariffs, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
	}

	protected override void CreatePartAndClassification(string partNum, string tariff, OrgHeader importer)
	{
		var part = Factory.New<OrgSupplierPart>();
		part.OP_PartNum = partNum;
		part.RelatedOrganisations.AddOwner(importer);
		var classification = Factory.New<BaseCusClassification>();
		classification.CC_ClassificationType = Common.ClassificationType.Both;
		classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		classification.CC_TariffNum = tariff;
		var pivot = Factory.New<BaseCusClassPartPivot>();
		pivot.CI_CC = classification.PK;
		pivot.CI_OP = part.PK;
	}

	#region TestSettingAddinfoItemSetsHasChanges
	public void TestSettingAddinfoItemSetsHasChanges()
	{
		TestDeclaration.HasChanges = false;
		TestDeclaration.JE_ExitPoint = "ABC";
		AssertEquals("HasChanges", true, TestDeclaration.HasChanges);
	}

	#endregion

	public override void TestIsDeclarationWithEntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		Assert("Customs Entry Instruction Provider", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
	}

	public void TestDefaultDataGroupingForCusProcedure() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertEquals("Data Grouping for Interface type", Core.Constants.CountryCodes.UnitedArabEmirates, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));

		declaration.JE_ApplicationCode = AEDeclarationApplicationCodeList.Codes.Dubai;
		AssertEquals("Data Grouping for Dubai type", AEDeclarationApplicationCodeList.Codes.Dubai, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
	});

	#region Implementation
	#region TestDeclaration
	JobDeclaration TestDeclaration
	{
		get
		{
			if (fTestDeclaration == null)
			{
				fTestDeclaration = (JobDeclaration)GetJobDeclaration();
				fTestDeclaration.HasChanges = false;
			}

			return fTestDeclaration;
		}
	}

	JobDeclaration fTestDeclaration;
	JobDeclarationForTesting GetJobDeclarationForTesting()
	{
		var dec = Factory.New<JobDeclarationForTesting>();
		dec.DisableDefaultPackingInformation = true;
		dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		return dec;
	}

	#endregion
	#region GetJobDeclaration
	protected override BaseJobDeclaration GetJobDeclaration()
	{
		return JobDeclaration.New(Factory);
	}

	#endregion
	#region ConsigneeOrg
	OrgHeader ConsigneeOrg
	{
		get
		{
			if (fConsigneeOrg == null)
			{
				fConsigneeOrg = Factory.New<OrgHeader>();
				fConsigneeOrg.OH_Code = "NEWCONSCODE";
				fConsigneeOrg.OH_FullName = "Consignee";
			}

			return fConsigneeOrg;
		}
	}

	OrgHeader fConsigneeOrg;
	#endregion

	public void InitialiseDefaultLists()
	{
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);

		var helper = new UAEUniversalReferenceTestHelper(Factory);

		helper.InitialiseRefDataWithCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEClearanceLocation, "Clearance Location");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedArabEmirates,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEClearanceLocation, AEConstants.ClearanceLocationAirportFreeZone, "Airport Freezone", startDate, endDate);

		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEExitPoint, "Exit Point", Core.Constants.CountryCodes.UnitedArabEmirates);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedArabEmirates,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEExitPoint, AEConstants.ExitPointAirportFreeZone, "Airport Freezone", startDate, endDate);

		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEPlaceofDischarge, "Place of Discharge");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedArabEmirates,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEPlaceofDischarge, AEConstants.PlaceOfDischargeAirportFreeZone, "Airport Freezone", startDate, endDate);
	}

	#endregion
}
