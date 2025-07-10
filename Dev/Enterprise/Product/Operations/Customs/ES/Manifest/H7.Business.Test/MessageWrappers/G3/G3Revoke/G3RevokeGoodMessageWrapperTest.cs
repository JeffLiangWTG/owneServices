using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3RevokeGoodMessageWrapper))]
	public class G3RevokeGoodMessageWrapperTest : G3CommonSendMessageWrapperTest<G3RevokeGoodMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'bills')", () => new G3RevokeGoodMessageWrapper(null, revokeReasonDictionary, certificate, "LRN"));

				AssertExceptionThrown("Throws Exception if revokeReasonDictionary is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'revokeReasonDictionary')", () => new G3RevokeGoodMessageWrapper(header.Bills, null, certificate, "LRN"));

				AssertExceptionThrown("Throws Exception if certificate is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'certificate')", () => new G3RevokeGoodMessageWrapper(header.Bills, revokeReasonDictionary, null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'localReferenceNumber')", () => new G3RevokeGoodMessageWrapper(header.Bills, revokeReasonDictionary, certificate, null));
#else
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: bills", () => new G3RevokeGoodMessageWrapper(null, revokeReasonDictionary, certificate, "LRN"));

				AssertExceptionThrown("Throws Exception if revokeReasonDictionary is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: revokeReasonDictionary", () => new G3RevokeGoodMessageWrapper(header.Bills, null, certificate, "LRN"));

				AssertExceptionThrown("Throws Exception if certificate is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: certificate", () => new G3RevokeGoodMessageWrapper(header.Bills, revokeReasonDictionary, null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: localReferenceNumber", () => new G3RevokeGoodMessageWrapper(header.Bills, revokeReasonDictionary, certificate, null));
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

		protected override G3RevokeGoodMessageWrapper GetProviderCore()
		{
			revokeReasonDictionary = header.Bills.ToDictionary(bill => bill.PK, bill => new DocumentCommonWrapper("G001", "Reason 1") as IDocumentsCommon);
			return new G3RevokeGoodMessageWrapper(header.Bills, revokeReasonDictionary, certificate, "LRN");
		}

		IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary;
	}
}
