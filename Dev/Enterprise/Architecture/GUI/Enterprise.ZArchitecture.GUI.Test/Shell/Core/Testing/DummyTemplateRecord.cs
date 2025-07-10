using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[PreventDelete(true)]
	public sealed class DummyTemplateRecord : DummyBusinessObject, ITemplateRecord, ICancellable
	{
		public DummyTemplateRecord(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ICancellable Members

		public string CanCancel()
		{
			return null;
		}

		public string CanReactivate()
		{
			return null;
		}

		public bool IsCancelled
		{
			get
			{
				return Z0_Bool;
			}
			set
			{
				Z0_Bool = value;
			}
		}

		public bool IsCancelledHasChanged
		{
			get { return false; }
		}

		#endregion
	}
}
