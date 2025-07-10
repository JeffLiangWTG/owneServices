using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class CriticalityStageMappingGridControl : CodeDescriptionBoolControl, ICodeDescriptionBoolTreeNodeGridExtension
	{
		public CriticalityStageMappingGridControl()
		{
			InitializeComponent();
		}

		public void SetLevel(CodeDescriptionBoolControl parent, CodeDescriptionBoolControl child)
		{
			this.ChildGrid = child;
			this.ParentGrid = parent;
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
			dataSource = CodeDescriptionBoolTreeControlHelper.GetTreeView<CriticalityStageMappingCollectionView>(this, dataSource);
			base.SetDataBinding(dataSource, dataMember);

			CodeDescriptionBoolTreeControlHelper.BindDataSourceCurrentChangeEvent(this, dataSource);
		}
	}
}
