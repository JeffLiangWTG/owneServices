using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using IDocument = CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class DocumentsAuthProvider : IDocumentsAuth
	{
		public DocumentsAuthProvider(AsycudaBill bill)
		{
			this.bill = bill;
		}
		readonly AsycudaBill bill;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??=
			bill.SupportingDocuments.Select(x => new DocumentProvider(x)).ToArray<IDocument>();
		IReadOnlyCollection<IDocument> supportingDocuments;

		public IAdditionalReference AdditionalReference => CachedValueHelper.GetValue(ref additionalReferenceCached, () =>
			AdditionalReferenceProvider.NewOrNull(bill.AdditionalDocuments.FirstOrDefault(x => x.IsAnAdditionalReference)));
		CachedValue<IAdditionalReference> additionalReferenceCached;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??=
			bill.AdditionalDocuments.Where(x => x.IsAnAdditionalInformation).Select(x => new CcQualifierAdditionalInformationProvider(x)).ToArray<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??=
			bill.PreviousDocuments.Select(x => new DocumentProvider(x)).ToArray<IDocument>();
		IReadOnlyCollection<IDocument> previousDocuments;

		public string ReferenceNumberUCR => bill.ABL_UCRNumber;
	}
}
