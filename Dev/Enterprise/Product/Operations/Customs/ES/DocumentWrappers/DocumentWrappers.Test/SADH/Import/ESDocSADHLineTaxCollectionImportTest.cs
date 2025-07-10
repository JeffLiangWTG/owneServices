using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	[TestedType(typeof(ESDocSADHLineTaxCollectionImport))]
	sealed class ESDocSADHLineTaxCollectionImportTest : DocSADHLineTaxCollectionTest<ESDocSADHLineTaxCollectionImport>
	{
		protected override object GetNewObjectToWrap() => null;

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			var result = ESDocSADHLineTaxImport.New(ESSupporters[0], Factory);
			collection.Add(result);
			return result;
		}

		protected override ESDocSADHLineTaxCollectionImport GetNewDocumentWrapperCollection() => new ESDocSADHLineTaxCollectionImport(ESSupporters, Factory);

		public void TestTotalAmountInDeclarationCurrencyES()
		{
			var lineTaxCollection = new ESDocSADHLineTaxCollectionImport(ESSupporters, Factory);
			CombineAssertions("[PRE-CONDITIONS]", () =>
			{
				AssertEquals("Collection count", 3, lineTaxCollection.Count);
				AssertEquals($"'{lineTaxCollection[0].G4_Type}' amount", "111.11", lineTaxCollection[0].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lineTaxCollection[1].G4_Type}' amount", "222.12", lineTaxCollection[1].G4_Amount_InDeclarationCurrency);
				AssertEquals($"'{lineTaxCollection[2].G4_Type}' amount", "999.99", lineTaxCollection[2].G4_Amount_InDeclarationCurrency);
			});
			AssertEquals("Total amount", "1,222.11", lineTaxCollection.TotalAmountInDeclarationCurrency);
		}

		public void TestTotalBox47MethodOfPaymentES()
		{
			var lineTaxCollection = new ESDocSADHLineTaxCollectionImport(ESSupporters, Factory);
			AssertEquals("Test TotalMethodOfPayment data", "R", lineTaxCollection.TotalBox47MethodOfPayment);
		}

		public void TestTotalBox47MethodOfPaymentESWhenAllDeffered()
		{
			supporters = new List<IESDocSADHLineTaxBoxSupporter>
			{
				ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00","100.00","RDY","S","OVR","111.11", FeeMethodOfPayment.Deferred, true, "R", "A"),
				ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("D10","678.90","RDY","S","OVR","222.12",FeeMethodOfPayment.Deferred, true, "R", "A"),
				ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00","122.12","RDY","S","OVR","999.99",FeeMethodOfPayment.Deferred, true, "R", "A"),
			};
			var lineTaxCollection = new ESDocSADHLineTaxCollectionImport(supporters, Factory);
			AssertEquals("Test TotalMethodOfPayment data", ZString.Empty, lineTaxCollection.TotalBox47MethodOfPayment);
		}

		List<IESDocSADHLineTaxBoxSupporter> ESSupporters
		{
			get
			{
				if (supporters == null)
				{
					supporters = new List<IESDocSADHLineTaxBoxSupporter>
					{
						ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00","100.00","RDY","S","OVR","111.11", FeeMethodOfPayment.Deferred, true, "R", "A"),
						ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("D10","678.90","RDY","S","OVR","222.12","A", true, "R", "A"),
						ESDocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00","122.12","RDY","S","OVR","999.99","A", true, "R", "A"),
					};
				}
				return supporters;
			}
		}
		List<IESDocSADHLineTaxBoxSupporter> supporters;
	}
}
