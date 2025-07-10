using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class ServiceLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public ServiceLayoutWithGrid()
		{
			Layout = CreateServiceLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateServiceLayout()
		{
			var builder = new ServiceLayoutBuilder<NctsHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ServiceTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContractorFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLocationAddressControl, ControlWidthClass.Auto);
			builder.Add(commonBag.SubLocationTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BookedDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CompletedDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.RateAndCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MeasurementBasisDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NotesTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ServiceCountCalcEdit, ControlWidthClass.Auto, commonBag.BookedDateEdit);
			builder.Add(commonBag.DurationTimeEdit, ControlWidthClass.Auto, commonBag.CompletedDateEdit);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Auto, commonBag.NotesTextBox);

			return builder.Build();
		}

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(ServicesGridUserControl);

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
