using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
{
	public override bool AllowOriginalMessage(IMessageParent parent) => !HasManifestBeenAcceptedByCustoms(parent);

	public override bool AllowModificationMessage(IMessageParent parent) => HasManifestBeenAcceptedByCustoms(parent);

	public override bool AllowCancellationMessage(IMessageParent parent) => HasManifestBeenAcceptedByCustoms(parent);

	public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => parent != null && (parent.HasCustomsNumbers || parent.IsCustomsCleared);
}
