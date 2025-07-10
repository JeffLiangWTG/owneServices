using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class AnnexNCTS5SendMessageWrapper : NCTS5CommonSendMessageWrapper, IAnnexNCTSMessageDataProvider
	{
		public AnnexNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData, IEnumerable<NctsCusStorageDocPivot> docPivots, ZString requestDispatch) : base(header, certificateData)
		{
			this.docPivots = Argument.NotNull(docPivots, nameof(docPivots));
			Argument.GreaterThan(docPivots.Count(), 0, nameof(docPivots));
			Argument.NotNullOrEmpty(requestDispatch, nameof(requestDispatch));
			DispatchRequestCode = requestDispatch == Customs.Business.YesNoList.Codes.Yes ? (ZString)YesCodeES : requestDispatch;
		}
		readonly IEnumerable<NctsCusStorageDocPivot> docPivots;
		const string YesCodeES = "S";

		public INCTSCommonTransitOperationMRN TransitOperation => transitOperation ?? (transitOperation = new NCTS5CommonTransitOperationMRNWrapper(nctsHeader));
		NCTS5CommonTransitOperationMRNWrapper transitOperation;

		public ZString DispatchRequestCode { get; }

		public IReadOnlyCollection<IAnnexDocCommon> Documents
		{
			get
			{
				if (documents == null)
				{
					var documentsList = new List<AnnexDocCommonWrapper>();

					docPivots.Where(x => x != null).ForEach(x => documentsList.Add(new AnnexDocCommonWrapper(x.Document, x.CSD_Description)));

					documents = documentsList.AsReadOnly();
				}
				return documents;
			}
		}
		IReadOnlyCollection<AnnexDocCommonWrapper> documents;
	}
}
