using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers.Tests
{
	internal class EntitySetXmlDeserializerTest : TestCaseWithDummy
	{
		public void TestSuccessfulDeserialise_Success()
		{
			var deserializer = new EntitySetXmlDeserializer();
			deserializer.DefinitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };

			const string xml =
			@"
		<Product>
			<OrgSupplierPart Action='MERGE'>
				<PartNum>PART1</PartNum>
				<StockKeepingUnit>UNT</StockKeepingUnit>
				<Weight>0.000</Weight>
				<Cubic>0.000</Cubic>
				<LastCost>3.50000</LastCost>
				<Desc>TEST PART 1</Desc>
				<OrgPartRelationCollection>
					<OrgPartRelation Action='MERGE'>
						<Relationship>OWN</Relationship>
						<OrgHeader>
							<Code>ADESTE</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
				<StmNoteCollection>
					<StmNote Action='MERGE'>
						<Description>MYNOTE</Description>
						<NoteText>CRAZY HORSE</NoteText>
						<NoteType>INT</NoteType>
						<NoteContext>AAA</NoteContext>
					</StmNote>
				</StmNoteCollection>
			</OrgSupplierPart>
		</Product>";

			var element = XElement.Parse(xml);
			EntitySet entitySet = null;
			AssertNoExceptionThrown("OrgSupplierPart deserialised", () => { entitySet = deserializer.Deserialize(element, new AncillaryImportServices()); });
			AssertNotNull("EntitySet", entitySet);
			AssertEquals("Product", entitySet?.Name);
		}

		public void TestDeserialiseWithDuplicateProperties_Fail()
		{
			var deserializer = new EntitySetXmlDeserializer();
			deserializer.DefinitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };

			const string xml =
			@"
		<Product>
			<OrgSupplierPart Action='MERGE'>
				<PartNum>PART1</PartNum>
				<StockKeepingUnit>UNT</StockKeepingUnit>
				<Weight>0.000</Weight>
				<Cubic>0.000</Cubic>
				<LastCost>3.50000</LastCost>
				<LastCost>4.1278</LastCost>
				<Desc>TEST PART 1</Desc>
				<OrgPartRelationCollection>
					<OrgPartRelation Action='MERGE'>
						<Relationship>OWN</Relationship>
						<OrgHeader>
							<Code>ADESTE</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
				<StmNoteCollection>
					<StmNote Action='MERGE'>
						<Description>MYNOTE</Description>
						<NoteText>CRAZY HORSE</NoteText>
						<NoteType>INT</NoteType>
						<NoteContext>AAA</NoteContext>
					</StmNote>
				</StmNoteCollection>
			</OrgSupplierPart>
		</Product>";

			var element = XElement.Parse(xml);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), () => { deserializer.Deserialize(element, new AncillaryImportServices()); });
		}

		public void TestDeserialiseWithMoreThanOneElementInBody_Fail()
		{
			var deserializer = new EntitySetXmlDeserializer();
			deserializer.DefinitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };

			const string xml =
			@"
		<Product>
			<OrgSupplierPart Action='MERGE'>
				<PartNum>PART1</PartNum>
				<StockKeepingUnit>UNT</StockKeepingUnit>
				<Weight>0.000</Weight>
				<Cubic>0.000</Cubic>
				<LastCost>3.50000</LastCost>
				<Desc>TEST PART 1</Desc>
				<OrgPartRelationCollection>
					<OrgPartRelation Action='MERGE'>
						<Relationship>OWN</Relationship>
						<OrgHeader>
							<Code>ADESTE</Code>
						</OrgHeader>
					</OrgPartRelation>
				</OrgPartRelationCollection>
				<StmNoteCollection>
					<StmNote Action='MERGE'>
						<Description>MYNOTE</Description>
						<NoteText>CRAZY HORSE</NoteText>
						<NoteType>INT</NoteType>
						<NoteContext>AAA</NoteContext>
					</StmNote>
				</StmNoteCollection>
			</OrgSupplierPart>
			<OrgSupplierPart Action='MERGE'>
			</OrgSupplierPart>
		</Product>";

			var element = XElement.Parse(xml);
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), () => { deserializer.Deserialize(element, new AncillaryImportServices()); });
		}
	}
}
