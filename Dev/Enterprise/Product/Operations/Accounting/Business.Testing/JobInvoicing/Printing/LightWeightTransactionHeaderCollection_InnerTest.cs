using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.ComponentModel;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.JobInvoicePrintingFilter;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(LightWeightTransactionHeaderCollection))]
	public class LightWeightTransactionHeaderCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new LightWeightTransactionHeaderCollection(Factory, true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ARInvoice>();
		}

		public void TestModuleIDIsSet()
		{
			ModuleIDAttribute[] attributes = (ModuleIDAttribute[])GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
			Assert(attributes.Length > 0);
			AssertEquals("Module Id should be ARTransaction", Enterprise.ZArchitecture.Modules.ModuleId.ARTransaction, attributes[0].ModuleIdentifier.ID);
		}
	}
}
