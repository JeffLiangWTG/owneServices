using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARComplianceDocumentController))]
	public class ARComplianceDocumentControllerTest : ComplianceDocumentControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.ARComplianceDocument;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			Factory.Save();
			return header;
		}

		protected override SecurityCheckpoint CheckpointForVoid
		{
			get { return Env.Security.VoidReceivablesComplianceDocuments; }
		}

		protected override AccComplianceDocumentHeader NewComplianceDocumentHeader()
		{
			return Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
		}
	}
}
