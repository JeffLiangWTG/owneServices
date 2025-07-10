using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DynamicDataMergeTest : DynamicDataTestCase
	{
		public void TestMergeProperty_InvalidData()
		{
			const string macro = "\"<GoodsValue>\"";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GoodsValue = 12345.67
			};

			var data = shipment.MakeDynamic();

			var xml = XDocument.Parse(
				@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""GoodsValue"">
    <Value>6 789,01</Value>
  </Property>
</Entity>");
			data.MergeDataFromXml(xml);

			var expr = macro.CreateExpression();
			expr.Evaluate(data);

			AssertNoErrors(expr);

			var nameProperty = data.GetDynamicProperty(nameof(shipment.GoodsValue));

			AssertEquals("Value has not been merged because is in invalid format", (ZDecimal)12345.67, nameProperty.Value);
		}

		public void TestMergeCollection_ElementDependsOnCustomField_OverriddenToTrue()
		{
			const string macro =
@"def isReceivedForShipment = LocalCustomField(""IsReceivedForShipment"", @data.BillOfLadingClauseCollection.Any({Type.Code == ""RFS""}));
def isReceivedForShipmentHandler = {if @isReceivedForShipment then @data.BillOfLadingClauseCollection.Create({Type = {Code = ""RFS"", Description=""ReceivedForShipment""}}) else (def element = @data.BillOfLadingClauseCollection.First({Type.Code == ""RFS""}); @data.BillOfLadingClauseCollection.Remove(@element))};
Eval(@data, @isReceivedForShipmentHandler);";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dynamicShipment = shipment.MakeDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""IsReceivedForShipment"" State=""Added"">
    <Value>Y</Value>
  </Property>
</Entity>");

			dynamicShipment.MergeDataFromXml(xml);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = macro
					.With<StandardLibrary>()
					.And<DocumentLibrary>()
					.And<DataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());

				var isReceivedForShipment = (IDynamicData)scope.GetVariable("isReceivedForShipment");

				AssertEquals("isReceivedForShipment.Value", true, isReceivedForShipment.Value);
				AssertEquals("isReceivedForShipment.IsOverridden", true, isReceivedForShipment.IsOverriddenIncludingChildren);

				var billOfLadingClauses = (IDynamicDataCollection)dynamicShipment.GetDynamicProperty("BillOfLadingClauseCollection");
				AssertEquals("BillOfLadingClauseCollection",
					1, billOfLadingClauses.Count());
				AssertEquals("BillOfLadingClauseCollection.RemovedElements",
					0, billOfLadingClauses.GetAllElements().Count(elem => elem.IsRemoved()));
			}
		}

		public void TestMergeCollection_ElementDependsOnCustomField_OverriddenToTrue_OverrideItem()
		{
			const string macro =
@"def isReceivedForShipment = LocalCustomField(""IsReceivedForShipment"", @data.BillOfLadingClauseCollection.Any({Type.Code == ""RFS""}));
def isReceivedForShipmentHandler = {if @isReceivedForShipment then @data.BillOfLadingClauseCollection.Create({Type = {Code = ""RFS"", Description=""ReceivedForShipment""}}) else (def element = @data.BillOfLadingClauseCollection.First({Type.Code == ""RFS""}); @data.BillOfLadingClauseCollection.Remove(@element))};
Eval(@data, @isReceivedForShipmentHandler);";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dynamicShipment = shipment.MakeDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""IsReceivedForShipment"" State=""Added"">
    <Value>Y</Value>
  </Property>
</Entity>");

			dynamicShipment.MergeDataFromXml(xml);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = macro
					.With<StandardLibrary>()
					.And<DocumentLibrary>()
					.And<DataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());

				var isReceivedForShipment = (IDynamicData)scope.GetVariable("isReceivedForShipment");

				AssertEquals("isReceivedForShipment.Value", true, isReceivedForShipment.Value);
				AssertEquals("isReceivedForShipment.IsOverridden", true, isReceivedForShipment.IsOverriddenIncludingChildren);
			}
		}

		public void TestMergeCollection_ElementDependsOnCustomField_OverriddenToFalse()
		{
			const string macro =
@"def isReceivedForShipment = LocalCustomField(""IsReceivedForShipment"", @data.BillOfLadingClauseCollection.Any({Type.Code == ""RFS""}));
def isReceivedForShipmentHandler = {if @isReceivedForShipment then @data.BillOfLadingClauseCollection.Create({Type = {Code = ""RFS"", Description=""ReceivedForShipment""}}) else (def element = @data.BillOfLadingClauseCollection.First({Type.Code == ""RFS""}); @data.BillOfLadingClauseCollection.Remove(@element))};
Eval(@data, @isReceivedForShipmentHandler);";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetBillOfLadingClauseCollection(() => new List<BillOfLadingClause>
				{
					new BillOfLadingClause
					{
						Type = new CodeDescriptionPair
						{
							Code = "RFS",
							Description = "ReceivedForShipment"
						}
					}
				});

			var dynamicShipment = shipment.MakeDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""IsReceivedForShipment"" State=""Added"">
    <Value>N</Value>
  </Property>
