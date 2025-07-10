using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.ExportMessageConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXExportSendMessageWrapper : ExportSendMessageCommonWrapper, IComplXExportMessageDataProvider
	{
		public ComplXExportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData)
			: base(cusEntryHeader, certificateData) { }
		const string LocalReferenceNumberSuffix = "_C";

		public ZString MessageType => ExportDeclarationWrapperMessageTypeCodeList.ExportComplementaryXDeclaration;

		protected override ZString LocalReferenceNumberCore => entryHeader.CH_BGMReference + LocalReferenceNumberSuffix;

		public ZString CustomsProcedureCategory5 => entryHeader.MovementReferenceNumber;

		public IReadOnlyCollection<IComplXExportLine> Lines => lines ?? (lines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new ComplXExportLineWrapper(x)).ToArray());
		IReadOnlyCollection<IComplXExportLine> lines;
	}
}
