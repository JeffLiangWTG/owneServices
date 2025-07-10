using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class APRHLineTest : AirProfitShareLineBaseTestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorNullParams()
		{
			new APRHLine(null, null);
		}

		public void TestLineAsString()
		{
			AssertEquals(ExpectedLineAsString, Line.LineAsString);
		}

		public void TestLineAsString_PrepaidShipment()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Prepaid);
			AssertEquals(ExpectedLineAsString.Replace(";C;", ";P;"), Line.LineAsString);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			JASDataRegistry.Instance.ProfitSplitDueDestinationPercentage = 99;
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.APRHFieldCount;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.APRH;
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new APRHLine(Consol, Shipment);
		}

		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
					OrgHeader forwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
					forwarder.OH_Code = "FORTED1";
					forwarder.OH_IsDebtor = true;
					forwarder.Factory.Save();
					fConsol.SetDefaultReceivingForwarderAddress(forwarder);
				}

				return fConsol;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
					fShipment.JS_HouseBill = "HB101";
					fShipment.JS_OuterPacks = 11;
					fShipment.JS_ActualChargeable = 56.3m;
					fShipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Collect);
					PopulateChargesForTest();
				}

				return fShipment;
			}
		}

		void PopulateChargesForTest()
		{
			JASOrgHeader differentSellAccount = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
			differentSellAccount.OH_Code = "DIFSEL1";
			differentSellAccount.OH_IsDebtor = true;
			differentSellAccount.Factory.Save();
			AddCharge(Shipment, Env.Registry.FreightChargeCode, 150m, 100m, 200m, 0, Consol.ReceivingForwarder);
			AddCharge(Shipment, Env.Registry.FreightChargeCode, 1250m, 0, 300m, 315m, differentSellAccount);
			AddCharge(Shipment, ZGuid.Empty, 50m, 0, 60m, 0, Consol.ReceivingForwarder);
			AddCharge(Shipment, ZGuid.Empty, 0, 0, 70m, 80m, Consol.ReceivingForwarder);
			AddCharge(Shipment, ZGuid.Empty, 0, 11m, 0, 13m, differentSellAccount);
			AddCharge(Shipment, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 0, 0, -15m, 0, Consol.ReceivingForwarder);
			AddCharge(Shipment, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 0, 0, 0, -22m, Consol.ReceivingForwarder);
			AddCharge(Shipment, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 0, 0, 0, 12m, Consol.ReceivingForwarder);
			AddCharge(Shipment, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 0, 0, 0, -100m, differentSellAccount);
		}

		JASForwardingConsol fConsol;
		JASForwardingShipment fShipment;
		const string ExpectedLineAsString = "APRH3100;HB101;11;56.3;C;515;340;1350;61;896;99;25;AUD";
		#endregion
	}
}
