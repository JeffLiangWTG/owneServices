using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module
{
	sealed class BusinessObjectSelector : TargetRecordSelection
	{
		public BusinessObjectSelector(IEnumerable<BusinessObject> businessObjects)
		{
			this.businessObjects = businessObjects;
		}

		readonly IEnumerable<BusinessObject> businessObjects;

		public override ISelectedRecords GetSelectedRecords()
		{
			var targets = ExcludeBusinessObject(businessObjects);

			return new SelectedRecords()
			{
				AutoSelectedAllKeys = false,
				PrimaryKeys = targets?.Select(target => target.PK).ToArray() ?? System.Array.Empty<ZGuid>(),
			};
		}

		public override int FilterRowCount => 0;
	}
}
