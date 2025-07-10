using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
{
	public void TestJI_FormattedProcedure_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_FormattedProcedureInfo, "Procedure", "[UCC 1/10] Procedure", "EXP");
	}

	public void TestAdditionalProcedureCodesAsString_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.AdditionalProcedureCodesAsStringInfo, "Add. Procedures", "[UCC 1/11] Add. Procedures", "EXP");
	}

	public void TestJI_LinePrice_Caption()
	{
		AssertEquals("[UCC 4/14] Price", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_LinePriceInfo).Caption);
	}

	public void TestJI_ValuationCode_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_ValuationCodeInfo, "Valuation Method", "[UCC 4/16] Valuation Method");
	}

	public void TestJI_PrimaryPreference_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_PrimaryPreferenceInfo, "Preference", "[UCC 4/17] Preference", "IMP");
	}

	public void TestJI_CountryOfOrigin_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CountryOfOriginInfo, "Country of Origin", "[UCC 5/15] Country of Origin", "EXP");
		AssertCaptionAndFullDescription(InvoiceLine.JI_CountryOfOriginInfo, "Pref. Orig.", "[UCC 5/16] Country of Preferential Origin", "IMP");
	}

	public void TestZG_CountryOfSupply_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CountryOfSupplyInfo, "Country of Origin", "[UCC 5/15] Country of Origin", "IMP");
	}

	public void TestZG_CountryOfDispatch_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CountryOfDispatchInfo, "[UCC 5/14] Dispatch", "[UCC 5/14] Country of Dispatch", expectedShortCaption: "[UCC 5/14] Disp.");
	}

	public void TestJI_RN_NKCountryOfExport_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_RN_NKCountryOfExportInfo, "[UCC 5/14] Dispatch", "[UCC 5/14] Country of Dispatch", "EXP", expectedShortCaption: "[UCC 5/14] Disp.");
	}

	public void TestZG_CountryOfDestination_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CountryOfDestinationInfo, "[UCC 5/8] Dest.", "[UCC 5/8] Country of Destination", "IMP");
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CountryOfDestinationInfo, "[UCC 5/8] Dest.", "[UCC 5/8] Country of Destination", "EXP");
	}

	public void TestJI_Weight_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_WeightInfo, "Gross Weight", "[UCC 6/5] Gross Weight");
	}

	public void TestJI_NetWeight_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_NetWeightInfo, "Net Weight", "[UCC 6/1] Net Weight");
	}

	public void TestJI_Description_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_DescriptionInfo, "Goods Description", "[UCC 6/8] Goods Description");
	}

	public new void TestZG_CusNumber_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CusNumberInfo, "CUS Code", "[UCC 6/13] CUS Code");
	}

	public void TestJI_Tariff_Caption()
	{
		AssertEquals("[UCC 6/14] Tariff", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_TariffInfo).Caption);
	}

	public void TestJI_ConcessionOrder_Caption()
	{
		AssertEquals("[UCC 8/1] Quota", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_ConcessionOrderInfo).Caption);
	}

	public void TestZG_TransNature_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_TransNatureInfo, "Tran. Nature", "[UCC 8/5] Transaction Nature");
	}

	public void TestJI_CustomsQuantity_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CustomsQuantityInfo, "Customs Qty.", "Customs Quantity");
	}

	public void TestJI_CustomsSecondQuantity_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CustomsSecondQuantityInfo, "Supply Qty.", "Supply Quantity");
	}

	public void TestJI_CustomsThirdQuantity_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CustomsThirdQuantityInfo, "Third Qty.", "Third Quantity");
	}

	public void TestZG_UCRReference()
	{
		AssertEquals("UCR Reference", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ZG_UCRReferenceInfo).Caption);
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be BE", Core.Constants.CountryCodes.Belgium, InvoiceLine.CustomsCountryCode);
	}

	protected override Type GetExpectedEntryInstructionType()
	{
		return typeof(CusEntryInstruction);
	}

	public void TestPropertyAttributes()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		CombineAssertions(() =>
		{
			AssertEquals("JI_StateOrRegionOfOrigin: Caption", "Region of Dispatch", jobComInvoiceLine.JI_StateOrRegionOfOriginInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
			AssertEquals("JI_StateOrRegionOfOrigin: List", "Lookups.RegionOfDispatchList", jobComInvoiceLine.JI_StateOrRegionOfOriginInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("JI_StateOrRegionOfOrigin: MaxLength", 1, jobComInvoiceLine.JI_StateOrRegionOfOriginInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		});
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Belgium, partDetails.CustomsCountryCode);
			AssertEquals(typeof(MasterFiles.OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var typeSupporter = this.InvoiceLine as Integration.Customs.ICusSupportingInfoTypeSupporter;

		CombineAssertions(() =>
		{
			AssertEquals("PreviousDocument type should be overridden in BE", typeof(PreviousDocument), typeSupporter.GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			AssertEquals("AdditionalInfo type should be overridden in BE", typeof(AdditionalInfo), typeSupporter.GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		});
	}

	protected override Type GetExpectedPartType() => typeof(MasterFiles.OrgSupplierPart);
	protected override Type GetExpectedCusEntryLineType() => typeof(CusEntryLine);

	new JobComInvoiceLine InvoiceLine => base.InvoiceLine;

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	public void TestMaxNumberOfAdditionalProcedureCode()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export", 99, invoiceLine.MaxNumberOfAdditionalProcedureCode);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import", 99, invoiceLine.MaxNumberOfAdditionalProcedureCode);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Miscellaneous", 0, invoiceLine.MaxNumberOfAdditionalProcedureCode);
		});
	}

	void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
	}

	public void TestRegionOfDestinationListType()
	{
		AssertType<BERegionList>("Region of destination has to be of correct list type", Factory.New<JobComInvoiceLine>().AddInfoLookups.RegionOfDestinationList);
	}

	public void TestJI_ZZF_NKTaxType_Caption()
	{
		AssertEquals("BTW", CargoWise.EntityFramework.DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().JI_ZZF_NKTaxTypeInfo).Caption);
	}

	public void TestImportInvoiceLineValidation()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<ImportJobComInvoiceLineValidation>(invoiceLine.Validation);
	}

	public void TestExportInvoiceLineValidation()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<JobComInvoiceLineValidation>(invoiceLine.Validation);
	}

	public void TestMiscInvoiceLineValidation()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
		var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<JobComInvoiceLineValidation>(invoiceLine.Validation);
	}

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
	{
		return new BECustomsChargeTypeList();
	}

	protected override void SetUp()
	{
		base.SetUp();

		// change DistributeBy to Value based as in the base test the apportion charge is tested presuming OFT is value based
		IncoTermAndCustomsChargeFactory incoTermAndCustomsChargeFactory = new IncoTermAndCustomsChargeFactory();
		(incoTermAndCustomsChargeFactory.GetCharge("OFT") as CustomsChargeCode).DistributeBy = ChargeDistributeByList.Codes.Value;
	}

	public new void TestChargesToImportForLandedCosting()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoiceHeader = dec.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var chargeTypeList = ((Customs.Business.ICommonInvoice)invoiceLine).ChargeTypeList;

		var containsDiscount = false;
		var containsOverseasFreight = false;
		foreach (CodeDescriptionPair charge in chargeTypeList)
		{
			if (charge.Code == Common.CustomsChargeTypeList.Codes.Discount)
			{
				var lineCharge = invoiceLine.Charges.AddNew(charge.Code, 100m, dec.LocalCurrencyCode);
				containsDiscount = ((Enterprise.MasterFiles.Business.IDefaultLandedCostInput)lineCharge).IsValidToImport;
			}
			else if (charge.Code == Common.CustomsChargeTypeList.Codes.OverseasFreight)
			{
				var lineCharge = invoiceLine.Charges.AddNew(charge.Code, 200m, dec.LocalCurrencyCode);
				containsOverseasFreight = ((Enterprise.MasterFiles.Business.IDefaultLandedCostInput)lineCharge).IsValidToImport;
			}
		}
		invoiceLine.Charges.AddNew("AA!", 400m, dec.LocalCurrencyCode);
		dec.ApportionmentDirty = false;

		int expectedCount = (containsDiscount ? 1 : 0) + (containsOverseasFreight ? 1 : 0);
		var chargesToImportForLandedCosting = ((Enterprise.MasterFiles.Business.ILandedCostChargeHolder)invoiceLine).ChargesToImportForLandedCosting;
		AssertEquals("ChargesToImportForLandedCosting count", expectedCount, chargesToImportForLandedCosting.Count());

		if (containsDiscount)
		{
			var landedCostInput = chargesToImportForLandedCosting.FirstOrDefault(c => c.ChargeDescription.ToLower().Contains(BECustomsChargeTypeList.Descriptions.Discount.ToString().ToLower()));
			AssertEquals("Amount", -100M, landedCostInput.AmountToDistribute.Amount);
		}
		if (containsOverseasFreight)
		{
			var landedCostInput = chargesToImportForLandedCosting.FirstOrDefault(c => c.ChargeDescription.ToLower().Contains(BECustomsChargeTypeList.Descriptions.OverseasFreight.ToString().ToLower()));
			AssertEquals("Amount", 200M, landedCostInput.AmountToDistribute.Amount);
		}
	}

	public void TestDefaultingOfFiscalReferences()
	{
		var invoiceLine = GetInvoiceLineWithEntryInstruction(EUJobMessageTypeList.Codes.Import);

		CombineAssertions(() =>
		{
			invoiceLine.EntryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation;
			invoiceLine.JI_Procedure = "42";
			var fiscalReference = invoiceLine.FiscalReferences.Cast<CusFiscalReference>().FirstOrDefault(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer);
			AssertNotNull("The BE invoice line should contain a FR2 type of fiscal reference for type H1 & procedure 42.", fiscalReference);
			AssertEquals("Fiscal reference should be empty", ZString.Empty, fiscalReference.CFR_Reference);
			invoiceLine.EntryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified;
			AssertEquals("The BE invoice line should contain a FR2 type of fiscal reference for type I1 & procedure 42.", true, invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer));
			invoiceLine.EntryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired;
			AssertEquals("The BE invoice line should contain a FR2 type of fiscal reference for type H6 & procedure 42.", true, invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer));
			invoiceLine.EntryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods;
			AssertEquals("The BE invoice line should not contain a FR2 type of fiscal reference.", false, invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer));

			invoiceLine.JI_Procedure = "";
			invoiceLine.EntryInstruction.CEI_Style = ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation;
			invoiceLine.JI_Procedure = "63";
			AssertEquals("The BE invoice line should contain a FR2 type of fiscal reference for type H1 & procedure 63.", true, invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer));

			invoiceLine.JI_Procedure = "XX";
			AssertEquals("The BE invoice line should not contain a FR2 type of fiscal reference with procedure other than 42 or 63.", false, invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer));
		});
	}

	public void TestCreateNewAdditionalInfoCollection()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<AdditionalInfoCollection>(invoiceLine.AdditionalInfos);
	}

	public void TestGetCusAuthorizationUsages()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>>(invoiceLine.CusAuthorizationUsages);
	}

	public void TestPackagesForInvoiceLines()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<CusLinkPackageCollection>(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
	}

	void AssertCaptionAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, string messageType = "", string expectedShortCaption = "")
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null, [messageType == "IMP" ? JobDeclaration.CaptionKeyImportUCC6 : messageType == "EXP" ? JobDeclaration.CaptionKeyExportUCC6 : null]);
		CombineAssertions(() =>
		{
			if (!string.IsNullOrEmpty(expectedShortCaption))
			{
				AssertEquals($"{propertyInfo.Name} {messageType} ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
			}
			AssertEquals($"{propertyInfo.Name} {messageType} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} {messageType} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		});
	}

	JobComInvoiceLine GetInvoiceLineWithEntryInstruction(string messageType)
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		if (!string.IsNullOrEmpty(messageType))
		{
			jobDeclaration.JE_MessageType = messageType;
		}
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = jobDeclaration.CustomsEntryInstructions.AddNew().PK;
		return invoiceLine;
	}
}
