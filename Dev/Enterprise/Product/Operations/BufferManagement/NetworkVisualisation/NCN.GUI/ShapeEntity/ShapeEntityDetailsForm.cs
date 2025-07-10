using System.ComponentModel;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class ShapeEntityDetailsForm : ZChildForm
	{
		public ShapeEntityDetailsForm()
		{
			InitializeComponent();
		}

		public ShapeEntityDetailsForm(ShapeNetworkEntity shape)
			: base(shape)
		{
			InitializeComponent();

			var processHeader = shape.ProcessHeader;
			if (processHeader != null)
			{
				shape.RegisterEditableChildObject(processHeader);
			}

			OKButton.Click += (sender, args) => Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (BusinessEntity.RunValidationAndCheckErrors(errorsToIgnoreOnClosing) || BusinessEntity.RunValidationAndCheckErrorsForChildObjects(errorsToIgnoreOnClosing))
			{
				e.Cancel = true; // Stops the window from actually closing
				ShowErrorsDialog();
			}

			base.OnClosing(e);
		}

		readonly string[] errorsToIgnoreOnClosing =
		{
			BMNCNShapeValidation.BufferNotAcceptedErrorMessage,
			ShapeNetworkEntityValidation.LinkedDiagramConflictsValidationError,
		};

		public new ShapeNetworkEntity BusinessEntity => (ShapeNetworkEntity)base.BusinessEntity;

		public override string FormCaption => BusinessEntity.Name;

		public override string FormVerb => FormVerbs.Edit;
	}
}
