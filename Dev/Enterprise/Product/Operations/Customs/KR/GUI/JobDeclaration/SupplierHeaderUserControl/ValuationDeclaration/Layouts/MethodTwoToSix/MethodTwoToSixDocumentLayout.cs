using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MethodTwoToSixDocumentLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public MethodTwoToSixDocumentLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new MethodTwoToSixDocumentLayoutBuilder<JobDeclaration>(MethodTwoToSixControlBag.InstanceForDeclaration);
			var bag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(bag.ExpectedCustomsValueCalcEdit, ControlWidthClass.Auto);
			builder.Add(bag.SupportingDocument1TextBox, ControlWidthClass.Long);
			builder.Add(bag.SupportingDocument2TextBox, ControlWidthClass.Long);

			builder.SetVisibility(bag.ExpectedCustomsValueCalcEdit, g => !g.Is5SM);

			return builder.Build();
		}
	}
}
