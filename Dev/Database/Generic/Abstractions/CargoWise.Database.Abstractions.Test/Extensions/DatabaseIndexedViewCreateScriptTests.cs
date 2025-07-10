using CargoWise.Database.Abstractions.Extensions;
using NUnit.Framework;

namespace CargoWise.Database.Abstractions.Test
{
	class DatabaseIndexedViewCreateScriptTests
	{
		[Test]
		public void DatabaseIndexedViewCreateScript_Constructor()
		{
			var viewScript = new DatabaseIndexedViewCreateScript("ViewName", "CreateView", "create index", "DropView", "VIEW");
			Assert.That(viewScript.ObjectName, Is.EqualTo("ViewName"), "View name");
			Assert.That(viewScript.CreateScript, Is.EqualTo("CreateView"), "View create script");
			Assert.That(viewScript.DropScript, Is.EqualTo("DropView"), "View drop script");
			Assert.That(viewScript.ObjectType, Is.EqualTo("VIEW"), "View type");
			Assert.That(viewScript.IndexCreateScript, Is.EqualTo("create index"));
		}
	}
}
