using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class SynchronousResponseTests : TestCase
	{
		public void TestAllProperties()
		{
			var sr = new SynchronousResponse(@"<SynchronousResponse>
               <status>202</status>
               <code>ACCEPTED</code>
               <ResponseHeaders>
                              <x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id>
               </ResponseHeaders>
</SynchronousResponse>
");
			AssertEquals("202", sr.Status);
			AssertEquals("ACCEPTED", sr.Code);
			Assert(sr.IsAccepted);
		}
	}
}
