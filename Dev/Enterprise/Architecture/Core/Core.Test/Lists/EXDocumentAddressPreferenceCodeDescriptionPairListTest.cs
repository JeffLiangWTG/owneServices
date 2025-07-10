using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class EXDocumentAddressPreferenceCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			EXDocumentAddressPreferenceCodeDescriptionPairList list = new EXDocumentAddressPreferenceCodeDescriptionPairList();
			AssertEquals(3, list.Count);
			AssertEquals(true, list.ContainsCode(OrgConstants.AddressType.Documentary));
			AssertEquals(true, list.ContainsCode(OrgConstants.AddressType.Office));
			AssertEquals(true, list.ContainsCode(OrgConstants.AddressType.Pickup));
		}
	}
}
