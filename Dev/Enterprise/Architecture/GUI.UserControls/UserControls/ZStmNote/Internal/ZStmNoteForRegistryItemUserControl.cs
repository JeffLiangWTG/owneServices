using System.ComponentModel;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public class ZStmNoteForRegistryItemUserControl : ZStmNoteUserControl
	{
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is IStmNoteParent)
			{
				base.SetDataBinding(dataSource, dataMember);
			}
		}
	}
}
