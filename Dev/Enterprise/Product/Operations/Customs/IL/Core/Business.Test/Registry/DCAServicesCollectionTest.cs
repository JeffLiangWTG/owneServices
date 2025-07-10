using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(DCAServicesCollection))]
	sealed class DCAServicesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DCAServicesCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override DCAServicesCollection GetCollectionToTest()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var parameter = new DCAParameters(fallbackLevel, Factory);
			return new DCAServicesCollection(parameter, fallbackLevel, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DCAService() { Name = Guid.NewGuid().ToString() };
	}
}
