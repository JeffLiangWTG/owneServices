using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3PresentGoodMessageWrapper))]
	public class G3PresentGoodMessageWrapperTest : G3CommonSendMessageWrapperTest<G3PresentGoodMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'bills')", () => new G3PresentGoodMessageWrapper(null, certificate, "LRN"));

				AssertExceptionThrown("Throws Exception if certificate is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'certificate')", () => new G3PresentGoodMessageWrapper(header.Bills, null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'localReferenceNumber')", () => new G3PresentGoodMessageWrapper(header.Bills, certificate, null));
#else
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: bills", () => new G3PresentGoodMessageWrapper(null, certificate, "LRN"));

				AssertExceptionThrown("Throws Exception if certificate is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: certificate", () => new G3PresentGoodMessageWrapper(header.Bills, null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: localReferenceNumber", () => new G3PresentGoodMessageWrapper(header.Bills, certificate, null));
#endif
			});
		}

		public void TestHeader()
		{
			CombineAssertions(() =>
			{
				var header = Provider.Header;
				AssertNotNull("Expected filled Header", header);
				AssertEquals("LRN", header.LRN);

				AssertSame("Cached Header", header, Provider.Header);
			});
		}

		protected override G3PresentGoodMessageWrapper GetProviderCore()
		{
			return new G3PresentGoodMessageWrapper(header.Bills, certificate, "LRN");
		}
	}
}
