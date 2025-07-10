using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
{
	public CusGoodsLocationLayout()
	{
		Layout = CreateCusGoodsLocationLayout();
	}

	PanelLayout Layout { get; }

	#region IPanelLayoutProviderWithExtensions

	IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
	{
		new CusGoodsLocationWebAddressValidationExtension()
	};

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	#endregion

	PanelLayout CreateCusGoodsLocationLayout()
	{
		var builder = new CusGoodsLocationLayoutBuilder<Business.CusGoodsLocation>();
		var commonBag = builder.CommonBag;
		var itBag = CusGoodsLocationControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.OrganisationFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.AuthorizationCodeFindBox, ControlWidthClass.Long);
		builder.Add(itBag.AdditionalIdentifierDropEdit, ControlWidthClass.Long);

		return builder.Build();
	}
}
