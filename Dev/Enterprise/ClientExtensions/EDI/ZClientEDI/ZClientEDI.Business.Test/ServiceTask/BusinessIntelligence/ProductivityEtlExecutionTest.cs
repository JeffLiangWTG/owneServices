using System;
using System.Globalization;
using System.Reflection;
using CargoWise.Bi.Product.DataLoad.Testing;
using CargoWise.Common;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.ServiceTask.Test.BusinessIntelligence
{
	public abstract class ProductivityEtlExecutionTest : EdwEtlExecutionTest
	{
		#region Setup

		protected override void SetUp()
		{
			RunClientDbCreateScripts();
			base.SetUp();
		}

		void RunClientDbCreateScripts()
		{
			Type type = AssemblyLoader.LoadAssembly("Enterprise.ZArchitecture.Core").GetType("Enterprise.ZArchitecture.Core.Testing.ClientDbSchemaCreationForTesting");
			type.InvokeMember("RunClientDbCreateScripts", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), new object[] { false }, CultureInfo.InvariantCulture);
		}

		#endregion

		protected IncidentMainBase CreateIncidentMain()
		{
			var incident = factory.NewWithValidTestData<IncidentMainBase>();
			factory.Save();
			return incident;
		}
	}
}
