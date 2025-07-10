using Enterprise.Customs.MY.Business;

namespace Enterprise.Customs.MY.GUI
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
