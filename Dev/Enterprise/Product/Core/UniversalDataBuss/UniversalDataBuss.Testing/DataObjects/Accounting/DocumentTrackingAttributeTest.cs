using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(DocumentTrackingAttribute))]
	class DocumentTrackingAttributeTest : DataObjectTestCase<DocumentTrackingAttribute>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(DocumentTrackingAttribute.Type), JobRequiredDocAttribSchema.D0_AttribName.MaxLength },
				{ nameof(DocumentTrackingAttribute.Value), JobRequiredDocAttribSchema.D0_AttribValue.MaxLength }
			};
		}
	}
}
