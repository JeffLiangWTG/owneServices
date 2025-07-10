using System;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IL.Manifest.Business
{
	sealed class DeclarationAdditionalInformationWrapper : IDeclarationAdditionalInformation
	{
		DeclarationAdditionalInformationWrapper(AsycudaManifestHeader asycudaManifestHeader)
		{
			lazyStatementCode = new Lazy<ICodeType>(() => CodeTypeWrapper.NewOrNull(GetStatementCode(asycudaManifestHeader.AMA_TransportMode)));

			lazyStatementTypeCode = new Lazy<ICodeType>(() => CodeTypeWrapper.NewOrNull(Constants.AdditionalInformation.ManifestStatementTypeCode));
		}

		internal static DeclarationAdditionalInformationWrapper NewOrNull(AsycudaManifestHeader asycudaManifestHeader)
			=> asycudaManifestHeader == null
			? null
			: new DeclarationAdditionalInformationWrapper(asycudaManifestHeader);

		#region IAdditionalInformation

		ICodeType IDeclarationAdditionalInformation.StatementCode => lazyStatementCode.Value;
		readonly Lazy<ICodeType> lazyStatementCode;

		ICodeType IDeclarationAdditionalInformation.StatementTypeCode => lazyStatementTypeCode.Value;
		readonly Lazy<ICodeType> lazyStatementTypeCode;

		#endregion

		string GetStatementCode(ZString transportMode)
		{
			switch (transportMode)
			{
				case RefTransportModeList.Codes.SEA:
					return "1";

				case RefTransportModeList.Codes.ROA:
					return "2";

				default:
					return null;
			}
		}
	}
}
