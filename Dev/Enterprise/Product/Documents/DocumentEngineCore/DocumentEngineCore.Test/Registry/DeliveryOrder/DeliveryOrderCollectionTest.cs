using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DeliveryOrderCollection))]
	public class DeliveryOrderCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DeliveryOrderCollection>
	{
		#region Implementation

		protected override FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override DeliveryOrderCollection GetCollectionToTest()
		{
			return new DeliveryOrderCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DeliveryOrder(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
