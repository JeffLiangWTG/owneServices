using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StaffColumnToGroupDescriptionScimMapping : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string GroupDescriptionMapping = "GroupDescriptionMapping";
			public const string StaffColumnName = "StaffColumnName";
		}

		#endregion

		#region StaffColumnToGroupDescriptionScimMappingLookups

		public StaffColumnToGroupDescriptionScimMappingLookups Lookups => lookups ?? (lookups = new StaffColumnToGroupDescriptionScimMappingLookups(this));
		StaffColumnToGroupDescriptionScimMappingLookups lookups;

		#endregion

		#region GroupDescriptionMapping
		public ZString GroupDescriptionMapping
		{
			get => groupDescriptionMapping;
			set
			{
				SetNonPersistentPropertyValue(GroupDescriptionMappingInfo, ref groupDescriptionMapping, value);
				if (!IsValidationSuspended)
				{
					ValidateGroupDescriptionMapping();
				}
			}
		}

		public ZPropertyInfo GroupDescriptionMappingInfo => GetZPropertyInfo(Schema.GroupDescriptionMapping);

		ZString groupDescriptionMapping;

		#endregion

		#region StaffColumnName
		[List("Lookups.StaffList")]
		public ZString StaffColumnName
		{
			get => staffColumnName;
			set
			{
				SetNonPersistentPropertyValue(StaffColumnNameInfo, ref staffColumnName, value);
				if (!IsValidationSuspended)
				{
					ValidateStaffColumnName();
				}
			}
		}

		public ZPropertyInfo StaffColumnNameInfo => GetZPropertyInfo(Schema.StaffColumnName);

		ZString staffColumnName;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StaffColumnToGroupDescriptionScimMapping
			{
				GroupDescriptionMapping = GroupDescriptionMapping,
				StaffColumnName = StaffColumnName
			};
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			GroupDescriptionMapping = reader.ReadElementString(Schema.GroupDescriptionMapping);
			StaffColumnName = reader.ReadElementString(Schema.StaffColumnName);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.GroupDescriptionMapping, GroupDescriptionMapping);
			writer.WriteElementString(Schema.StaffColumnName, StaffColumnName);
		}

		#endregion

		#region Validation
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				ValidateGroupDescriptionMapping();
				ValidateStaffColumnName();
			}
		}

		public void ValidateGroupDescriptionMapping()
		{
			GroupDescriptionMappingInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(GroupDescriptionMappingInfo);
			if (!GroupDescriptionMapping.IsEmpty)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(GroupDescriptionMappingInfo);
			}
		}

		public void ValidateStaffColumnName()
		{
			StaffColumnNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StaffColumnNameInfo);
			if (!StaffColumnName.IsEmpty)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(StaffColumnNameInfo);
				ListValidation.ErrorIfInvalidCode(StaffColumnNameInfo, Lookups.StaffList);
			}
		}

		#endregion
	}
}
