using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class ExitControlNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation => Res.GetString("8aa9a0e9-da9e-406d-83fb-47d58bcd971b", "Customs -> European Union (Common) -> Exit Control -> Exit Control Job Number Customization");

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(ExitControlCustomsDataRegistry.Instance.ExitControlCustomization);
		}

		protected override int GetMaxLengthCore()
		{
			return CusExitHeaderSchema.CXH_JobReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("3562a92e-4c28-48b5-b74b-452ffc2857ca", "Exit control job number");
		}
	}
}
