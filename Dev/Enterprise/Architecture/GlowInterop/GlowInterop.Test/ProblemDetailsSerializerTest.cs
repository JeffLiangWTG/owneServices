using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class ProblemDetailsSerializerTest : TestCase
	{
		public void TestSerializeAndDeserialize()
		{
			var problemDetails = new ProblemDetails()
			{
				Detail = "Excel file version is too old.",
				Type = ProblemType.FileTooOld,
			};

			var json = problemDetails.Serialize();
			AssertContains("\"detail\":\"Excel file version is too old.\"", json);
			AssertContains("\"type\":\"https://glow.wisetechglobal.com/data-import/file-too-old\"", json);
		}
	}
}
