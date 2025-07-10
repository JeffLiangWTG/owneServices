using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(LimitedFiscalRepresentationNumberCustomisation))]
	sealed class LimitedFiscalRepresentationNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<LimitedFiscalRepresentationNumberCustomisation>
	{
		public static void Set(LimitedFiscalRepresentationNumberCustomisation customisation, string key, byte order, bool fountain, string detail = null)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Fountain = fountain;
			element.Detail = detail;
		}

		protected override LimitedFiscalRepresentationNumberCustomisation GetBusinessObjectToClone()
		{
			return new LimitedFiscalRepresentationNumberCustomisation();
		}

		protected override LimitedFiscalRepresentationNumberCustomisation GetBusinessObjectToSerialise()
		{
			return new LimitedFiscalRepresentationNumberCustomisation();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LimitedFiscalRepresentationNumberCustomisation();
		}
	}
}
