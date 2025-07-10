using CargoWise.Application;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class ArrivalNotificationDetailsLayout : IPanelLayoutProvider
	{
		public ArrivalNotificationDetailsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();
			var esBag = ArrivalNotificationDetailsControlBag.Instance;
			builder.AddControlBag(esBag);
			var euBag = builder.CommonBag;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			builder.Add(euBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(euBag.MrnTextBox, ControlWidthClass.Long);
			builder.Add(euBag.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.AuthorizationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.NumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.ArrivalGoodsLocationZCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.IncidentFlagDropEdit, ControlWidthClass.Long);
			builder.Add(esBag.AdditionalArrivalNotificationDetailsUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(euBag.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
			builder.Add(esBag.RepresentativeTraderZDocAddressControl, ControlWidthClass.Auto);
			builder.Add(esBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(esBag.CertificateDropEdit, ControlWidthClass.Auto);
			builder.Add(esBag.TrainingCheckBox, ControlWidthClass.Long);

			builder.SetVisibility(esBag.TrainingCheckBox, h => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());

			return builder.Build();
		}
	}
}
