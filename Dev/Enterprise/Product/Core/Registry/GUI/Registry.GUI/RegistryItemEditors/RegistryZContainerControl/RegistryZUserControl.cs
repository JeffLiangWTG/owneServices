using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RegistryZUserControl : ZUserControl
	{
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				SetControlOrBusinessEntityReadOnly(value);
			}
		}

		protected virtual void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
		}

		public BusinessObject BoundBusinessObject
		{
			get
			{
				return DataSource as BusinessObject;
			}
		}

		bool fReadOnly;

		public void NotifyChanges()
		{
			(FindForm() as IRegistryForm)?.UpdateHasChanges();
		}
	}
}
