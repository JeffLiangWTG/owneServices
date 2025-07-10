using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public class EUH7JobNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation => EUH7CustomsDataRegistry.Instance.EUH7JobNumberCustomization.Location();

		protected override int GetMaxLengthCore() => AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength;

		protected override ZString GetNameCore() => (NoResString)"Low Value (H7) Job Number Customization";

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(EUH7CustomsDataRegistry.Instance.EUH7JobNumberCustomization);
		}
	}
}
