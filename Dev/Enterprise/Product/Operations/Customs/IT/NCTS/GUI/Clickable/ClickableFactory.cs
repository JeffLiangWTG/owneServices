using System;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

using OnCreateMenuItem = Action<ZMenuItem, IClickableItem>;

public static class ClickableFactory
{
	public static ClickableMenuItemComponent CreateClickableIrildesRequestMenuItemComponent
		(NctsHeader header, OnCreateMenuItem onCreateMenuItem = null)
		=> new(new ClickableIrildesRequestContext(header), onCreateMenuItem);

	public static ClickableMenuItemComponent CreateClickableNctsElectronicFolderStatusRequestMenuItemComponent
		(NctsHeader header, OnCreateMenuItem onCreateMenuItem = null)
		=> new(new ClickableNctsElectronicFolderStatusRequestContext(header), onCreateMenuItem);

	public static ClickableMenuItemComponent CreateClickableTransitAccompanyingDocumentRequestMenuItemComponent
		(NctsHeader header, OnCreateMenuItem onCreateMenuItem = null)
		=> new(new ClickableTransitAccompanyingDocumentRequestContext(header), onCreateMenuItem);
}
