using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract class MessageStatusProvider
	{
		public abstract bool AllowOriginalMessage(IMessageParent parent);
		public abstract bool AllowModificationMessage(IMessageParent parent);
		public abstract bool AllowCancellationMessage(IMessageParent parent);
		public virtual bool AllowManifestCancellationMessage(IMessageParent parent) => false;
		public abstract bool HasManifestBeenAcceptedByCustoms(IMessageParent parent);
		public virtual bool HasManifestBeenSubmittedToCustoms(IMessageParent parent) => HasManifestBeenAcceptedByCustoms(parent) || IsLastMessageStatusSentToCustoms(parent);
		protected virtual bool IsLastMessageStatusSentToCustoms(IMessageParent parent) => parent != null && MessageStatusCodeList.HasBeenSentCustoms(parent.MessageStatus);
		public virtual bool MessageStatusCanBeReset(IMessageParent parent) => false;

		public virtual CodeDescriptionPairList GetMessageStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<MessageStatusCodeList>();

		public virtual CodeDescriptionPairList GetArrivalStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<MessageStatusCodeList>();

		public CodeDescriptionPairList GetRegistrationStatusList(BusinessObjectFactory factory, ZString countryCode, ZString manifestTypes)
			=> factory.GetCachedValue(string.Join("|", "Customs.ASYCUDA.Business.MessageStatusProvider.RegistrationStatusList", countryCode, manifestTypes), () => GetRegistrationStatusListCore(factory, countryCode));

		public virtual CodeDescriptionPairList GetCargoStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<CargoStatusList>();

		public virtual CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode)
			=> AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(factory, countryCode, RegistrationStatusListCodeType);

		public virtual ZString RegistrationStatusListCodeType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus;

		public virtual string GetMessageFunctionSubTypeForSend(AsycudaManifestHeader header) => header?.MessageFunctionSubTypeForSend ?? string.Empty;

		public virtual string GetMessageFunctionSubTypeForAmend(AsycudaManifestHeader header) => header?.MessageFunctionSubTypeForAmend ?? string.Empty;

		public virtual string GetMessageFunctionSubTypeForCancel(AsycudaManifestHeader header) => header?.MessageFunctionSubTypeForCancel ?? string.Empty;
	}
}
