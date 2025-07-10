using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsSADLineCommonWrapperTest<TLineWrapper> : TestCaseWithFactory
	where TLineWrapper : NctsSADLineCommonWrapper
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("goodsItem mandatory", () => GetNewLineWrapper(goodsItem: null));
		AssertExceptionThrown<ArgumentNullException>("goodsItem.Header mandatory", () => GetNewLineWrapper(goodsItem: Factory.New<NctsDepartureCargoDesc>()));
	}

	public void TestConsignor()
	{
		var organization = GetOrganizationForTest();

		goodsItem.Consignor.E2_OA_Address = organization.MainAddress.PK;
		AssertTraderWrapper(lineWrapper.Consignor);
	}

	public void TestConsignee()
	{
		var organization = GetOrganizationForTest();

		goodsItem.Consignee.E2_OA_Address = organization.MainAddress.PK;
		AssertTraderWrapper(lineWrapper.Consignee);
	}

	public void TestDestinationCountryCode()
	{
		AssertEquals(nameof(lineWrapper.DestinationCountryCode), ZString.Empty, lineWrapper.DestinationCountryCode);

		goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
		AssertEquals(nameof(lineWrapper.DestinationCountryCode), Core.Constants.CountryCodes.Germany, lineWrapper.DestinationCountryCode);
	}

	public void TestSecurityBlock()
	{
		nctsHeader.BH_FTZMove = false;
		AssertType<NctsSADLineEmptySecurityBlockWrapper>($"{nameof(lineWrapper.SecurityBlock)} type when security flag is not ticked", lineWrapper.SecurityBlock);

		nctsHeader.BH_FTZMove = true;
		AssertType<NctsSADLineSecurityBlockWrapper>($"{nameof(lineWrapper.SecurityBlock)} type when security flag is ticked", lineWrapper.SecurityBlock);
	}

	public void TestPackages()
	{
		AssertEquals($"{nameof(lineWrapper.Packages)} count when no packages linked to the goods item", 0, lineWrapper.Packages.Count());

		goodsItem.Packages.AddNew();
		AssertEquals($"{nameof(lineWrapper.Packages)} count when one package linked to the goods item", 1, lineWrapper.Packages.Count());
		AssertType<SADLinePackageWrapper>($"{nameof(lineWrapper.Packages)}[0] type", lineWrapper.Packages.ElementAt(0));
	}

	public void TestContainers()
	{
		AssertEquals($"{nameof(lineWrapper.Containers)} count when no containers linked to the goods item", 0, lineWrapper.Containers.Count());

		var cnt1 = nctsHeader.DepartureHeaderContainers.AddNew();
		cnt1.BC_ContainerNum = "A";
		var cnt2 = nctsHeader.DepartureHeaderContainers.AddNew();
		cnt2.BC_ContainerNum = "B";
		var cnt3 = nctsHeader.DepartureHeaderContainers.AddNew();
		cnt3.BC_ContainerNum = "C";

		var pivot1 = goodsItem.ContainersPivots.AddNew();
		pivot1.Container = cnt1;
		pivot1.ContainerSelected = true;
		var pivot2 = goodsItem.ContainersPivots.AddNew();
		pivot2.Container = cnt2;
		pivot2.ContainerSelected = false;
		var pivot3 = goodsItem.ContainersPivots.AddNew();
		pivot3.Container = cnt3;
		pivot3.ContainerSelected = true;
		AssertArrayEqualsByElements($"{nameof(lineWrapper.Containers)} count when some containers linked to the goods item", new ZString[] { "A", "C" }, lineWrapper.Containers.ToArray());
	}

	public void TestGoodsDescription()
	{
		goodsItem.BY_Description = ZString.Empty;
		AssertEquals(nameof(lineWrapper.GoodsDescription), ZString.Empty, lineWrapper.GoodsDescription);

		goodsItem.BY_Description = "this is the description";
		AssertEquals(nameof(lineWrapper.GoodsDescription), "this is the description", lineWrapper.GoodsDescription);
	}

	public void TestItemNumber()
	{
		goodsItem.BY_LineNo = 1;
		AssertEquals(nameof(lineWrapper.ItemNumber), 1, lineWrapper.ItemNumber);
	}

	public void TestCombinedNomenclature()
	{
		goodsItem.BY_HarmonisedTariff = ZString.Empty;
		AssertEquals(nameof(lineWrapper.CombinedNomenclature), ZString.Empty, lineWrapper.CombinedNomenclature);

		goodsItem.BY_HarmonisedTariff = "12345678";
		AssertEquals(nameof(lineWrapper.CombinedNomenclature), "12345678", lineWrapper.CombinedNomenclature);

		goodsItem.BY_HarmonisedTariff = "1234567890";
		AssertEquals(nameof(lineWrapper.CombinedNomenclature), "12345678", lineWrapper.CombinedNomenclature);
	}

	public void TestAdditionalCodes()
	{
		AssertEquals($"When no {nameof(goodsItem.AdditionalSupplementaryCodes)} are entered, {nameof(lineWrapper.AdditionalCodes)} count", 0, lineWrapper.AdditionalCodes.Count());

		goodsItem.AdditionalSupplementaryCodes.AddNew("1111");
		goodsItem.AdditionalSupplementaryCodes.AddNew("2222");
		goodsItem.AdditionalSupplementaryCodes.AddNew("");
		goodsItem.AdditionalSupplementaryCodes.AddNew("2222");
		goodsItem.AdditionalSupplementaryCodes.AddNew(" ");
		AssertContainsExactElementsInAnyOrder($"When {nameof(goodsItem.AdditionalSupplementaryCodes)} are entered, {nameof(lineWrapper.AdditionalCodes)}", new string[] { "1111", "2222" }, lineWrapper.AdditionalCodes);
	}

	public void TestCountryOfOrigin()
	{
		goodsItem.BY_RW_NKOriginState = ZString.Empty;
		AssertEquals(nameof(lineWrapper.CountryOfOrigin), ZString.Empty, lineWrapper.CountryOfOrigin);

		goodsItem.BY_RW_NKOriginState = "PD";
		AssertEquals(nameof(lineWrapper.CountryOfOrigin), "PD", lineWrapper.CountryOfOrigin);
	}

	public void TestGrossMass()
	{
		goodsItem.BY_GrossWeight = ZDecimal.Zero;
		AssertEquals(nameof(lineWrapper.GrossMass), ZDecimal.Zero, lineWrapper.GrossMass);

		goodsItem.BY_GrossWeight = 1m;
		goodsItem.BY_GrossWeightUnit = "KG";
		AssertEquals(nameof(lineWrapper.GrossMass), 1m, lineWrapper.GrossMass);

		goodsItem.BY_GrossWeight = 2000m;
		goodsItem.BY_GrossWeightUnit = "G";
		AssertEquals(nameof(lineWrapper.GrossMass), 2m, lineWrapper.GrossMass);
	}

	public void TestProcedure()
	{
		goodsItem.BY_Procedure = ZString.Empty;
		AssertEquals(nameof(lineWrapper.Procedure), ZString.Empty, lineWrapper.Procedure);

		goodsItem.BY_Procedure = "4000";
		AssertEquals(nameof(lineWrapper.Procedure), "4000", lineWrapper.Procedure);
	}

	public void TestNationalProcedures()
	{
		AssertEquals($"{nameof(lineWrapper.NationalProcedures)} count", 0, lineWrapper.NationalProcedures.Count());
	}

	public void TestNetMass()
	{
		goodsItem.BY_NetWeight = ZDecimal.Zero;
		AssertNull(nameof(lineWrapper.NetMass), lineWrapper.NetMass);

		goodsItem.BY_NetWeight = 1m;
		goodsItem.BY_NetWeightUnit = "KG";
		AssertEquals(nameof(lineWrapper.NetMass), 1m, lineWrapper.NetMass);

		goodsItem.BY_NetWeight = 2000m;
		goodsItem.BY_NetWeightUnit = "G";
		AssertEquals(nameof(lineWrapper.NetMass), 2m, lineWrapper.NetMass);
	}

	public void TestPreviousAdministrativeDocument()
	{
		AssertType<SADEmptyPreviousDocumentWrapper>($"{nameof(lineWrapper.PreviousAdministrativeDocument)} type when no previous documents are found", lineWrapper.PreviousAdministrativeDocument);

		goodsItem.PreviousDocuments.AddNew();
		AssertType<SADPreviousDocumentWrapper>($"{nameof(lineWrapper.PreviousAdministrativeDocument)} type when only 1 previous document is found", lineWrapper.PreviousAdministrativeDocument);

		goodsItem.PreviousDocuments.AddNew();
		AssertType<SADPreviousDocumentM2IndicatorWrapper>($"{nameof(lineWrapper.PreviousAdministrativeDocument)} type when more than 1 previous documents are found", lineWrapper.PreviousAdministrativeDocument);
	}

	public void TestSupplementaryUnit()
	{
		AssertNull("Supplementary Unit should be null", lineWrapper.SupplementaryUnit);

		goodsItem.BY_CustomsSecondQuantity = 200m;
		AssertEquals("Supplementary Unit should be", 200m, lineWrapper.SupplementaryUnit);

		goodsItem.BY_CustomsSecondQuantity = 0m;
		AssertNull("When CustomsSecondQuantity=0, Supplementary Unit should be null", lineWrapper.SupplementaryUnit);
	}

	public void TestSpecialMentionGroup()
	{
		var specialMentionGroup = lineWrapper.SpecialMentionGroup;
		AssertNotNull(nameof(specialMentionGroup), specialMentionGroup);
		AssertEquals($"{nameof(lineWrapper.SpecialMentionGroup)} type", ExpectedSpecialMentionGroup, specialMentionGroup.GetType());
	}

	public void TestCertificates()
	{
		AssertEquals($"{nameof(lineWrapper.Certificates)} count when no supporting documents linked to the goods item", 0, lineWrapper.Certificates.Count());

		goodsItem.SupportingDocuments.AddNew();
		AssertEquals($"{nameof(lineWrapper.Certificates)} count when one supporting document linked to the goods item", 1, lineWrapper.Certificates.Count());
		AssertType<SADCertificateWrapper>($"{nameof(lineWrapper.Certificates)}[0] type", lineWrapper.Certificates.ElementAt(0));
	}

	public void TestComplementOfInformation()
	{
		AssertEquals(nameof(lineWrapper.ComplementOfInformation), ZString.Empty, lineWrapper.ComplementOfInformation);
	}

	public void TestComplementOfInformationLng()
	{
		AssertEquals(nameof(lineWrapper.ComplementOfInformationLng), ZString.Empty, lineWrapper.ComplementOfInformationLng);
	}

	public void TestNotes()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When Remarks field is empty", ZString.Empty, lineWrapper.Notes);

			goodsItem.Remarks = "hello this is a remark";
			AssertEquals("When Remarks field contains only allowed chars", "hello this is a remark", lineWrapper.Notes);

			goodsItem.Remarks = "hello\tthis\r\nis\ra\nremark";
			AssertEquals("When Remarks field also contains not allowed chars", "hello this is a remark", lineWrapper.Notes);
		});
	}

	public void TestStatisticalValueAmount()
	{
		AssertNull(nameof(lineWrapper.StatisticalValueAmount), lineWrapper.StatisticalValueAmount);
	}

	public void TestDuties()
	{
		AssertEquals($"{nameof(lineWrapper.Duties)} count when no fees linked to the goods item", 0, lineWrapper.Duties.Count());

		var fee = goodsItem.Fees.AddNew();
		fee.BFE_ChargeType = "A";
		fee.BFE_BaseValue = 0.00m;
		fee.BFE_Rate = 0.000000m;
		fee.BFE_MethodOfCalculation = "%";
		fee.BFE_ChargeAmount = 0.00m;
		fee.BFE_MethodOfPayment = "A";

		AssertEquals($"{nameof(lineWrapper.Duties)} count when one fee linked to the goods item", 1, lineWrapper.Duties.Count());
		var feeWrapper = lineWrapper.Duties.ElementAt(0);
		AssertType<SADDutyTaxFeeWrapper>($"{nameof(lineWrapper.Duties)}[0] type", feeWrapper);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(feeWrapper.Type), "A", feeWrapper.Type);
			AssertEquals(nameof(feeWrapper.Base), 0.00m, feeWrapper.Base);
			AssertEquals(nameof(feeWrapper.CalculationFactor1), "X", feeWrapper.CalculationFactor1);
			AssertEquals(nameof(feeWrapper.Rate1), 0.000000m, feeWrapper.Rate1);
			AssertEquals(nameof(feeWrapper.CalculationFactor2), "%", feeWrapper.CalculationFactor2);
			AssertEquals(nameof(feeWrapper.Rate2), null, feeWrapper.Rate2);
			AssertEquals(nameof(feeWrapper.CalculationFactor3), ZString.Empty, feeWrapper.CalculationFactor3);
			AssertEquals(nameof(feeWrapper.Rate3), null, feeWrapper.Rate3);
			AssertEquals(nameof(feeWrapper.CalculationFactor4), ZString.Empty, feeWrapper.CalculationFactor4);
			AssertEquals(nameof(feeWrapper.Amount), 0.00m, feeWrapper.Amount);
			AssertEquals(nameof(feeWrapper.MethodOfPayment), "A", feeWrapper.MethodOfPayment);
		});

		fee.BFE_BaseValue = 10m;
		fee.BFE_Rate = 1m;
		fee.BFE_ChargeAmount = 112.12m;
		CombineAssertions(() =>
		{
			AssertEquals(nameof(feeWrapper.Type), "A", feeWrapper.Type);
			AssertEquals(nameof(feeWrapper.Base), 10m, feeWrapper.Base);
			AssertEquals(nameof(feeWrapper.CalculationFactor1), "X", feeWrapper.CalculationFactor1);
			AssertEquals(nameof(feeWrapper.Rate1), 1m, feeWrapper.Rate1);
			AssertEquals(nameof(feeWrapper.CalculationFactor2), "%", feeWrapper.CalculationFactor2);
			AssertEquals(nameof(feeWrapper.Rate2), null, feeWrapper.Rate2);
			AssertEquals(nameof(feeWrapper.CalculationFactor3), ZString.Empty, feeWrapper.CalculationFactor3);
			AssertEquals(nameof(feeWrapper.Rate3), null, feeWrapper.Rate3);
			AssertEquals(nameof(feeWrapper.CalculationFactor4), ZString.Empty, feeWrapper.CalculationFactor4);
			AssertEquals(nameof(feeWrapper.Amount), 112.12m, feeWrapper.Amount);
			AssertEquals(nameof(feeWrapper.MethodOfPayment), "A", feeWrapper.MethodOfPayment);
		});
	}

	public void TestExcludedDuties()
	{
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.ImmediatePaymentInCashA);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentCustomsProcedureF);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE);

		lineWrapper = GetNewLineWrapper(goodsItem);
		AssertNotNull("Duties should not be null", lineWrapper.Duties);
		AssertEquals("Duties count", 2, lineWrapper.Duties.Count());

		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemporaryAntiDumpingDuty);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemportaryCountervailingDuty);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR);

		lineWrapper = GetNewLineWrapper(goodsItem);
		AssertNotNull("Duties should not be null", lineWrapper.Duties);
		AssertEquals("Duties count", 4, lineWrapper.Duties.Count());

		void AddLineFee(NctsDepartureCargoDesc goodsItem, ZString methodOfPayment, string chargeType = null)
		{
			var lineFee = goodsItem.Fees.AddNew();
			lineFee.BFE_MethodOfPayment = methodOfPayment;
			lineFee.BFE_ChargeType = chargeType;
		}
	}

	public void TestDutiesOrder()
	{
		SetUpFees(goodsItem.Fees, "A35", "A30", "A20", "A10", "A00");
		AssertEquals("entryLine.Fees count", 5, goodsItem.Fees.Count);

		var lineWrapper = GetNewLineWrapper(goodsItem);
		var wrappedDuties = lineWrapper.Duties;
		AssertNotNull("lineWrapper.Duties not null", wrappedDuties);
		AssertEquals("lineWrapper.Duties count", 5, wrappedDuties.Count());
		AssertArrayEqualsByElements("lineWrapper.Duties order", new ZString[] { "A00", "A10", "A20", "A30", "A35" }, wrappedDuties.Select(x => x.Type).ToArray());
	}

	void SetUpFees(NctsCargoDescFeeCollection feeCollection, params ZString[] rateCodesToAdd)
	{
		foreach (var rateCode in rateCodesToAdd)
		{
			var newFee = feeCollection.AddNew();
			newFee.BFE_ChargeType = rateCode;
			newFee.BFE_MethodOfPayment = "A";
		}
	}

	public void TestTotalItemTaxedAmount()
	{
		AssertEquals(nameof(lineWrapper.TotalItemTaxedAmount), 0.00m, lineWrapper.TotalItemTaxedAmount);

		var fee1 = goodsItem.Fees.AddNew();
		fee1.BFE_ChargeAmount = 10m;
		fee1.BFE_MethodOfPayment = "A";
		var fee2 = goodsItem.Fees.AddNew();
		fee2.BFE_ChargeAmount = 12.99m;
		fee2.BFE_MethodOfPayment = "A";
		AssertEquals(nameof(lineWrapper.TotalItemTaxedAmount), 22.99m, lineWrapper.TotalItemTaxedAmount);
	}

	public void TestGrandTotalTaxedAmount()
	{
		AssertEquals("GrandTotalTaxedAmount when there are no fees", 0.00m, lineWrapper.GrandTotalTaxedAmount);

		var goodsItem1 = nctsMovementHeader.GoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		var fee1 = goodsItem1.Fees.AddNew();
		fee1.BFE_ChargeAmount = 40.3485m;
		fee1.BFE_MethodOfPayment = "A";

		var goodsItem2 = nctsMovementHeader.GoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;
		var fee2 = goodsItem2.Fees.AddNew();
		fee2.BFE_ChargeAmount = 10.455m;
		fee2.BFE_MethodOfPayment = "A";

		var goodsItem3 = nctsMovementHeader.GoodsItems.AddNew();
		goodsItem3.BY_LineNo = 3;
		var fee31 = goodsItem3.Fees.AddNew();
		fee31.BFE_ChargeAmount = 20.9999m;
		fee31.BFE_MethodOfPayment = "A";
		var fee32 = goodsItem3.Fees.AddNew();
		fee32.BFE_ChargeAmount = 30.578m;
		fee32.BFE_MethodOfPayment = "A";

		AssertEquals($"GrandTotalTaxedAmount on {goodsItem1}", null, new TransitLineWrapper(goodsItem1).GrandTotalTaxedAmount);
		AssertEquals($"GrandTotalTaxedAmount on {goodsItem2}", null, new TransitLineWrapper(goodsItem2).GrandTotalTaxedAmount);
		AssertEquals($"GrandTotalTaxedAmount on {goodsItem3}", 102.39m, new TransitLineWrapper(goodsItem3).GrandTotalTaxedAmount);

		goodsItem1.Fees[0].BFE_ChargeAmount = 0m;
		goodsItem2.Fees[0].BFE_ChargeAmount = 0m;
		goodsItem3.Fees[0].BFE_ChargeAmount = 0m;
		goodsItem3.Fees[1].BFE_ChargeAmount = 0m;
		AssertEquals($"GrandTotalTaxedAmount on {goodsItem3}", 0.00m, new TransitLineWrapper(goodsItem3).GrandTotalTaxedAmount);
	}

	protected abstract TLineWrapper GetNewLineWrapper(NctsDepartureCargoDesc goodsItem);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		goodsItem = nctsMovementHeader.GoodsItems.AddNew();
		lineWrapper = GetNewLineWrapper(goodsItem);
	}

	protected NctsHeader nctsHeader;
	protected NctsDepartureMovementHeader nctsMovementHeader;
	protected NctsDepartureCargoDesc goodsItem;
	protected TLineWrapper lineWrapper;

	#region Implementation

	void AssertTraderWrapper(ITrader trader)
	{
		AssertNotNull(nameof(trader), trader);
		CombineAssertions(() =>
		{
			AssertType<SADTraderWrapper>($"{nameof(trader)} type", trader);
			AssertEquals(nameof(trader.IdCountryCode), "IT", trader.IdCountryCode);
			AssertEquals(nameof(trader.ID), "385040449", trader.ID);
			AssertEquals(nameof(trader.Name), "IKEA", trader.Name);
			AssertEquals(nameof(trader.Address), "MAIN ADDRESS", trader.Address);
			AssertEquals(nameof(trader.Postcode), "4000", trader.Postcode);
			AssertEquals(nameof(trader.City), "ABCEXPMEL", trader.City);
			AssertEquals(nameof(trader.CountryCode), "ZA", trader.CountryCode);
		});
	}

	OrgHeader GetOrganizationForTest()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("EOR", "385040449", "IT");
		orgHeader.MainAddress.CompanyName = "IKEA";
		orgHeader.MainAddress.Address1 = "MAIN";
		orgHeader.MainAddress.Address2 = "ADDRESS";
		orgHeader.MainAddress.Postcode = "4000";
		orgHeader.MainAddress.City = "ABCEXPMEL";
		orgHeader.MainAddress.OA_RN_NKCountryCode = "ZA";
		return orgHeader;
	}

	protected abstract Type ExpectedSpecialMentionGroup { get; }

	#endregion
}
