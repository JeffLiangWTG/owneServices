using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaContainerSynchroniser : ASYCUDA.Business.AsycudaContainerSynchroniser
	{
		public AsycudaContainerSynchroniser(ASYCUDA.Business.AsycudaContainer cusContainer, ForwardingContainer sourceContainer) : base(cusContainer, sourceContainer, null)
		{
		}

		protected override IEnumerable<ZPropertyInfo> GetSourceValuesAffectingContainerMode()
		{
			yield return Source.JC_IsEmptyContainerInfo;
		}

		protected override IZType GetContainerMode()
		{
			return Source.JC_IsEmptyContainer
				? (ZString)ILEmptyFullIndicatorList.Codes.A
				: (ZString)ILEmptyFullIndicatorList.Codes.B;
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_Seal1UnloadingStateInfo, () => GetUnloadingState(Source.JC_SealNum, Source.JC_IsSealOk, Destination.ACN_Seal1UnloadingState), () => new[] { Source.JC_SealNumInfo, Source.JC_IsSealOkInfo }, dontSetFieldsReadOnly: true));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_Seal2UnloadingStateInfo, () => GetUnloadingState(Source.JC_AdditionalSealNum, Source.JC_IsSealOk, Destination.ACN_Seal2UnloadingState), () => new[] { Source.JC_AdditionalSealNumInfo, Source.JC_IsSealOkInfo }, dontSetFieldsReadOnly: true));
			Synchronisers.Add(new FieldSynchroniser(Destination.ACN_Seal3UnloadingStateInfo, () => GetUnloadingState(Source.JC_Additional2SealNum, Source.JC_IsSealOk, Destination.ACN_Seal3UnloadingState), () => new[] { Source.JC_Additional2SealNumInfo, Source.JC_IsSealOkInfo }, dontSetFieldsReadOnly: true));
		}

		IZType GetUnloadingState(ZString sealNum, ZBool isSealOk, ZString currentUnloadingState)
		{
			if (sealNum.IsEmpty)
			{
				return currentUnloadingState;
			}

			return isSealOk ? new ZString("DEC") : ZString.Empty;
		}
	}
}
