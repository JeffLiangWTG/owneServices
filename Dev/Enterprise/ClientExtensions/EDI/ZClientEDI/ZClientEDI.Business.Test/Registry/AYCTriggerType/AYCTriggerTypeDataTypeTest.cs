using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(AYCTriggerTypeDataType))]
	class AYCTriggerTypeDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AYCTriggerTypeDataType>
	{
		#region Implementation

		protected override AYCTriggerTypeDataType GetNewDataType()
		{
			return new AYCTriggerTypeDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "AYCTriggerTypeRegistryEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new AYCTriggerTypeSettings();
			var second = new AYCTriggerTypeSettings();
			using (second.GetValidationSuspender())
			{
				second.PrimaryChargeCode = "Primary1";
				second.SecondaryChargeCode = "Second1";
			}

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, DataType.Serialise(first)),
				new ValidSampleAndBinaryValueInDB(second, DataType.Serialise(second)),
			};
		}

		#endregion
	}
}
