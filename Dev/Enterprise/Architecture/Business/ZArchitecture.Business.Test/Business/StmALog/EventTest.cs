using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EventTest : TestCase
	{
		public void TestCode()
		{
			string code = "XXX";
			Event @event = new Event(code, (NoResString)"", ZGuid.NewZGuid());
			AssertEquals(code, @event.Code);
		}

		public void TestDescription()
		{
			var desc = ResString.GetMultilingualString("FFF", "Fred Flintstone");
			Event @event = new Event("XXX", desc, ZGuid.NewZGuid());
			AssertEquals(desc, @event.MultilingualDescription);
			AssertEquals("Fred Flintstone", @event.Description);
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("FFF", new ResourceStringData("FFF", "Enotstnilf Derf"));
				AssertEquals("Enotstnilf Derf", @event.Description);
			}
		}

		public void TestPK()
		{
			ZGuid pK = ZGuid.NewZGuid();
			Event @event = new Event("XXX", (NoResString)"", pK);
			AssertEquals(pK, @event.PK);
		}
	}
}
