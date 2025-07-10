using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryHeader))]
	public class AddInfoCusEntryHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			return new AddInfoCusEntryHeader(entryHeader.CH_AddInfoInfo);
		}
	}
}
