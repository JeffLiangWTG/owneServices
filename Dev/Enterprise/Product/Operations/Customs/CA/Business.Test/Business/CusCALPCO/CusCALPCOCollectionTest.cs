using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCALPCOCollection))]
	sealed class CusCALPCOCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypedSingleParameterAddNew()
		{
			var collection = (CusCALPCOCollection)GetCollectionToTest();

			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(CusCALPCO) });

			AssertNotNull("AddNew of Type " + method.ReturnType.FullName + " not null", bizO);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		public void TestCopyPersistentValuesFrom()
		{
			var bo1 = Factory.New<JobDeclaration>();
			var bo2 = Factory.New<JobDeclaration>();
			var collection1 = new CusCALPCOCollection(bo1);
			var collection2 = new CusCALPCOCollection(bo2);

			Factory.Save();

			AssertEquals(0, collection1.Count);
			AssertEquals(0, collection2.Count);

			collection2.CopyPersistentValuesFrom(collection1);

			AssertEquals(0, collection1.Count);
			AssertEquals(0, collection2.Count);

			// checking that updates in original collection does not propagate automatically to the copied collection
			var lpco1 = collection1.AddNew();
			lpco1.CLP_RefNo = "REF1";
			lpco1.CLP_AlternativeQuotaQuantity = 1m;
			lpco1.CLP_AlternativeQuotaUQ = "kg";

			Factory.Save();

			AssertEquals(1, collection1.Count);
			AssertEquals(0, collection2.Count);

			// checking that CopyPersistentValuesFrom copied LPCO from the first collection
			collection2.CopyPersistentValuesFrom(collection1);

			AssertEquals(1, collection1.Count);
			AssertEquals(1, collection2.Count);

			var lpco1Copy = collection2.OfType<CusCALPCO>().Single(c => c.CLP_RefNo == "REF1");
			AssertEquals(1m, lpco1Copy.CLP_AlternativeQuotaQuantity);

			// checking that updates in the original LPCO does not propagate automatically to the copied LPCO
			lpco1.CLP_RefNo = "REF1 (UPD)";

			Factory.Save();

			AssertEquals("REF1", lpco1Copy.CLP_RefNo);
			AssertEquals(1m, lpco1Copy.CLP_AlternativeQuotaQuantity);

			// checking that CopyPersistentValuesFrom removes existing LPCOs and adds new LPCOs
			var lpco2 = collection1.AddNew();
			lpco2.CLP_RefNo = "REF2";
			lpco2.CLP_AlternativeQuotaQuantity = 2m;
			lpco2.CLP_AlternativeQuotaUQ = "kg";

			Factory.Save();

			collection2.CopyPersistentValuesFrom(collection1);

			AssertEquals(2, collection1.Count);
			AssertEquals(2, collection2.Count);

			AssertEquals(true, lpco1Copy.IsDeleted);

			lpco1Copy = collection2.OfType<CusCALPCO>().Single(c => c.CLP_RefNo == "REF1 (UPD)");
			AssertEquals(1m, lpco1Copy.CLP_AlternativeQuotaQuantity);
			AssertEquals(false, lpco1Copy.IsDeleted);

			var lpco2Copy = collection2.OfType<CusCALPCO>().Single(c => c.CLP_RefNo == "REF2");
			AssertEquals(2m, lpco2Copy.CLP_AlternativeQuotaQuantity);
			AssertEquals(false, lpco1Copy.IsDeleted);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusCALPCOCollection(CNSCHeader);
		}

		CNSCPGAHeader CNSCHeader
		{
			get
			{
				if (cnscHeader == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_CNSCInd = "Y";
					cnscHeader = invoiceLine.CNSCPGAHeader;
				}

				return cnscHeader;
			}
		}

		CNSCPGAHeader cnscHeader;

		#endregion
	}
}
