using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	internal class StaffColumnToGroupNamesMappingUpdateAction
	{
		StaffColumnToGroupDescriptionScimMappingCollection staffColumnToGroupDescriptionScimMappingCollection;
		internal void UpdateCurrentScimMappingCollection(StaffColumnToGroupDescriptionScimMappingCollection currentCollection)
		{
			staffColumnToGroupDescriptionScimMappingCollection = new StaffColumnToGroupDescriptionScimMappingCollection();
			foreach (StaffColumnToGroupDescriptionScimMapping mapping in currentCollection)
			{
				staffColumnToGroupDescriptionScimMappingCollection.Add(
					new StaffColumnToGroupDescriptionScimMapping()
					{
						GroupDescriptionMapping = mapping.GroupDescriptionMapping,
						StaffColumnName = mapping.StaffColumnName
					});
			}
		}

		internal void Update(Guid companyPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			var newCollection = (StaffColumnToGroupDescriptionScimMappingCollection)newValue;

			var deleted = new List<StaffColumnToGroupDescriptionScimMapping>();
			var added = new List<StaffColumnToGroupDescriptionScimMapping>();

			foreach (StaffColumnToGroupDescriptionScimMapping item in staffColumnToGroupDescriptionScimMappingCollection)
			{
				if (!newCollection.Cast<StaffColumnToGroupDescriptionScimMapping>().Any(m => m.GroupDescriptionMapping.EqualsIgnoringCase(item.GroupDescriptionMapping) && m.StaffColumnName.EqualsIgnoringCase(item.StaffColumnName)))
				{
					deleted.Add(item);
				}
				else
				{
					added.Add(item);
				}
			}

			foreach (StaffColumnToGroupDescriptionScimMapping item in newCollection)
			{
				if (!staffColumnToGroupDescriptionScimMappingCollection.Cast<StaffColumnToGroupDescriptionScimMapping>().Any(m => m.GroupDescriptionMapping.EqualsIgnoringCase(item.GroupDescriptionMapping) && m.StaffColumnName.EqualsIgnoringCase(item.StaffColumnName)))
				{
					added.Add(item);
				}
			}

			var factory = new BusinessObjectFactory();
			var staffs = new Dictionary<ZGuid, BusinessObject>();
			var needsSave = SetScimStaffFlagValues(factory, deleted, ZBool.False, staffs);
			needsSave |= SetScimStaffFlagValues(factory, added, ZBool.True, staffs);

			if (needsSave)
			{
				factory.Save();
			}

			UpdateCurrentScimMappingCollection(newCollection);
		}

		static bool SetScimStaffFlagValues(BusinessObjectFactory factory, List<StaffColumnToGroupDescriptionScimMapping> groups, ZBool value, Dictionary<ZGuid, BusinessObject> processedStaff)
		{
			var needsSave = false;
			foreach (var item in groups)
			{
				var group = factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Desc, item.GroupDescriptionMapping));
				if (group != null && !item.StaffColumnName.IsEmpty)
				{
					foreach (BusinessObject staff in group.Staff.Cast<IGlbStaff>().Where(s => !s.GS_ExternalId.IsEmpty))
					{
						var staffToProcess = staff;
						if (processedStaff.TryGetValue(staff.PK, out var loadedStaff))
						{
							staffToProcess = loadedStaff;
						}
						else
						{
							staffToProcess.ReloadSafe();
							processedStaff.Add(staffToProcess.PK, staffToProcess);
						}

						staffToProcess[item.StaffColumnName] = value;
						staffToProcess.HasChanges = true;
						needsSave = true;
					}
				}
			}

			return needsSave;
		}
	}
}
