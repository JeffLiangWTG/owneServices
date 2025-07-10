using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	sealed class AccumulativeTotalFATTest : TestCaseWithFactory
	{
		public void FAT_ReplacementPerformance()
		{
			var factory = new BusinessObjectFactory();

			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test Template",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<Collection.Number>]   {E}-[<Collection.Decimal>]   {F}-[<Collection.Number>]   {G}-[<AccumulativeTotal Decimal>]   {H}-[<AccumulativeTotal Number>]
{A}-[#GroupBy:Collection.Text]
{B}-[Total1]   {C}-[<Collection.Text>]   {D}-[<AccumulativeTotal Decimal>]   {E}-[<AccumulativeTotal Number>]
{B}-[Total2]   {C}-[<Collection.Text>]   {D}-[<AccumulativeTotal Collection.Decimal>]   {E}-[<AccumulativeTotal Collection.Number>]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<Collection.Number>]   {E}-[<Collection.Decimal>]   {F}-[<Collection.Number>]   {G}-[<AccumulativeTotal Collection.Decimal>]   {H}-[<AccumulativeTotal Collection.Number>]
{A}-[#GroupBy:Collection.Text]
{B}-[Total1]   {C}-[<Collection.Text>]   {D}-[<AccumulativeTotal Decimal>]   {E}-[<AccumulativeTotal Number>]
{B}-[Total2]   {C}-[<Collection.Text>]   {D}-[<AccumulativeTotal Collection.Decimal>]   {E}-[<AccumulativeTotal Collection.Number>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = factory.New<DummyDocumentSupportable>();

			for (var i = 0; i < 100; i++)
			{
				var text = $"test{i}";
				for (var j = 0; j < 100; j++)
				{
					var child = dummy.Collection.AddNew();
					child.Z0_VarCharMax = text;
					child.Z0_Decimal = j;
					child.Z0_Number = j;
				}
			}

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			AssertNoExceptionThrown(() => DeliveryTestHelper.DeliverDocument(documentCommand));
		}
	}
}
