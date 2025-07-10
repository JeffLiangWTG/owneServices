using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APComplianceDocumentController))]
	public class APComplianceDocumentControllerTest : ComplianceDocumentControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.APComplianceDocument;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			Factory.Save();
			return header;
		}

		protected override SecurityCheckpoint CheckpointForVoid
		{
			get { return Env.Security.VoidPayablesComplianceDocuments; }
		}

		protected override AccComplianceDocumentHeader NewComplianceDocumentHeader()
		{
			return Factory.NewWithValidTestData<APComplianceDocumentHeader>();
		}
	}
}
