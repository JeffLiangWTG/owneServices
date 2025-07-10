using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IAttachmentPrintingSupporter
{
	ZBool RequiresAttachment { get; }
}
