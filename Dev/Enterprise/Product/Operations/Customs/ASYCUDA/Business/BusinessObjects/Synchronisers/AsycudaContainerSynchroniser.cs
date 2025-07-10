using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaContainerSynchroniser : ContainerSynchroniser<AsycudaContainer>
	{
		public AsycudaContainerSynchroniser(AsycudaContainer destination, ForwardingContainer source, ForwardingShipment shipmentSource)
			: base(destination, source, shipmentSource)
		{ }

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			var shouldMakeContainerTypeEditable = Source.Container == null || Source.Container.RC_Code.IsEmpty;
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_RC_ContainerTypeInfo, Source.JC_RCInfo, shouldMakeContainerTypeEditable));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_EmptyFullIndicatorInfo, GetContainerMode, GetSourceValuesAffectingContainerMode, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_NumberOfPackagesInfo, GetPackageCount, GetSourceInfosAffectingPackageCount, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_GoodsWeightInfo, Source.JC_GrossWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_GoodsWeightUQInfo, Source.JC_GrossWeightUQInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_SealingPartyTypeInfo, () => GetSealPartyType(Source.JC_SealParty), () => new[] { Source.JC_SealPartyInfo }, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_SealingPartyType2Info, () => GetSealPartyType(Source.JC_AdditionalSealParty), () => new[] { Source.JC_AdditionalSealPartyInfo }, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_SealingPartyType3Info, () => GetSealPartyType(Source.JC_Additional2SealParty), () => new[] { Source.JC_Additional2SealPartyInfo }, true));
		}

		protected virtual IZType GetContainerMode()
		{
			switch (Source.JC_ContainerMode)
			{
				case Core.Constants.ContainerModes.BreakBulk:
				case Core.Constants.ContainerModes.RollOnRollOff:
					return ZString.Empty;

				case Core.Constants.ContainerModes.LCL:
				case Core.Constants.ContainerModes.LTL:
				case Core.Constants.ContainerModes.Groupage:
					return (ZString)EmptyFullIndicatorList.Codes.LessThanFullContainerLoad;

				default:
					return (ZString)EmptyFullIndicatorList.Codes.FullContainerLoad;
			}
		}

		protected virtual IEnumerable<ZPropertyInfo> GetSourceValuesAffectingContainerMode()
		{
			yield return Source.JC_ContainerModeInfo;
		}

		IZType GetPackageCount()
		{
			ZInt packCount = 0;
			var header = Destination.Header;
			if (header != null)
			{
				var consol = header.Consol;
				foreach (var shipment in Source.GetParentShipments())
				{
					foreach (PackLine packLine in shipment.OuterPackLines)
					{
						var commonContainer = packLine.GetContainer(consol);
						if (commonContainer != null && !commonContainer.IsDeleted && commonContainer.JC_ContainerNum == Destination.ACN_ContainerNumber)
						{
							packCount += packLine.JL_PackageCount;
						}
					}
				}
			}
			return packCount;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingPackageCount()
		{
			foreach (var ship in Source.GetParentShipments())
			{
				foreach (PackLine pack in ship.OuterPackLines)
				{
					yield return pack.JL_Calc_JS_DestinationInfo;
					yield return pack.JL_PackageCountInfo;
				}
			}
		}

		IZType GetSealPartyType(ZString sealPartyType)
		{
			return Destination.Lookups.SealingPartyList.ContainsCode(sealPartyType) ? sealPartyType : ZString.Empty;
		}
	}
}
