using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CFIARegistrationNumberCollection))]
	sealed class CFIARegistrationNumberCollectionTest : CusCodeDataCollectionTest<CFIARegistrationNumber>
	{
		public void TestAddCFIARegNumsAsAString()
		{
			var coll = GetCollectionToTest() as CFIARegistrationNumberCollection;
			coll.Add("111/1111,222/2222;333/3333");
			Assert(coll.ContainsRegNum("111", "1111"));
			Assert(coll.ContainsRegNum("222", "2222"));
			Assert(coll.ContainsRegNum("333", "3333"));
		}

		protected override CusCodeDataCollection<CFIARegistrationNumber> GetCusCodeDataCollection()
		{
			return new CFIARegistrationNumberCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CFIARegistrationNumber>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
