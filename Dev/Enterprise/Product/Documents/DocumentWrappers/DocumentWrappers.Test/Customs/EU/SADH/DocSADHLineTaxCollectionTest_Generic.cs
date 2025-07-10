using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	public abstract class DocSADHLineTaxCollectionTest<T> : DocumentWrappers.Testing.DocBaseWrapperCollectionTest<T> where T : DocSADHLineTaxCollection
	{
		protected override object GetNewObjectToWrap() => null;

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			var result = DocSADHLineTax.New(Supporters[0], Factory);
			collection.Add(result);
			return result;
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DocSADHLineTaxCollection(null, Factory));
			AssertNoExceptionThrown(() => new DocSADHLineTaxCollection(Supporters, Factory));
		}

		public void TestTotalAmountInDeclarationCurrency()
		{
			var lineTaxCollection = new DocSADHLineTaxCollection(Supporters, Factory);
			CombineAssertions("[PRE-CONDITIONS]", () =>
			{
				AssertEquals("Collection count", 3, lineTaxCollection.Count);
				AssertEquals($"'{lineTaxCollection[0].G4_Type}' amount", "111.11", lineTaxCollection[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lineTaxCollection[1].G4_Type}' amount", "222.12", lineTaxCollection[1].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lineTaxCollection[2].G4_Type}' amount", "999.99", lineTaxCollection[2].G4_Amount_InDeclarationCurrency);
			});
			AssertEquals("Total amount", "1333.22", lineTaxCollection.TotalAmountInDeclarationCurrency);
		}

		public void TestTotalBox47MethodOfPayment()
		{
			var lineTaxCollection = new DocSADHLineTaxCollection(Supporters, Factory);
			AssertEquals("Test TotalMethodOfPayment data", ZString.Empty, lineTaxCollection.TotalBox47MethodOfPayment);
		}

		protected List<IDocSADHLineTaxBoxSupporter> Supporters
		{
			get
			{
				if (supporters == null)
				{
					supporters = new List<IDocSADHLineTaxBoxSupporter>
					{
						DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B10","100.00","RDY","S","OVR","111.11","A"),
						DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("D10","678.90","RDY","S","OVR","222.12","A"),
						DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00","122.12","RDY","S","OVR","999.99","A"),
					};
				}
				return supporters;
			}
		}
		List<IDocSADHLineTaxBoxSupporter> supporters;
	}
}
