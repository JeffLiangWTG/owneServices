using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentSupplyChainActorLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public HouseConsignmentSupplyChainActorLayoutWithGrid()
		{
			Layout = CreateLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateLayout()
		{
			var builder = new SupplyChainActorLayoutBuilder<CusSupplyChainActorReference>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.RoleDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.OwnerOrganisationFindBox, ControlWidthClass.Auto);
			return builder.Build();
		}

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(HouseConsignmentSupplyChainActorsGridUserControl);

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
