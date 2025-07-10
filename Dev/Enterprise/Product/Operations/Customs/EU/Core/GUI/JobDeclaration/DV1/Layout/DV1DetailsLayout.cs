using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class DV1DetailsLayout : IPanelLayoutProvider
	{
		PanelLayout DV1Details { get; }

		PanelLayout IPanelLayoutProvider.Layout => DV1Details;

		public DV1DetailsLayout()
		{
			DV1Details = CreateDV1DetailsLayout();
		}

		PanelLayout CreateDV1DetailsLayout()
		{
			var builder = new DV1DetailsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.RelationshipDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.PriceInfluenceDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.RestrictionsDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.ConsiderationDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.RoyalitiesLicenceDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.ResaleDropEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.RelationDetailsTextBox, ControlWidthClass.LongControl, commonBag.PriceInfluenceDropEdit);
			builder.Add(commonBag.RestrictionsConsiderationTextBox, ControlWidthClass.LongControl, commonBag.ConsiderationDropEdit);
			builder.Add(commonBag.RoyalitiesLicenceDetailsTextBox, ControlWidthClass.LongControl, commonBag.RoyalitiesLicenceDropEdit);
			builder.Add(commonBag.ResaleDetailsTextBox, ControlWidthClass.LongControl, commonBag.ResaleDropEdit);
			builder.Add(commonBag.CustomsDecisionNumberTextBox, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
