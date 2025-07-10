using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Module
{
	internal abstract class TargetRecordSelection : ITargetRecordSelection
	{
		public TargetRecordSelection()
		{
			ExclusionReasons = new List<string>();
		}

		public IList<String> ExclusionReasons
		{
			get;
			private set;
		}

		public abstract int FilterRowCount { get; }

		public abstract ISelectedRecords GetSelectedRecords();

		protected IEnumerable<BusinessObject> ExcludeBusinessObject(IEnumerable<BusinessObject> targets)
		{
			if (targets == null)
			{
				return null;
			}

			if (!targets.Any())
			{
				return targets.ToArray();
			}

			ExclusionReasons.Clear();

			return targets.Where(target =>
			{
				var canBeExcluded = target as ICanBeExcludedFromOperationalActions;

				if (canBeExcluded != null)
				{
					if (canBeExcluded.ShouldExclude)
					{
						ExclusionReasons.Add(canBeExcluded.ReasonForExclusion);
					}
					return !canBeExcluded.ShouldExclude;
				}

				return true;
			});
		}
	}
}
