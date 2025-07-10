using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Registry;

namespace ZClientEDI.Business.Test.Registry.Telematics
{
	[TestedType(typeof(TcaRimEnrollmentSchemeCollection))]
	public class TcaRimEnrollmentSchemeCollectionTest : RegistryBusinessObjectCollectionTestCase<TcaRimEnrollmentSchemeCollection>
	{
		protected override TcaRimEnrollmentSchemeCollection GetCollectionToTest()
		{
			return new TcaRimEnrollmentSchemeCollection();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBool();
		}
	}
}
