using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AlternativeEvidenceProviderTest : Customs.Business.Testing.DataProviderTestCase<AlternativeEvidenceProvider>
	{
		public void TestConstructor_NullArgument()
		{
			AssertExceptionThrown<ArgumentException>(() => new AlternativeEvidenceProvider(null));
		}

		public void TestConstructor_EmptyArgument()
		{
			AssertExceptionThrown<ArgumentException>(() => new AlternativeEvidenceProvider(Enumerable.Empty<AlternativeEvidence>()));
		}

		public void TestType()
		{
			AssertEquals(AlternativeEvidenceTypeList.Codes._14, Provider.Type);
		}

		public void TestTransportDocuments()
		{
			AssertEquals(2, Provider.TransportDocuments.Count);
		}

		public void TestTransportDocumentsSpecified()
		{
			CombineAssertions(() =>
			{
				AssertTransportDocumentSpecified(AlternativeEvidenceTypeList.Codes._11, true);
				AssertTransportDocumentSpecified(AlternativeEvidenceTypeList.Codes._14, true);
				AssertTransportDocumentSpecified(AlternativeEvidenceTypeList.Codes._15, true);
				AssertTransportDocumentSpecified(AlternativeEvidenceTypeList.Codes._17, true);
				AssertTransportDocumentSpecified(AlternativeEvidenceTypeList.Codes._16, false);
			});

			void AssertTransportDocumentSpecified(ZString type, ZBool typeFlag)
			{
				var newAlternativeEvidence = new AlternativeEvidenceProvider(new List<AlternativeEvidence>
				{
					new AlternativeEvidence(Factory) { EvidenceType = type }
				});
				AssertEquals($"Type:{type}, TransportDocumentsSpecified is {typeFlag}", typeFlag, newAlternativeEvidence.TransportDocumentsSpecified);
			}
		}

		protected override AlternativeEvidenceProvider GetProvider() => new AlternativeEvidenceProvider(
			new List<AlternativeEvidence>
			{
				new AlternativeEvidence(Factory) { EvidenceType = AlternativeEvidenceTypeList.Codes._14 },
				new AlternativeEvidence(Factory) { EvidenceType = AlternativeEvidenceTypeList.Codes._14 }
			});
	}
}
