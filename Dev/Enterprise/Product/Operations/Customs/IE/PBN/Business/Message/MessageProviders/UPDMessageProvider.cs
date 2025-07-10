using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces.PBN;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class UPDMessageProvider : IUPDHeader
	{
		public UPDMessageProvider(PBNMessageSendingObject sendingObject)
		{
			Argument.NotNull(sendingObject, nameof(sendingObject));
			header = sendingObject.Header;
		}
		readonly AsycudaManifestHeader header;

		public IReadOnlyCollection<IPBNDeclaration> Declarations => declarationsCached ??= header.CustomsReferenceCollection.Where(x => x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBA && !x.CSI_ReferenceNumber.IsEmpty).Select(PBNDeclarationProvider.New)
			.Union(header.TransitDeclarationCollection.Where(x => x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBA && !x.CSI_ReferenceNumber.IsEmpty).Select(PBNDeclarationProvider.New)).ToArray();
		IReadOnlyCollection<IPBNDeclaration> declarationsCached;

		public IReadOnlyCollection<IPBNDeclaration> DeclarationsToDelete => declarationsToDeleteCached ??= header.CustomsReferenceCollection.Where(x => x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBD && !x.CSI_ReferenceNumber.IsEmpty).Select(PBNDeclarationProvider.New)
			.Union(header.TransitDeclarationCollection.Where(x => x.CSI_Status == PBNDeclarationReferenceStatusList.Codes.TBD && !x.CSI_ReferenceNumber.IsEmpty).Select(PBNDeclarationProvider.New)).ToArray();
		IReadOnlyCollection<IPBNDeclaration> declarationsToDeleteCached;
	}
}
