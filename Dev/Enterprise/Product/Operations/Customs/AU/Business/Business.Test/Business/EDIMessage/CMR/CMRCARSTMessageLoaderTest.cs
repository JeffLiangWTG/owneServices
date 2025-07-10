using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCARSTMessage.Loader))]
	sealed class CMRCARSTMessageLoaderTest : LoaderTestCase
	{
		public void TestGetApplicationAndOwnerReferenceQuery()
		{
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_ApplicationReference = "APPREF";
			message.EM_MessageOwner = "OWNER";

			Assert(message.MatchesFilter(new CMRCARSTMessage.Loader(Factory).GetApplicationAndOwnerReferenceQuery("APPREF", "OWNER")));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CMRCARSTMessage.Loader(Factory);
	}
}
