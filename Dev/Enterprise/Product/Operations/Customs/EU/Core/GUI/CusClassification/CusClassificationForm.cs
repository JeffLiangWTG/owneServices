using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.GUI
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
