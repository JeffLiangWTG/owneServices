using System;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APComplianceDocumentController : ComplianceDocumentController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.APComplianceDocument;

		public override Type TypeOfTopLevelBusinessObject => typeof(APComplianceDocumentHeader);

		public override ControllerID ID => ControllerIDs.APComplianceDocument;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ViewPayablesComplianceDocument;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.DeletePayablesComplianceDocument;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EditPayablesComplianceDocument;

		protected override SecurityCheckpoint CheckPointForVoid => Env.Security.VoidPayablesComplianceDocuments;
	}
}
