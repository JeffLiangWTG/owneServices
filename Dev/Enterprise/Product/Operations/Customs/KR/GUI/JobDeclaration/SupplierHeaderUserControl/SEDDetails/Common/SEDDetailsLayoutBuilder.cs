using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class SEDDetailsLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, SEDDetailsControlBag>
	{
		public override SEDDetailsControlBag CommonBag { get; } = SEDDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.ManufacturerAddressControl, h => h.IsExport, h => h.JE_MessageTypeInfo);
			SetVisibility(CommonBag.ManufacturerGuidFindBox, h => h.IsLocalExport, h => h.JE_MessageTypeInfo);
			SetVisibility(CommonBag.ManufacturerIPCCodeFindBox, h => h.IsExport, h => h.JE_MessageTypeInfo);
			SetVisibility(CommonBag.BuyerIDTextBox, h => h.IsExport, h => h.JE_MessageTypeInfo);
		}
	}
}
