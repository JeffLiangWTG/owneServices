using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

sealed class DocSADHLineTaxTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper() => DocSADHLineTax.New(Supporter, Factory);

	public void TestConstructor()
	{
		AssertNull(DocSADHLineTax.New(taxSupporter: null, Factory));
		AssertNotNull(DocSADHLineTax.New(taxSupporter: Supporter, Factory));
	}

	public void TestWrapper()
	{
		var supporter = Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "122.12", "RDY", "S", "OVR", "999.99", "2", "A445", "R");
		var wrapper = DocSADHLineTax.New(supporter, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("G4_Amount_InDeclarationCurrency", "999.99", wrapper.G4_Amount_InDeclarationCurrency);
			AssertEquals("G4_MethodOfPayment", "2", wrapper.G4_MethodOfPayment);
			AssertEquals("Box47c1", "RDY", wrapper.Box47c1);
			AssertEquals("G4_RateDuty", "S", wrapper.G4_RateDuty);
			AssertEquals("G4_RateOverride", "OVR", wrapper.G4_RateOverride);
			AssertEquals("Box47b", "122.12", wrapper.Box47b);
			AssertEquals("G4_Type", "A00", wrapper.G4_Type);

			AssertEquals("TaxType", "A445/A00", wrapper.TaxType);
			AssertEquals("TaxMethodOfPayment", "NC/R", wrapper.TaxMethodOfPayment);
		});

		supporter = DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "122.12", "RDY", "S", "OVR", "999.99", "1", "", "R");
		wrapper = DocSADHLineTax.New(supporter, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("G4_Amount_InDeclarationCurrency", "999.99", wrapper.G4_Amount_InDeclarationCurrency);
			AssertEquals("G4_MethodOfPayment", "1", wrapper.G4_MethodOfPayment);
			AssertEquals("Box47c1", "RDY", wrapper.Box47c1);
			AssertEquals("G4_RateDuty", "S", wrapper.G4_RateDuty);
			AssertEquals("G4_RateOverride", "OVR", wrapper.G4_RateOverride);
			AssertEquals("Box47b", "122.12", wrapper.Box47b);
			AssertEquals("G4_Type", "A00", wrapper.G4_Type);

			AssertEquals("TaxType", "    /A00", wrapper.TaxType);
			AssertEquals("TaxMethodOfPayment", "C/R", wrapper.TaxMethodOfPayment);
		});
	}

	public void TestTaxtType()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
		Factory.Save();

		var refCusMapType = Factory.New<RefCusMapType>();
		refCusMapType.ZZP_MapType = RefCusMapTypeList.Codes.FRDTY;
		refCusMapType.ZZP_Direction = "OUT";
		refCusMapType.ZZP_Description = "FR tax code to EU tax code";
		var refCusMap = Factory.New<RefCusMap>();
		refCusMap.ZZM_ZZP_NKMapType = RefCusMapTypeList.Codes.FRDTY;
		refCusMap.ZZM_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
		refCusMap.ZZM_CW1orCommercialValue = "U395";
		refCusMap.ZZM_CustomsValue = "A10";
		refCusMap.ZZM_StartDate = ZDateTime.Now;
		refCusMap.ZZM_EndDate = ZDateTime.Now.AddDays(1);
		Factory.Save();

		var supporter = DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "122.12", "RDY", "S", "OVR", "999.99", "1", "A445", "R");
		var wrapper = DocSADHLineTax.New(supporter, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("G4_Type", "A00", wrapper.G4_Type);
			AssertEquals("TaxType", "A445/A00", wrapper.TaxType);
		});

		supporter = DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "122.12", "RDY", "S", "OVR", "999.99", "1", "", "R");
		wrapper = DocSADHLineTax.New(supporter, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("G4_Type", "A00", wrapper.G4_Type);
			AssertEquals("TaxType", "    /A00", wrapper.TaxType);
		});

		supporter = DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "122.12", "RDY", "S", "OVR", "999.99", "1", "U395", "R");
		wrapper = DocSADHLineTax.New(supporter, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("G4_Type", "A00", wrapper.G4_Type);
			AssertEquals("TaxType", "U395/A10", wrapper.TaxType);
		});
	}

	IDocSADHLineTaxBoxSupporter Supporter
	{
		get
		{
			if (supporter == null)
			{
				supporter = DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "122.12", "RDY", "S", "OVR", "999.99", "A", "A445", "R");
			}
			return supporter;
		}
	}
	IDocSADHLineTaxBoxSupporter supporter;
}
