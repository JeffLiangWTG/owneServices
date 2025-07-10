using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>))]
	public class CusEntryHeaderChargesCollectionTest : Customs.Business.Testing.CusEntryHeaderChargesCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var headerCharges = entryHeader.Charges;
			return (CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)headerCharges;
		}
	}
}
