using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract class MessagingProvider
	{
		public abstract MessageStatusProvider MessageStatusProvider { get; }

		public virtual EDIFACTMessageStatusCalculator GetEDIFACTStatusCalculator() => null;

		public virtual ZString GetInterchangeSenderID(BusinessObjectFactory factory) => ZString.Empty;

		public ZString GetMostSevereValueMessageStatus(AsycudaManifestHeader manifestHeader) => GetMostSevereValueMessageStatusCore(manifestHeader);

		protected virtual ZString GetMostSevereValueMessageStatusCore(AsycudaManifestHeader manifestHeader) => ZString.Empty;

		public ZString GetMostSevereValueCustomsStatus(AsycudaManifestHeader manifestHeader) => GetMostSevereValueCustomsStatusCore(manifestHeader);

		protected virtual ZString GetMostSevereValueCustomsStatusCore(AsycudaManifestHeader manifestHeader) => ZString.Empty;

		public ZString GetMostSevereValueCustomsStatus(AsycudaBill bill) => GetMostSevereValueCustomsStatusCore(bill);

		protected virtual ZString GetMostSevereValueCustomsStatusCore(AsycudaBill bill) => ZString.Empty;

		public ZString GetMostSevereValueMessageStatus(AsycudaBill bill) => GetMostSevereValueMessageStatusCore(bill);

		protected virtual ZString GetMostSevereValueMessageStatusCore(AsycudaBill bill) => ZString.Empty;

		public bool ShouldUpdateBillCustomsStatus(ZString actionPurpose) => ShouldUpdateBillCustomsStatusCore(actionPurpose);

		protected virtual bool ShouldUpdateBillCustomsStatusCore(ZString actionPurpose) => false;

		public bool ShouldClearCustomStatus(ZString customsStatus) => ShouldClearCustomStatusCore(customsStatus);
		protected virtual bool ShouldClearCustomStatusCore(ZString customsStatus) => false;

		public virtual Type GetAsycudaEDIMessageType() => typeof(AsycudaEDIMessage);

		public virtual CodeDescriptionPairList GetCustomsEntryNumberTypeList(BusinessObjectFactory factory, string countryCode)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes);
		}
	}
}
