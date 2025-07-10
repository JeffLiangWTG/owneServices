using System;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	sealed class MessageManifestWrapper : IMessageManifest
	{
		internal MessageManifestWrapper(AsycudaManifestHeader asycudaManifestHeader)
		{
			asycudaManifestHeader = Argument.NotNull(asycudaManifestHeader, nameof(asycudaManifestHeader));
			lazyDeclaration = new Lazy<IDeclaration>(() => DeclarationWrapper.NewOrNull(asycudaManifestHeader));
		}

		internal static MessageManifestWrapper NewOrNull(AsycudaManifestHeader asycudaManifestHeader) => asycudaManifestHeader == null ? null : new MessageManifestWrapper(asycudaManifestHeader);

		IDeclaration IMessageManifest.Declaration => lazyDeclaration.Value;

		IRequestContentHeader IMessageManifest.RequestContentHeader => RequestContentHeaderWrapper.New();

		readonly Lazy<IDeclaration> lazyDeclaration;
	}
}
