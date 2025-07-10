using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(QuantityPerUnitInfoCollection))]
	public class QuantityPerUnitInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<QuantityPerUnitInfo>
	{
		protected override CusSupportingInfoCollection<QuantityPerUnitInfo> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new QuantityPerUnitInfoCollection(jobComInvoice);
		}

		public void TestAddNewWithRateCode()
		{
			var collection = GetCusSupportingInfoCollection() as QuantityPerUnitInfoCollection;
			AssertEquals("CSI_SubType", Constants.RateCodes.Antidumping, collection.AddNew(Constants.RateCodes.Antidumping).CSI_SubType);
			AssertEquals("Has Changes should be False", false, collection.HasChanges);
		}

		public void TestFindByRateCode()
		{
			var collection = GetCusSupportingInfoCollection() as QuantityPerUnitInfoCollection;
			var legalAct = collection.AddNew(Constants.RateCodes.Antidumping);
			AssertEquals(legalAct, collection.FindByRateCode(Constants.RateCodes.Antidumping));
			AssertNull(collection.FindByRateCode(Constants.RateCodes.IPI));
		}
	}
}
