using System.Collections.Generic;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class EnterpriseCodeExternalCodeMappingsTest_ForCoreFunctionality : TestCase
	{
		public void TestGetExternalCode()
		{
			TestEnterpriseCodeExternalCodeMappings mappings = new TestEnterpriseCodeExternalCodeMappings();
			NotificationBuffer buffer = new NotificationBuffer();
			AssertEquals("Should map correctly", "aaa", mappings.GetExternalCode("111", "", buffer));
			AssertEquals("No errors should ensure", false, buffer.HasErrors);
			AssertEquals("Should not find mapping", null, mappings.GetExternalCode("zzz", "", buffer));
			AssertEquals("Should no error", false, buffer.HasErrors);
			AssertEquals("Should have a warning because mapping not found", true, buffer.HasWarnings);
		}

		public void TestGetEnterpriseCode()
		{
			TestEnterpriseCodeExternalCodeMappings mappings = new TestEnterpriseCodeExternalCodeMappings();
			NotificationBuffer buffer = new NotificationBuffer();

			AssertEquals("Should map correctly", "111", mappings.GetEnterpriseCode("aaa", "", buffer));
			AssertEquals("No errors should ensure", false, buffer.HasErrors);
			AssertEquals("No warnings should ensure", false, buffer.HasWarnings);

			AssertEquals("Should not find mapping", null, mappings.GetEnterpriseCode("zzz", "", buffer));
			AssertEquals("No errors", false, buffer.HasErrors);
			AssertEquals("But should have warnings", true, buffer.HasWarnings);
		}

		public void TestGetEnterpriseCode_WithBlankExternalCode()
		{
			TestEnterpriseCodeExternalCodeMappings mappings = new TestEnterpriseCodeExternalCodeMappings();
			NotificationBuffer buffer = new NotificationBuffer();

			AssertEquals("Should not map an enterprise code", null, mappings.GetEnterpriseCode("", "", buffer));
			AssertEquals("Should not have any errors", false, buffer.HasErrors);
			AssertEquals("Should not have any warnings", false, buffer.HasWarnings);

			AssertEquals("Should not map an enterprise code", null, mappings.GetEnterpriseCode((string)null, "", buffer));
			AssertEquals("Should not have any errors", false, buffer.HasErrors);
			AssertEquals("Should not have any warnings", false, buffer.HasWarnings);
		}

		[Immutable]
		class TestEnterpriseCodeExternalCodeMappings : EnterpriseCodeExternalCodeMappings
		{
			protected override IEnumerable<Mapping> GetMappings()
			{
				yield return new Mapping("111", "aaa");
				yield return new Mapping("222", "bbb");
			}

			protected override string Name
			{
				get { return "test code"; }
			}
		}
	}
}
