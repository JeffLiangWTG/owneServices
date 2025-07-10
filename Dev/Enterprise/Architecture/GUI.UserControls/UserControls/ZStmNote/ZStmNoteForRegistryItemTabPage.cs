using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressResourceStringContainerControlNameKeyPrefix]
	public class ZStmNoteForRegistryItemTabPage : ZStmNoteTabPage
	{
		protected override void CreateZStmNoteUserControl()
		{
			ZStmNoteUserControl = new ZStmNoteForRegistryItemUserControl();
			AddStmNoteUserControlToControls();
		}

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			this.fBusinessEntity = (BusinessObject)dataSource;
			base.SetDataBindingCore(dataSource, dataMember);
		}

#if DEBUG
		internal void SetDataBindingCoreForTest(object dataSource, string dataMember)
		{
			this.SetDataBindingCore(dataSource, dataMember);
		}

		internal BusinessObject BusinessEntityForTest => fBusinessEntity;
#endif
	}
}
