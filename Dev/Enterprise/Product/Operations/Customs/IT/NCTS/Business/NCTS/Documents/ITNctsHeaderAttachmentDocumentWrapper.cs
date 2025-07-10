using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using ITNctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class ITNctsHeaderAttachmentDocumentWrapper : DocBaseWrapper
{
	ITNctsHeaderAttachmentDocumentWrapper(ITNctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	readonly ITNctsHeader nctsHeader;
	readonly BusinessObjectFactory factory;

	public static ITNctsHeaderAttachmentDocumentWrapper New(ITNctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
	{
		return new ITNctsHeaderAttachmentDocumentWrapper(nctsHeader, factoryToWrap);
	}

	public ITNctsDepCargoDescAttachmentWrapperCollection LinesWithAttachment => linesWithAttachment ?? (linesWithAttachment = new ITNctsDepCargoDescAttachmentWrapperCollection(nctsHeader, factory));

	ITNctsDepCargoDescAttachmentWrapperCollection linesWithAttachment;

	public ZString JobNumber => nctsHeader.JobNumber;

	public ZString MRN => nctsHeader.MovementReferenceNumber;
}
