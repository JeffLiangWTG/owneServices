using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.VerboseLoggingRegistryItemEditor, Enterprise.Registry.GUI")]
	public class VerboseLoggingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<VerboseLoggingCollection>
	{
		public VerboseLoggingRegistryDataType()
		{
			AddPair("HOST", ResString.GetMultilingualString("{E9B8FFFF-00E4-48D1-891D-5C98304974B9}", "System Logging"), ZDateTime.Empty);

			void AddPair(string code, ResourceString description, ZDateTime value)
			{
				DefaultValue.CodeMaxLength = 4;
				var item = DefaultValue.AddNew();
				item.Code = code;
				item.Description = description;
				item.Value = value;
				item.SystemDefined = true;
			}
		}

		protected override void ValidateCore(IRegistryItem registryItem, VerboseLoggingCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var duplicates = proposedValue
				.Cast<VerboseLoggingBusinessObject>()
				.GroupBy(o => o.Code)
				.Where(objects => objects.Count() > 1)
				.Select(objects => objects.Key)
				.OrderBy(s => s)
				.ToArray();
			if (duplicates.Length > 0)
			{
				throw new RegistryValidationException(Res.GetString(
					"{CBDAAB15-D083-454B-93EE-C796ADD49913}",
					"There should not be duplicated task codes. Duplicates: {0}.",
					string.Join(", ", duplicates)));
			}

			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}
	}
}
