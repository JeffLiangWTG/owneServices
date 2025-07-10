using System.Collections.Generic;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ResolutionAndClosureBehaviourControl : RegistryZUserControl, ICodeDescriptionBoolTreePanelExtension
	{
		public ResolutionAndClosureBehaviourControl()
		{
			InitializeComponent();
			Grids = new[] { Grid1, Grid2 };
			GridContainers = new[] { GroupBox1, GroupBox2 };
		}

		public ResolutionAndClosureBehaviourControl(ResolutionAndClosureBehaviourRegistryEditorInfo editorInfo)
			: this()
		{
			this.EditorInfo = editorInfo;
			CodeDescriptionBoolTreeControlHelper.InitializePanelGrids(this);
		}
		public CodeDescriptionBoolTreeRegistryEditorInfo EditorInfo { get; private set; }

		public IReadOnlyList<CodeDescriptionBoolControl> Grids { get; private set; }

		public IReadOnlyList<ZGroupBox> GridContainers { get; private set; }

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			foreach (ResolutionAndClosureBehaviourGridControl grid in Grids)
			{
				grid.ReadOnly = readOnly;
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Ensure parents get set first
			foreach (ResolutionAndClosureBehaviourGridControl grid in Grids)
			{
				grid.SetDataBinding(dataSource, dataMember);
			}
			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
