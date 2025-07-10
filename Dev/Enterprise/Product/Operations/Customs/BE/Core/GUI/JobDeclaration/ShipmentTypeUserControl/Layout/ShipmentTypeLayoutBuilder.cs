using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.BE.GUI;

public class ShipmentTypeLayoutBuilder : ShipmentTypeLayoutBuilder<JobDeclaration>
{
	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, h => !h.IsImport && h.IsUCC6, h => h.JE_MessageTypeInfo);
	}

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();

		SetCaption(ShipmentTypeControlBag.Instance.EntryStyleDropEdit, h => EntryStyleCaption, h => h.JE_MessageTypeInfo);
	}

	internal static ResourceStringData EntryStyleCaption => Res.GetData("3D15F6E7-FD53-4456-B58D-57C843B704D1", "Entry Style", "[UCC 1/1] Entry Style");
}
