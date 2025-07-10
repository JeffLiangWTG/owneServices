using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DepotCusOutturnHeaderOutturnReportHeaderInformationTest : TestCaseWithFactory
	{
		public void TestResponsiblePartyID()
		{
			const string ABN = "36103224237";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ABN;
			AssertEquals("comes from currenty company abn", ABN, HeaderInfo.ResponsiblePartyID);
		}

		public void TestEstablishmentID()
		{
			Header.C6_OutturningPremiseID = "54123";
			AssertEquals("comes from outturningpremiseID", "54123", HeaderInfo.EstablishmentID);
		}

		public void TestVoyageNumber()
		{
			Header.C6_VoyageNum = "66112";
			AssertEquals("comes from voyagenum", "66112", HeaderInfo.VoyageNumber);
		}

		public void TestVesselID()
		{
			Header.C6_LloydsIMO = "7891";
			AssertEquals("comes from lloydsimo", "7891", HeaderInfo.VesselID);
		}

		public void TestLines()
		{
			var outturn = Header.Outturns.AddNew();
			outturn.C5_MasterBill = "foo";
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			outturn = Header.Outturns.AddNew();
			outturn.C5_MasterBill = "bar";
			int linesGenerated = 0;
			foreach (var line in HeaderInfo.Lines)
			{
				linesGenerated++;
				AssertEquals("that line is our outturn", "foo", line.OceanBillOfLading);
			}

			AssertEquals("one line found", 1, linesGenerated);
		}

		DepotCusOutturnHeaderOutturnReportHeaderInformation headerInfo;
		DepotCusOutturnHeaderOutturnReportHeaderInformation HeaderInfo => headerInfo ?? (headerInfo = new DepotCusOutturnHeaderOutturnReportHeaderInformation(Header));

		CusOutturnHeader header;
		CusOutturnHeader Header => header ?? (header = CusOutturnHeader.New(Factory));
	}
}
