using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Modules
{
	public abstract class GlowOnlyModule : ZFilterGridModule
	{
		protected internal override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerID);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override bool SupportsWorkflow => false;

		protected abstract ControllerID ControllerID { get; }
	}
}
