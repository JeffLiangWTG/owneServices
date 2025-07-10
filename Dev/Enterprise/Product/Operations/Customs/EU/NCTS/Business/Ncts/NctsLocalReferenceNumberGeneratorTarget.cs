using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsLocalReferenceNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return Res.GetString("72098B23-529D-448F-9F97-B0C6EE99D6D4", "Customs -> NCTS Job Number Customization"); }
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(CustomsDataRegistry.Instance.NctsLocalReferenceNumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return CusInBondHeaderSchema.BH_JobReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("12345678-56a6-4fd3-a36d-ad85e78dd504", "NCTS Job Number");
		}
	}
}
