using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class EnumerationChecker
	{
		public void CheckEnums(Type enumerationType, int maxLength)
		{
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			foreach (int value in Enum.GetValues(enumerationType))
			{
				if (!dictionary.ContainsKey(value))
				{
					dictionary.Add(value, true);
				}
				else
				{
					string elements = "";
					foreach (string name in Enum.GetNames(enumerationType))
					{
						if ((int)Enum.Parse(enumerationType, name) == value)
						{
							elements += name + Environment.NewLine;
						}
					}
					if (elements.Length > 0)
					{
						Assertion.Fail("Duplicate numbers found on the following enumeration elements:" + Environment.NewLine + elements);
					}
				}
			}
			foreach (var name in Enum.GetNames(enumerationType))
			{
				Assertion.Assert("Name (" + name + ") should be " + maxLength.ToString() + " chars or less in length because of database constraint", name.Length <= maxLength);
			}
		}
	}
}
