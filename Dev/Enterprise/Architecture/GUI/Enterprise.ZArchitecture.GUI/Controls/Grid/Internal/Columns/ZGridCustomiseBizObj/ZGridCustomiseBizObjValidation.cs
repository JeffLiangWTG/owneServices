using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class ZGridCustomiseBizObjValidation : AutoZGridCustomiseBizObjValidation
	{
		public ZGridCustomiseBizObjValidation(AutoZGridCustomiseBizObj parent)
			: base(parent) { }

		protected override void CheckCurrentLayoutNameDisplay()
		{
			base.CheckCurrentLayoutNameDisplay();

			if (Parent.CurrentLayout == null)
			{
				Parent.CurrentLayoutNameDisplayInfo.AddError(InvalidLayout);
			}
		}

		public static string InvalidLayout
		{
			get { return Res.GetString("de560b98-9bd6-404e-9064-9d9afde8d2f4", "You have selected an invalid layout."); }
		}

		#region Implementation

		public new ZGridCustomiseBizObj Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ZGridCustomiseBizObj)base.Parent; }
		}

		#endregion
	}
}
