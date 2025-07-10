using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class CommercialInvoiceXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", new TestCommercialInvoiceXmlImportTask().GetImporterType() == typeof(InvoiceXmlDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("39856dc8-e06c-42c9-b2e1-a1f23222961a", "Commercial Invoice XML Import"), new TestCommercialInvoiceXmlImportTask().TaskDescription);
		}

		class TestCommercialInvoiceXmlImportTask : CommercialInvoiceXmlImportTask
		{
			public TestCommercialInvoiceXmlImportTask()
				: base(new StringRegistryItem("", null, null, null, RegistryStorageFlags.System), null, new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.System))
			{ }

			public Type GetImporterType()
			{
				return NewImporter().GetType();
			}
		}
	}
}
