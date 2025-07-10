using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class VisualizerTemplateValidation : StmTemplateValidation
	{
		public VisualizerTemplateValidation(VisualizerTemplate template)
			: base(template)
		{
			this.template = template;
		}

		readonly VisualizerTemplate template;

		protected override void CheckSO_Name()
		{
			base.CheckSO_Name();

			MandatoryValidation.CheckEntered(template.SO_NameInfo);
		}

		protected override void CheckSO_DataContext()
		{
			base.CheckSO_DataContext();

			MandatoryValidation.CheckEntered(template.SO_DataContextInfo);
		}

		protected override void CheckSO_Template()
		{
			base.CheckSO_Template();

			MandatoryValidation.CheckEntered(template.SO_TemplateInfo);
		}
	}
}