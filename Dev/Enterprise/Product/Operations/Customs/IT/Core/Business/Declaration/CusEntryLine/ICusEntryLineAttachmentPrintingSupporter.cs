using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public interface ICusEntryLineAttachmentPrintingSupporter : IAttachmentPrintingSupporter
{
	ZBool RequiresContainersSection { get; }
	ZBool RequiresSupportingDocumentsSection { get; }
	IEnumerable<ZString> SupportingDocumentsFormatted { get; }
	ZBool RequiresFeesSection { get; }
	IEnumerable<ZString> AdditionalInfosFormatted { get; }
}
