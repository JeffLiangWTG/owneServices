using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APComplianceDocumentModule : ComplianceDocumentModule
	{
		public override ModuleIdentifier ID => ModuleIDs.APComplianceDocument;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.PayablesComplianceDocuments;

		public override string WorkflowType => WorkflowDescriptors.APComplianceDocumentCode;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.APComplianceDocument);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APComplianceDocumentFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var controller = new ComplianceDocumentFilterStripControl(GridCollection, (APComplianceDocumentFilterStripBusinessObject)FilterBusinessObject);
			controller.FilteredGrid.ColorContextKey = "APComplianceDocumentFilterStripControl";
			return controller;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new APComplianceDocumentHeaderCollection(Factory);
		}
	}
}
