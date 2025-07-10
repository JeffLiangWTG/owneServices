using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public sealed class MiscOptionsLayouts : IPanelLayoutProvider
	{
		public PanelLayout MiscOptions { get; }

		PanelLayout IPanelLayoutProvider.Layout => MiscOptions;

		public MiscOptionsLayouts()
		{
			MiscOptions = CreateMiscOptionsLayouts();
		}

		PanelLayout CreateMiscOptionsLayouts()
		{
			var builder = new CommonMiscOptionsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var specificBag = MiscOptionsControlBag.Instance;
			builder.AddControlBag(specificBag);

			builder.AddColumn();
			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
			builder.Add(specificBag.PaymentAccountNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);
			builder.Add(specificBag.AsycudaRelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);

			builder.SetVisibility(commonBag.PaidByDropEdit, x => true);

			return builder.Build();
		}
	}
}
