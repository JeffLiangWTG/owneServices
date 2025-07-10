using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace CargoWise.Main.Navigation;

static class HomeFeaturesManagerProvider
{
	internal static HomeFeaturesManager GetInstance() =>
		new HomeFeaturesManager()
			.AddFeatureToggle(HomeFeature.Snapshots, IsSnapshotsEnabledPredicate());

	static Func<bool> IsSnapshotsEnabledPredicate()
	{
		var userCode = GlbStaff.CurrentUser.GS_Code.ToString();
		var userName = GlbStaff.CurrentUser.GS_LoginName.ToString();

		var set = new HashSet<(string, string)>(new TupleEqualityComparer());
		set.AddRange(Homies);
		set.AddRange(CWNextStakeholders);

#if WINZOR
		var fallback = true;
#else
		var fallback = false;
#endif
		return () => set.Contains((userCode, userName)) || fallback;
	}

	static readonly IReadOnlyCollection<(string, string)> Homies = new List<(string, string)>
	{
#if DEBUG
		// Local
		("E", "CWSupport"),
#endif
		// SAND
		("M53", "Leonam.Santos"),

		// ediProd
		("AQV", "Anirudh.Pandey"),
		("LNS", "Leonam.Santos"),
		("LSZ", "Louis.Zhou"),
		("LTG", "Leon.Tang"),
		("M71", "MohammadShams.Azam"),
		("QWN", "Colin.Wang"),
		("RAL", "Renan.Alvarenga"),
		("RDA", "Rachana.Dholakia"),
		("RK6", "RohitKumar.Rajak"),
		("RVW", "Riven.Wang"),
	};

	static readonly IReadOnlyCollection<(string, string)> CWNextStakeholders = new List<(string, string)>
	{
		("MS", "Mike.Sverdlov"),
		("NRD", "Neil.Roodyn"),
	};

	static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> range)
	{
		if (collection is null || range is null)
		{
			return;
		}

		foreach (var item in range)
		{
			collection.Add(item);
		}
	}
}
