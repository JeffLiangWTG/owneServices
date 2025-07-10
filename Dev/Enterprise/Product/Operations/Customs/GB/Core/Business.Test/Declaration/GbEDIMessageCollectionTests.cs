using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(GbEDIMessageCollection))]
	class GbEDIMessageCollectionTests : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GbEDIMessage>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			return entryHeader.Messages;
		}
	}
}
