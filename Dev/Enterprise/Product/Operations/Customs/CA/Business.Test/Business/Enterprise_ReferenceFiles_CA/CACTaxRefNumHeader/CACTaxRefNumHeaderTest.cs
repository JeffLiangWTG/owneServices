using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACTaxRefNumHeader))]
	sealed class CACTaxRefNumHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsCigars()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var header = classHeader.RefNumbers.AddNew();
			var child = header.RefNumbers.AddNew();
			child.ZE_ExciseTaxRefNumber = "XXX";
			Assert(!header.IsCigars);
			child.ZE_ExciseTaxRefNumber = "E01";
			Assert(header.IsCigars);
		}

		public void TestProperties()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var header = classHeader.RefNumbers.AddNew();
			AssertEquals("ClassHeader", classHeader, header.ClassHeader);
			AssertEquals("Rates", typeof(CACTaxRefNumberCollection), header.RefNumbers.GetType());

			//Delete
			var child = header.RefNumbers.AddNew();
			header.Delete();
			AssertEquals("CACTaxRefNumHeader ChildObject deleted", true, child.IsDeleted);
		}

		public void TestLoader()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var header = classHeader.RefNumbers.AddNew();
			header.ZD_EffectiveDate = ZDateTime.Today.AddDays(+1);
			header.ZD_ExpiryDate = ZDateTime.Today.AddDays(+1);

			header = classHeader.RefNumbers.AddNew();
			header.ZD_EffectiveDate = ZDateTime.Today.AddDays(-2);
			header.ZD_ExpiryDate = ZDateTime.Today.AddDays(+1);

			header = classHeader.RefNumbers.AddNew();
			header.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			header.ZD_ExpiryDate = ZDateTime.Today.AddDays(+1);

			AssertEquals("CACTaxRefNumHeader", header, CACTaxRefNumHeader.Load(classHeader, ZDateTime.Today));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CACClassHeader>();
			header.ZA_AreaCode = "A A";
			return header.RefNumbers.AddNew();
		}
	}
}
