using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StaffReportingRoleCollection : CodeDescriptionBoolCollection
	{
		public StaffReportingRoleCollection()
			: this(false, false)
		{
		}

		public StaffReportingRoleCollection(bool defaultSharedRoleAllowedForNewChild, bool defaultIsMandatoryForNewChild)
			: base(null, true, 3)
		{
			DefaultSharedRoleAllowedForNewChild = defaultSharedRoleAllowedForNewChild;
			DefaultIsMandatoryForNewChild = defaultIsMandatoryForNewChild;
		}

		readonly bool DefaultSharedRoleAllowedForNewChild;
		readonly bool DefaultIsMandatoryForNewChild;

		public new StaffReportingRole this[int i]
		{
			get { return (StaffReportingRole)base[i]; }
		}

		public new StaffReportingRole AddNew()
		{
			var newRole = (StaffReportingRole)base.AddNew();
			newRole.SharedRoleAllowed = DefaultSharedRoleAllowedForNewChild;
			newRole.IsMandatory = DefaultIsMandatoryForNewChild;
			return newRole;
		}

		protected override CargoWise.EntityFramework.BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StaffReportingRole();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new StaffReportingRoleCollection(DefaultSharedRoleAllowedForNewChild, DefaultIsMandatoryForNewChild);
		}

		public StaffReportingRole Add(ZString code, MultilingualString description, bool enabled, bool isMandatory, bool sharedRoleAllowed)
		{
			var result = (StaffReportingRole)base.Add(code, description, enabled);
			result.SharedRoleAllowed = sharedRoleAllowed;
			result.IsMandatory = isMandatory;
			return result;
		}

		public StaffReportingRole AddSystemDefined(ZString code, MultilingualString description, bool enabled, bool isMandatory, bool sharedRoleAllowed)
		{
			var result = Add(code, description, enabled, isMandatory, sharedRoleAllowed);
			result.SystemDefined = true;
			return result;
		}
	}
}
