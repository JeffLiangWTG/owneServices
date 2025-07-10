using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportDeclarationMessageSendingActionParent))]
	class ImportDeclarationMessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSendingObjectProperties()
		{
			var parent = (ImportDeclarationMessageSendingActionParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;
			CombineAssertions(() =>
			{
				AssertEquals("Declaration Type", properties.ElementAt(0).ResourceString.Caption);
				AssertEquals("Sub Style", properties.ElementAt(1).ResourceString.Caption);
				AssertEquals("Description", properties.ElementAt(2).ResourceString.Caption);
				AssertEquals("Entry Status", properties.ElementAt(3).ResourceString.Caption);
				AssertEquals("CUSCON?", properties.ElementAt(4).ResourceString.Caption);
				AssertEquals("Registration Number", properties.ElementAt(5).ResourceString.Caption);

				AssertEquals("Declaration Type", 105, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Sub Style", 80, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Description", 200, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Entry Status", 80, properties.ElementAt(3).ColumnWidth);
				AssertEquals("CUSCON?", 61, properties.ElementAt(4).ColumnWidth);
				AssertEquals("Registration Number", 123, properties.ElementAt(5).ColumnWidth);

				Assert(properties.All(x => x.IsMandatory));
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportDeclarationMessageSendingActionParent(Declaration);
		}

		JobDeclaration Declaration => fJobDeclaration ?? (fJobDeclaration = Factory.New<JobDeclaration>());
		JobDeclaration fJobDeclaration;
	}
}
