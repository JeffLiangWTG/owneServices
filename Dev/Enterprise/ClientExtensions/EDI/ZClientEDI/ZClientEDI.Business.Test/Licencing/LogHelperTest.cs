using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class LogHelperTest : TestCaseWithFactory
	{
		public void TestBuildLog()
		{
			LicenceHeader bizo = Factory.NewWithValidTestData<LicenceHeader>();
			bizo.LA_SupportMode = "AAA";
			Factory.Save();
			bizo.LA_SupportMode = "ZZZ";

			ZStringBuilder log = new ZStringBuilder("Foo");
			LogHelper.BuildLog(log, " | Bar:", bizo.LA_SupportModeInfo);
			AssertEquals("Foo | Bar:AAA=>ZZZ", log.ToString());
		}

		public void TestAddChangeLog()
		{
			LicenceHeader bizo = Factory.NewWithValidTestData<LicenceHeader>();
			bizo.LA_SupportMode = "AAA";
			Factory.Save();
			bizo.LA_SupportMode = "ZZZ";

			ZString log = LogHelper.AddChangeLog(ZString.Empty, "Some Text", bizo.LA_SupportModeInfo);
			AssertEquals("Some Text: ZZZ(AAA)", log);

			log = LogHelper.AddChangeLog(log, "Other Text", bizo.LA_SupportModeInfo);
			AssertEquals("Some Text: ZZZ(AAA) Other Text: ZZZ(AAA)", log);
		}

		public void TestAddShortDateChangeLog()
		{
			LicenceHeader bizo = Factory.NewWithValidTestData<LicenceHeader>();
			Factory.Save();
			bizo.LA_SiteLiveDate = new ZDateTime(2010, 7, 1);

			ZString log = LogHelper.AddShortDateChangeLog(ZString.Empty, "Some Date", bizo.LA_SiteLiveDateInfo);
			AssertEquals("Some Date: 01-Jul-10()", log);

			log = LogHelper.AddShortDateChangeLog(log, "Other Date", bizo.LA_SiteLiveDateInfo);
			AssertEquals("Some Date: 01-Jul-10() Other Date: 01-Jul-10()", log);
		}

		public void TestMaxLength()
		{
			ZString log = ZString.Replicate('x', StmALogSchema.SL_Reference.MaxLength - 1);
			ZString log2 = LogHelper.AddChangeLog(log, "foo", 1, 2);
			Assert(log2.EndsWith("foo: 1(2)"));

			log = ZString.Replicate('x', StmALogSchema.SL_Reference.MaxLength);
			log2 = LogHelper.AddChangeLog(log, "foo", 1, 2);
			AssertEquals("no change since string already at max length", log, log2);
		}
	}
}