</Entity>");

			dynamicShipment.MergeDataFromXml(xml);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = macro
					.With<StandardLibrary>()
					.And<DocumentLibrary>()
					.And<DataLibrary>()
					.CreateExpression();

				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());

				var isReceivedForShipment = (IDynamicData)scope.GetVariable("isReceivedForShipment");

				AssertEquals("isReceivedForShipment.Value", false, isReceivedForShipment.Value);
				AssertEquals("isReceivedForShipment.IsOverridden", true, isReceivedForShipment.IsOverriddenIncludingChildren);

				var billOfLadingClauses = (IDynamicDataCollection)dynamicShipment.GetDynamicProperty("BillOfLadingClauseCollection");
				AssertEquals("BillOfLadingClauseCollection",
					0, billOfLadingClauses.Count());
				AssertEquals("BillOfLadingClauseCollection.RemovedElements",
					1, billOfLadingClauses.GetAllElements().Count(elem => elem.IsRemoved()));
			}
		}

		public void TestMergeCollection_CreateElement_NoMergeFor()
		{
			const string macro = "\"<NoteCollection.First.Description>\"";

			var note = new Note();
			note.Description = "ABCD";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetNoteCollection(() => new DataObjectList<Note> { note });

			var dynamicShipment = shipment.MakeDynamic();
			var dynamicAddressCollection = dynamicShipment.GetDynamicProperty("NoteCollection");

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<EntityCollection>
  <Items>
    <Entity State=""Added"">
      <Property Name=""Description"">
        <Value>AddBCD</Value>
      </Property>
    </Entity>
  </Items>
</EntityCollection>");
			dynamicAddressCollection.MergeDataFromXml(xml);

			var macroRun = new MacroRun
			{
				Data = dynamicShipment,
				ExpectedResult = "ABCD"
			};

			AssertMacroRun(macro, macroRun);
		}

		public void TestDoNotCreateDuplicatedValues()
		{
			const string macro = "NoteCollection.SetNaturalKey(\"Description\")";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetNoteCollection(() => new DataObjectList<Note>());

			var data = consol.MakeDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""NoteCollection"">
    <EntityCollection>
      <Items>
        <Entity State=""Added"" >
          <Property Name=""Description"" NaturalKey =""true"">
            <Value>OtherBillClauses</Value>
          </Property>
          <Property Name=""NoteText"">
            <Value>bla</Value>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>");
			data.MergeDataFromXml(xml);

			var expr = macro.With<MetaDataLibrary>().CreateExpression();
			expr.Evaluate(data);

			AssertNoErrors(expr);

			var notes = (IDynamicDataCollection)data.GetDynamicProperty(nameof(consol.NoteCollection));
			AssertEquals("merged note Description", "bla", notes.Single<IDynamicData>().GetDynamicProperty("NoteText").Value);
		}
	}
}