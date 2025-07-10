using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RFPNumberWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertRFPNumber(0, new[] { 0, 1 });
			AssertRFPNumber(1, new[] { 0, 1 });
			AssertRFPNumber(2, new[] { 2 });
		}

		internal static int GetCount(IEnumerable lines)
		{
			var count = 0;
			foreach (var line in lines)
			{
				count++;
			}
			return count;
		}

		internal static void AddRFPNumber(JobComInvoiceLine invLine, ZString rfpNum, int rfpLine)
		{
			var rfpNumber = invLine.RFPNumbers.AddNew();
			rfpNumber.ZA_RFPNumber = rfpNum;
			rfpNumber.ZA_RFPLine = rfpLine;
			rfpNumber.ZA_RFPNetQuantity = 10m;
			rfpNumber.ZA_RFPQtyUM = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			rfpNumber.ZA_RFPPackCount = 10;
			rfpNumber.ZA_RFPPackType = EXDOCPacakgeTypeCodes.Codes.Drums;
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			AddRFPNumber(invoiceLine, "RFP1234", 1);
			AddRFPNumber(invoiceLine, "RFP1234", 2);
			AddRFPNumber(invoiceLine, "RFP4321", 1);
		}

		JobComInvoiceLine invoiceLine;

		void AssertRFPNumber(int num, int[] expectedLineNums)
		{
			var coutainers = new[] { new RFPContainerForTesting { ContainerNumber = "12" } };
			var wrapper = new RFPNumberWrapper(invoiceLine.RFPNumbers[num], invoiceLine.RFPNumbers, coutainers);
			AssertEquals("RFPNumber", invoiceLine.RFPNumbers[num].ZA_RFPNumber, wrapper.RFPNumber);
			AssertEquals("RFPLines count", expectedLineNums.Length, GetCount(wrapper.RFPLines));

			int i = 0;
			foreach (IRFPLine line in wrapper.RFPLines)
			{
				AssertEquals("RFPLineNumber", invoiceLine.RFPNumbers[expectedLineNums[i]].ZA_RFPLine, line.RFPLineNumber);
				AssertEquals("NetLineQuantity", invoiceLine.RFPNumbers[expectedLineNums[i]].ZA_RFPNetQuantity, line.NetLineQuantity);
				AssertEquals("NetQuantityUnit", invoiceLine.RFPNumbers[expectedLineNums[i]].ZA_RFPQtyUM, line.NetQuantityUnit);
				AssertEquals("PackQuantity", invoiceLine.RFPNumbers[expectedLineNums[i]].ZA_RFPPackCount, line.PackQuantity);
				AssertEquals("PackType", invoiceLine.RFPNumbers[expectedLineNums[i]].ZA_RFPPackType, line.PackType);
				AssertEquals("Containers", coutainers, line.Containers);
				i++;
			}
		}
	}
}
