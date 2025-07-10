using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	internal class AsycudaAdditionalInfoSynchronizer : BusinessObjectSynchroniser
	{
		public AsycudaAdditionalInfoSynchronizer(AsycudaAdditionalInfo destination, ForwardingConsol source) : base(destination, source)
		{
		}

		public new AsycudaAdditionalInfo Destination => (AsycudaAdditionalInfo)base.Destination;

		public new ForwardingConsol Source => (ForwardingConsol)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_DescriptionInfo, SourceValueDelegate, GetInfosAffectingCSI_Description));
		}

		IZType SourceValueDelegate() =>
			Source.JK_ConsolMode.ToString() switch
			{
				ContainerModes.LTL or ContainerModes.LCL or ContainerModes.Other =>
					((AsycudaBill)Destination.Parent).ABL_ManifestUQ.ToString() switch
					{
						Constants.AsycudaBill.PackageTypes.PX => new ZString(Constants.AsycudaAdditionalInfoDescriptions.Pallet),
						Constants.AsycudaBill.PackageTypes.NE => new ZString(Constants.AsycudaAdditionalInfoDescriptions.Bulk),
						_ => new ZString(Constants.AsycudaAdditionalInfoDescriptions.NoPacks)
					},
				ContainerModes.FCL or ContainerModes.FTL or ContainerModes.Groupage or ContainerModes.BuyersConsol or ContainerModes.ShippersConsol => new ZString(Constants.AsycudaAdditionalInfoDescriptions.Containerized),
				_ => ZString.Empty,
			};

		IEnumerable<ZPropertyInfo> GetInfosAffectingCSI_Description()
		{
			yield return Source.JK_ConsolModeInfo;
			yield return ((AsycudaBill)Destination.Parent).ABL_ManifestUQInfo;
		}
	}
}
