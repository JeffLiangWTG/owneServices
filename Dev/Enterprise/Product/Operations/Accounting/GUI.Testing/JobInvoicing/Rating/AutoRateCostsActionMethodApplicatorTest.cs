using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AutoRatingActionMethodApplicator))]
	internal class AutoRateCostsActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestRunAction_OnlyAutorateCosts()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			transportProvider.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "HKHKG";
			var chargeCode = "FRT";

			Helper.NewClientRateWithSingleRateLine(consignee, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, origin, destination, chargeCode, 200m);
			var costingRate = Helper.NewCosting(transportProvider);
			var costingEntry = costingRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, origin, destination);
			costingEntry.RateLines.RemoveAndDeleteAll();
			var costingLine = costingEntry.AddRateLine(chargeCode, FlatCalculator.Code, "", Constants.CurrencyCodes.Australia);
			costingLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_OA_ShippingLineAddress = transportProvider.MainAddress.PK;
			consol.CreditorPK = transportProvider.PK;
			Factory.Save();

			using (var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (job.GetValidationSuspender())
			{
				var expectedLog = "";
				ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);

				var charges = job.Charges.Cast<Charge>().ToArray();
				AssertEquals(1, charges.Length);
				var charge = charges[0];
				AssertEquals(100m, (decimal)charge.JR_LocalCostAmt);
				AssertEquals(100m, (decimal)charge.JR_LocalSellAmt);
				AssertEquals(true, charge.JR_CostRated);
				AssertEquals(false, charge.JR_SellRated);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AutoRatingActionMethodApplicator(Factory, true, false);
		}

		Rating.Business.Testing.TestHelper Helper => helper ?? (helper = new Rating.Business.Testing.TestHelper(Factory));
		Rating.Business.Testing.TestHelper helper;

		#endregion
	}
}
