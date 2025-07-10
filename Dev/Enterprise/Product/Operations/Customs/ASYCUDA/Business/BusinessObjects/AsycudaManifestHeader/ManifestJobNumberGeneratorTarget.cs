using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ManifestJobNumberGeneratorTarget : NumberGeneratorTarget
	{
		#region Overrides of NumberGeneratorTarget

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(ManifestCustomsDataRegistry.Instance.ManifestJobNumberCustomization);
		}

		protected override int GetMaxLengthCore()
		{
			return AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return "Manifest job number";
		}

		public override string NumberCustomisationLocation
		{
			get { return ((IRegistryItemInternals)ManifestCustomsDataRegistry.Instance.ManifestJobNumberCustomization).Location; }
		}

		#endregion
	}
}
