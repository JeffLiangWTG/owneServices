using System.Collections.Generic;
using Enterprise.Registry.Business.Web;

namespace Enterprise.Registry.Business.Testing
{
	public sealed class DummyAccessRules : AccessRulesBase
	{
		public static class Roles
		{
			public const string role1 = "role1";
			public const string role2 = "role2";
			public const string role3 = "role3";
		}

		public override string[] GetRoles()
		{
			return new[]
					   {
							Roles.role1,
							Roles.role2,
							Roles.role3
						   };
		}

		public static class Captions
		{
			public const string caption1 = "caption1";
			public const string caption2 = "caption2";
			public const string caption3 = "caption3";
		}

		public override string[] GetCaptions()
		{
			return new[]
					   {
							Captions.caption1,
							Captions.caption2,
							Captions.caption3,
						   };
		}

		public override KeyValuePair<string, string>[] GetDefaultTicked()
		{
			return new[]
					   {
							new KeyValuePair<string, string>(Roles.role1, Captions.caption1),
							new KeyValuePair<string, string>(Roles.role1, Captions.caption2),
							new KeyValuePair<string, string>(Roles.role2, Captions.caption3),
						   };
		}
	}
}
