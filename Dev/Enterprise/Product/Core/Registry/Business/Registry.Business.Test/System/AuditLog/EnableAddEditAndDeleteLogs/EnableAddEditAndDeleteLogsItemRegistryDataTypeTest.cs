using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EnableAddEditAndDeleteLogsItemRegistryDataType))]
	sealed class EnableAddEditAndDeleteLogsItemRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EnableAddEditAndDeleteLogsItemRegistryDataType>
	{
		protected override EnableAddEditAndDeleteLogsItemRegistryDataType GetNewDataType()
		{
			return new EnableAddEditAndDeleteLogsItemRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var firstCollection = new EnableAddEditAndDeleteLogsItemCollection();
			var bizoA = firstCollection.AddNew();
			bizoA.Table = "Table1";
			bizoA.EnableADDLogs = false;
			bizoA.EnableEDTLogs = false;
			bizoA.EnableDELLogs = false;

			var secondCollection = new EnableAddEditAndDeleteLogsItemCollection();
			var bizoB = secondCollection.AddNew();
			bizoB.Table = "Table2";
			bizoB.EnableADDLogs = false;
			bizoB.EnableEDTLogs = false;
			bizoB.EnableDELLogs = false;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(firstCollection, new EnableAddEditAndDeleteLogsItemRegistryDataType().Serialise(firstCollection)),
				new ValidSampleAndBinaryValueInDB(secondCollection, new EnableAddEditAndDeleteLogsItemRegistryDataType().Serialise(secondCollection)),
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "EnableAddEditAndDeleteLogsItemsRegistryItemEditor"; }
		}
	}
}
