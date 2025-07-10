using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class BillLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonBillControlBag> where T : Business.AsycudaBill
	{
		public override CommonBillControlBag CommonBag { get; } = CommonBillControlBag.Instance;

		protected override int MaxColumns => 4;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.BillIssuerTextBox, b => b.IsIssuerCodeMandatory && b.BillIssuerFieldType == ZArchitecture.FieldType.Text, b => b.Header?.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.BillIssuerNameTextBox, b => b.IsIssuerCodeMandatory && b.BillIssuerFieldType == ZArchitecture.FieldType.Text, b => b.Header?.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.BillIssuerCodeFindBox, b => b.IsIssuerCodeMandatory && b.BillIssuerFieldType == ZArchitecture.FieldType.TextCodeFindBox, b => b.Header?.AMA_ManifestTypeInfo);
		}
	}
}
