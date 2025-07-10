using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing;

[TestedType(typeof(FinalizeAVABRMessageSendingActionParent))]
public class FinalizeAVABRMessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingObjectProperties()
	{
		var parent = (FinalizeAVABRMessageSendingActionParent)GetNewBusinessObject();
		var properties = parent.MessageSendingObjectProperties.ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("Declaration Type", properties.ElementAt(0).ResourceString.Caption);
			AssertEquals("Description", properties.ElementAt(1).ResourceString.Caption);
			AssertEquals("Entry Status", properties.ElementAt(2).ResourceString.Caption);
			AssertEquals("Registration Number", properties.ElementAt(3).ResourceString.Caption);

			AssertEquals("Declaration Type", 105, properties.ElementAt(0).ColumnWidth);
			AssertEquals("Description", 200, properties.ElementAt(1).ColumnWidth);
			AssertEquals("Entry Status", 80, properties.ElementAt(2).ColumnWidth);
			AssertEquals("Registration Number", 123, properties.ElementAt(3).ColumnWidth);

			Assert(properties.All(x => x.IsMandatory));
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new FinalizeAVABRMessageSendingActionParent(declaration);
	}
}
