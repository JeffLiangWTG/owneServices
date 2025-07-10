using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemSupplyChainActorLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5GoodsItemSupplyChainActorLayoutWithGrid()
		{
			Layout = CreateGoodsItemSupplyChainActorLayout();
		}

		public PanelLayout Layout { get; }

		static PanelLayout CreateGoodsItemSupplyChainActorLayout()
		{
			var builder = new SupplyChainActorLayoutBuilder<CusSupplyChainActorReference>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.RoleDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.OwnerOrganisationFindBox, ControlWidthClass.Auto);
			return builder.Build();
		}

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemSupplyChainActorsGridUserControl);

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
