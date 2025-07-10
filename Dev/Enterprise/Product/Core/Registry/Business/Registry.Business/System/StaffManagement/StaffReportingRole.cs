using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StaffReportingRole : CodeDescriptionBool
	{
		#region Schema

		new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string SharedRoleAllowed = "SharedRoleAllowed";
			public const string IsMandatory = "IsMandatory";
		}

		#endregion

		#region Properties

		#region Code

		[ReadOnlyMember(nameof(IsDirectManager))]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		#endregion

		#region Bool

		[ResourceStringData("StaffReportingRole|Bool", Caption = "Enabled")]
		public override ZBool Bool
		{
			get { return base.Bool; }
			set
			{
				base.Bool = value;
				if (!value)
				{
					IsMandatory = false;
				}
			}
		}

		#endregion

		#region Shared Role Allowed

		[ReadOnlyMember(nameof(IsDirectManager))]
		public ZBool SharedRoleAllowed
		{
			get => sharedRoleAllowed;
			set => SetNonPersistentPropertyValue(SharedRoleAllowedInfo, ref sharedRoleAllowed, value);
		}
		ZBool sharedRoleAllowed;

		public ZPropertyInfo SharedRoleAllowedInfo => GetZPropertyInfo(Schema.SharedRoleAllowed);

		ZBool IsDirectManager => Code == DefaultStaffReportingRoles.Codes.DirectManager;

		#endregion

		#region Mandatory

		public ZBool IsMandatory
		{
			get => isMandatory;
			set
			{
				SetNonPersistentPropertyValue(IsMandatoryInfo, ref isMandatory, value);
				if (!IsValidationSuspended)
				{
					ValidateIsMandatory();
				}
			}
		}
		ZBool isMandatory;

		public ZPropertyInfo IsMandatoryInfo => GetZPropertyInfo(Schema.IsMandatory);

		void ValidateIsMandatory()
		{
			IsMandatoryInfo.ClearAllNotifications();
			if (!Bool && IsMandatory)
			{
				IsMandatoryInfo.AddError(Res.GetString("c853789b-e094-470b-a327-29440fae7038", "Role which is disabled cannot be mandatory."));
			}
		}

		#endregion

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StaffReportingRole();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var role = clone as StaffReportingRole;
			role.SharedRoleAllowed = SharedRoleAllowed;
			role.IsMandatory = IsMandatory;
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			SharedRoleAllowed = false;
			IsMandatory = false;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateIsMandatory();
		}

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.SharedRoleAllowed, SharedRoleAllowed.ToString());
			writer.WriteElementString(Schema.IsMandatory, IsMandatory.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			SharedRoleAllowed = new ZBool(reader.ReadElementString(Schema.SharedRoleAllowed));
			IsMandatory = new ZBool(reader.ReadElementString(Schema.IsMandatory));
		}

		#endregion
	}
}
