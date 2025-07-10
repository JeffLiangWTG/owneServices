using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(DocumentTracking))]
	class DocumentTrackingTest : DataObjectTestCase<DocumentTracking>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(DocumentTracking.DocumentNumber), JobRequiredDocumentSchema.EQ_DocNumber.MaxLength },
				{ nameof(DocumentTracking.DocumentNote), JobRequiredDocumentSchema.EQ_DocumentNotes.MaxLength }
			};
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(DocumentTracking.DocumentNote)
		};
	}
}
