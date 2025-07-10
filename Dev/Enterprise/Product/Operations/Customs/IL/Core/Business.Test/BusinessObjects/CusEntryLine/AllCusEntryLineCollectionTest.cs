using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(AllCusEntryLineCollection))]
	class AllCusEntryLineCollectionTest : Customs.Business.Testing.AllCusEntryLineCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AllCusEntryLineCollection((CusEntryHeader)base.EntryHeader);
	}
}
