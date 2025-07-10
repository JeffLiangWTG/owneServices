using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMessageDecoderContainerTest : TestCaseWithFactory
	{
		public void TestProcessEQD()
		{
			testEXDOCMessageDecoderContainer.Process();
			Assert("Container Number is empty", testEXDOCMessageDecoderContainer.containerNumber.IsEmpty);
			EXDOCMessageUtilities.PopulateEQD(group16.EQD.InstantiateAChildAndAddItToChildrenCollection(), EquipmentQualifierList.Container, "SECU122143");
			testEXDOCMessageDecoderContainer.Process();
			AssertEquals("Container Number", "SECU122143", testEXDOCMessageDecoderContainer.containerNumber);
		}

		public void TestProcessMEA()
		{
			testEXDOCMessageDecoderContainer.Process();
			Assert("IMA1 Net Weight is empty", testEXDOCMessageDecoderContainer.iMA1NetWeight.IsEmpty);
			Assert("IMA1 Gross Weight is empty", testEXDOCMessageDecoderContainer.iMA1GrossWeight.IsEmpty);
			EXDOCMessageUtilities.PopulateMEA(group16.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.Weights, PropertyMeasuredCodedList.TotalNetWeight, "234.345");
			EXDOCMessageUtilities.PopulateMEA(group16.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementPurposeQualifierList.Weights, PropertyMeasuredCodedList.ItemGrossWeight, "4353.34");
			testEXDOCMessageDecoderContainer.Process();
			AssertEquals("IMA1 Net Weight", new ZDecimal(234.345), testEXDOCMessageDecoderContainer.iMA1NetWeight);
			AssertEquals("IMA1 Gross Weight", new ZDecimal(4353.34), testEXDOCMessageDecoderContainer.iMA1GrossWeight);
		}

		public void TestProcessIMD()
		{
			testEXDOCMessageDecoderContainer.Process();
			Assert("IMA1 Product Description is empty", testEXDOCMessageDecoderContainer.iMA1ProductDescription.IsEmpty);
			EXDOCMessageUtilities.PopulateIMD(group16.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitDairyLineMessageBuilder.IMA1ProductDescription, "This is a test description for IMA1 product description from the line.");
			testEXDOCMessageDecoderContainer.Process();
			AssertEquals("IMA1 Product Description", "This is a test description for IMA1 product description from the line.", testEXDOCMessageDecoderContainer.iMA1ProductDescription);
		}

		public void TestProcessGIN()
		{
			testEXDOCMessageDecoderContainer.Process();
			Assert("IMA1 Serial Number is empty", testEXDOCMessageDecoderContainer.iMA1SerialNumber.IsEmpty);
			Assert("IMA1 Invoice Number is empty", testEXDOCMessageDecoderContainer.iMA1SerialNumber.IsEmpty);
			EXDOCMessageUtilities.PopulateGIN(group16.GIN.InstantiateAChildAndAddItToChildrenCollection(), IdentityNumberQualifierList.SerialNumber, "1234HJK");
			EXDOCMessageUtilities.PopulateGIN(group16.GIN.InstantiateAChildAndAddItToChildrenCollection(), IdentityNumberQualifierList.InvoiceLineNumber, "98432");
			testEXDOCMessageDecoderContainer.Process();
			AssertEquals("IMA1 Serial Number", "1234HJK", testEXDOCMessageDecoderContainer.iMA1SerialNumber);
			AssertEquals("IMA1 Invoice Number", "98432", testEXDOCMessageDecoderContainer.iMA1InvoiceNumber);
		}

		public void TestProcessDTM()
		{
			testEXDOCMessageDecoderContainer.Process();
			Assert("IMA1 Invoice Date is empty", testEXDOCMessageDecoderContainer.iMA1InvoiceDate.IsEmpty);
			EXDOCMessageUtilities.PopulateDTM(group16.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.InvoiceDateTime, "20061214", DateTimePeriodFormatQualifierList.Ccyymmdd);
			testEXDOCMessageDecoderContainer.Process();
			AssertEquals("IMA1 Invoice Date", new ZDateTime(2006, 12, 14), testEXDOCMessageDecoderContainer.iMA1InvoiceDate);
		}

		public void TestProcessFTX()
		{
			testEXDOCMessageDecoderContainer.Process();
			Assert("IMA1 Quota Year is empty", testEXDOCMessageDecoderContainer.iMA1QuotaYear.IsEmpty);
			EXDOCMessageUtilities.PopulateFTX(group16, TextSubjectQualifierList.GeneralInformation, 15, 15, "December 2006");
			testEXDOCMessageDecoderContainer.Process();
			AssertEquals("IMA1 Quota Year", "December 2006", testEXDOCMessageDecoderContainer.iMA1QuotaYear);
		}

		public void TestProcessSEL()
		{
			testEXDOCMessageDecoderContainer.Process();
			Assert("Container Seal is empty", testEXDOCMessageDecoderContainer.containerSeal.IsEmpty);
			SegmentGroup17 group17 = group16.Group17.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateSEL(group17.SEL.InstantiateAChildAndAddItToChildrenCollection(), "34587", "", "");
			testEXDOCMessageDecoderContainer.Process();
			AssertEquals("Container Seal", "34587", testEXDOCMessageDecoderContainer.containerSeal);
		}

		protected override void SetUp()
		{
			base.SetUp();
			group16 = new SegmentGroup16();
			testEXDOCMessageDecoderContainer = new EXDOCMessageDecoderContainer(group16);
		}

		SegmentGroup16 group16;
		EXDOCMessageDecoderContainer testEXDOCMessageDecoderContainer;
	}
}
