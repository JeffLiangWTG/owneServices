using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.IN.Manifest.Module;

sealed class INManifestOperationalActionMethodProvider : OperationalActionMethodProvider
{
	public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
	{
		return new OperationalActionMethod[] { new MessageSendingOperationalActionMethod() };
	}
}
