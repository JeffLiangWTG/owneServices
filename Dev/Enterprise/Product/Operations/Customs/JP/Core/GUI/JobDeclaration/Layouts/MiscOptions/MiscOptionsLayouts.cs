using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class MiscOptionsLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new CommonMiscOptionsLayoutBuilder<JobDeclaration>();
				var commonBag = builder.CommonBag;
				var specificBag = MiscOptionsControlBag.Instance;

				builder.AddControlBag(specificBag);
				builder.AddColumn();
				builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
				builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
				builder.Add(specificBag.NACCSCredentialGuidDropEdit, ControlWidthClass.Auto);
				builder.SetCaption(specificBag.NACCSCredentialGuidDropEdit, _ => Res.GetData("1BA6D9F8-2D7E-462D-87E9-1DD1646C74BD", "NACCS Credential"));
				builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);
				builder.Add(specificBag.PaymentOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				builder.Add(specificBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
				builder.Add(specificBag.PaymentDeadlineExtensionDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.DefermentAccountNumberTextBox, ControlWidthClass.Auto);

				builder.SetVisibility(specificBag.PaymentOptionsSeparatorUserControl, x => x.IsImport);
				builder.SetVisibility(specificBag.PaymentPartyDropEdit, x => x.IsImport);
				builder.SetVisibility(specificBag.PaymentDeadlineExtensionDropEdit, x => x.IsImport);
				builder.SetVisibility(commonBag.DefermentAccountNumberTextBox, x => x.IsImport);

				return builder.Build();
			}
		}
	}
}
