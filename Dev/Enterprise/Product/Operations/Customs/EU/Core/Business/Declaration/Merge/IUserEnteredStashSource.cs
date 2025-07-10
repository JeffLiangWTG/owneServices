using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IUserEnteredStashSource
	{
		IDictionary<ZPropertyInfo, ZBool> StashedPropertiesAndConditions { get; }

		ZBool ShouldStash();

		ZString GetStashKey();
	}
}
