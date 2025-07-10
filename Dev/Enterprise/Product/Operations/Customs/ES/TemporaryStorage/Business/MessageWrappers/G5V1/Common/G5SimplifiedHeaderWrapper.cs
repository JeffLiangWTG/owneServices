using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5SimplifiedHeaderWrapper : IG5SimplifiedHeader
	{
		public G5SimplifiedHeaderWrapper(TemporaryStorageHeader tempHeader)
		{
			this.tempHeader = Argument.NotNull(tempHeader, nameof(tempHeader));
			bill = tempHeader.Bills.FirstOrDefault();
		}

		protected readonly TemporaryStorageHeader tempHeader;
		protected readonly TemporaryStorageBill bill;

		public ZString LRN => tempHeader.LRN;

		public IG5PartyInfo Declarant => declarant ??= G5PartyInfoWrapper.New(tempHeader.Declarant);
		G5PartyInfoWrapper declarant;

		public IG5RepresentativeInfo Representative => representative ??= G5RepresentativeInfoWrapper.New(tempHeader.Representative);
		G5RepresentativeInfoWrapper representative;

		public IReadOnlyCollection<IDocumentsCommon> AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					var docs = new List<DocumentCommonWrapper>();

					if (bill != null)
					{
						docs.AddRange(bill.AdditionalInfos.Where(doc => doc.CSI_SubType == AdditionalDocList.Codes.AdditionalInformation)
															.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_Description)));
					}

					additionalInfos = docs.AsReadOnly();
				}
				return additionalInfos;
			}
		}
		IReadOnlyCollection<DocumentCommonWrapper> additionalInfos;
	}
}
