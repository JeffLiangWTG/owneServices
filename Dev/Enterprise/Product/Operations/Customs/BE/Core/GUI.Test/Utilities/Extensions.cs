using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

static class Extensions
{
	public static ZString GetCaption(this PanelLayout layout, ControlReference controlReference, BusinessObject dataItem)
	{
		return layout != null && layout.TryGetCaption(controlReference, dataItem, out var resourceStringData) ? resourceStringData.Caption : string.Empty;
	}
}
