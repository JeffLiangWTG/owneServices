using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.OIA.Business.Testing
{
	public class OIAGLDataRowTest : TestCase
	{
		public void TestProperties()
		{
			OIAGLDataRow row = new OIAGLDataRow();
			AssertEquals("Row should have 4 fields", 4, row.FieldCount);
			row.LocalClientOrgCode = new ZString("mwrjpgokohffasz");
			row.ARAccountGroup = new ZString("psvcaqpvmegmvmq");
			row.CustomisableText1 = new ZString("maevzagflhdxraptcpypnhpjisxukjpbswjrboidnrsjqmguturpnwcflungclucrcfvqysnhvcwoftwlcbqranvuefgbcabwprohlevbeirufqgbquakelrojblslmfvaaynfpayfomswbonzflharreaaxkscqscwtfsswaubeqyzrimxtgbxvjiunesqfdyupzhjvhiknynbfaioyujjrfbkwxeridwaseksacblxqkwvhomwdsktodkyxmx");
			row.CustomisableText2 = new ZString("ndiuppwiayeudujnbtuvtpbczsmilxtfkxzurjutpmuwtqpihqyykkoycgvwdnwdezjgvovsyafhufjideavyuuaiaslnxqbprruhimsvgakfhewxntynbekejxvwkebkercnoaurhzsukubwvnaeroomgpylrvlbyzbpbjimyhfbmpuowzbwewtpdqgzcjaloqvltmumfogjjmzrukpsupwbflmlcikohtnvujujepqqqjfqqpesuhrzutfizj");
			AssertEquals("LocalClientOrgCode", new ZString("mwrjpgokohffasz"), row.GetField(OIAGLDataRow.Schema.LocalClientOrgCode));
			AssertEquals("ARAccountGroup", new ZString("psvcaqpvmegmvmq"), row.GetField(OIAGLDataRow.Schema.ARAccountGroup));
			AssertEquals("Customisable1", new ZString("maevzagflhdxraptcpypnhpjisxukjpbswjrboidnrsjqmguturpnwcflungclucrcfvqysnhvcwoftwlcbqranvuefgbcabwprohlevbeirufqgbquakelrojblslmfvaaynfpayfomswbonzflharreaaxkscqscwtfsswaubeqyzrimxtgbxvjiunesqfdyupzhjvhiknynbfaioyujjrfbkwxeridwaseksacblxqkwvhomwdsktodkyxmx"), row.GetField(OIAGLDataRow.Schema.CustomisableText1));
			AssertEquals("Customisable2", new ZString("ndiuppwiayeudujnbtuvtpbczsmilxtfkxzurjutpmuwtqpihqyykkoycgvwdnwdezjgvovsyafhufjideavyuuaiaslnxqbprruhimsvgakfhewxntynbekejxvwkebkercnoaurhzsukubwvnaeroomgpylrvlbyzbpbjimyhfbmpuowzbwewtpdqgzcjaloqvltmumfogjjmzrukpsupwbflmlcikohtnvujujepqqqjfqqpesuhrzutfizj"), row.GetField(OIAGLDataRow.Schema.CustomisableText2));
		}
	}
}
