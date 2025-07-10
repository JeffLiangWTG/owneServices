using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class MiscOptionsLayouts : IPanelLayoutProvider
{
	public PanelLayout Layout => MiscOptions;

	PanelLayout MiscOptions { get; }

	public MiscOptionsLayouts()
	{
		MiscOptions = CreateMiscOptionsLayouts();
	}

	PanelLayout CreateMiscOptionsLayouts()
	{
		var builder = new MiscOptionsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var inBag = MiscOptionsControlBag.Instance;

		builder.AddControlBag(inBag);

		builder.AddColumn();
		builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(inBag.NonStandardExchangeRateGroupBox, ControlWidthClass.LongControl);

		return builder.Build();
	}
}
