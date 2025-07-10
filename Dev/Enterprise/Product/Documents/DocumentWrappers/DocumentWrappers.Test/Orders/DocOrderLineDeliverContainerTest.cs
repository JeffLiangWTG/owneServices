using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderLineDeliverContainer))]
	sealed class DocOrderLineDeliverContainerTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrderLineDeliverContainer.New(Container, Factory)
			};
		}

		#region Overrides

		public void TestToString()
		{
			AssertEquals("ToString()", ZString.Empty, ContainerWrapper.ToString());
		}

		#endregion

		#region Wrapper Fields

		public void TestDelivery()
		{
			Container.J5_J4 = Factory.New(typeof(OrderLineDelivery)).PK;
			AssertNotNull("Delivery", ContainerWrapper.Delivery);
			AssertEquals("Delivery is of type DocOrderLineDelivery", typeof(DocOrderLineDelivery), ContainerWrapper.Delivery.GetType());
		}

		public void TestRefContainer()
		{
			AssertNull("RefContainer", ContainerWrapper.RefContainer);

			RefContainer containerType = Factory.LoadTop1<RefContainer>(new ZQuery());
			Container.J5_RC_NKContainerType = containerType.RC_Code;
			AssertNotNull("RefContainer", ContainerWrapper.RefContainer);
			AssertEquals("RefContainer is of type DocRefContainer", typeof(DocRefContainer), ContainerWrapper.RefContainer.GetType());
		}

		public void TestNKLoadPort()
		{
			AssertNull("NKLoadPort", ContainerWrapper.NKLoadPort);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Container.J5_RL_NKLoadPort = uNLOCO.RL_Code;
			AssertNotNull("NKLoadPort", ContainerWrapper.NKLoadPort);
			AssertEquals("NKLoadPort is of type DocUNLOCO", typeof(DocUNLOCO), ContainerWrapper.NKLoadPort.GetType());
		}

		public void TestNKArrivalVessel()
		{
			AssertNull("NKArrivalVessel", ContainerWrapper.NKArrivalVessel);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Container.J5_RV_NKArrivalVessel = vessel.RV_Code;
			AssertNotNull("NKArrivalVessel", ContainerWrapper.NKArrivalVessel);
			AssertEquals("NKArrivalVessel is of type DocVessel", typeof(DocVessel), ContainerWrapper.NKArrivalVessel.GetType());
		}

		#endregion

		#region ZString Fields

		public void TestContainerNum()
		{
			ZString containerNum = new ZString("ContainerNum");
			Container.J5_ContainerNum = containerNum;
			AssertEquals("ContainerNum", containerNum.ToUpper(), ContainerWrapper.ContainerNum);
		}

		public void TestContainerSeal()
		{
			ZString containerSeal = new ZString("ContainerSeal");
			Container.J5_ContainerSeal = containerSeal;
			AssertEquals("ContainerSeal", containerSeal, ContainerWrapper.ContainerSeal);
		}

		public void TestCustomAttribute1()
		{
			ZString customAttribute1 = new ZString("CustomAtt1");
			Container.J5_CustomAttribute1 = customAttribute1;
			AssertEquals("CustomAttribute1", customAttribute1, ContainerWrapper.CustomAttribute1);
		}

		public void TestCustomAttribute2()
		{
			ZString customAttribute2 = new ZString("CustomAtt2");
			Container.J5_CustomAttribute2 = customAttribute2;
			AssertEquals("CustomAttribute2", customAttribute2, ContainerWrapper.CustomAttribute2);
		}

		public void TestCustomAttribute3()
		{
			ZString customAttribute3 = new ZString("CustomAttribute3");
			Container.J5_CustomAttribute3 = customAttribute3;
			AssertEquals("CustomAttribute3", customAttribute3, ContainerWrapper.CustomAttribute3);
		}

		public void TestMasterBill()
		{
			ZString masterBill = new ZString("MasterBill");
			Container.J5_MasterBill = masterBill;
			AssertEquals("MasterBill", masterBill, ContainerWrapper.MasterBill);
		}

		public void TestPackUQ()
		{
			ZString packUQ = new ZString("PPP");
			Container.J5_F3_NKPackType = packUQ;
			AssertEquals("PackUQ", packUQ, ContainerWrapper.PackUQ);
		}

		public void TestVolumeUQ()
		{
			ZString volumeUQ = new ZString("VV");
			Container.J5_VolumeUQ = volumeUQ;
			AssertEquals("VolumeUQ", volumeUQ, ContainerWrapper.VolumeUQ);
		}

		public void TestVoyage()
		{
			ZString voyage = new ZString("Voyage");
			Container.J5_Voyage = voyage;
			AssertEquals("Voyage", voyage, ContainerWrapper.Voyage);
		}

		public void TestWeightUQ()
		{
			ZString weightUQ = new ZString("WW");
			Container.J5_WeightUQ = weightUQ;
			AssertEquals("WeightUQ", weightUQ, ContainerWrapper.WeightUQ);
		}

		#endregion

		#region ZDateTime Fields

		public void TestCustomDate1()
		{
			ZDateTime customDate1 = new ZDateTime(2004, 04, 04);
			Container.J5_CustomDate1 = customDate1;
			AssertEquals("CustomDate1", customDate1, ContainerWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			ZDateTime customDate2 = new ZDateTime(2004, 04, 04);
			Container.J5_CustomDate2 = customDate2;
			AssertEquals("CustomDate2", customDate2, ContainerWrapper.CustomDate2);
		}

		public void TestCustomDate3()
		{
			ZDateTime customDate3 = new ZDateTime(2004, 04, 04);
			Container.J5_CustomDate3 = customDate3;
			AssertEquals("CustomDate3", customDate3, ContainerWrapper.CustomDate3);
		}

		public void TestETA()
		{
			ZDateTime eTA = new ZDateTime(2004, 04, 04);
			Container.J5_ETA = eTA;
			AssertEquals("ETA", eTA, ContainerWrapper.ETA);
		}

		public void TestETD()
		{
			ZDateTime eTD = new ZDateTime(2004, 04, 04);
			Container.J5_ETD = eTD;
			AssertEquals("ETD", eTD, ContainerWrapper.ETD);
		}

		public void TestInstoreDate()
		{
			ZDateTime instoreDate = new ZDateTime(2004, 04, 04);
			Container.J5_InstoreDate = instoreDate;
			AssertEquals("InstoreDate", instoreDate, ContainerWrapper.InstoreDate);
		}

		public void TestSightedDate()
		{
			ZDateTime sightedDate = new ZDateTime(2004, 04, 04);
			Container.J5_SightedDate = sightedDate;
			AssertEquals("SightedDate", sightedDate, ContainerWrapper.SightedDate);
		}

		#endregion

		#region ZDecimal

		public void TestCustomDecimal1()
		{
			ZDecimal customDecimal1 = new ZDecimal(1);
			Container.J5_CustomDecimal1 = customDecimal1;
			AssertEquals("CustomDecimal1", customDecimal1, ContainerWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			ZDecimal customDecimal2 = new ZDecimal(1);
			Container.J5_CustomDecimal2 = customDecimal2;
			AssertEquals("CustomDecimal2", customDecimal2, ContainerWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			ZDecimal customDecimal3 = new ZDecimal(1);
			Container.J5_CustomDecimal3 = customDecimal3;
			AssertEquals("CustomDecimal3", customDecimal3, ContainerWrapper.CustomDecimal3);
		}

		public void TestQuantityInStore()
		{
			ZDecimal quantityInStore = new ZDecimal(1);
			Container.J5_QuantityInStore = quantityInStore;
			AssertEquals("QuantityInStore", quantityInStore, ContainerWrapper.QuantityInStore);
		}

		public void TestQuantityInvoiced()
		{
			ZDecimal quantityInvoiced = new ZDecimal(1);
			Container.J5_QuantityInvoiced = quantityInvoiced;
			AssertEquals("QuantityInvoiced", quantityInvoiced, ContainerWrapper.QuantityInvoiced);
		}

		public void TestVolume()
		{
			ZDecimal volume = new ZDecimal(1);
			Container.J5_Volume = volume;
			AssertEquals("Volume", volume, ContainerWrapper.Volume);
		}

		public void TestWeight()
		{
			ZDecimal weight = new ZDecimal(1);
			Container.J5_Weight = weight;
			AssertEquals("Weight", weight, ContainerWrapper.Weight);
		}

		#endregion

		#region ZBool Fields

		public void TestCustomFlag1()
		{
			Container.J5_CustomFlag1 = ZBool.False;
			Assert("!CustomFlag1", !ContainerWrapper.CustomFlag1);

			Container.J5_CustomFlag1 = ZBool.True;
			Assert("CustomFlag1", ContainerWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			Container.J5_CustomFlag2 = ZBool.False;
			Assert("!CustomFlag2", !ContainerWrapper.CustomFlag2);

			Container.J5_CustomFlag2 = ZBool.True;
			Assert("CustomFlag2", ContainerWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			Container.J5_CustomFlag3 = ZBool.False;
			Assert("!CustomFlag3", !ContainerWrapper.CustomFlag3);

			Container.J5_CustomFlag3 = ZBool.True;
			Assert("CustomFlag3", ContainerWrapper.CustomFlag3);
		}

		#endregion

		#region ZShort Fields

		public void TestPackCount()
		{
			Container.J5_PackCount = 1;
			AssertEquals("PackCount", "1", ContainerWrapper.PackCount.ToString());
		}

		#endregion

		#region Implementation

		OrderLineDeliverContainer Container;
		DocOrderLineDeliverContainer ContainerWrapper;

		protected override void SetUp()
		{
			var theOrder = Factory.New<Order>();
			OrderLine theLine = theOrder.OrderLines.AddNew();
			OrderLineDelivery theDelivery = theLine.Deliveries.AddNew();
			Container = theDelivery.Containers.AddNew();
			ContainerWrapper = DocOrderLineDeliverContainer.New(Container, Factory);

			base.SetUp();
		}

		#endregion

	}
}
