using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class NotificationSourceTest : TestCaseWithFactory
	{
		#region TestCellDescription

		public void TestCellDescription()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	<Z0_Description>
#End");
			worksheet.Name = "Dummy";

			IStandardTemplate template = new StandardTemplate(worksheet);

			var data = Factory.New<DummyBusinessObject>();
			data.Z0_Description = "some description";

			var libraries = new IMacroLibrary[]
			{
				new StandardLibrary()
			};

			var document = template.CreateDocument(
				template.Name,
				"test",
				new MacroScope(data.MakeDynamic()),
				libraries.CreateContext(),
				null);

			var cell = document.GetDocumentCell(1, 2);

			AssertNotNull("Cell is not null", cell);

			INotificationSource source = cell.CreateNotificationSource();

			AssertEquals("Description", "Cell [1,2,1,2] Macro[\"<Z0_Description>\"]", source.Description);
		}

		#endregion
	}
}