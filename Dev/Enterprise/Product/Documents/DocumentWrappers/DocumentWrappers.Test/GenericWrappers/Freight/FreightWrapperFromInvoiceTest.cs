using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromInvoice))]
	sealed class FreightWrapperFromInvoiceTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var invoice = Factory.New<ARInvoice>();
			var wrapper = new FreightWrapperFromInvoice(invoice, Factory);

			AssertEquals("TrackingBusinessObjectPK", invoice.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestCO2eEmissions()
		{
			var wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals("No emissions when invoice does not have a related shipment.", 0, wrapper.CO2eEmissions.Count);

			var shipment = Factory.New<ForwardingShipment>();
			SetInvoiceJobForForwardingShipment(invoice, shipment);
			wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals(0, wrapper.CO2eEmissions.Count);

			shipment.Transports.AddNew();
			shipment.Transports.AddNew();
			wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals(2, wrapper.CO2eEmissions.Count);
		}

		public override void TestFormattedTotalCO2e()
		{
			var wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals("No formatted co2e when invoice does not have a related shipment.", ZString.Empty, wrapper.FormattedTotalCO2e);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = consol.Shipments.AddNew();
			shipment.SetTotalCO2e(9.07244m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			SetInvoiceJobForForwardingShipment(invoice, shipment);
			wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals("9.072", wrapper.FormattedTotalCO2e);

			shipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals(ZString.Empty, wrapper.FormattedTotalCO2e);
		}

		public override void TestCO2eCalculationDate()
		{
			var wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals("No CO2e calculation date when invoice does not have a related shipment.", ZDateTime.Empty, wrapper.CO2eCalculationDate);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100m;
			shipment.SetCO2ePerTonneInKg(200m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			SetInvoiceJobForForwardingShipment(invoice, shipment);
			wrapper = FreightWrapper.New(invoice, Factory).First();
			AssertEquals((shipment.GetOrCreateJobCO2e() as JobCO2e).JCO_SystemLastEditTimeUtc, wrapper.CO2eCalculationDate);
		}

		#region Implementation

		void SetInvoiceJobForForwardingShipment(ARInvoice invoice, ForwardingShipment shipment)
		{
			var header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "00001000";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.AH_JH = header.PK;
			Factory.Save();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromInvoice(invoice, Factory);
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<ARInvoice>();
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoice = Factory.New<ARInvoice>();
		}

		ARInvoice invoice;

		#endregion
	}
}
