using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(LocalTransportCompanyBrandingCollection))]
	public class LocalTransportCompanyCollectionTestCase : RegistryBusinessObjectCollectionTestCase<LocalTransportCompanyBrandingCollection>
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

		protected override LocalTransportCompanyBrandingCollection GetCollectionToTest()
		{
			return new LocalTransportCompanyBrandingCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LocalTransportCompanyBranding(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
