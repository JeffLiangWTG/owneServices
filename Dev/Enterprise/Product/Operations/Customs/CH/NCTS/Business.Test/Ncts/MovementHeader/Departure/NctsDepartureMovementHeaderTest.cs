using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeader))]
sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestMessages()
	{
		var messages = DepartureMovement.Messages;
		AssertEquals(DepartureMovement, messages.Master);
	}

	public void TestOnSaving_GoodsItemSupplyChainActorReferences_deleted() => CombineAssertions(() =>
	{
		var bill1 = DepartureMovement.Header.Bills.AddNew();
		var bill2 = DepartureMovement.Header.Bills.AddNew();
		var reference1 = AddNewCusSupplyChainActorReferences(bill1);
		var reference2 = AddNewCusSupplyChainActorReferences(bill2);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;

		Factory.Save();
		AssertCount(1);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;

		Factory.Save();
		AssertCount(0);
		Assert("CusSupplyChainActorReference deleted", reference1.IsDeleted);
		Assert("CusSupplyChainActorReference deleted", reference2.IsDeleted);

		void AssertCount(int expectedCount)
		{
			AssertEquals($"Bill 1 BM_InBondEntryType={DepartureMovement.BM_InBondEntryType}", expectedCount, bill1.GoodsItems.FirstOrDefault().CusSupplyChainActorReferences.Count);
			AssertEquals($"Bill 2 BM_InBondEntryType={DepartureMovement.BM_InBondEntryType}", expectedCount, bill2.GoodsItems.FirstOrDefault().CusSupplyChainActorReferences.Count);
		}

		CusSupplyChainActorReference AddNewCusSupplyChainActorReferences(NctsBill bill)
		{
			var reference = bill.GoodsItems.AddNew().CusSupplyChainActorReferences.AddNew();
			reference.CFR_Code = SupplyChainActorRoleList.Codes.MF;
			reference.CFR_Reference = "1";
			return reference;
		}
	});

	public void TestOnSaving_HeaderSupplyChainActorReferences_deleted() => CombineAssertions(() =>
	{
		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		var supplyChainActor = DepartureMovement.CusSupplyChainActors.AddNew();
		supplyChainActor.CFR_Code = SupplyChainActorRoleList.Codes.MF;
		supplyChainActor.CFR_Reference = "1";
		Factory.Save();
		AssertEquals($"Count - BM_InBondEntryType={DepartureMovement.BM_InBondEntryType}", 1, DepartureMovement.CusSupplyChainActors.Count);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		Factory.Save();
		AssertEquals($"Count - BM_InBondEntryType={DepartureMovement.BM_InBondEntryType}", 0, DepartureMovement.CusSupplyChainActors.Count);
		Assert("CusSupplyChainActors deleted", supplyChainActor.IsDeleted);
	});

	public void TestOnSaving_CountriesOfRouting_deleted() => CombineAssertions(() =>
	{
		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		var countryOfRouting = DepartureMovement.Header.CountriesOfRouting.AddNew();
		Factory.Save();
		AssertEquals($"Count - BM_InBondEntryType={DepartureMovement.BM_InBondEntryType}", 1, DepartureMovement.Header.CountriesOfRouting.Count);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		Factory.Save();
		AssertEquals($"Count - BM_InBondEntryType={DepartureMovement.BM_InBondEntryType}", 0, DepartureMovement.Header.CountriesOfRouting.Count);
		Assert("CountryOfRouting deleted", countryOfRouting.IsDeleted);
	});

	[TestDate(2022, 12, 22)]
	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedWhenEmpty()
	{
		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "1000088059", "CH");

			CombineAssertions(() =>
			{
				AssertEquals("BM_PaperlessInbondNum empty before saved", ZString.Empty, DepartureMovement.BM_PaperlessInbondNum);
				Factory.Save();
				AssertEquals("BM_PaperlessInbondNum automatically generated after saved", "2210000880590000000001", DepartureMovement.BM_PaperlessInbondNum);
			});
		}
	}

	[TestDate(2022, 12, 22)]
	public void TestBM_PaperlessInbondNum_NotAutomaticallyGeneratedWhenNctsIsManualCustomerReferenceEnabled()
	{
		using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "1000088059", "CH");

			CombineAssertions(() =>
			{
				AssertEquals("BM_PaperlessInbondNum empty before saved", ZString.Empty, DepartureMovement.BM_PaperlessInbondNum);
				Factory.Save();
				AssertEquals("BM_PaperlessInbondNum not automatically generated after saved", ZString.Empty, DepartureMovement.BM_PaperlessInbondNum);
			});
		}
	}

	public void TestValuationDate()
	{
		CombineAssertions(() =>
		{
			AssertEquals("BM_ValuationDate empty", ZDateTime.Today, DepartureMovement.ValuationDate);

			var valuationDate = new ZDateTime(2023, 02, 07);
			DepartureMovement.BM_ValuationDate = valuationDate;

			AssertEquals("BM_ValuationDate entered", valuationDate, DepartureMovement.ValuationDate);
		});
	}

	public void TestBM_SpecificCircumstance()
	{
		AssertEquals("Max Length", 3, DepartureMovement.BM_SpecificCircumstanceInfo.MaxLength);
	}

	public void TestBM_ExportTimeLimit()
	{
		AssertEquals("Max Length", 2, DepartureMovement.BM_ExportTimeLimitInfo.MaxLength);
	}

	public void TestICusGoodsLocationProvider_ProviderKey()
	{
		AssertEquals("CHNCTS", (DepartureMovement as ICusGoodsLocationProvider).ProviderKey);
	}

	public void TestRepresentative_ReadOnly()
	{
		AssertEquals("Representative ReadOnly", true, DepartureMovement.Representative.ReadOnly);
	}

	public void TestRepresentative_Default() => CombineAssertions(() =>
	{
		var branchOrgHeader = Factory.New<OrgHeader>();
		branchOrgHeader.OH_Code = "XXX";
		branchOrgHeader.Addresses.AddNewMainAddress();
		Factory.Save();

		GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgHeader.PK;
		AssertRepresentativeWithSave(nameof(GlbBranch.CurrentBranch), GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK);

		GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
		AssertNotNull("Pre-condition - company has address", GlbCompany.CurrentCompany.OrgProxy?.MainAddress);
		AssertRepresentativeWithSave(nameof(GlbCompany.CurrentCompany), GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK);
	});

	public void TestIsNationalTransitSwitzerland() => CombineAssertions(() =>
	{
		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertEquals("Is NationalTransitSwitzerland", true, DepartureMovement.IsNationalTransitSwitzerland);
		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertEquals("Not NationalTransitSwitzerland", false, DepartureMovement.IsNationalTransitSwitzerland);
	});

	public void TestIsUniformCountryOfDestination() => CombineAssertions(() =>
	{
		var bill1 = DepartureMovement.Header.Bills.AddNew();
		var bill2 = DepartureMovement.Header.Bills.AddNew();
		var item11 = bill1.GoodsItems.AddNew();
		var item12 = bill1.GoodsItems.AddNew();
		var item21 = bill2.GoodsItems.AddNew();

		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		AssertResult(true, country1, noCountry, noCountry, noCountry, noCountry, noCountry);
		AssertResult(true, country1, country1, noCountry, noCountry, noCountry, noCountry);
		AssertResult(true, country1, noCountry, country1, noCountry, noCountry, noCountry);
		AssertResult(true, noCountry, country1, country1, noCountry, noCountry, country1);
		AssertResult(true, noCountry, country1, noCountry, noCountry, country1, noCountry);
		AssertResult(true, noCountry, country1, noCountry, noCountry, noCountry, country1);
		AssertResult(true, noCountry, noCountry, country1, country1, noCountry, country1);
		AssertResult(true, country1, country2, noCountry, noCountry, country2, noCountry);
		AssertResult(true, country1, noCountry, country2, country2, country2, noCountry);
		AssertResult(false, noCountry, noCountry, country1, country2, noCountry, noCountry);
		AssertResult(false, noCountry, country1, noCountry, noCountry, country2, noCountry);
		AssertResult(false, noCountry, country1, noCountry, noCountry, noCountry, country2);

		void AssertResult(bool expectedResult, ZString headerCountry, ZString bill1Country, ZString item11Country, ZString item12Country, ZString bill2Country, ZString item21Country)
		{
			DepartureMovement.BM_RL_NKDestinationPort = headerCountry;
			bill1.B0_RN_NKCountryOfDestination = bill1Country;
			item11.BY_RN_NKCountryOfDestination = item11Country;
			item12.BY_RN_NKCountryOfDestination = item12Country;
			bill2.B0_RN_NKCountryOfDestination = bill2Country;
			item21.BY_RN_NKCountryOfDestination = item21Country;
			AssertEquals($"header({headerCountry}) bill1({bill1Country}) item11({item11Country}) item12({item12Country}) bill2({bill2Country}) item21({item21Country})", expectedResult, DepartureMovement.IsUniformCountryOfDestination);
		}
	});

	void AssertRepresentativeWithSave(string origin, ZGuid expectedAddressPk)
	{
		var departureOriginal = GetNewBusinessObjectForDeleteTest(Factory) as NctsDepartureMovementHeader;

		AssertEquals($"From {origin} before save", expectedAddressPk, departureOriginal.Representative?.E2_OA_Address);
		Factory.Save();

		var departureLoaded = new BusinessObjectFactory().Load<NctsDepartureMovementHeader>(departureOriginal.PK);
		AssertEquals($"From {origin} after load", expectedAddressPk, departureLoaded.Representative?.E2_OA_Address);
	}

	public void TestDepartureCustomerReferenceNumberFountain()
	{
		AssertEquals("Departure Number Fountain", "CHLocalReferenceNumber", (DepartureMovement as ILRNGenerator).LrnNumberFountain.Name);
	}

	public void TestBM_MethodOfPayment_ReadOnly()
	{
		CombineAssertions(() =>
		{
			DepartureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals($"Type Of Security is ENT.", false, DepartureMovement.BM_MethodOfPaymentInfo.ReadOnly);

			DepartureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals($"Type Of Security is NON.", false, DepartureMovement.BM_MethodOfPaymentInfo.ReadOnly);
		});
	}

	public void TestBM_TypeOfSecurity_BM_InBondEntryType_Changed() => CombineAssertions(() =>
	{
		DepartureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		AssertBM_TypeOfSecurity(NctsTypeOfSecurityList.Codes.ENT, false);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertBM_TypeOfSecurity(NctsTypeOfSecurityList.Codes.NON, true);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		AssertBM_TypeOfSecurity(NctsTypeOfSecurityList.Codes.NON, false);

		void AssertBM_TypeOfSecurity(ZString expectedValue, bool expectedReadOnly)
		{
			AssertEquals("BM_TypeOfSecurity value", expectedValue, DepartureMovement.BM_TypeOfSecurity);
			AssertEquals("BM_TypeOfSecurityInfo ReadOnly", expectedReadOnly, DepartureMovement.BM_TypeOfSecurityInfo.ReadOnly);
		}
	});

	public void TestRelatedExportEntryHeaders()
	{
		AssertType<RelatedExportEntryHeaderGenPivot>(DepartureMovement.RelatedExportEntryHeaders.AddNew());
		AssertSame("cached", DepartureMovement.RelatedExportEntryHeaders, DepartureMovement.RelatedExportEntryHeaders);
	}

	public void TestCusReferenceTypeSupporter()
	{
		ICusReferenceTypeSupporter supporter = DepartureMovement;
		AssertEquals(typeof(CusSupplyChainActorReference), supporter.GetCusReferenceTypes()[CusReferenceTypeList.Codes.SupplyChainActor]);
	}

	public void TestGuarantees() => AssertType<NctsGuarantee>(DepartureMovement.Guarantees.AddNew());

	public void TestRelatedExportMergedLinesCount() => CombineAssertions(() =>
	{
		CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.MergedLines.AddNew();
		DepartureMovement.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
		AssertEquals(1, DepartureMovement.RelatedExportMergedLinesCount);
		entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.MergedLines.AddNew();
		DepartureMovement.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
		AssertEquals(2, DepartureMovement.RelatedExportMergedLinesCount);
		entryHeader.MergedLines.AddNew();
		AssertEquals(3, DepartureMovement.RelatedExportMergedLinesCount);
	});

		public void TestSupportingDocuments() => AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(DepartureMovement.SupportingDocuments);

		protected override BusinessObject GetNewBusinessObject() => DepartureMovement;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => DepartureMovement;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
	{
		return new LightValidationTesterExcludingJobDocAddress(bizObjToTest);
	}

	NctsDepartureMovementHeader DepartureMovement => departureMovement ?? (departureMovement = GetNewBusinessObjectForDeleteTest(Factory) as NctsDepartureMovementHeader);
	NctsDepartureMovementHeader departureMovement;
}

class LightValidationTesterExcludingJobDocAddress : LightValidationTester
{
	public LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : base(bo)
	{
	}

	protected override bool ShouldTestProperty(ZPropertyInfo info)
	{
		var objectType = info.BizObj.GetType();
		if (objectType == typeof(CusGoodsLocation) || objectType == typeof(CusGoodsLocationAddress) || objectType == typeof(JobDocAddress))
		{
			return false;
		}
		else
		{
			return base.ShouldTestProperty(info);
		}
	}
}
