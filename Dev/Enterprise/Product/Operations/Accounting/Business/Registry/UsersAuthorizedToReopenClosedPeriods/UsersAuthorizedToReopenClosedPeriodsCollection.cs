using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class UsersAuthorizedToReopenClosedPeriodsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public UsersAuthorizedToReopenClosedPeriodsCollection()
		{
		}

		public UsersAuthorizedToReopenClosedPeriodsCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public UsersAuthorizedToReopenClosedPeriods Find(ZGuid staffPK)
		{
			UsersAuthorizedToReopenClosedPeriods result = null;

			foreach (UsersAuthorizedToReopenClosedPeriods current in this)
			{
				if (current.StaffPK == staffPK)
				{
					result = current;
					break;
				}
			}
			return result;
		}

		#region Overriden

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UsersAuthorizedToReopenClosedPeriodsCollection(fallbackLevel, CurrentFactory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UsersAuthorizedToReopenClosedPeriods(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion

		public new UsersAuthorizedToReopenClosedPeriods this[int index]
		{
			get { return (UsersAuthorizedToReopenClosedPeriods)Elements[index]; }
		}

		public new UsersAuthorizedToReopenClosedPeriods AddNew()
		{
			return (UsersAuthorizedToReopenClosedPeriods)base.AddNew();
		}
	}
}
