using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobComInvoiceGroupHeader))]
	class EMCSJobComInvoiceGroupHeaderTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			Assert("EMCSJobComInvoiceGroupHeader only load from EMCSJobDeclaration.", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.JobComInvoiceHeaders.AddNew();
			groupHeader.Charges.AddNew();

			return groupHeader;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
