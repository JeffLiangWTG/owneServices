using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	sealed partial class CompactTableStrategy
	{
		sealed class GroupStrategy : IPricingPageRateLineListGroupStrategy<string>
		{
			public const string All = "[ALL]";
			public const string Containerised = "[CON]";
			public const string NonContainerised = "[NCON]";

			public string GroupKey(RateEntry rateEntry)
			{
				if (rateEntry.IsFCL())
				{
					var container = rateEntry.Container;

					if (container == null)
					{
						return Containerised;
					}
					else
					{
						return container.RC_Code;
					}
				}
				else if (rateEntry.IsLCL())
				{
					return NonContainerised;
				}
				else
				{
					return All;
				}
			}

			public bool IncludeContainer(string group, RefContainer container)
			{
				switch (group)
				{
					case All:
					case Containerised:
					case NonContainerised:
						return true;

					default:
						return container == null || container.RC_Code == group;
				}
			}

			public void CrossPollinate(IDictionary<string, List<RateLine>> groups)
			{
				// rates added to the general cases should be coppied to the specific cases.

				List<RateLine> all;
				List<RateLine> containerised;
				List<RateLine> nonContainerised;

				groups.TryGetValue(All, out all);
				groups.TryGetValue(Containerised, out containerised);
				groups.TryGetValue(NonContainerised, out nonContainerised);

				if (all != null)
				{
					hasAll = true;

					if (containerised == null)
					{
						containerised = all;
						groups.Add(Containerised, containerised);
					}
					else
					{
						containerised.AddRange(all);
						hasContainerised = true;
					}

					if (nonContainerised == null)
					{
						nonContainerised = all;
						groups.Add(NonContainerised, nonContainerised);
					}
					else
					{
						nonContainerised.AddRange(all);
						hasNonContainerised = true;
					}
				}
				else
				{
					hasContainerised |= (containerised != null);
					hasNonContainerised |= (NonContainerised != null);
				}

				foreach (var pair in groups)
				{
					switch (pair.Key)
					{
						case All:
						case Containerised:
						case NonContainerised:
							continue;
					}

					if (containerised != null)
					{
						pair.Value.AddRange(containerised);
					}
				}
			}

			public void Purge(IDictionary<string, List<PricingPageRateLineList>> groups)
			{
				if (hasAll)
				{
					if (hasContainerised || hasNonContainerised)
					{
						groups.Remove(All);
					}
					else
					{
						groups.Remove(Containerised);
						groups.Remove(NonContainerised);
					}
				}
			}

			public void Reset()
			{
				hasAll = false;
				hasContainerised = false;
				hasNonContainerised = false;
			}

			bool hasAll;
			bool hasContainerised;
			bool hasNonContainerised;
		}
	}
}
