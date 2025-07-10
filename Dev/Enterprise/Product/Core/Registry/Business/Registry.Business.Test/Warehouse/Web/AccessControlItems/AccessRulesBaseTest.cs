using System.Collections.Generic;
using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	abstract class AccessRulesBaseTest : TestCase
	{
		protected abstract AccessRulesBase GetTestRules();

		public void TestDefaults()
		{
			KeyValuePair<string, string>[] defaults = GetTestRules().GetDefaultTicked();

			if (defaults.Length > 0)
			{
				for (int i = 0; i < defaults.Length; i++)
				{
					AssertCollectionContains(string.Format("There is no such role in {0}: {1}", GetTestRules(), defaults[i].Key), defaults[i].Key, GetTestRules().GetRoles());
					AssertCollectionContains(string.Format("There is no such caption in {0}: {1}", GetTestRules(), defaults[i].Value), defaults[i].Value, GetTestRules().GetCaptions());

					if (i != (defaults.Length - 1))
					{
						for (int j = (i + 1); j < defaults.Length; j++)
						{
							if (defaults[i].Key == defaults[j].Key && defaults[i].Value == defaults[j].Value)
							{
								Fail(string.Format("Dublicates foundund. Indexes: {0}, {1}", i, j));
							}
						}
					}
				}
			}

			Assert(true);
		}
	}
}
