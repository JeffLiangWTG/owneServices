using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ShipmentTypeLayoutBuilder<T> : Customs.GUI.ShipmentTypeLayoutBuilder<T> where T : JobDeclaration
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, h => !h.IsLocalExport, h => h.JE_MessageTypeInfo);
			SetVisibility(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, h => h.ContainerModeVisible && !h.IsLocalExport, new System.Func<T, CargoWise.EntityFramework.ZPropertyInfo>[] { h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo });
			SetVisibility(ShipmentTypeControlBag.Instance.TransactionTypeDropEdit, h => h.IsExport || h.IsLocalExport, h => h.JE_MessageTypeInfo);
			SetVisibility(ShipmentTypeControlBag.Instance.DeclarationTypeDropEdit, h => h.IsExport || h.IsImport, h => h.JE_MessageTypeInfo);
			SetVisibility(ShipmentTypeControlBag.Instance.ExporterTypeDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);
			SetVisibility(ShipmentTypeControlBag.Instance.TransactionDropEdit, h => h.IsImport, h => h.JE_MessageTypeInfo);
			SetVisibility(ShipmentTypeControlBag.Instance.PaymentTypeDropEdit, h => h.IsImport, h => h.JE_MessageTypeInfo);
			SetVisibility(ShipmentTypeControlBag.Instance.PlanTypeDropEdit, h => h.IsImport, h => h.JE_MessageTypeInfo);
			SetVisibility(ShipmentTypeControlBag.Instance.ImporterTypeDropEdit, h => h.IsImport, h => h.JE_MessageTypeInfo);
			AddControlBehaviour(ShipmentTypeControlBag.Instance.ImporterTypeDropEdit, new System.Action<ZDropEdit, T>((control, declaration) => {
				if (declaration != null && declaration.IsImport)
				{
					control.PreBoundMaxLength = 1;
				}
			}), h => h.JE_MessageTypeInfo);
		}
	}
}
