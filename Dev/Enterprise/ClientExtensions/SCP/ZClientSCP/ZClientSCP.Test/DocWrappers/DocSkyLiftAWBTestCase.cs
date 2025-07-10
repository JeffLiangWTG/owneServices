using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SCP.Testing
{
	[TestedType(typeof(DocSkyLiftAWB))]
	public class DocSkyLiftAWBTestCase : DocumentWrapperTestCase
	{
		public void TestAccountingInformation1()
		{
			AssertAccountingInfo("AccountingInformation1", 1);
		}

		public void TestAccountingInformation2()
		{
			AssertAccountingInfo("AccountingInformation2", 2);
		}

		public void TestAccountingInformation3()
		{
			AssertAccountingInfo("AccountingInformation3", 3);
		}

		public void TestAccountingInformation4()
		{
			AssertAccountingInfo("AccountingInformation4", 4);
		}

		public void TestAccountingInformation5()
		{
			AssertAccountingInfo("AccountingInformation5", 5);
		}

		public void TestAlsoNotify()
		{
			AssertEquals("Also Notify not entered", "", SkyLiftAWBWrapper.AlsoNotify);
			AssertEquals("Notify Overriden should be false", ZBool.False, SkyLiftAWBWrapper.IsNotifyOverriden);
			AWB.EH_AlsoNotifyName = "Also Notify Name";
			AssertEquals("Also Notify Name", "ALSO NOTIFY NAME", SkyLiftAWBWrapper.AlsoNotify);
			AWB.EH_AlsoNotifyAddress = "Also Notify Address";
			AssertEquals("Also Notify Name & Address", "ALSO NOTIFY NAME\nALSO NOTIFY ADDRESS", SkyLiftAWBWrapper.AlsoNotify);
			AWB.EH_IsNotifyOverriden = ZBool.True;
			AWB.EH_NotifyOverride1 = "NOTIFY OVERRIDE 1";
			AWB.EH_NotifyOverride2 = "NOTIFY OVERRIDE 2";
			AWB.EH_NotifyOverride4 = "NOTIFY OVERRIDE 4";
			AssertEquals("Also Notify Name & Address Overriden", "NOTIFY OVERRIDE 1\nNOTIFY OVERRIDE 2\nNOTIFY OVERRIDE 4", SkyLiftAWBWrapper.AlsoNotify);
		}

		public void TestConsolNumber()
		{
			AssertEquals("No Consol number", "", SkyLiftAWBWrapper.ConsolNumber);
			var consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C99990293";
			AssertEquals("Consol number", "C99990293", SkyLiftAWBWrapper.ConsolNumber);
		}

		public void TestShipmentNumber()
		{
			Shipment.JS_UniqueConsignRef = "S99937323";
			AssertEquals("Shipment number", "S99937323", SkyLiftAWBWrapper.ShipmentNumber);
		}

		#region Implementation
		protected ExportAWBHeader AWB;
		protected DocSkyLiftAWB SkyLiftAWBWrapper;
		protected ForwardingShipment Shipment;
		protected override void SetUp()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AWB = Shipment.AWBHeader;
			SkyLiftAWBWrapper = (DocSkyLiftAWB)DocSkyLiftAWB.New(AWB, Factory);
			AssertNotNull("SkyLift AWB Wrapper should not be null", SkyLiftAWBWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { SkyLiftAWBWrapper };
		}

		protected void AssertAccountingInfo(ZString propertyName, ZInt count)
		{
			AssertEquals("No accounting information", "", SkyLiftAWBWrapper[propertyName]);
			for (int i = 1; i <= count; i++)
			{
				ZString accountingInfoID = "AC" + i.ToString();
				ExportAWBAccountingInformation accInfo = AWB.AWBAccountingInformations.AddNew();
				accInfo.EA_InformationID = accountingInfoID;
				accInfo.EA_Information = "INFORMATION FOR " + accountingInfoID;
			}

			AssertEquals("Accounting information" + count.ToString(), "AC" + count.ToString() + " INFORMATION FOR AC" + count.ToString(), SkyLiftAWBWrapper[propertyName]);
		}
		#endregion
	}
}
