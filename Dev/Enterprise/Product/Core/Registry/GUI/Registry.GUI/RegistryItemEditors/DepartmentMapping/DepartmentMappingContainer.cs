using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DepartmentMappingContainer : ZUserControl
	{
		public DepartmentMappingContainer()
		{
			InitializeComponent();
		}

		#region ReadOnly

		bool fReadOnly;
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				zGrid1.ReadOnly = value;
			}
		}

		#endregion
	}
}
