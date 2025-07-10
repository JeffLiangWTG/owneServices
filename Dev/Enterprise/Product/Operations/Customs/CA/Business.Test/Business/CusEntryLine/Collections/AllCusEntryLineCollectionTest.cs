using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AllCusEntryLineCollection))]
	sealed class AllCusEntryLineCollectionTest : Customs.Business.Testing.AllCusEntryLineCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AllCusEntryLineCollection(EntryHeader);
		}

		new CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)base.EntryHeader; }
		}
	}
}
