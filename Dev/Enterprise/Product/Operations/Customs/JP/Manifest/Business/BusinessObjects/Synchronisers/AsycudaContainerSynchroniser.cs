using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaContainerSynchroniser : ASYCUDA.Business.AsycudaContainerSynchroniser
	{
		public AsycudaContainerSynchroniser(ASYCUDA.Business.AsycudaContainer cusContainer, ForwardingContainer sourceContainer) : base(cusContainer, sourceContainer, null)
		{
		}

		protected new AsycudaContainer Destination => (AsycudaContainer)base.Destination;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.CustomsTareWeightInfo, GetTareWeight, GetSourceValuesAffectingCustomsTareWeight));
			Synchronisers.Add(new FieldSynchroniser(Destination.CustomsWeightUQInfo, GetCustomsWeightUQ, GetSourceValuesAffectingCustomsWeightUQ));
			if (Source.Consol is ForwardingConsol consol)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.ACN_MoveOutDateInfo, consol.JK_PackDepotDispatchRequestedInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.VanningLocationCodeInfo, () => GetVanningLocationCode(consol), () => GetSourceValuesAffectingVanningLocationCode(consol)));
			}
		}

		IZType GetTareWeight()
		{
			return (ZDecimal)NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(Source.JC_TareWeight, Source.JC_GrossWeightUQ);
		}

		IZType GetCustomsWeightUQ()
		{
			return NACCSUnitConverter.IsCustomsWeightUnit(Source.JC_GrossWeightUQ) ? Source.JC_GrossWeightUQ : new ZString(Weight.Kilograms);
		}

		IZType GetVanningLocationCode(ForwardingConsol consol)
		{
			var result = ZString.Empty;
			if (consol.PackDepotAddress is OrgAddress packDepotAddress)
			{
				var ccpNumberFromAddress = packDepotAddress.CustomsCodes?.GetCustomsRegNo(CodeTypes.ControlledPremisesID, CountryCodes.Japan) ?? ZString.Empty;
				result = ccpNumberFromAddress.IsEmpty ? packDepotAddress.Header?.CustomsCodes?.GetCustomsRegNo(CodeTypes.ControlledPremisesID, CountryCodes.Japan) ?? ZString.Empty : ccpNumberFromAddress;
			}
			return result.Left(Destination.VanningLocationCodeInfo.MaxLength);
		}

		IEnumerable<ZPropertyInfo> GetSourceValuesAffectingCustomsTareWeight()
		{
			yield return base.Source.JC_TareWeightInfo;
			yield return base.Source.JC_GrossWeightUQInfo;
		}

		IEnumerable<ZPropertyInfo> GetSourceValuesAffectingCustomsWeightUQ()
		{
			yield return base.Source.JC_GrossWeightUQInfo;
		}

		IEnumerable<ZPropertyInfo> GetSourceValuesAffectingVanningLocationCode(ForwardingConsol consol)
		{
			yield return consol.JK_OA_PackDepotAddressInfo;
		}
	}
}
