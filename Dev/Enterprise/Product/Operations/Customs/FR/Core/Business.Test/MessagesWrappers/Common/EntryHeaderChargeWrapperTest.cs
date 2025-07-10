using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class EntryHeaderChargeWrapperTest : TestCaseWithFactory
	{
		public void TestTaxType()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var charge = entryHeader.Charges.AddNew();
			var wrapper = new EntryHeaderChargeWrapper(entryHeader.Charges.Cast<CusEntryHeaderCharges>().FirstOrDefault());
			AssertEquals(FeeTypeCodeConverter.StandardAdvalorumOrSecondQuantiy, wrapper.TaxType);
		}
	}
}
