using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Messaging;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ATLASResponseMessageDetailsTest : TestCaseWithFactory
	{
		public void TestResponseMessages()
		{
			AssertEquals(31, ATLASResponseMessageDetails.Instance.ResponseMessages.Count);
		}

		public void TestDEERRF()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DEERRF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Collective.Incoming.DEERRF.xsd",
				typeof(ATLASVersion10_1.DEERRF),
				typeof(Messaging.ATLASVersion10_1.ERRNCKProvider),
				typeof(AtlasInboundEDIMessage<IERRNCK>));
		}

		public void TestDEERRG()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DEERRG)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DEERRG.xsd",
				typeof(ATLASVersion10_1.DEERRG),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.ERRNCKGProvider),
				typeof(AtlasInboundEDIMessage<IERRNCK>));

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DEERRG)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DEERRG.xsd",
				typeof(ATLASVersion10_1.DEERRG),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.ERRNCKGProvider),
				typeof(AtlasInboundEDIMessage<IERRNCK>));
		}

		public void TestGCNOAD()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.GCNOAD)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Collective.Incoming.GCNOAD.xsd",
				typeof(ATLASVersion10_1.GCNOAD),
				typeof(Messaging.ATLASVersion10_1.CUSNOAProvider),
				typeof(AtlasInboundEDIMessage<ICUSNOA>));
		}

		public void TestDETQSC()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETQSC)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETQSC.xsd",
				typeof(ATLASVersion10_1.DETQSC),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.TRQSTAProvider),
				typeof(AtlasInboundEDIMessage<ITRQSTA>));

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETQSC)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETQSC.xsd",
				typeof(ATLASVersion10_1.DETQSC),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.TRQSTAProvider),
				typeof(AtlasInboundEDIMessage<ITRQSTA>));
		}

		public void TestDETSSB()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETSSB)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETSSB.xsd",
				typeof(ATLASVersion10_1.DETSSB),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESSTAProvider),
				typeof(AtlasInboundEDIMessage<IDESSTA>)
			);

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETSSB)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETSSB.xsd",
				typeof(ATLASVersion10_1.DETSSB),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESSTAProvider),
				typeof(AtlasInboundEDIMessage<IDESSTA>)
			);
		}

		public void TestDETPJF()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETPJF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPJF.xsd",
				typeof(ATLASVersion10_1.DETPJF),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPREJProvider),
				typeof(AtlasInboundEDIMessage<IDEPREJ>));

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETPJF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPJF.xsd",
				typeof(ATLASVersion10_1.DETPJF),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPREJProvider),
				typeof(AtlasInboundEDIMessage<IDEPREJ>));
		}

		public void TestDETPIA()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETPIA)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPIA.xsd",
				typeof(ATLASVersion10_1.DETPIA),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPINCProvider),
				typeof(AtlasInboundEDIMessage<IDEPINC>));

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETPIA)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPIA.xsd",
				typeof(ATLASVersion10_1.DETPIA),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPINCProvider),
				typeof(AtlasInboundEDIMessage<IDEPINC>));
		}

		public void TestDETPRH()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETPRH)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPRH.xsd",
				typeof(ATLASVersion10_1.DETPRH),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPRELProvider),
				typeof(AtlasInboundEDIMessage<IDEPREL>));

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETPRH)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPRH.xsd",
				typeof(ATLASVersion10_1.DETPRH),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPRELProvider),
				typeof(AtlasInboundEDIMessage<IDEPREL>));
		}

		public void TestDETPSF()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETPSF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPSF.xsd",
				typeof(ATLASVersion10_1.DETPSF),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPSTAProvider),
				typeof(AtlasInboundEDIMessage<IDEPSTA>)
			);

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETPSF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETPSF.xsd",
				typeof(ATLASVersion10_1.DETPSF),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPSTAProvider),
				typeof(AtlasInboundEDIMessage<IDEPSTA>)
			);
		}

		public void TestDETSPC()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETSPC)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETSPC.xsd",
				typeof(ATLASVersion10_1.DETSPC),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESPERProvider),
				typeof(AtlasInboundEDIMessage<IDESPER>)
			);

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETSPC)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETSPC.xsd",
				typeof(ATLASVersion10_1.DETSPC),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESPERProvider),
				typeof(AtlasInboundEDIMessage<IDESPER>)
			);
		}

		public void TestDETSJB()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETSJB)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETSJB.xsd",
				typeof(ATLASVersion10_1.DETSJB),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESREJProvider),
				typeof(AtlasInboundEDIMessage<IDESREJ>)
			);

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETSJB)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETSJB.xsd",
				typeof(ATLASVersion10_1.DETSJB),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESREJProvider),
				typeof(AtlasInboundEDIMessage<IDESREJ>)
			);
		}

		public void TestDETGAE()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages[nameof(ATLASVersion10_1.DETGAE)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETGAE.xsd",
				typeof(ATLASVersion10_1.DETGAE),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.GUAACKProvider),
				typeof(AtlasInboundEDIMessage<IGUAACK>)
			);

			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DETGAE)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NCTS.Incoming.DETGAE.xsd",
				typeof(ATLASVersion10_1.DETGAE),
				typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.GUAACKProvider),
				typeof(AtlasInboundEDIMessage<IGUAACK>)
			);
		}

		public void TestGCRECF()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.GCRECF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Collective.Incoming.GCRECF.xsd",
				typeof(ATLASVersion10_1.GCRECF),
				typeof(Messaging.ATLASVersion10_1.CUSRECProvider),
				typeof(AtlasInboundEDIMessage<ICUSREC>));
		}

		public void TestGCTRAG()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.GCTRAG)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Collective.Incoming.GCTRAG.xsd",
				typeof(ATLASVersion10_1.GCTRAG),
				typeof(Messaging.ATLASVersion10_1.CUSTRAProvider),
				typeof(AtlasInboundEDIMessage<ICUSTRA>));
		}

		public void TestSCCANE()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.SCCANE)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SumA.Incoming.SCCANE.xsd",
				typeof(ATLASVersion10_1.SCCANE),
				typeof(Messaging.ATLASVersion10_1.CUSCANProvider),
				typeof(AtlasInboundEDIMessage<ICUSCAN>));
		}

		public void TestSCFING()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.SCFING)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SumA.Incoming.SCFING.xsd",
				typeof(ATLASVersion10_1.SCFING),
				typeof(Messaging.ATLASVersion10_1.CUSFINProvider),
				typeof(AtlasInboundEDIMessage<ICUSFIN>));
		}

		public void TestSCFSTF()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.SCFSTF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SumA.Incoming.SCFSTF.xsd",
				typeof(ATLASVersion10_1.SCFSTF),
				typeof(Messaging.ATLASVersion10_1.CUSFSTProvider),
				typeof(AtlasInboundEDIMessage<IUnderCustomsControl>));
		}

		public void TestSCSTAB()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.SCSTAB)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SumA.Incoming.SCSTAB.xsd",
				typeof(ATLASVersion10_1.SCSTAB),
				typeof(Messaging.ATLASVersion10_1.CUSSTAProvider),
				typeof(AtlasInboundEDIMessage<ICUSSTA>));
		}

		public void TestSCSTPC()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.SCSTPC)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SumA.Incoming.SCSTPC.xsd",
				typeof(ATLASVersion10_1.SCSTPC),
				typeof(Messaging.ATLASVersion10_1.CUSSTPProvider),
				typeof(AtlasInboundEDIMessage<ICUSSTP>));
		}

		public void TestSCTSTJ()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.SCTSTJ)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SumA.Incoming.SCTSTJ.xsd",
				typeof(ATLASVersion10_1.SCTSTJ),
				typeof(Messaging.ATLASVersion10_1.CUSTSTProvider),
				typeof(AtlasInboundEDIMessage<ICUSTST>));
		}

		public void TestDEIACA()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.DEIACA)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SumA.Incoming.DEIACA.xsd",
				typeof(ATLASVersion10_1.DEIACA),
				typeof(Messaging.ATLASVersion10_1.ENSCTLProvider),
				typeof(AtlasInboundEDIMessage<IENSCTL>));
		}

		public void TestFCREVH()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.FCREVH)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Import.Incoming.FCREVH.xsd",
				typeof(ATLASVersion10_1.FCREVH),
				typeof(Messaging.ATLASVersion10_1.CUSREVProvider),
				typeof(AtlasInboundEDIMessage<ICUSREV>));
		}

		public void TestFFTAXE()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.FFTAXE)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Import.Incoming.FFTAXE.xsd",
				typeof(ATLASVersion10_1.FFTAXE),
				typeof(Messaging.ATLASVersion10_1.FINTAXProvider),
				typeof(AtlasInboundEDIMessage<IFINTAX>));
		}

		public void TestGCRELG()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.GCRELG)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Collective.Incoming.GCRELG.xsd",
				typeof(ATLASVersion10_1.GCRELG),
				typeof(Messaging.ATLASVersion10_1.CURRELProvider),
				typeof(AtlasInboundEDIMessage<ICURREL>));
		}

		public void TestLECWIF()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.LECWIF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Import.Incoming.LECWIF.xsd",
				typeof(ATLASVersion10_1.LECWIF),
				typeof(Messaging.ATLASVersion10_1.ECWINFProvider),
				typeof(AtlasInboundEDIMessage<IECWINF>));
		}

		public void TestGCTAXM()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.GCTAXM)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Collective.Incoming.GCTAXM.xsd",
				typeof(ATLASVersion10_1.GCTAXM),
				typeof(Messaging.ATLASVersion10_1.CUSTAXProvider),
				typeof(AtlasInboundEDIMessage<ICUSTAX>));
		}

		public void TestGNTAXK()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.GNTAXK)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Collective.Incoming.GNTAXK.xsd",
				typeof(ATLASVersion10_1.GNTAXK),
				typeof(Messaging.ATLASVersion10_1.NFFTAXProvider),
				typeof(AtlasInboundEDIMessage<INFFTAX>));
		}

		public void TestNSREVC()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.NSREVC)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Import.Incoming.NSREVC.xsd",
				typeof(ATLASVersion10_1.NSREVC),
				typeof(Messaging.ATLASVersion10_1.SRAREVProvider),
				typeof(AtlasInboundEDIMessage<ISRAREV>));
		}

		public void TestNSTAXK()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.NSTAXK)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Import.Incoming.NSTAXK.xsd",
				typeof(ATLASVersion10_1.NSTAXK),
				typeof(Messaging.ATLASVersion10_1.SRATAXProvider),
				typeof(AtlasInboundEDIMessage<ISRATAX>));
		}

		public void TestLSCWIF()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.LSCWIF)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Import.Incoming.LSCWIF.xsd",
				typeof(ATLASVersion10_1.LSCWIF),
				typeof(Messaging.ATLASVersion10_1.SCWINFProvider),
				typeof(AtlasInboundEDIMessage<ISCWINF>));
		}

		public void TestLCWSIE()
		{
			TestHelper.AssertResponseDetails(ATLASResponseMessageDetails.Instance.ResponseMessages[nameof(ATLASVersion10_1.LCWSIE)],
				"CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.Import.Incoming.LCWSIE.xsd",
				typeof(ATLASVersion10_1.LCWSIE),
				typeof(Messaging.ATLASVersion10_1.CWSINFProvider),
				typeof(AtlasInboundEDIMessage<ICWSINF>));
		}
	}
}
