using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMREstablishmentCodesCollection))]
	sealed class CMREstablishmentCodesCollectionTest : ActiveBusinessObjectCollectionTestCase<CMREstablishmentCodesCollection>
	{
		protected override CMREstablishmentCodesCollection GetCollectionToTest() => new SmallCMREstablishmentCodesCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = base.GetNewElementToAddToTheCollection() as CMREstablishmentCodes;
			result.EC_EstablishmentAQISPremisesIndicator = true;
			return result;
		}

		sealed class SmallCMREstablishmentCodesCollection : CMREstablishmentCodesCollection
		{
			public SmallCMREstablishmentCodesCollection(BusinessObjectFactory factory)
				: base(factory)
			{
				AdditionalFilter = new ZQuery(CMREstablishmentCodesSchema.EC_EstablishmentAQISPremisesIndicator, true);
			}

			protected override void OnAdded(CMREstablishmentCodes businessObject)
			{
				businessObject.EC_EstablishmentAQISPremisesIndicator = true;
			}
		}
	}
}
