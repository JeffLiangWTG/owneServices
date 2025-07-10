using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AnnexAESSendMessageWrapper : AESCommonSendMessageWrapper, IAnnexAESMessageDataProvider
	{
		public AnnexAESSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, IEnumerable<CusStorageDocPivot> docPivots, ZString requestDispatch) : base(cusEntryHeader, certificateData)
		{
			this.docPivots = Argument.NotNull(docPivots, nameof(docPivots));
			Argument.GreaterThan(docPivots.Count(), 0, nameof(docPivots));
			Argument.NotNullOrEmpty(requestDispatch, nameof(requestDispatch));
			DispatchRequestCode = requestDispatch == Customs.Business.YesNoList.Codes.Yes ? (ZString)YesCodeES : requestDispatch;
		}
		readonly IEnumerable<CusStorageDocPivot> docPivots;
		const string YesCodeES = "S";

		public IAESCommonExportOperationMRN ExportOperation => exportOperation ?? (exportOperation = new AESCommonExportOperationMRNWrapper(entryHeader));
		AESCommonExportOperationMRNWrapper exportOperation;

		public ZString DispatchRequestCode { get; }

		public IReadOnlyCollection<IAnnexDocCommon> Documents
		{
			get
			{
				if (documents == null)
				{
					var docsList = new List<AnnexDocCommonWrapper>();

					docPivots.Where(x => x != null).ForEach(x => docsList.Add(new AnnexDocCommonWrapper(x.Document, x.CSD_Description)));
					documents = docsList.AsReadOnly();
				}
				return documents;
			}
		}
		IReadOnlyCollection<AnnexDocCommonWrapper> documents;
	}
}
