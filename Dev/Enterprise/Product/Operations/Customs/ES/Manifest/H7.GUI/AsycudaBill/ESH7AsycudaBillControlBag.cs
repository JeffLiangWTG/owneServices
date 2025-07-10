using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESH7AsycudaBillControlBag : ControlBag
	{
		public static ESH7AsycudaBillControlBag Instance => billControlBag.Value;

		ESH7AsycudaBillControlBag()
		{
			DocumentationRequiredTextBox = RegisterControl(nameof(ESH7AsycudaBillControlTemplate.DocumentationRequiredTextBox));
			G3LocalReferenceNumberTextBox = RegisterControl(nameof(ESH7AsycudaBillControlTemplate.G3LocalReferenceNumberTextBox));
			G3MovementReferenceNumberTextBox = RegisterControl(nameof(ESH7AsycudaBillControlTemplate.G3MovementReferenceNumberTextBox));
			H7MovementReferenceNumberTextBox = RegisterControl(nameof(ESH7AsycudaBillControlTemplate.H7MovementReferenceNumberTextBox));
		}

		public ControlReference DocumentationRequiredTextBox { get; }
		public ControlReference G3LocalReferenceNumberTextBox { get; }
		public ControlReference G3MovementReferenceNumberTextBox { get; }
		public ControlReference H7MovementReferenceNumberTextBox { get; }

		protected override Control CreateTemplate() => new ESH7AsycudaBillControlTemplate();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<ESH7AsycudaBillControlBag> billControlBag = new Lazy<ESH7AsycudaBillControlBag>(() => new ESH7AsycudaBillControlBag());
	}
}
