using CargoWise.Database.Abstractions.Extensions;
using NUnit.Framework;

namespace CargoWise.Database.Abstractions.Test
{
	class DatabaseObjectCreateScriptTests
	{
		[Test]
		public void DatabaseObjectCreateScript_Constructor()
		{
			var script = new DatabaseObjectCreateScript("TableName", "CreateTable", "DropTable");
			Assert.That(script.ObjectName, Is.EqualTo("TableName"), "Table name");
			Assert.That(script.CreateScript, Is.EqualTo("CreateTable"), "Table create script");
			Assert.That(script.DropScript, Is.EqualTo("DropTable"), "Table drop script");
		}

		[Test]
		public void DatabaseObjectCreateScript_IsObjectNameSpecified()
		{
			Assert.That(new DatabaseObjectCreateScript("ObjectName", "", "").IsObjectNameSpecified, Is.True, "ObjectName specified");
			Assert.That(new DatabaseObjectCreateScript(null, "", "").IsObjectNameSpecified, Is.False, "ObjectName null");
			Assert.That(new DatabaseObjectCreateScript("", "", "").IsObjectNameSpecified, Is.False, "ObjectName empty string");
		}
	}
}
