using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZCompressorQuirksTest : TestCase
	{
		public void TestIsColumnExcludedFromCompression()
		{
			AssertEquals("OC_PasswordHash", true, ZCompressorQuirks.IsColumnExcludedFromCompression("OC_PasswordHash"));
			AssertEquals("OC_PasswordSalt", true, ZCompressorQuirks.IsColumnExcludedFromCompression("OC_PasswordSalt"));
			AssertEquals("GS_PasswordHash", true, ZCompressorQuirks.IsColumnExcludedFromCompression("GS_PasswordHash"));
			AssertEquals("GS_PasswordSalt", true, ZCompressorQuirks.IsColumnExcludedFromCompression("GS_PasswordSalt"));
			AssertEquals("GS_SqlLoginPasswordHash", true, ZCompressorQuirks.IsColumnExcludedFromCompression("GS_SqlLoginPasswordHash"));
			AssertEquals("PWH_Hash", true, ZCompressorQuirks.IsColumnExcludedFromCompression("PWH_Hash"));
			AssertEquals("PWH_Salt", true, ZCompressorQuirks.IsColumnExcludedFromCompression("PWH_Salt"));
			AssertEquals("PWH_TruncatedHash", true, ZCompressorQuirks.IsColumnExcludedFromCompression("PWH_TruncatedHash"));
			AssertEquals("PER_PasswordHash", true, ZCompressorQuirks.IsColumnExcludedFromCompression("PER_PasswordHash"));
			AssertEquals("PER_PasswordSalt", true, ZCompressorQuirks.IsColumnExcludedFromCompression("PER_PasswordSalt"));
			AssertEquals("SCK_KeyValue", true, ZCompressorQuirks.IsColumnExcludedFromCompression("SCK_KeyValue"));
			AssertEquals("SC_EncryptedDataKey", true, ZCompressorQuirks.IsColumnExcludedFromCompression("SC_EncryptedDataKey"));

			AssertEquals("SC_ImageData", false, ZCompressorQuirks.IsColumnExcludedFromCompression("SC_ImageData"));
		}
	}
}
