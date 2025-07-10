using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public class RegistryFactory : BusinessObjectFactory
	{
		protected RegistryFactory()
		{
			NameForDebugging = "Registry Factory";
			((IBusinessObjectFactoryInternals)this).CanSave = false;
			if (!Globals.IsUserInteractive)
			{
				RefreshEnabled = false;
			}
		}

		public static RegistryFactory Instance
		{
			get
			{
				if (instance.Value == null)
				{
					instance.Value = new RegistryFactory();
				}
				return instance.Value;
			}
		}

		public Guid GetGroupPK(string code)
		{
			IGlbGroup group = LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, code));
			return (group == null) ? Guid.Empty : group.PK.ToGuid();
		}

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			return new RegistryFactory();
		}

		public static void RenewFactory()
		{
			instance.Value = null;
		}

		static readonly ThreadLocalOverridable<RegistryFactory> instance = new ThreadLocalOverridable<RegistryFactory>();
	}
}
