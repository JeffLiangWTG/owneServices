using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SACMessageLineTest : BaseMessageLineAbstractTest
	{
		public override void TestPopulate()
		{
			invoiceLine.JI_Description = "TEST DESCRIPTION";
			invoiceLine.JI_Tariff = "2003.30.40 05";
			invoiceLine.AddInfo.ZA_GSTE = "GSTE";
			invoiceLine.AddInfo.ZA_WETE = "WETE";
			invoiceLine.AddInfo.ZA_WETQ = "Y";
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 123M;
			invoiceLine.AddInfo.ZA_QT2 = 543M;
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			invoiceLine.AddInfo.ZA_WAR = "9515C";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERMIT3";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERMIT2";
			invoiceLine.ICSPermits.AddNew().CY_Data = "PERMIT1";
			testDec.DoMerge();
			testDec.CustomsEntryHeaders[0].MergedLines[0].CL_CustomsValue = 123.45M;

			var sACMessageLineToTest = new SACMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[0], new SegmentGroup30());
			sACMessageLineToTest.Populate(1, LineAction.Insert);
			var result = sACMessageLineToTest.Group30.ToString(new UNOCCMRCharacterSet());

			const string expectedResult =
@"CST+1+I::95'
FTX+AAA+++TEST DESCRIPTION'
MEA+AAA++NO:123.00000'
MEA+AAA++LA:543.00000'
MOA+40:123.45:AUD'
RFF+ABD:20033040'
RFF+AED:05'
RFF+ASA:GSTE'
RFF+DA:WETE'
RFF+AHW:PERMIT1'
RFF+AHW:PERMIT2'
RFF+AHW:PERMIT3'
GIS+WET:109:95'";

			AssertMultilineEquals("Should be no differences", expectedResult.Replace("\r\n", ""), result, '\'');
		}

		public override void TestSegmentGroup33()
		{
			entryLine.CL_CustomsValue = 123.45M;
			SACMessageLineToTest.PopulateGroup33();
			var result = Group30String;
			AssertEquals("Customs Value", true, result.Contains("MOA+40"));
		}

		protected override BaseMessageLine GetMessageLineToTest => new SACMessageLine(testDec.CustomsEntryHeaders[0].MergedLines[0], new SegmentGroup30());

		protected override ZString Group30String => SACMessageLineToTest.Group30.ToString(new UNOCCMRCharacterSet());

		SACMessageLine sacMessageLineToTest;
		SACMessageLine SACMessageLineToTest
		{
			get
			{
				if (sacMessageLineToTest == null)
				{
					sacMessageLineToTest = (SACMessageLine)messageLineToTest;
					sacMessageLineToTest.Group30 = new SegmentGroup30();
				}

				return sacMessageLineToTest;
			}
		}
	}
}
