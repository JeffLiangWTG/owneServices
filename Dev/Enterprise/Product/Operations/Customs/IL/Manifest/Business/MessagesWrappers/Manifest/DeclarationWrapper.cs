using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business
{
	sealed class DeclarationWrapper : IDeclaration
	{
		DeclarationWrapper(AsycudaManifestHeader asycudaManifestHeader)
		{
			this.asycudaManifestHeader = Argument.NotNull(asycudaManifestHeader, nameof(asycudaManifestHeader));

			lazyID = new Lazy<IIDType>(() => IDTypeWrapper.NewOrNull(asycudaManifestHeader.AMA_ManifestNumber));

			lazySubmitter = new Lazy<IDeclarationSubmitter>(() => DeclarationSubmitterWrapper.NewOrNull(asycudaManifestHeader));

			lazyTypeCode = new Lazy<ICodeType>(() => CodeTypeWrapper.NewOrNull(asycudaManifestHeader.AMA_ManifestType));

			lazyAdditionalInformation = new Lazy<ICollection<IDeclarationAdditionalInformation>>(GetAdditionalInformation);
		}

		internal static DeclarationWrapper NewOrNull(AsycudaManifestHeader asycudaManifestHeader) => asycudaManifestHeader == null ? null : new DeclarationWrapper(asycudaManifestHeader);

		#region IDeclaration

		ICollection<IDeclarationAdditionalInformation> IDeclaration.AdditionalInformation => lazyAdditionalInformation.Value;
		readonly Lazy<ICollection<IDeclarationAdditionalInformation>> lazyAdditionalInformation;

		ICollection<IDeclarationConsignment> IDeclaration.Consignment
			=> new List<IDeclarationConsignment>(asycudaManifestHeader
				.Bills
				.Where(b => b.ABL_BolType == AsycudaBillKindList.Codes.HWB)
				.Select(s => DeclarationConsignmentWrapper.NewOrNull(s))).AsReadOnly();

		IIDType IDeclaration.Id => lazyID.Value;
		readonly Lazy<IIDType> lazyID;

		IDeclarationSubmitter IDeclaration.Submitter => lazySubmitter.Value;
		readonly Lazy<IDeclarationSubmitter> lazySubmitter;

		ICodeType IDeclaration.TypeCode => lazyTypeCode.Value;
		readonly Lazy<ICodeType> lazyTypeCode;

		ICollection<IDeclarationAgent> IDeclaration.Agent => null;

		ICollection<IDeclarationBorderTransportMeans> IDeclaration.BorderTransportMeans => borderTransportMeans ?? (borderTransportMeans = GetBorderTransportMeans());
		ICollection<IDeclarationBorderTransportMeans> borderTransportMeans;

		ICollection<IDeclarationCarrier> IDeclaration.Carrier => carrier ?? (carrier = GetCarrier());
		ICollection<IDeclarationCarrier> carrier;

		#endregion

		ICollection<IDeclarationAdditionalInformation> GetAdditionalInformation()
			=> new List<IDeclarationAdditionalInformation>() { DeclarationAdditionalInformationWrapper.NewOrNull(asycudaManifestHeader) }.AsReadOnly();

		ICollection<IDeclarationCarrier> GetCarrier()
			=> new List<IDeclarationCarrier>() { DeclarationCarrierWrapper.NewOrNull(asycudaManifestHeader) }.AsReadOnly();

		ICollection<IDeclarationBorderTransportMeans> GetBorderTransportMeans()
		{
			var result = new Collection<IDeclarationBorderTransportMeans>();
			asycudaManifestHeader.TransportMeans?.Cast<TransportMean>().ForEach(t => result.Add(DeclarationBorderTransportMeansWrapper.NewOrNull(t)));
			return result;
		}

		readonly AsycudaManifestHeader asycudaManifestHeader;
	}
}
