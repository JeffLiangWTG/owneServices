using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class ContactNameValidataionHelper
	{
		public static ZString ValidationContactName(ZString contactName, bool fromStaff = false)
		{
			var result = ZString.Empty;
			var invalidCharacters = ZString.Empty;
			foreach (var c in contactName.ToString().ToCharArray())
			{
				var ascii = (int)c;
				if (ascii < 32 || ascii == 33 || ascii == 124 || ascii > 126)
				{
					invalidCharacters = invalidCharacters.IsEmpty ? ZString.Format("'{0}'", c.ToString()) : ZString.Format("{0},'{1}'", invalidCharacters, c.ToString());
				}
			}
			if (!invalidCharacters.IsEmpty)
			{
				if (fromStaff)
				{
					result = Res.GetString("4fba2466-42bb-48e7-967a-0495205276cb", "The following characters are not allowed in the full name: {0}", invalidCharacters);
				}
				else
				{
					result = Res.GetString("d54d7fb4-1864-450b-9c1c-f30001e76d53", "The following characters are not allowed in the contact name: {0}", invalidCharacters);
				}
			}

			return result;
		}
	}
}
