using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class ShipmentTypeLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ShipmentTypeLayout()
	{
		Layout = CreateShipmentTypeLayout();
	}

	static PanelLayout CreateShipmentTypeLayout()
	{
		var builder = new EU.GUI.ShipmentTypeLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.ShipmentTypeControlBag.Instance;
		var itBag = ShipmentTypeControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.AuthorisationNumberDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.EntryStyleDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.BorderTransportMeansDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.CTStatusIDDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.MessageVersionDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.IsSecurityDeclarationCheckBox, ControlWidthClass.Auto);

		var ucc6Dependencies = new Func<JobDeclaration, ZPropertyInfo>[]
		{
			d => d.JE_MessageTypeInfo,
			d => d.MessageVersionInfo,
		};

		var securityDependencies = new Func<JobDeclaration, ZPropertyInfo>[]
		{
			d => d.JE_MessageTypeInfo,
			d => d.MessageVersionInfo,
			d => d.JE_EntryStyleInfo,
		};

		var messageVersionDependencies = new Func<JobDeclaration, ZPropertyInfo>[]
		{
			d => d.JE_ApplicationCodeInfo,
			d => d.JE_MessageTypeInfo,
		};

		builder.SetVisibility(itBag.AuthorisationNumberDropEdit, d => !d.IsUCC6AndIsExport, ucc6Dependencies);
		builder.SetVisibility(itBag.MessageVersionDropEdit, d => d.IsMessageVersionApplicable, messageVersionDependencies);
		builder.SetVisibility(euBag.IsSecurityDeclarationCheckBox, d => d.IsSecurityAllowed(), securityDependencies);
		builder.SetVisibility(euBag.BorderTransportMeansDropEdit, d => d.IsUCC6AndIsExport, ucc6Dependencies);

		return builder.Build();
	}
}
