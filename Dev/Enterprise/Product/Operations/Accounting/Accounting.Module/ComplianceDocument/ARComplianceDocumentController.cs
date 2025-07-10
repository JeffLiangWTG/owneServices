using System;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARComplianceDocumentController : ComplianceDocumentController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.ARComplianceDocument;

		public override Type TypeOfTopLevelBusinessObject => typeof(ARComplianceDocumentHeader);

		public override ControllerID ID => ControllerIDs.ARComplianceDocument;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ViewReceivablesComplianceDocument;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.DeleteReceivablesComplianceDocument;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EditReceivablesComplianceDocument;

		protected override SecurityCheckpoint CheckPointForVoid => Env.Security.VoidReceivablesComplianceDocuments;
	}
}
