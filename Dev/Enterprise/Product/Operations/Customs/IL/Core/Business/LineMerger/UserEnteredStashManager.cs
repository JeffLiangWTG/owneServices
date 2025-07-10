using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class UserEnteredStashManager
	{
		readonly Dictionary<ZString, UserEnteredStash> userEnteredStashes = new Dictionary<ZString, UserEnteredStash>();

		public void Stash(IUserEnteredStashSource userEnteredStashSource)
		{
			if (userEnteredStashSource.ShouldStash())
			{
				var stash = new UserEnteredStash();
				stash.Stash(userEnteredStashSource);
				var feeNaturalKey = userEnteredStashSource.GetStashKey();
				userEnteredStashes[feeNaturalKey] = stash;
			}
		}

		public bool Apply(IUserEnteredStashSource userEnteredStashSource)
		{
			var stashKey = userEnteredStashSource.GetStashKey();
			if (userEnteredStashes.ContainsKey(stashKey))
			{
				var stash = userEnteredStashes[stashKey];
				stash.Apply(userEnteredStashSource);
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
