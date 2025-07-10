using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class ILBillControlBag : ControlBag
	{
		public static ILBillControlBag Instance => billControlBag.Value;

		public ILBillControlBag()
		{
			DischargePortCodeFindBox = RegisterControl(nameof(ILBillDetailsUserControl.DischargePortCodeFindBox));
			ConditionDropEdit = RegisterControl(nameof(ILBillDetailsUserControl.ConditionDropEdit));
		}

		public ControlReference DischargePortCodeFindBox { get; }
		public ControlReference ConditionDropEdit { get; }

		protected override Control CreateTemplate() => new ILBillDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<ILBillControlBag> billControlBag = new Lazy<ILBillControlBag>(() => new ILBillControlBag());
	}
}
