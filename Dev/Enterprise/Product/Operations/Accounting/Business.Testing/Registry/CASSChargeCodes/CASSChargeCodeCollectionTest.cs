using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CASSChargeCodeCollection))]
	public class CASSChargeCodeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CASSChargeCodeCollection>
	{
		#region Implementation

		protected override CASSChargeCodeCollection GetCollectionToTest()
		{
			return new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CASSChargeCode();
		}

		#endregion

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
	}
}
