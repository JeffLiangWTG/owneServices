using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI
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
