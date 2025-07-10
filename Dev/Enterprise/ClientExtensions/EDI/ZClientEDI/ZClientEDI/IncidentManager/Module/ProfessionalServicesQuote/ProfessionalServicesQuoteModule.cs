
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class ProfessionalServicesQuoteModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.ProfessionalServicesQuote; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.ProfessionalServicesQuote);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ProfessionalServicesQuoteFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ProfessionalServicesQuoteCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ProfessionalServicesQuoteFilterControl(GridCollection, (ProfessionalServicesQuoteFilterBusinessObject)FilterBusinessObject);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.ProfessionalServicesQuote; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return EDIJobInvoicingConsumerTypes.PSQuote.Code; }
		}
	}
}
