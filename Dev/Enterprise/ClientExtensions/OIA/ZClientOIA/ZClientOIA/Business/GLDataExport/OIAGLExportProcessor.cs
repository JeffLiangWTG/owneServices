using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.OIA.Business
{
	internal class OIAGLExportProcessor : GLTransactionExportProcessor
	{
		#region Constructor

		protected OIAGLExportProcessor() { }

		public static new OIAGLExportProcessor New()
		{
			return new OIAGLExportProcessor();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		protected override GLTransactionExporter GetNewGLTransactionExporter(GLTransactionBusinessObject glBizObj, NotificationBuffer notification)
		{
			return new OIAGLTransactionExporter((OIAGLTransactionBusinessObject)glBizObj, notification, true);
		}

		protected override GLTransactionBusinessObject GetNewGLTransactionBusinessObject()
		{
			return new OIAGLTransactionBusinessObject(new BusinessObjectFactory());
		}
	}
}
