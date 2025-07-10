using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APPaymentProcessingModule : PaymentProcessingModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.APPaymentProcessing; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.APPaymentProcessing);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.APPaymentProcessing; }
		}

		protected override SecurityCheckpoint PostCheckpoint
		{
			get { return Env.Security.APPaymentProcessingPost; }
		}

		protected override SecurityCheckpoint PrintCheckpoint
		{
			get { return Env.Security.APPaymentProcessingPrint; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APPaymentProcessingFilterBusinessObject();
		}
	}
}
