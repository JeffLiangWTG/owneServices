using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	sealed class ConsolidatedDeclarationLayoutProvider : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration>();
				var commonBag = builder.CommonBag;
				var auBag = ConsolidatedDeclarationControlBag.Instance;
				builder.AddControlBag(auBag);

				builder.AddColumn();
				builder.Add(commonBag.JobNumberTextBox, ControlWidthClass.Medium);
				builder.Add(auBag.ConsolidatedDeclarationDetailsUserControl, ControlWidthClass.Auto);
				builder.Add(auBag.PaymentStatusTextBox, ControlWidthClass.Medium);
				builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
				builder.Add(auBag.EntryStyleDropEdit, ControlWidthClass.Long);
				builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
				builder.Add(commonBag.ImporterGuidFindBox, ControlWidthClass.Long);
				builder.Add(auBag.VesselCodeFindBox, ControlWidthClass.Long);
				builder.Add(auBag.VoyageFlightNoTextBox, ControlWidthClass.Medium);
				builder.Add(commonBag.DischargeETADateEdit, ControlWidthClass.Medium);
				builder.Add(commonBag.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				builder.Add(commonBag.PortOfDischargeCodeFindBox, ControlWidthClass.Long);

				builder.SetVisibility(auBag.VesselCodeFindBox, consolidateDeclaration => consolidateDeclaration.LeadDeclaration.IsSea, _ => null);
				builder.SetCaption(auBag.VoyageFlightNoTextBox, consolidatedDeclaration => consolidatedDeclaration.LeadDeclaration.IsAir ? Enterprise.Customs.AU.Declaration.GUI.Res.GetData("5382B5CA-8043-4695-ACA3-FE87751810FD", "Arrival Flight") : Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9CB5AD82-F5AA-4203-9ACA-A5CF30933637", "Voyage"), _ => null);

				return builder.Build();
			}
		}
	}
}
