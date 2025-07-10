using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

[TestedType(typeof(DocSADHLineTaxCollection))]
sealed class DocSADHLineTaxCollectionTest : DocBaseWrapperCollectionTest<DocSADHLineTaxCollection>
{
	protected override object GetNewObjectToWrap() => null;

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var result = DocSADHLineTax.New(Supporters[0], Factory);
		collection.Add(result);
		return result;
	}

	protected override DocSADHLineTaxCollection GetNewDocumentWrapperCollection() => new DocSADHLineTaxCollection(Supporters, Factory);

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DocSADHLineTaxCollection(null, Factory));
		AssertNoExceptionThrown(() => new DocSADHLineTaxCollection(Supporters, Factory));
	}

	public void TestTotalAmountInDeclarationCurrency()
	{
		var lineTaxCollection = GetNewDocumentWrapperCollection();
		AssertEquals("Total amount should be equal to the sum of tax amount for TaxMethodOfPayment 1 and TaxMethodOfPayment 2 only", "18.65", lineTaxCollection.TotalAmountInDeclarationCurrency);
	}

	List<IDocSADHLineTaxBoxSupporter> Supporters
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
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("A00", "123.16", "RDY", "S", "OVR", "12.99", "1", "", "R"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("B00", "53.22", "RDY", "S", "OVR", "5.66", "2", "", "R"),
					DocSADHLineTaxBoxSupporterTestHelper.CreateSupporter("C00", "28.18", "RDY", "S", "OVR", "2.42", "5", "", "R")
				};
			}
			return supporters;
		}
	}
	List<IDocSADHLineTaxBoxSupporter> supporters;
}
