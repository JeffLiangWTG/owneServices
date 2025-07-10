using Enterprise.Customs.DE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class HeaderDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new HeaderDetailsLayoutBuilder<CusReconDeclaration>();
			var deBag = builder.CommonBag;
			var commonBag = CommonHeaderDetailsControlBag.Instance;
			builder.AddControlBag(commonBag);

			builder.AddColumn();
			builder.Add(commonBag.EntryTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PeriodFromDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PeriodToDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarantTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarantAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.RepresentativeAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.BuyingAgentAddressControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.EntryStatusTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MessageStatusTextBox, ControlWidthClass.Auto);
			builder.Add(deBag.RegistrationNumberTextBox, ControlWidthClass.Long);
			builder.Add(deBag.BranchGuidFindBox, ControlWidthClass.Long);
			builder.Add(deBag.IsFinalizedCheckBox, ControlWidthClass.Long);
			builder.Add(deBag.IsDeclarantImporterCheckBox, ControlWidthClass.Long, commonBag.DeclarantAddressControl);
			builder.Add(commonBag.AuthorizationNumberGuidDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(deBag.UnlinkedDeclarationsNumberLabel, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
