using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARPaymentProcessingModule : PaymentProcessingModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ARPaymentProcessing; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ARPaymentProcessing);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ARPaymentProcessing; }
		}

		protected override SecurityCheckpoint PostCheckpoint
		{
			get { return Env.Security.ARPaymentProcessingPost; }
		}

		protected override SecurityCheckpoint PrintCheckpoint
		{
			get { return Env.Security.ARPaymentProcessingPrint; }
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARPaymentProcessingFilterBusinessObject();
		}
	}
}
