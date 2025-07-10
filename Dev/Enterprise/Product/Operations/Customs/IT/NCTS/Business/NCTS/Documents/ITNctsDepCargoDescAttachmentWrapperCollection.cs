using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using ITNctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.Business;

public class ITNctsDepCargoDescAttachmentWrapperCollection : DocBaseWrapperCollection<ITNctsDepCargoDescAttachmentWrapper>
{
	public ITNctsDepCargoDescAttachmentWrapperCollection(ITNctsHeader nctsHeader, BusinessObjectFactory factory) : base(factory)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		var goodsItems = nctsHeader.IsPhase5 ? nctsHeader.Bills.SelectMany(x => x.GoodsItems) : nctsHeader.MovementHeader.GoodsItems;
		goodsItems
			.Where(x => x.AttachmentPrintingSupporter.RequiresAttachment)
			.ForEach(x => Add(ITNctsDepCargoDescAttachmentWrapper.New(x, factory)));
	}
}
