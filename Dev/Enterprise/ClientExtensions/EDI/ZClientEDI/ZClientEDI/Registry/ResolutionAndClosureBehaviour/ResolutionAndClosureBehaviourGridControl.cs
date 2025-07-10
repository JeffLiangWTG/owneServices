using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ResolutionAndClosureBehaviourGridControl : CodeDescriptionBoolControl, ICodeDescriptionBoolTreeNodeGridExtension
	{
		public ResolutionAndClosureBehaviourGridControl()
		{
			InitializeComponent();
		}

		public void SetLevel(CodeDescriptionBoolControl parent, CodeDescriptionBoolControl child)
		{
			ChildGrid = child;
			ParentGrid = parent;
		}

		public bool CodeDescriptionBoolGridReadOnly
		{
			get { return CodeDescriptionBoolGrid.ReadOnly; }
		}

		public CodeDescriptionBoolControl ParentGrid { get; private set; }

		public CodeDescriptionBoolControl ChildGrid { get; private set; }

		public ZGrid InnerGrid => CodeDescriptionBoolGrid;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			dataSource = CodeDescriptionBoolTreeControlHelper.GetTreeView<ResolutionAndClosureBehaviourCollectionView>(this, dataSource);
			base.SetDataBinding(dataSource, dataMember);
			CodeDescriptionBoolTreeControlHelper.BindDataSourceCurrentChangeEvent(this, dataSource);
		}
	}
}
