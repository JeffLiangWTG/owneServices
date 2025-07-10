using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	sealed class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EDIMessage>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIMessageCollection(Factory, ManifestHeader);
		}

		[ExpectNoExceptions]
		public void TestManifestHeaderMessageCollection()
		{
			AssertEquals(ManifestHeader.Messages.GetType(), typeof(EDIMessageCollection));
		}

		AsycudaManifestHeader ManifestHeader => manifestHeader ??= Factory.New<AsycudaManifestHeader>();
		AsycudaManifestHeader manifestHeader;
	}
}
