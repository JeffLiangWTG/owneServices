using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(ITDocSADHLineTaxCollection))]
sealed class ITDocSADHLineTaxCollectionTest : DocSADHLineTaxCollectionTest<ITDocSADHLineTaxCollection>
{
	public void TestTotalAmountInDeclarationCurrencyIT()
	{
		var lineTaxCollection = new ITDocSADHLineTaxCollection(ITSupporters, Factory);
		CombineAssertions("[PRE-CONDITIONS]", () =>
		{
			AssertEquals("Collection count", 7, lineTaxCollection.Count);
			AssertITDocSADHLineTax(lineTaxCollection, index: 0, expectedAmount: "1.11");
			AssertITDocSADHLineTax(lineTaxCollection, index: 1, expectedAmount: "2.22");
			AssertITDocSADHLineTax(lineTaxCollection, index: 2, expectedAmount: "3.33");
			AssertITDocSADHLineTax(lineTaxCollection, index: 3, expectedAmount: "4.44");
			AssertITDocSADHLineTax(lineTaxCollection, index: 4, expectedAmount: "5.55");
			AssertITDocSADHLineTax(lineTaxCollection, index: 5, expectedAmount: "6.66");
			AssertITDocSADHLineTax(lineTaxCollection, index: 6, expectedAmount: "7.77");
		});

		AssertEquals("Total amount, exclude Method Of Payment(MP) in [ O, R, S, U, V ]", "14.43", lineTaxCollection.TotalAmountInDeclarationCurrency);
	}

	protected override ITDocSADHLineTaxCollection GetNewDocumentWrapperCollection() => new ITDocSADHLineTaxCollection(ITSupporters, Factory);

	List<IDocSADHLineTaxBoxSupporter> ITSupporters
	{
		get
		{
			if (supporters == null)
			{
				supporters = new List<IDocSADHLineTaxBoxSupporter>
				{
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter(type: "B10",taxBase: "100.00",rate: "RDY",rateDuty: "S",rateOverride: "OVR",amount: "1.11",methodOfPayment: "O"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter(type: "D10",taxBase: "678.90",rate: "RDY",rateDuty: "S",rateOverride: "OVR",amount: "2.22",methodOfPayment: "R"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter(type: "A00",taxBase: "122.12",rate: "RDY",rateDuty: "S",rateOverride: "OVR",amount: "3.33",methodOfPayment: "S"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter(type: "A00",taxBase: "122.12",rate: "RDY",rateDuty: "S",rateOverride: "OVR",amount: "4.44",methodOfPayment: "U"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter(type: "A00",taxBase: "122.12",rate: "RDY",rateDuty: "S",rateOverride: "OVR",amount: "5.55",methodOfPayment: "V"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter(type: "A00",taxBase: "122.12",rate: "RDY",rateDuty: "S",rateOverride: "OVR",amount: "6.66",methodOfPayment: "A"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter(type: "A00",taxBase: "122.12",rate: "RDY",rateDuty: "S",rateOverride: "OVR",amount: "7.77",methodOfPayment: "G"),
				};
			}
			return supporters;
		}
	}
	List<IDocSADHLineTaxBoxSupporter> supporters;

	void AssertITDocSADHLineTax(ITDocSADHLineTaxCollection lineTaxCollection, int index, string expectedAmount)
	{
		AssertEquals($"'{lineTaxCollection[index].G4_Type},{lineTaxCollection[index].G4_MethodOfPayment}' amount", expectedAmount, lineTaxCollection[index].G4_Amount_InDeclarationCurrency);
	}
}
