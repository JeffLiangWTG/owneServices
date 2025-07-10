using System;

namespace CargoWise.BuildTools
{
	public interface IReleaseInfo
	{
		string ReleaseRing { get; }
		DateTime ReleaseDate { get; }
	}
}
