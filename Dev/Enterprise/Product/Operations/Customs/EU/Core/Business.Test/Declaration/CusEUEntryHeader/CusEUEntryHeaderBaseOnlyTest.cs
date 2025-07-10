using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEUEntryHeader))]
	class CusEUEntryHeaderBaseOnlyTest : CusEUEntryHeaderAbstractTest<CusEUEntryHeader>
	{
		public void TestICusEUEntryHeaderMembers()
		{
			var data = (BusinessObject)Factory.New<Integration.Customs.EU.ICusEUEntryHeader>();
			AssertType<CusEUEntryHeader>(data);
			AssertType<CusEUEntryHeader>(Factory.Load(data.TablePrefix, data.PK));
		}
	}
}
