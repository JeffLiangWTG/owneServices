using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.OIA.Business.Testing
{
	class OIAGLExportProcessorTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			OIAGLExportProcessorForTest processor = new OIAGLExportProcessorForTest();
			AssertEquals("Exporter type", typeof(OIAGLTransactionExporter), processor.GetNewGLTransactionExporter(new OIAGLTransactionBusinessObject(Factory), new NotificationBuffer()).GetType());
			AssertEquals("GL Business Object Type", typeof(OIAGLTransactionBusinessObject), processor.GetNewGLTransactionBusinessObject().GetType());
		}

		class OIAGLExportProcessorForTest : OIAGLExportProcessor
		{
			public new GLTransactionExporter GetNewGLTransactionExporter(GLTransactionBusinessObject bizObj, NotificationBuffer buffer)
			{
				return base.GetNewGLTransactionExporter(bizObj, buffer);
			}

			public new GLTransactionBusinessObject GetNewGLTransactionBusinessObject()
			{
				return base.GetNewGLTransactionBusinessObject();
			}
		}
	}
}
