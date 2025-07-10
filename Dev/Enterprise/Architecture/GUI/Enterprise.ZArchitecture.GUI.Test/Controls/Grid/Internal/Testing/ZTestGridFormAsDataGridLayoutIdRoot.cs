using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTestGridFormAsDataGridLayoutIdRoot : ZTestGridForm, IDataGridLayoutIdentifierRoot
	{
		public ZTestGridFormAsDataGridLayoutIdRoot(DummyBusinessObject entity)
			: base(entity)
		{
		}

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get { return ((DummyBusinessObject)BusinessEntity).Z0_Code; }
		}

		#endregion
	}
}
