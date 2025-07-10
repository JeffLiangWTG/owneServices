using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class PropertyUpdateNode : StepUpdateNode
	{
		internal PropertyUpdateNode(PropertyInfo info)
			: base(info) { }

		public override IEnumerable<BusinessObject> Apply(IEnumerable<BusinessObject> targets)
		{
			return base.Apply(PropertyTargets(targets));
		}

		IEnumerable<BusinessObject> PropertyTargets(IEnumerable<BusinessObject> fromTargets)
		{
			Dictionary<ZGuid, BusinessObject> result = new Dictionary<ZGuid, BusinessObject>();

			foreach (BusinessObject target in fromTargets)
			{
				(target as IOperationalActionsNullValueInitializer)?.CreateValueIfNull(Info);
				BusinessObject next = (BusinessObject)Info.GetValue(target, null);
				if (next != null && !result.ContainsKey(next.PK))
				{
					result.Add(next.PK, next);
				}
			}

			return result.Values;
		}

		internal IEnumerable<BusinessObject> ApplyToBizOsOnCollections(IEnumerable<IBusinessObjectCollection> collections)
		{
			var extractedBizOs = new List<BusinessObject>();
			foreach (var collection in collections)
			{
				var next = (BusinessObject)Info.GetValue(collection);
				extractedBizOs.Add(next);
			}

			return base.Apply(extractedBizOs);
		}
	}
}
