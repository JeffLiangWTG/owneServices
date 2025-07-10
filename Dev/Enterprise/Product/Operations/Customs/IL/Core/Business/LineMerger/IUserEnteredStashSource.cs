using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public interface IUserEnteredStashSource
	{
		IDictionary<ZPropertyInfo, ZBool> StashedPropertiesAndConditions { get; }

		ZBool ShouldStash();

		ZString GetStashKey();
	}
}
