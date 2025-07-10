using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class ZGridCustomiseBizObjLookups : ZLookups
	{
		public ZGridCustomiseBizObjLookups(ZGridCustomiseBizObj parent)
			: base(parent) { }

		public GridLayoutStorageBizOCollection Layouts
		{
			get
			{
				return layouts ?? (layouts = new GridLayoutStorageBizOCollection(Parent.GridIDsForStmModuleFilter, Parent.GridIDsForStmData, Parent.LayoutContextPK, Factory));
			}
		}
		GridLayoutStorageBizOCollection layouts;

		public void ResetLayouts() => layouts = null;

		#region Implementation

		protected new ZGridCustomiseBizObj Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ZGridCustomiseBizObj)base.Parent; }
		}

		#endregion
	}
}
