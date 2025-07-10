using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IN;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
{
	public void TestCalculateLinePriceForExportDeclaration()
	{
		RefDataSetupTestHelper.SetupCusPack(Factory, "CMM", "CM1", 12);
		RefDataSetupTestHelper.SetupCusPack(Factory, "CMM", "CM2", 0.08333m);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertJI_LinePrice(120m, "CMM", 10m, "CMM", 1, 1200m);
		AssertJI_LinePrice(120m, "CMM", 10m, "CMM", 2, 600m);
		AssertJI_LinePrice(120m, "CMM", 10m, "CM1", 1, 14400m);
		AssertJI_LinePrice(120m, "CMM", 10m, "CM1", 2, 7200m);
		AssertJI_LinePrice(120m, "CMM", 10m, "CM2", 1, 100m);
		AssertJI_LinePrice(120m, "CMM", 10m, "CM2", 2, 50m);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_LinePrice = 0m;
		AssertJI_LinePrice(120m, "CMM", 10m, "CMM", 1, 0m);
	}

	void AssertJI_LinePrice(ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal unitPrice, ZString unitUQ, ZInt unitQuantity, ZDecimal expectValue)
	{
		InvoiceLine.JI_InvoiceQuantity = invoiceQuantity;
		InvoiceLine.JI_InvoiceUQ = invoiceUQ;
		InvoiceLine.JI_UnitPrice = unitPrice;
		InvoiceLine.JI_UnitUQ = unitUQ;
		InvoiceLine.JI_UnitQuantity = unitQuantity;
		AssertEquals(expectValue, InvoiceLine.JI_LinePrice);
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.India, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestPartType()
	{
		var factory2 = new BusinessObjectFactory();
		var importer = factory2.New<OrgHeader>();
		importer.FillWithValidTestData();
		var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
		product.OP_PartNum = "TestTEST";
		product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
		factory2.Save();

		var customsTemplate_Company = Factory.New<GlbCompany>();
		customsTemplate_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		var customsTemplate_Branch = customsTemplate_Company.Branches.AddNew();
		customsTemplate_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.India)).RL_Code;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_GB = customsTemplate_Branch.PK;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		invoiceLine.JI_PartNo = "TestTEST";
		AssertType<OrgSupplierPart>("Product type gets changed depending on who is requesting", invoiceLine.Part);
		Factory.Save();

		GlbCompany.CurrentCompany.SetCountry("AU");
		var factory3 = new BusinessObjectFactory();
		var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
		AssertType<OrgSupplierPart>("product type still the type", declarationLoaded.InvoiceLines[0].Part);
	}

	public void TestSWConstituentCollection()
	{
		var collection = Factory.New<JobComInvoiceLine>().SWConstituents;
		AssertType<SWConstituentCollection>(collection);
	}

	public void TestSWConstituentLineNumberGenerator()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<HugeSequenceNumberGenerator>(invoiceLine.SWConstituentLineNumberGenerator);
	}

	public void TestSWControlCollection()
	{
		var collection = Factory.New<JobComInvoiceLine>().SWControls;
		AssertType<SWControlCollection>(collection);
	}

	public void TestJobWorksCollection()
	{
		var collection = Factory.New<JobComInvoiceLine>().JobWorks;
		AssertType<JobWorkCollection>(collection);
	}

	public void TestJobWorkLineNumberGenerator()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<HugeSequenceNumberGenerator>(invoiceLine.JobWorkLineNumberGenerator);
	}

	public void TestSWProductiontCollection()
	{
		var collection = Factory.New<JobComInvoiceLine>().SWProductions;
		AssertType<SWProductionDetailsCollection>(collection);
	}

	public void TestSWProductionLineNumberGenerator()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<HugeSequenceNumberGenerator>(invoiceLine.SWProductionsLineNumberGenerator);
	}

	public void TestICusSupportingInfoTypeSupporter()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		Integration.Customs.ICusSupportingInfoTypeSupporter supporter = invoiceLine;
		var supportingInfoTypes = supporter.GetCusSupportingInfoTypes();
		AssertEquals(typeof(SWConstituent), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SingleWindowConstituent]);
		AssertEquals(typeof(SWControl), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SingleWindowControl]);
		AssertEquals(typeof(DfiaExportItemDetail), supportingInfoTypes[CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization]);
		AssertEquals(typeof(JobWork), supportingInfoTypes[CusSupportingInfoTypeList.Codes.JobWork]);
		AssertEquals(typeof(SupportingDocument), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);
		AssertEquals(typeof(SWProduction), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SingleWindowProduction]);

		AssertType<CusSupportingInfoTypeSupporterFetchStrategy>(supporter.GetFetchStrategies().First());
	}

	public void TestDfiaExportItemDetailsCollection()
	{
		var collection = Factory.New<JobComInvoiceLine>().DfiaExportItemDetails;
		AssertType<DfiaExportItemDetailCollection>(collection);
	}

	public void TestDfiaExportItemDetailsLineNumberGenerator()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<HugeSequenceNumberGenerator>(invoiceLine.DfiaExportItemDetailsLineNumberGenerator);
	}

	public void TestValidationMessageType()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceLineValidation>(InvoiceLine.Validation);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<JobComInvoiceLineValidation>(InvoiceLine.Validation);
		});
	}

	public void TestJI_LinePrice()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var resData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_LinePriceInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Total Price", resData.Caption);
			AssertEquals("MediumCaption", "Total Price", resData.MediumCaption);
			AssertEquals("ShortCaption", "Price", resData.ShortCaption);
			AssertEquals("ReadOnly when export", true, InvoiceLine.JI_LinePriceInfo.ReadOnly);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Non-ReadOnly when import", false, InvoiceLine.JI_LinePriceInfo.ReadOnly);
		});
	}
	public void TestJI_ValuationMarkup()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var resData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_ValuationMarkupInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "PMV (INR)", resData.Caption);
			AssertEquals("MediumCaption", "PMV (INR)", resData.MediumCaption);
			AssertEquals("ShortCaption", "PMV", resData.ShortCaption);
		});
	}

	public void TestJI_PMVAndTotalPMV_LocalCurrency()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.India;
		var resData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_TotalPMVInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Total PMV Value", resData.Caption);
			AssertEquals("MediumCaption", "Total PMV", resData.MediumCaption);
			AssertEquals("ShortCaption", "Total PMV", resData.ShortCaption);

			InvoiceLine.JI_ValuationMarkup = 11.00m;
			AssertEquals("non-editable calculated fields when PMV% is not equal to 0.00", true, InvoiceLine.JI_PMVInfo.ReadOnly);
			AssertEquals("non-editable calculated fields when PMV% is not equal to 0.00", true, InvoiceLine.JI_TotalPMVInfo.ReadOnly);

			InvoiceLine.JI_InvoiceQuantity = 10.00m;
			InvoiceLine.JI_UnitPrice = 100.00m;
			AssertEquals("PMV (INR)", 11.00m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 110.00m, InvoiceLine.JI_TotalPMV);

			InvoiceLine.JI_InvoiceQuantity = 20.00m;
			AssertEquals("PMV (INR)", 11.00m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 220.00m, InvoiceLine.JI_TotalPMV);

			InvoiceLine.JI_UnitPrice = 120.00m;
			AssertEquals("PMV (INR)", 13.20m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 264.00m, InvoiceLine.JI_TotalPMV);

			InvoiceLine.JI_ValuationMarkup = 110.00m;
			AssertEquals("PMV (INR)", 132.00m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 2640.00m, InvoiceLine.JI_TotalPMV);

			InvoiceLine.JI_ValuationMarkup = 0.00m;
			AssertEquals("editable fields when PMV% is not equal t0 0.00", false, InvoiceLine.JI_PMVInfo.ReadOnly);
			AssertEquals("editable fields when PMV% is not equal to 0.00", false, InvoiceLine.JI_TotalPMVInfo.ReadOnly);

			InvoiceLine.JI_PMV = 12.00m;
			AssertEquals("PMV (INR)", 12.00m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 240.00m, InvoiceLine.JI_TotalPMV);

			InvoiceLine.JI_InvoiceQuantity = 30.00m;
			AssertEquals("PMV (INR)", 12.00m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 360.00m, InvoiceLine.JI_TotalPMV);

			InvoiceLine.JI_TotalPMV = 300.00m;
			AssertEquals("PMV (INR)", 10.00m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 300.00m, InvoiceLine.JI_TotalPMV);
		});

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_TotalPMV = 0m;
		InvoiceLine.JI_InvoiceQuantity = 10m;
		InvoiceLine.JI_PMV = 10m;
		AssertEquals(0m, InvoiceLine.JI_TotalPMV);

		InvoiceLine.JI_PMV = 0m;
		InvoiceLine.JI_ValuationMarkup = 0m;
		InvoiceLine.JI_TotalPMV = 100m;
		AssertEquals(0m, InvoiceLine.JI_PMV);
	}

	public void TestJI_PMVAndTotalPMV_ForeignCurrency()
	{
		var foreignCurrency = "XYZ";
		RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory, foreignCurrency);
		RefDataSetupTestHelper.SetExchangeRates(Factory, foreignCurrency, 80.00m);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = foreignCurrency;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_ValuationMarkup = 11.00m;
			InvoiceLine.JI_InvoiceQuantity = 10.00m;
			InvoiceLine.JI_UnitPrice = 100.00m;
			AssertEquals("PMV (INR)", 880.00m, InvoiceLine.JI_PMV);
			AssertEquals("Total PMV", 8800.00m, InvoiceLine.JI_TotalPMV);
		});
	}

	public void TestJI_EndUse()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_EndUse), false, attribute => attribute.Caption == "End Use");
		AssertHasCustomAttribute<ListAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_EndUse), false, attribute => attribute.ListDataSourceMember == "Lookups.EndUseCodes");
	}

	public void TestJI_StateOrRegionOfOrigin()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var resData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_StateOrRegionOfOriginInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Source State", resData.Caption);
			AssertEquals("MediumCaption", "State", resData.MediumCaption);
			AssertEquals("ShortCaption", "State", resData.ShortCaption);

			AssertEquals("Max Length", 2, InvoiceLine.JI_StateOrRegionOfOriginInfo.MaxLength);
		});
	}

	public void TestJI_RN_NKCountryOfTransit()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var resData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_RN_NKCountryOfTransitInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Transit Country", resData.Caption);
			AssertEquals("MediumCaption", "Trans. Ctry.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Tr. Ctry.", resData.ShortCaption);
		});
	}

	public void TestJI_UnitPrice()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var resData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_UnitPriceInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Rate", resData.Caption);
		});
	}

	public void TestJI_UnitQuantity()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var resData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_UnitQuantityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Rate Per Unit", resData.Caption);
			AssertEquals("MediumCaption", "Per/UOM", resData.MediumCaption);
			AssertEquals("ShortCaption", "Per/UOM", resData.ShortCaption);
		});
	}

	public void TestJI_JobWorkNotificationNoInfo()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var resData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_JobWorkNotificationNoInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Job Work Notification No.", resData.Caption);
			AssertEquals("MediumCaption", "Job Work Notif. No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Notification No.", resData.ShortCaption);
		});
	}

	public void TestJI_UnitUQ()
	{
		AssertHasCustomAttribute<ListAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_UnitUQ), false, x => x.ListDataSourceMember == "Lookups.UnitUQList");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_UnitUQ), false, x => x.Caption == "UOM");
	}

	public void TestJI_InvoiceUQ()
	{
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		invoiceLine.JI_UnitUQ = ZString.Empty;
		invoiceLine.JI_InvoiceUQ = "PCS";

		AssertEquals("PCS", invoiceLine.JI_UnitUQ);
	}

	public void TestJI_AccessoryStatus()
	{
		var accessoryStatusInfo = Factory.New<JobComInvoiceLine>().JI_AccessoryStatusInfo;
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(accessoryStatusInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Accessory Status", captionResourceString.Caption);
			AssertEquals("Accessory Status", captionResourceString.MediumCaption);
			AssertEquals("Acc. Status", captionResourceString.ShortCaption);
			AssertEquals("MaxLength", 1, accessoryStatusInfo.MaxLength);
		});
	}

	public void TestAccessoryDescription()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var accessoryDescriptionInfo = Factory.New<JobComInvoiceLine>().AccessoryDescriptionInfo;
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(accessoryDescriptionInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Accessory Description", captionResourceString.Caption);
			AssertEquals("Accessory Desc.", captionResourceString.MediumCaption);
			AssertEquals("Acsry. Desc.", captionResourceString.ShortCaption);
			AssertEquals("MaxLength", 500, accessoryDescriptionInfo.MaxLength);

			InvoiceLine.AccessoryDescription = "Accessory Description";
			var note = FindNote(InvoiceLine.PK);
			AssertEquals("Accessory Description should be", "Accessory Description", note.ST_NoteText);

			InvoiceLine.JI_AccessoryStatus = "0";
			AssertEquals("ReadOnly when export", true, InvoiceLine.AccessoryDescriptionInfo.ReadOnly);
			AssertEquals("Accessory Description should be empty", ZString.Empty, InvoiceLine.AccessoryDescription);

			InvoiceLine.JI_AccessoryStatus = "1";
			AssertEquals("ReadOnly when export", false, InvoiceLine.AccessoryDescriptionInfo.ReadOnly);

			InvoiceLine.JI_AccessoryStatus = "2";
			AssertEquals("ReadOnly when export", false, InvoiceLine.AccessoryDescriptionInfo.ReadOnly);

			InvoiceLine.AccessoryDescription = "Accessory Description";
			InvoiceLine.JI_AccessoryStatus = "3";
			AssertEquals("ReadOnly when export", true, InvoiceLine.AccessoryDescriptionInfo.ReadOnly);
			AssertEquals("Accessory Description should be empty", ZString.Empty, InvoiceLine.AccessoryDescription);

			InvoiceLine.Delete();
			AssertEquals("Accessory Description Note should be deleted", true, note.IsDeleted);
		});

		StmNote FindNote(ZGuid parentID)
		{
			var query = new ZQuery();
			query.AddToFilter(StmNoteSchema.ST_ParentID, parentID);
			return Factory.LoadTop1<StmNote>(query);
		}
	}

	public void TestJI_GSTPayNotApplicable()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		CaptionTestHelper.AssertCaptions(invoiceLine.JI_GSTPayNotApplicableInfo, "Payment Not Applicable", "Pay. NA", "P. NA");

		invoiceHeader.JZ_GSTPaymentStatus = IGSTPaymentStatusCodeList.Codes.NotApplicable;
		AssertEquals(invoiceLine.JI_GSTPayNotApplicable, true);
		AssertReadOnlyProperty(invoiceLine.JI_GSTPayNotApplicableInfo, true);

		invoiceHeader.JZ_GSTPaymentStatus = IGSTPaymentStatusCodeList.Codes.ExportAgainstPayment;
		AssertEquals(invoiceLine.JI_GSTPayNotApplicable, false);
		AssertReadOnlyProperty(invoiceLine.JI_GSTPayNotApplicableInfo, false);

		invoiceHeader = null;
		AssertEquals(invoiceLine.JI_GSTPayNotApplicable, false);
		AssertReadOnlyProperty(invoiceLine.JI_GSTPayNotApplicableInfo, false);

		var newInvoiceHeader = Factory.New<JobComInvoiceHeader>();
		newInvoiceHeader.JZ_GSTPaymentStatus = IGSTPaymentStatusCodeList.Codes.NotApplicable;
		invoiceLine.JI_JZ = newInvoiceHeader.PK;
		AssertEquals(invoiceLine.JI_GSTPayNotApplicable, true);
		AssertReadOnlyProperty(invoiceLine.JI_GSTPayNotApplicableInfo, true);
	}

	void AssertReadOnlyProperty(ZPropertyInfo info, bool expectedReadOnly)
	{
		ZString message = $"The readonly of {info.Name} should be {expectedReadOnly}.";
		AssertEquals(message, expectedReadOnly, info.ReadOnly);
	}

	public void TestJI_RewardItem()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var resData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_RewardItemInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Reward Item", resData.Caption);
			AssertEquals("MediumCaption", "Reward", resData.MediumCaption);
			AssertEquals("ShortCaption", "Reward", resData.ShortCaption);
		});
	}

	public void TestJI_Tariff()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		CaptionTestHelper.AssertCaptions(invoiceLine.JI_TariffInfo, "RITC Code", "RITC", "RITC");
		AssertEquals("Max Length", 8, invoiceLine.JI_TariffInfo.MaxLength);
	}

	public override void TestJI_FormattedTariff()
	{
		var tariff = "12345678";
		InvoiceLine.JI_Tariff = tariff;
		AssertEquals("JI_FormattedTariff", "1234.56.78", InvoiceLine.JI_FormattedTariff);
		tariff = "9876 .54 .32";
		InvoiceLine.JI_FormattedTariff = tariff;
		AssertEquals("JI_FormattedTariff", "9876.54", InvoiceLine.JI_FormattedTariff);
	}

	public void TestGetSequenceNumberGenerator()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var supportingInfoParent = (ICusSupportingInfoWithSerialNoParent)invoiceLine;

		var expectedValues = new[]
		{
		invoiceLine.SWConstituentLineNumberGenerator,
		invoiceLine.SWControlsLineNumberGenerator,
		invoiceLine.DfiaExportItemDetailsLineNumberGenerator,
		invoiceLine.JobWorkLineNumberGenerator,
		invoiceLine.SupportingDocumentLineNumberGenerator,
		invoiceLine.SWProductionsLineNumberGenerator
		};

		var actualValues = supportingInfoParent.GetCusSupportingInfoTypes().Keys.Select(type => supportingInfoParent.GetSequenceNumberGenerator(type));

		AssertEquals("Count", expectedValues.Length, actualValues.Count());
		AssertContainsExactElementsInExactOrder("Sequence Number Generators", expectedValues, actualValues);
	}

	public void TestSupportingDocuments()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var collection = invoiceLine.SupportingDocuments;
		AssertType<SupportingDocumentCollection>(collection);
	}

	[TestDate(2024, 6, 13)]
	public void TestIDateOfValuationProvider()
	{
		var provider = (IDateOfValuationProvider)InvoiceLine;

		AssertEquals("Declaration without ValuationDate set", new ZDate(2024, 6, 13), provider.DateOfValuation);
		Declaration.JE_ValuationDate = new ZDate(2022, 3, 16);
		AssertEquals("Declaration with ValuationDate set", new ZDate(2022, 3, 16), provider.DateOfValuation);
	}

	public void TestJI_MPG_CodeType()
	{
		CaptionTestHelper.AssertCaptions(InvoiceLine.JI_MPG_CodeTypeInfo, "Code Type", "Cd. Type", "Cd. Ty.");
	}

	public void TestJI_MPG_Code()
	{
		CaptionTestHelper.AssertCaptions(InvoiceLine.JI_MPG_CodeInfo, "Code", "Code", "Code");
	}

	public void TestJI_OA_ManufacturerAddress()
	{
		CaptionTestHelper.AssertCaptions(InvoiceLine.JI_OA_ManufacturerAddressInfo, "Organization", "", "");
	}

	public override void TestChargeTypeList()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Customs.Business.ICommonInvoice commonInvoice = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		var chargeTypeList = commonInvoice.ChargeTypeList;
		var chargeTypeList2 = commonInvoice.ChargeTypeList;
		AssertSame("cached import", chargeTypeList, chargeTypeList2);
		var customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
		customsChargeTypeList.Sort();
		AssertEquals("import", customsChargeTypeList.CodesAsString, chargeTypeList.CodesAsString);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Customs.Business.ICommonInvoice line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		var chargeTypeList3 = line.ChargeTypeList;
		var chargeTypeList4 = line.ChargeTypeList;
		AssertSame("Cached export", chargeTypeList3, chargeTypeList4);
		AssertEquals("export", string.Empty, chargeTypeList3.CodesAsString);
	}

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
}
