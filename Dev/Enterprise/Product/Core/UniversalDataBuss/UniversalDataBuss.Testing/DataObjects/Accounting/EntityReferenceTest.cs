using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(EntityReference))]
	class EntityReferenceTest : DataObjectTestCase<EntityReference>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(EntityReference.Key), GetDefaultFieldLength() },
				{ nameof(EntityReference.Type), GetEntityTypeLength() }
			};
		}
	}
}

