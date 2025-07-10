using System;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class SupplyChainActorLayout : IPanelLayoutWithGridProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		public Type GridUserControlType => typeof(SupplyChainActorReferencesUserControl);

		static PanelLayout CreateLayout()
		{
			var builder = new SupplyChainActorLayoutBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.RoleDropEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.OwnerOrganisationFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			return builder.Build();
		}
	}
}
