using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using EMCSVersion2_4 = CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using EMCSVersion2_5 = CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EmcsResponseMessageDetailsTest : TestCaseWithFactory
	{
		public void TestResponseMessages()
		{
			AssertEquals(21, EmcsResponseMessageDetails.Instance.ResponseMessages.Count);
		}

		public void TestED801D()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_4.ED801D)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.Incoming.c_ead_val.xsd",
				typeof(EMCSVersion2_4.ED801D),
				typeof(Messaging.Version2_4.ED801Provider),
				typeof(EmcsInboundEDIMessage<IED801>));
		}

		public void TestED802B()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_4.ED802B)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.Incoming.c_exc_rem.xsd",
				typeof(EMCSVersion2_4.ED802B),
				typeof(Messaging.Version2_4.ED802Provider),
				typeof(EmcsInboundEDIMessage<IED802>));
		}

		public void TestED813E()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_4.ED813E)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.c_upd_dat.xsd",
				typeof(EMCSVersion2_4.ED813E),
				typeof(Messaging.Version2_4.ED813Provider),
				typeof(EmcsInboundEDIMessage<IED813>));
		}

		public void TestED818C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_4.ED818C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.c_del_dat.xsd",
				typeof(EMCSVersion2_4.ED818C),
				typeof(Messaging.Version2_4.ED818Provider),
				typeof(EmcsInboundEDIMessage<IED818>));
		}

		public void TestED819C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_4.ED819C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.c_rej_dat.xsd",
				typeof(EMCSVersion2_4.ED819C),
				typeof(Messaging.Version2_4.ED819Provider),
				typeof(EmcsInboundEDIMessage<IED819>));
		}

		public void TestED840C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_4.ED840C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.Incoming.c_evt_dat.xsd",
				typeof(EMCSVersion2_4.ED840C),
				typeof(Messaging.Version2_4.ED840Provider),
				typeof(EmcsInboundEDIMessage<IED840>));
		}

		public void TestED871C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_4.ED871C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.c_shr_exp.xsd",
				typeof(EMCSVersion2_4.ED871C),
				typeof(Messaging.Version2_4.ED871Provider),
				typeof(EmcsInboundEDIMessage<IED871>));
		}

		public void TestED704C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED704C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed704c.xsd",
				typeof(EMCSVersion2_5.ED704C),
				typeof(Messaging.Version2_5.ED704Provider),
				typeof(EmcsInboundEDIMessage<IED704>));
		}

		public void TestED801E()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED801E)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed801e.xsd",
				typeof(EMCSVersion2_5.ED801E),
				typeof(Messaging.Version2_5.ED801Provider),
				typeof(EmcsInboundEDIMessage<IED801>));
		}

		public void TestED802C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED802C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed802c.xsd",
				typeof(EMCSVersion2_5.ED802C),
				typeof(Messaging.Version2_5.ED802Provider),
				typeof(EmcsInboundEDIMessage<IED802>));
		}

		public void TestED803B()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED803B)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed803b.xsd",
				typeof(EMCSVersion2_5.ED803B),
				typeof(Messaging.Version2_5.ED803Provider),
				typeof(EmcsInboundEDIMessage<IED803>));
		}

		public void TestED807B()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED807B)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed807b.xsd",
				typeof(EMCSVersion2_5.ED807B),
				typeof(Messaging.Version2_5.ED807Provider),
				typeof(EmcsInboundEDIMessage<IED807>));
		}

		public void TestED810C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED810C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ed810c.xsd",
				typeof(EMCSVersion2_5.ED810C),
				typeof(Messaging.Version2_5.ED810Provider),
				typeof(EmcsInboundEDIMessage<IED810>));
		}

		public void TestED813F()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED813F)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ed813f.xsd",
				typeof(EMCSVersion2_5.ED813F),
				typeof(Messaging.Version2_5.ED813Provider),
				typeof(EmcsInboundEDIMessage<IED813>));
		}

		public void TestED818D()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED818D)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ed818d.xsd",
				typeof(EMCSVersion2_5.ED818D),
				typeof(Messaging.Version2_5.ED818Provider),
				typeof(EmcsInboundEDIMessage<IED818>));
		}

		public void TestED819D()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED819D)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ed819d.xsd",
				typeof(EMCSVersion2_5.ED819D),
				typeof(Messaging.Version2_5.ED819Provider),
				typeof(EmcsInboundEDIMessage<IED819>));
		}

		public void TestED829C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED829C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed829c.xsd",
				typeof(EMCSVersion2_5.ED829C),
				typeof(Messaging.Version2_5.ED829Provider),
				typeof(EmcsInboundEDIMessage<IED829>));
		}

		public void TestED839C()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED839C)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed839c.xsd",
				typeof(EMCSVersion2_5.ED839C),
				typeof(Messaging.Version2_5.ED839Provider),
				typeof(EmcsInboundEDIMessage<IED839>));
		}

		public void TestED840D()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED840D)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed840d.xsd",
				typeof(EMCSVersion2_5.ED840D),
				typeof(Messaging.Version2_5.ED840Provider),
				typeof(EmcsInboundEDIMessage<IED840>));
		}

		public void TestED871D()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED871D)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ed871d.xsd",
				typeof(EMCSVersion2_5.ED871D),
				typeof(Messaging.Version2_5.ED871Provider),
				typeof(EmcsInboundEDIMessage<IED871>));
		}

		public void TestED881A()
		{
			DE.Business.Testing.TestHelper.AssertResponseDetails(EmcsResponseMessageDetails.Instance.ResponseMessages[nameof(EMCSVersion2_5.ED881A)],
				"CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.Incoming.ed881a.xsd",
				typeof(EMCSVersion2_5.ED881A),
				typeof(Messaging.Version2_5.ED881Provider),
				typeof(EmcsInboundEDIMessage<IED881>));
		}
	}
}
