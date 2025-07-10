using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class IMDocumentAddressPreferenceCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			IMDocumentAddressPreferenceCodeDescriptionPairList list = new IMDocumentAddressPreferenceCodeDescriptionPairList();
			AssertEquals(3, list.Count);
			AssertEquals(true, list.ContainsCode(OrgConstants.AddressType.Documentary));
			AssertEquals(true, list.ContainsCode(OrgConstants.AddressType.Office));
			AssertEquals(true, list.ContainsCode(OrgConstants.AddressType.Delivery));
		}
	}
}
