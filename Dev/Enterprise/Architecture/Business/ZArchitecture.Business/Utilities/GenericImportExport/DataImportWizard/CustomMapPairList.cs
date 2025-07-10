using System;

using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class CustomMapPairList : NonPersistentBusinessObjectCollection<CustomMapPair>, ICodeDescriptionPairList
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomMapPair();
		}

		#region ICodeDescriptionPairList Members

		public bool ContainsCode(object code)
		{
			string codeString = code.ToString().Trim();
			foreach (ICodeDescription cd in this)
			{
				if (cd.Code.Equals(codeString, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		public string GetDescriptionFromCode(string code)
		{
			code = code.Trim();
			foreach (ICodeDescription cd in this)
			{
				if (cd.Code.Equals(code, StringComparison.CurrentCultureIgnoreCase))
				{
					return cd.Description;
				}
			}

			return null;
		}

		#endregion
	}
}
