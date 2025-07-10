using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	internal sealed class NewChargesStrategyTest : ChargeSpecificationStrategyTest<NewChargesStrategy>
	{
		public void TestSkipHeaders()
		{
			IChargeSpecificationStrategy strategy = NewStrategy();

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

			CombineAssertions(delegate
			{
				AssertEquals(false, strategy.SkipJobHeader(job));
				strategy.NotifySkippedHeaders();
			});
		}

		public void TestMatching()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

			AccChargeCode frt = GetChargeCode("FRT");
			AccChargeCode odoc = GetChargeCode("ODOC");

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = frt.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = odoc.PK;

			IChargeSpecificationStrategy strategy = NewStrategy();

			AssertNull(strategy.MatchCharge(job, frt, null, null, new Xsd.ChargeLine()
			{
				ChargeCode = "FRT",
			}));

			strategy.RemoveUnmatchedCharges();

			AssertContainsExactElementsInAnyOrder(new Charge[] { charge1, charge2 }, job.Charges.ToArray<Charge>());
		}

		#region Implementation

		protected override Xsd.ChargesSpecified Value
		{
			get { return Xsd.ChargesSpecified.NewCharges; }
		}

		#endregion
	}
}
