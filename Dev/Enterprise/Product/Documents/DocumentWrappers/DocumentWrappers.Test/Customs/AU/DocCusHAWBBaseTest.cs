using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusHAWBBase))]
	sealed class DocCusHAWBBaseTest : DocumentWrapperTestCase
	{
		public void TestGoodsCurrency()
		{
			var hAWB = Factory.New<CusHAWB>();
			hAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;

			var doc = DocCusHAWBBase.New(hAWB, Factory);
			AssertEquals("GoodsCurrency should be correct", Core.Constants.CurrencyCodes.Australia, doc.GoodsCurrency.Code);
		}

		public void TestServiceLevel()
		{
			var testServiceLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery());
			var hAWB = Factory.New<CusHAWB>();
			hAWB.CS_RS_NK_ServiceLevel = testServiceLevel.RS_Code;

			var doc = DocCusHAWBBase.New(hAWB, Factory);
			AssertEquals("ServiceLevel should be correct", testServiceLevel.RS_Code, doc.ServiceLevel.Code);
		}

		public void TestFreightPrepaidCollectDescription()
		{
			var testServiceLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery());

			CusHAWB.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertEquals("Legacy Prepaid", "Prepaid", DocCusHAWB.FreightPrepaidCollectDescription);
			CusHAWB.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertEquals("Legacy Collect", "Collect", DocCusHAWB.FreightPrepaidCollectDescription);

			CusHAWB.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.PrepaidOnly;
			AssertEquals("CMR PrepaidOnly", "Prepaid only", DocCusHAWB.FreightPrepaidCollectDescription);
			CusHAWB.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.Collect;
			AssertEquals("CMR Collect", "Collect", DocCusHAWB.FreightPrepaidCollectDescription);
			CusHAWB.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.CustomerPickUpBackhaul;
			AssertEquals("CMR CustomerPickUpBackhaul", "Customer pick-up/backhaul", DocCusHAWB.FreightPrepaidCollectDescription);
		}

		public void TestWeightInKG()
		{
			CusHAWB.CS_Weight = 1;
			AssertEquals("Kilograms should be assumed if no Weight UQ is entered", 1m, DocCusHAWB.WeightInKG);

			CusHAWB.CS_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("Weight should be converted to ", 0.001m, DocCusHAWB.WeightInKG);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var result = DocCusHAWBBase.New(CusHAWB, Factory);
			return new DocumentWrapper[] { result };
		}

		DocCusHAWBBase DocCusHAWB
		{
			get
			{
				if (fDocCusHAWB == null)
				{
					fDocCusHAWB = DocCusHAWBBase.New(CusHAWB, Factory);
				}
				return fDocCusHAWB;
			}
		}
		DocCusHAWBBase fDocCusHAWB;

		CusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<CusHAWB>();
				}
				return fCusHAWB;
			}
		}

		CusHAWB fCusHAWB;

		#endregion
	}
}
