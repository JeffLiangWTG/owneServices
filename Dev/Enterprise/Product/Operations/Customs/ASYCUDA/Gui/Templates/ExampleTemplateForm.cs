using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class ExampleTemplateForm : ZChildForm
	{
		public ExampleTemplateForm()
		{
		}

		public ExampleTemplateForm(AsycudaManifestHeader businessEntity) : base(new ExampleTemplateFormViewModel(businessEntity))
		{
			ViewModel.LayoutChanged += (_, __) => templateControl.UpdateLayout(ViewModel);
			templateControl.UpdateLayout(ViewModel);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			BindingSource.SetBindingMember(templateControl, nameof(ExampleTemplateFormViewModel.ManifestHeader));
		}

		ExampleTemplateFormViewModel ViewModel => (ExampleTemplateFormViewModel)BusinessEntity;
	}
}
