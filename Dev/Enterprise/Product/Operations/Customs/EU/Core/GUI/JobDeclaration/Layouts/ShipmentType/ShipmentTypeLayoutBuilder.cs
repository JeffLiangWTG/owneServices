using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI
{
	public class ShipmentTypeLayoutBuilder<T> : Customs.GUI.ShipmentTypeLayoutBuilder<T> where T : JobDeclaration
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, h => !h.IsImport, h => h.JE_MessageTypeInfo);
			SetVisibility(ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, h => h.IsImport && h.DV1DetailsSupport, h => h.JE_MessageTypeInfo);
		}
	}
}
