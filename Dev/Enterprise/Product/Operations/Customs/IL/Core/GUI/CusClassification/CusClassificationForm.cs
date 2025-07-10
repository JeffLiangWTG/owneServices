using System;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.GUI
{
	public partial class CusClassificationForm : Customs.GUI.BaseClassificationForm
	{
		[Obsolete("Do not call. Only for designer use.")]
		public CusClassificationForm()
		{
		}

		public CusClassificationForm(CusClassification classification)
			: base(classification)
		{
		}

		protected override Customs.GUI.BaseClassificationUserControl GetUserControl() => new CusClassificationUserControl();

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
