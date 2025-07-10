using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Web;

namespace Enterprise.Registry.Business.Testing
{
	public sealed class DummyAccessControlled : DummyBusinessObject, IAccessControlled
	{
		public DummyAccessControlled(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public AccessControlRegistryItem SuppressionItem
		{
			get { return item; }
			set { item = value; }
		}
		AccessControlRegistryItem item;

		public string GetRegistryCaption(string boundPropertyName)
		{
			switch (boundPropertyName)
			{
				case Schema.Z0_Date:
					return DummyAccessRules.Captions.caption1;
				case Schema.Z0_VarCharMax:
					return DummyAccessRules.Captions.caption2;
				case Schema.Z0_Code:
					return DummyAccessRules.Captions.caption3;
			}

			return null;
		}

		public bool LoggedInOrgIs(string role)
		{
			foreach (string orgsRole in LoggedInOrgsRoles)
			{
				if (role == orgsRole)
				{
					return true;
				}
			}
			return false;
		}
		public string[] LoggedInOrgsRoles = System.Array.Empty<string>();

		public bool IsInTextSuppressionMode
		{
			get; set;
		}
	}
}
