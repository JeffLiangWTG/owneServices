using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ShipmentTypeLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ShipmentTypeLayout()
		{
			Layout = CreateShipmentTypeLayout();
		}

		static PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new EU.GUI.ShipmentTypeLayoutBuilder<EU.Business.Declaration.JobDeclaration>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.ShipmentTypeControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.EntryStyleDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.BorderTransportMeansDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.CTStatusIDDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
			builder.SetVisibility(euBag.CTStatusIDDropEdit, d => !d.IsExport, d => d.JE_MessageTypeInfo);

			builder.SetCaption(commonBag.TransportModeDropEdit, j => GetTransportModeResString(j), j => j.JE_ApplicationCodeInfo);
			return builder.Build();
		}

		static ResourceStringData GetTransportModeResString(EU.Business.Declaration.JobDeclaration jobDeclaration)
		{
			if (jobDeclaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport)
			{
				return Res.GetData("IE.JobDeclaration.JE_TransportMode|UCC5", "[7/4] Trans. Mode", "[7/4] Transport Mode", "[7/4] Transport Mode at Border", "[7/4] Mode of transport at the border");
			}
			return Res.GetData("85E15FDD-5515-456D-B7CF-0346C18836E3", "Trans. Mode", "[19 03 001 000] Transport Mode at Border");
		}
	}
}
