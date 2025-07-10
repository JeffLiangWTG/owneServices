using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	public class DocSADHLineTaxTest : DocumentWrappers.Testing.DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper() => DocSADHLineTax.New(Supporter, Factory);

		public void TestConstructor()
		{
			AssertNull(DocSADHLineTax.New(taxSupporter: null, Factory));
			AssertNotNull(DocSADHLineTax.New(taxSupporter: Supporter, Factory));
		}

		public void TestWrapper()
		{
			var wrapper = DocSADHLineTax.New(Supporter, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("G4_Amount_InDeclarationCurrency", "999.99", wrapper.G4_Amount_InDeclarationCurrency);
				AssertEquals("G4_MethodOfPayment", "A", wrapper.G4_MethodOfPayment);
				AssertEquals("Box47c1", "RDY", wrapper.Box47c1);
				AssertEquals("G4_RateDuty", "S", wrapper.G4_RateDuty);
				AssertEquals("G4_RateOverride", "OVR", wrapper.G4_RateOverride);
				AssertEquals("Box47b", "122.12", wrapper.Box47b);
				AssertEquals("G4_Type", "A00", wrapper.G4_Type);

				AssertEquals("G4_Type", "A00", wrapper.TaxType);
				AssertEquals("G4_MethodOfPayment", "A", wrapper.TaxMethodOfPayment);
			});
		}

		public IDocSADHLineTaxBoxSupporter Supporter
		{
			get
			{
				if (supporter == null)
				{
					supporter = DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "122.12", "RDY", "S", "OVR", "999.99", "A");
				}
				return supporter;
			}
		}
		IDocSADHLineTaxBoxSupporter supporter;
	}
}
