using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CustomsManifestLineSequenceCollection))]
	public class CustomsManifestLineSequenceCollectionTest : CusCodeDataCollectionTest<CustomsManifestLineSequence>
	{
		protected override CusCodeDataCollection<CustomsManifestLineSequence> GetCusCodeDataCollection()
		{
			return new CustomsManifestLineSequenceCollection(Shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var esmLineSequence = Factory.New<CustomsManifestLineSequence>();
			esmLineSequence.CY_ParentID = Shipment.PK;
			esmLineSequence.CY_ParentTableCode = Shipment.TablePrefix;
			return esmLineSequence;
		}

		CommonShipment Shipment => shipment ?? (shipment = Factory.New<CommonShipment>());
		CommonShipment shipment;
	}
}
