using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CodeDescriptionOptionCollectionParent))]
	class CodeDescriptionOptionCollectionParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CodeDescriptionOptionCollectionParent(Factory, Factory.New<JobComInvoiceLine>().CargoAttributes);
		}

		public void TestLoad()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var allOptions = new CargoAttributeList().Cast<ICodeDescription>();
			var testParent = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes);
			var testCollection = testParent.OptionCollection.Cast<CodeDescriptionOption>();
			AssertEquals(allOptions.Count(), testCollection.Count());
			foreach (var option in allOptions)
			{
				Assert(testCollection.Any(o => o.Code == option.Code));
			}
		}
	}
}
