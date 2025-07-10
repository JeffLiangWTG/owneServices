using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ActivitySubtypeAssignmentCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ActivitySubtypeAssignmentCollection()
			: base() { }

		public ActivitySubtypeAssignmentCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new ActivitySubtypeAssignment this[int index]
		{
			get { return (ActivitySubtypeAssignment)Elements[index]; }
		}

		public new ActivitySubtypeAssignment AddNew()
		{
			return (ActivitySubtypeAssignment)base.AddNew();
		}

		public ActivitySubtypeAssignment AddNew(ZString activitySubtype, ZString description, ZBool capitalization)
		{
			var result = AddNew();
			result.ActivitySubtype = activitySubtype;
			result.Description = description;
			result.Capitalization = capitalization;
			return result;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ActivitySubtypeAssignmentCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ActivitySubtypeAssignment(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion
	}
}
