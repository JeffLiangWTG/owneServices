using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXAESLineWrapper : AESCommonLineWrapper, IComplXAESLine
	{
		public ComplXAESLineWrapper(CusEntryLine entryLine) : base(entryLine)
		{
		}

		const string ConcessionCode9PV = "9PV";

		public IAESCommonOrigin Origin => origin ?? (origin = new AESCommonOriginWrapper(randomLine));
		AESCommonOriginWrapper origin;

		public IComplXAESCommodity Commodity => commodity ?? (commodity = IsProcedureCode9PV
																		? new ComplXAESCommodityWrapper(entryLine)
																		: null);
		ComplXAESCommodityWrapper commodity;

		public IReadOnlyCollection<IAESCommonDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					var previousDocumentsList = new List<AESCommonDocumentWrapper>();

					var prevDocs = entryLine.PreviousDocuments;

					if (!prevDocs.IsNullOrEmpty() && !entryLine.HasPRECustomsOffice)
					{
						var hasNoClDoc = entryLine.GetPreviouslySentPreviousDocuments().IsNullOrEmpty();

						var prevDocsWithC651OrNMRN = hasNoClDoc
							? prevDocs.Cast<PreviousDocument>().Where(x => x.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.C651 || IsPreviousDocumentNMRNAndHasQuantity(x))
							: prevDocs.Cast<PreviousDocument>().Where(x => IsPreviousDocumentNMRNAndHasQuantity(x));
						if (!prevDocsWithC651OrNMRN.IsNullOrEmpty())
						{
							PreviousDocumentHelper.AddPreviousDocumentWithLengthForGoodsItemNumber(entryLine, previousDocumentsList, prevDocsWithC651OrNMRN.First(), 1);
						}
					}
					previousDocuments = previousDocumentsList.AsReadOnly();
				}
				return previousDocuments;
			}
		}
		IReadOnlyCollection<AESCommonDocumentWrapper> previousDocuments;

		public IReadOnlyCollection<IComplXAESSupportingDocument> SupportingDocuments => GetDocuments();

		IReadOnlyCollection<ComplXAESSupportingDocumentWrapper> GetDocuments()
		{
			if (supportingDocuments == null)
			{
				var documents = new List<ComplXAESSupportingDocumentWrapper>();
				var supDocsInCL = entryLine.GetPreviouslySentSupportingDocuments();

				var supDocs = entryLine.SupportingDocuments.Cast<SupportingDocument>()
													.Where(doc => doc.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInCL)).ToList();

				supDocs.AddRange(entryLine.Header.SupportingDocuments.Cast<SupportingDocument>()
													.Where(doc => doc.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInCL)).ToList());

				if (!supDocs.Any() && SupportingDocumentHelper.HasComplXExportDocuments(supDocsInCL))
				{
					supDocs.Add(SupportingDocumentHelper.GetFirstComplXExportDocument(supDocsInCL));
				}

				ZShort seqNum = 1;
				foreach (var doc in supDocs)
				{
					documents.Add(new ComplXAESSupportingDocumentWrapper(doc, seqNum));
					seqNum++;
				}
				supportingDocuments = documents.AsReadOnly();
			}
			return supportingDocuments;
		}
		ReadOnlyCollection<ComplXAESSupportingDocumentWrapper> supportingDocuments;

		ZBool IsProcedureCode9PV => randomLine.GetAdditionalProcedureCodesListForExportUccMessage().Contains(ConcessionCode9PV);

		ZBool IsPreviousDocumentNMRNAndHasQuantity(PreviousDocument prevDoc) => prevDoc.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.NMRN && !prevDoc.CSI_Quantity.IsEmpty;
	}
}
