using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentVisualizer.Business.Testing
{
	sealed class VisualizerDocumentDataProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPropagateEvent()
		{
			var consol = (EnterpriseBusinessObject)Factory.New<IForwardingConsol>();

			var visualizerDocumentData = Factory.NewWithValidTestData<VisualizerDocumentData>();
			visualizerDocumentData.JDD_ParentID = consol.PK;
			visualizerDocumentData.JDD_ParentTableCode = consol.TablePrefix;

			visualizerDocumentData.Logs.AddNew(Events.DocumentDelivered);

			Factory.Save();

			var workflowEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentDeliveredCode);
			Assert("DDV event propagated to Consol", consol.Logs.Find(workflowEventFilter).Any());
		}
	}
}
