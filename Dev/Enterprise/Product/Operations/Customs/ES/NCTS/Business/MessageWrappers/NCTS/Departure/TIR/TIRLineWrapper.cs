using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TIRLineWrapper : NctsDepartureBaseDepartureLineWrapper, ITIRLine
	{
		public TIRLineWrapper(NctsDepartureCargoDesc goodsItem)
			: base(goodsItem)
		{
		}

		public ITIRInternalPackagesInfo InternalPackages => internalPackages ?? (internalPackages = new TIRInternalPackagesInfoWrapper(goodsItem));
		TIRInternalPackagesInfoWrapper internalPackages;

		public IReadOnlyCollection<IDocumentsCommon> Documents
		{
			get
			{
				if (documents == null)
				{
					documents = goodsItem.SupportingDocuments
						.Cast<NctsSupportingDocument>()
						.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber))
						.ToList().AsReadOnly();
				}
				return documents;
			}
		}
		IReadOnlyCollection<DocumentCommonWrapper> documents;
	}
}
