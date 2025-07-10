using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using IDocument = CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class DocumentAuthorisationsProvider : IDocumentsAuth
	{
		public DocumentAuthorisationsProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}
		readonly AsycudaPackedItem packedItem;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??=
			packedItem.SupportingDocuments.Select(x => new DocumentProvider(x)).ToArray<IDocument>();
		IReadOnlyCollection<IDocument> supportingDocuments;

		public IAdditionalReference AdditionalReference => CachedValueHelper.GetValue(ref additionalReferenceCached, () =>
			AdditionalReferenceProvider.NewOrNull(packedItem.AdditionalDocuments.FirstOrDefault(x => x.IsAnAdditionalReference)));
		CachedValue<IAdditionalReference> additionalReferenceCached;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??=
			packedItem.AdditionalDocuments.Where(x => x.IsAnAdditionalInformation).Select(x => new CcQualifierAdditionalInformationProvider(x)).ToArray<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??=
			packedItem.PreviousDocuments.Select(x => new DocumentProvider(x)).ToArray<IDocument>();
		IReadOnlyCollection<IDocument> previousDocuments;

		public string ReferenceNumberUCR => packedItem.Bill.ABL_UCRNumber;
	}
}
