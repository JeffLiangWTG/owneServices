using Enterprise.Customs.AsycudaCustoms.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class CusClassificationForm : Customs.GUI.BaseClassificationForm
	{
		public CusClassificationForm(CusClassification classification)
			: base(classification)
		{
		}

		protected override Customs.GUI.BaseClassificationUserControl GetUserControl()
		{
			return new CusClassificationUserControl();
		}
	}
}
