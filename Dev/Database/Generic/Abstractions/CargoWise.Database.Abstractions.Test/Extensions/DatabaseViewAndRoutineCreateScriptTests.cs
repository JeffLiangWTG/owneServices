using CargoWise.Database.Abstractions.Extensions;
using NUnit.Framework;

namespace CargoWise.Database.Abstractions.Test
{
	class DatabaseViewAndRoutineCreateScriptTests
	{
		[Test]
		public void DatabaseViewAndRoutineCreateScript_Constructor()
		{
			var viewScript = new DatabaseViewAndRoutineCreateScript("ViewName", "CreateView", "DropView", "VIEW");
			Assert.That(viewScript.ObjectName, Is.EqualTo("ViewName"), "View name");
			Assert.That(viewScript.CreateScript, Is.EqualTo("CreateView"), "View create script");
			Assert.That(viewScript.DropScript, Is.EqualTo("DropView"), "View drop script");
			Assert.That(viewScript.ObjectType, Is.EqualTo("VIEW"), "View type");
		}
	}
}
