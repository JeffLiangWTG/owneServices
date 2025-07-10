using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AUAddInfoHeaderValidationTest : AUAddInfoValidationTest
	{
		public void TestCheckZA_DMP()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DMP=1";
			AssertEquals("DMP cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DMPInfo.HasMessageErrors());
		}

		public void TestCheckZA_DRE()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DRE=1";
			AssertEquals("DRE cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DREInfo.HasMessageErrors());
		}

		public void TestCheckZA_DSN()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DSN=AA";
			AssertEquals("DSN cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DSNInfo.HasMessageErrors());
		}

		public void TestCheckZA_DXP()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DXP=AA";
			AssertEquals("DXP cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DXPInfo.HasMessageErrors());
		}

		public void TestCheckZA_DTY()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DTY=1";
			AssertEquals("DTY cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DTYInfo.HasMessageErrors());
		}

		public void TestCheckZA_ICN()
		{
			invoiceHeader.AddInfo.AddInfoLine = "ICN=AA";
			AssertEquals("ICN cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_ICNInfo.HasMessageErrors());
		}

		public void TestCheckZA_ADJ()
		{
			invoiceHeader.AddInfo.AddInfoLine = "ADJ=AA";
			AssertEquals("ADJ cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_ADJInfo.HasMessageErrors());
		}

		public void TestCheckZA_LCT()
		{
			invoiceHeader.AddInfo.AddInfoLine = "LCT=1";
			AssertEquals("LCT cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_LCTInfo.HasMessageErrors());
		}

		public void TestCheckZA_ISS()
		{
			invoiceHeader.AddInfo.AddInfoLine = "ISS=1";
			AssertEquals("ISS cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_ISSInfo.HasMessageErrors());
		}

		public void TestCheckZA_LCTQ()
		{
			invoiceHeader.AddInfo.AddInfoLine = "LCTQ=AA";
			AssertEquals("LCTQ cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_LCTQInfo.HasMessageErrors());
		}

		public void TestCheckZA_LCTE()
		{
			invoiceHeader.AddInfo.AddInfoLine = "LCTE=AA";
			AssertEquals("LCTE cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_LCTEInfo.HasMessageErrors());
		}

		public void TestCheckZA_MLP()
		{
			invoiceHeader.AddInfo.AddInfoLine = "MLP=AA";
			AssertEquals("MLP cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_MLPInfo.HasMessageErrors());
		}

		public void TestCheckZA_ODF()
		{
			invoiceHeader.AddInfo.AddInfoLine = "ODF=1";
			AssertEquals("ODF cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_ODFInfo.HasMessageErrors());
		}

		public void TestCheckZA_RNO()
		{
			invoiceHeader.AddInfo.AddInfoLine = "RNO=AA";
			AssertEquals("RNO cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_RNOInfo.HasMessageErrors());
		}

		public void TestCheckZA_MD2()
		{
			invoiceHeader.AddInfo.AddInfoLine = "MD2=AA";
			AssertEquals("MD2 cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_MD2Info.HasMessageErrors());
		}

		public void TestCheckZA_TC2()
		{
			invoiceHeader.AddInfo.AddInfoLine = "TC2=AA";
			AssertEquals("TC2 cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_TC2Info.HasMessageErrors());
		}

		public void TestCheckZA_QT2()
		{
			invoiceHeader.AddInfo.AddInfoLine = "QT2=1";
			AssertEquals("QT2 cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_QT2Info.HasMessageErrors());
		}

		public void TestCheckZA_UQ2()
		{
			invoiceHeader.AddInfo.AddInfoLine = "UQ2=AA";
			AssertEquals("UQ2 cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_UQ2Info.HasMessageErrors());
		}

		public void TestCheckZA_STD()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DCX=AA";
			AssertEquals("DCX cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DCXInfo.HasMessageErrors());
		}

		public void TestCheckZA_TAN()
		{
			invoiceHeader.AddInfo.AddInfoLine = "TAN=AA";
			AssertEquals("TAN cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_TANInfo.HasMessageErrors());
		}

		public void TestCheckZA_TFQ()
		{
			invoiceHeader.AddInfo.AddInfoLine = "TFQ=AA";
			AssertEquals("TFQ cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_TFQInfo.HasMessageErrors());
		}

		public void TestCheckZA_WRQ()
		{
			invoiceHeader.AddInfo.AddInfoLine = "WRQ=1";
			AssertEquals("WRQ cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_WRQInfo.HasMessageErrors());
		}

		public void TestCheckZA_WRU()
		{
			invoiceHeader.AddInfo.AddInfoLine = "WRU=AA";
			AssertEquals("WRU cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_WRUInfo.HasMessageErrors());
		}

		public void TestCheckZA_WUV()
		{
			invoiceHeader.AddInfo.AddInfoLine = "WUV=1";
			AssertEquals("WUV cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_WUVInfo.HasMessageErrors());
		}

		public void TestCheckZA_WET()
		{
			invoiceHeader.AddInfo.AddInfoLine = "WET=1";
			AssertEquals("WET cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_WETInfo.HasMessageErrors());
		}

		public void TestCheckZA_WETQ()
		{
			invoiceHeader.AddInfo.AddInfoLine = "WETQ=AA";
			AssertEquals("WETQ cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_WETQInfo.HasMessageErrors());
		}

		public void TestCheckZA_WETE()
		{
			invoiceHeader.AddInfo.AddInfoLine = "WETE=AA";
			AssertEquals("WETE cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_WETEInfo.HasMessageErrors());
		}

		protected JobComInvoiceHeader invoiceHeader;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}
	}
}
