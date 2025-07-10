using System;
using System.Reflection;
using CargoWise.Application;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IOrganisationsDataRegistry
	{
		BooleanRegistryItem AllowNumericCharactersInCodeGeneration { get; }
		bool ComplianceWiseFeatureDevelopmentEnabled(string code);
	}

	public static class OrganisationsDataRegistryProvider
	{
		public static IOrganisationsDataRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (IOrganisationsDataRegistry)ObjectFactory.GetType<IOrganisationsDataRegistry>().GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
				}
				return instance;
			}
		}

		[ThreadStatic]
		static IOrganisationsDataRegistry instance;
	}
}
