using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Manifest.Business;

public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
{
	public override bool AllowCancellationMessage(IMessageParent parent) => false;

	public override bool AllowModificationMessage(IMessageParent parent) => false;

	public override bool AllowOriginalMessage(IMessageParent parent) => false;

	public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;

	public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent) => false;

	public override CodeDescriptionPairList GetMessageStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<JPMessageStatusList>();

	public override CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<JPCustomsStatusList>();

	public override CodeDescriptionPairList GetCargoStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<CargoStatusList>();
}
