using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CustomsManifestLineSequenceCollection))]
	public class CustomsManifestConsignmentLineSequenceCollectionTest : CusCodeDataCollectionTest<CustomsManifestLineSequence>
	{
		protected override CusCodeDataCollection<CustomsManifestLineSequence> GetCusCodeDataCollection()
		{
			return new CustomsManifestLineSequenceCollection(Consignment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var esmLineSequence = Factory.New<CustomsManifestLineSequence>();
			esmLineSequence.CY_ParentID = Consignment.PK;
			esmLineSequence.CY_ParentTableCode = HVLVConsignmentSchema.Constants.Prefix;
			return esmLineSequence;
		}

		IHVLVConsignment Consignment => consignment ?? (consignment = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment))));
		IHVLVConsignment consignment;
	}
}
