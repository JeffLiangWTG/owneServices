using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using NUnit.Framework;

namespace CargoWise.Database.Abstractions.Test
{
	class ExtensionObjectsTests
	{
		[Test]
		public void AllScripts()
		{
			var tableScript = new DatabaseObjectCreateScript("ObjectName", "CreateTable", "DropTable");
			var viewScript = new DatabaseViewAndRoutineCreateScript("ObjectName", "CreateView", "DropView", "VIEW");

			var extension = new ExtensionObjects(ImmutableArray.Create(tableScript), ImmutableArray.Create(viewScript));
			Assert.That(extension.TableCreationScripts[0], Is.EqualTo(tableScript), "Table script");
			Assert.That(extension.ViewAndRoutineCreationScripts[0], Is.EqualTo(viewScript), "View script");

			var allScripts = extension.GetAllScripts();
			Assert.That(allScripts, Has.Length.EqualTo(2), "2 scripts, 1 for the table and one for the view");
			Assert.That(allScripts[0], Is.EqualTo(tableScript), "Table script");
			Assert.That(allScripts[1], Is.EqualTo(viewScript), "View script");
		}
	}
}
