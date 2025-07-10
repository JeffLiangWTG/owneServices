using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.EventLogsListProvidersAndTheirCapacityRegistryEditor, ZClientEDI")]
	public class EventLogsListProvidersAndTheirCapacityDataType : NonPersistentBusinessObjectRegistryDataType<EventLogsListProvidersAndTheirCapacityCollection>
	{
		protected override void ValidateCore(IRegistryItem registryItem, EventLogsListProvidersAndTheirCapacityCollection collection, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, collection, companyPK, branchPK, departmentPK);
			var compositeKeyDuplicated = collection.Collection.GroupBy(x => new { x.ProviderCode, x.LevelCode }).Select(x => new { ProviderCode = x.Key.ProviderCode, LevelCode = x.Key.LevelCode, Count = x.Count() }).Where(x => x.Count > 1);
			if (compositeKeyDuplicated.Any())
			{
				throw new RegistryValidationException(Res.GetString("f4384e5b-7c6d-44c1-9282-afbaba3bb2f3", "Duplicated Composite Keys(ProviderCode-LevelCode): {0}", ZString.Join(", ", compositeKeyDuplicated.Select(x => ZString.Format("{0}-{1}", x.ProviderCode, x.LevelCode)).ToArray())));
			}
		}
	}
}

