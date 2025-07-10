using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Moq;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	sealed class ESDocSADHLineTaxImportTest : DocSADHLineTaxTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper() => ESDocSADHLineTaxImport.New(Supporter, Factory);
		public void TestG4_Type()
		{
			var fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "0", "", "", "", "0", "", false, "", "");

			ESDocSADHLineTaxImport wrapper = ESDocSADHLineTaxImport.New(fee, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("G4_Type no Canary Island", "B00", wrapper.G4_Type);
				fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "0", "", "", "", "0", "", true, "", "");
				wrapper = ESDocSADHLineTaxImport.New(fee, Factory);
				AssertEquals("G4_Type Canary Island", "3IG", wrapper.G4_Type);
			});
		}

		public void TestG4_RateOverride()
		{
			var fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "0", "", "%", "", "0", "", false, "", "");
			ESDocSADHLineTaxImport wrapper = ESDocSADHLineTaxImport.New(fee, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Expected % if method of calculation is %", "%", wrapper.G4_RateOverride);

				fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "0", "", "H4", "", "0", "", false, "", "");
				wrapper = ESDocSADHLineTaxImport.New(fee, Factory);
				AssertEquals("Expected E+Rate if method of calculation is not %", "EH4", wrapper.G4_RateOverride);
			});
		}

		public void TestG4_MethodOfPayment()
		{
			var fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "0", "", "", "", "0", FeeMethodOfPayment.Deferred, false, "", "");
			ESDocSADHLineTaxImport wrapper = ESDocSADHLineTaxImport.New(fee, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Expected D if Method of Payment is DEF", "D", wrapper.G4_MethodOfPayment);

				fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "0", "", "", "", "0", "", false, "R", "");
				wrapper = ESDocSADHLineTaxImport.New(fee, Factory);
				AssertEquals("Expected MethodOfPayment if is not DEF and not Canary Island Fee", "R", wrapper.G4_MethodOfPayment);

				fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "0", "", "", "", "0", "", true, "", "A");
				wrapper = ESDocSADHLineTaxImport.New(fee, Factory);
				AssertEquals("Expected MethodOfPayment2 if is not DEF and Canary Island Fee B00", "A", wrapper.G4_MethodOfPayment);

				fee = ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("3RM", "0", "", "", "", "0", "", true, "", "A");
				wrapper = ESDocSADHLineTaxImport.New(fee, Factory);
				AssertEquals("Expected MethodOfPayment2 if is not DEF and Canary Island Fee", "A", wrapper.G4_MethodOfPayment);
			});
		}
	}

	class ESDocSADHLineTaxBoxSupporterTestHelper
	{
		public static IESDocSADHLineTaxBoxSupporter CreateSupporter(ZString type, ZString taxBase, ZString rate, ZString rateDuty, ZString rateOverride, ZString amount, ZString methodOfPayment, ZBool destinationIsCanaryIsland, ZString entryLineMethodOfPayment, ZString entryLineMethodOfPayment2)
		{
			var mockSupporter = new Mock<IESDocSADHLineTaxBoxSupporter>();
			mockSupporter.Setup(m => m.Type).Returns(type);
			mockSupporter.Setup(m => m.TaxBase).Returns(taxBase);
			mockSupporter.Setup(m => m.Rate).Returns(rate);
			mockSupporter.Setup(m => m.RateDuty).Returns(rateDuty);
			mockSupporter.Setup(m => m.RateOverride).Returns(rateOverride);
			mockSupporter.Setup(m => m.AmountInDeclarationCurrency).Returns(amount);
			mockSupporter.Setup(m => m.MethodOfPayment).Returns(methodOfPayment);
			mockSupporter.Setup(m => m.DestinationStateIsCanaryIsland).Returns(destinationIsCanaryIsland);
			mockSupporter.Setup(m => m.EntryLineMethodOfPayment).Returns(entryLineMethodOfPayment);
			mockSupporter.Setup(m => m.EntryLineMethodOfPayment2).Returns(entryLineMethodOfPayment2);
			return mockSupporter.Object;
		}
	}
}
