using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ContingencyDataEmailAddressCollection))]
	sealed class ContingencyDataEmailAddressCollectionTest : RegistryBusinessObjectCollectionTestCase<ContingencyDataEmailAddressCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ContingencyDataEmailAddressCollection GetCollectionToTest()
		{
			return new ContingencyDataEmailAddressCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContingencyDataEmailAddress();
		}

		#endregion
	}
}
