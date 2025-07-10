using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityOverrideRegistryItemDataType))]
	class EventVisibilityOverrideRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EventVisibilityOverrideRegistryItemDataType>
	{
		#region Overrides

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new EventVisibilityOverrideCollection();
			var eventOverride1 = collection1.AddNew();
			eventOverride1.Code = "AAS";

			var collection2 = new EventVisibilityOverrideCollection();
			var eventOverride2 = collection2.AddNew();
			eventOverride2.Code = "AST";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}

		#endregion

		#region Implementation

		protected override EventVisibilityOverrideRegistryItemDataType GetNewDataType()
		{
			return new EventVisibilityOverrideRegistryItemDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "EventsVisibilityOverrideRegistryItemEditor"; }
		}

		protected EventVisibilityOverrideCollection GetValidSampleCollection()
		{
			var result = new EventVisibilityOverrideCollection();
			result.AddNew("AAS");
			return result;
		}

		#endregion
	}
}
