using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryHeader))]
	public class AddInfoCusEntryHeaderBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			return new AddInfoCusEntryHeader(entryHeader.CH_AddInfoInfo);
		}
	}
}
