using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class ExampleAdditionalControlBag : ControlBag
	{
		public static ExampleAdditionalControlBag Instance => exampleAdditionalControlBag.Value;

		ExampleAdditionalControlBag()
		{
			ManifestTypeDropEdit = RegisterControl(nameof(ExampleAdditionalControlTemplate.ManifestTypeDropEdit));
			MasterBOLTextBox = RegisterControl(nameof(ExampleAdditionalControlTemplate.MasterBOLTextBox));
		}

		public ControlReference ManifestTypeDropEdit { get; }
		public ControlReference MasterBOLTextBox { get; }

		protected override Control CreateTemplate() => new ExampleAdditionalControlTemplate();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<ExampleAdditionalControlBag> exampleAdditionalControlBag = new Lazy<ExampleAdditionalControlBag>(() => new ExampleAdditionalControlBag());
	}
}
