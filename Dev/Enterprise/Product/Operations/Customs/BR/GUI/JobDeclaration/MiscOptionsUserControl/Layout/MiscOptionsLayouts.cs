using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class MiscOptionsLayouts : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MiscOptionsLayoutBuilder();
			var commonBag = builder.CommonBag;
			var brBag = MiscOptionsControlBag.Instance;

			builder.AddControlBag(brBag);

			builder.AddColumn();
			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
			builder.Add(brBag.PaymentSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
			builder.Add(brBag.BankAccountGuidFindBox, ControlWidthClass.Auto);

			builder.SetVisibility(brBag.BankAccountGuidFindBox, d => d.IsImportExcludingLicense, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.PaymentSeparatorUserControl, d => d.IsImportExcludingLicense, d => d.JE_MessageTypeInfo);
			builder.SetVisibility(commonBag.PaymentPartyDropEdit, d => d.IsImportExcludingLicense, d => d.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
