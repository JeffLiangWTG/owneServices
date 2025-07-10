using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
	public sealed class BooleanRegistryItemTestAttribute : TestSetupAttribute
	{
		public BooleanRegistryItemTestAttribute(Type registryType, string registryItemPropertyName, bool currentCompany = false, bool currentBranch = false, bool currentDepartment = false)
			: this(registryType, new[] { registryItemPropertyName }, currentCompany, currentBranch, currentDepartment)
		{
		}

		public BooleanRegistryItemTestAttribute(Type registryType, string[] registryItemPropertyNames, bool currentCompany = false, bool currentBranch = false, bool currentDepartment = false)
		{
			Argument.NotNull(registryType, nameof(registryType));
			Argument.NotNull(registryItemPropertyNames, nameof(registryItemPropertyNames));

			if (registryType.IsInterface)
			{
				registryType = ObjectFactory.GetType(registryType);
			}

			var registryItemSet = registryType.InvokeMember("Instance", BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public, null, null, Array.Empty<object>());
			var items = new List<BooleanRegistryItem>();
			foreach (var name in registryItemPropertyNames)
			{
				Argument.NotNullOrEmpty(name, nameof(registryItemPropertyNames));
				var propertyInfo = registryItemSet.GetType().GetProperty(name);
				items.Add(propertyInfo.GetValue(registryItemSet, Array.Empty<object>()) as BooleanRegistryItem);
			}

			RegistryItems = items.ToArray();
			CurrentCompany = currentCompany;
			CurrentBranch = currentBranch;
			CurrentDepartment = currentDepartment;
		}

		public override void SetUp(TestCase testCase)
		{
			DisposeRegistryItems = RegistryItems.Select(registryItem => registryItem.SetTemporaryValue(
				CurrentCompany ? Env.CurrentCompanyPK : Guid.Empty,
				CurrentBranch ? Env.CurrentBranchPK : Guid.Empty,
				CurrentDepartment ? Env.CurrentDepartmentPK : Guid.Empty,
				true)).ToArray();
		}

		public override void TearDown(TestCase testCase)
		{
			DisposeRegistryItems.ForEach(registryItem => registryItem.Dispose());
		}

		#region Implementation

		bool CurrentCompany { get; }
		bool CurrentBranch { get; }
		bool CurrentDepartment { get; }
		BooleanRegistryItem[] RegistryItems { get; }
		IDisposable[] DisposeRegistryItems { get; set; }

		#endregion
	}
}
