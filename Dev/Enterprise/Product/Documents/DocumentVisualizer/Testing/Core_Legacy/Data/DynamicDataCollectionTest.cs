using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DynamicDataCollectionTest : DynamicDataTestCase
	{
		public void TestSetNaturalKey()
		{
			var uxml = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			uxml.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			});

			const string macro = "@data.NoteCollection.SetNaturalKey(\"Description\")";

			var expr = macro
				.With<StandardLibrary>()
				.And<MetaDataLibrary>()
				.CreateExpression();

			var data = uxml.MakeDynamic();

			expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());

			var consolNoteColleciton = data.GetDynamicProperty(nameof(uxml.NoteCollection));

			AssertEquals("consol.NoteCollection natural key",
				"Description", consolNoteColleciton.GetMetaData<string>(MetaDataType.NaturalKey));
		}
	}
}
