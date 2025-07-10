
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgHeaderCollection : OrgHeaderCollection
	{
		public EDIOrgHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EDIOrgHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Implementation

		public new EDIOrgHeader this[int index]
		{
			get { return (EDIOrgHeader)Elements[index]; }
		}

		public virtual new EDIOrgHeader AddNew()
		{
			return (EDIOrgHeader)base.AddNew();
		}

		#endregion
	}
}

