using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MethodTwoToSixDocumentSendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public MethodTwoToSixDocumentSendingObjectLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new MethodTwoToSixDocumentLayoutBuilder<ValuationDeclarationMessageSendingObjectParent>(MethodTwoToSixControlBag.InstanceForSendingObject);
			var bag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(bag.ExpectedCustomsValueCalcEdit, ControlWidthClass.Auto);
			builder.Add(bag.SupportingDocument1TextBox, ControlWidthClass.Long);
			builder.Add(bag.SupportingDocument2TextBox, ControlWidthClass.Long);

			builder.SetVisibility(bag.ExpectedCustomsValueCalcEdit, (x) => !x.ParentDeclaration.Is5SM);

			return builder.Build();
		}
	}
}
