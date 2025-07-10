using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public class CusClassificationForm : Customs.GUI.BaseClassificationForm
	{
		public CusClassificationForm(CusClassification classification)
			: base(classification)
		{
		}

		protected override Customs.GUI.BaseClassificationUserControl GetUserControl() => new CusClassificationUserControl();
	}
}
