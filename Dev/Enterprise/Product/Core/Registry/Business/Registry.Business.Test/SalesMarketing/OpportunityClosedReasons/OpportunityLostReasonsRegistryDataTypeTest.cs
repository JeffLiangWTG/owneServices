using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityClosedReasonsRegistryDataType))]
	sealed class OpportunityLostReasonsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OpportunityClosedReasonsRegistryDataType>
	{
		#region Implementation

		protected override OpportunityClosedReasonsRegistryDataType GetNewDataType()
		{
			return new OpportunityClosedReasonsRegistryDataType(new OpportunityClosedReasonsCollection());
		}

		protected override string ExpectedEditorName
		{
			get { return "OpportunityClosedReasonsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new OpportunityClosedReasonsCollection();
			collection.Add("A01", (NoResString)"A01").StatusRules.AddNew().Code = "WON";

			var collection2 = new OpportunityClosedReasonsCollection();
			collection2.Add("A02", (NoResString)"A02").StatusRules.AddNew().Code = "LOS";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new OpportunityClosedReasonsRegistryDataType(new OpportunityClosedReasonsCollection()).Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new OpportunityClosedReasonsRegistryDataType(new OpportunityClosedReasonsCollection()).Serialise(collection2))
			};
		}

		#endregion Implementation
	}
}
