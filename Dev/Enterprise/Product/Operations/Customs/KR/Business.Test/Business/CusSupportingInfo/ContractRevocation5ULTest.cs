using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ContractRevocation5UL))]
	sealed class ContractRevocation5ULTest : CusSupportingInfoTest<ContractRevocation5UL>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return SetCusReconEntryLine(Factory).ContractRevocations.AddNew();
		}

		protected override IEnumerable<ContractRevocation5UL> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return SetCusReconEntryLine(factory).ContractRevocations.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (ContractRevocation5UL)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.ContractRevocation5UL;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<ContractRevocation5UL>();
			AssertEquals(typeof(ContractRevocation5ULValidation), supportingInfo.Validation.GetType());
		}

		public void TestParent()
		{
			var cusReconEntryLine = SetCusReconEntryLine(Factory);
			var supportingInfo = cusReconEntryLine.ContractRevocations.AddNew();
			AssertEquals(cusReconEntryLine, supportingInfo.Parent);
		}

		public void TestFormattedLineNo()
		{
			var contractRevocation5UL = Factory.New<ContractRevocation5UL>();
			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.B;
			contractRevocation5UL.CSI_LineNo = 1;
			AssertEquals(ZString.Empty, contractRevocation5UL.FormattedLineNo);
			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.A;
			AssertEquals("001", contractRevocation5UL.FormattedLineNo);
		}

		public void TestReadonly()
		{
			var cusReconEntryLine = SetCusReconEntryLine(Factory);
			cusReconEntryLine.Header.ReconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.B;
			var contractRevocation5UL = cusReconEntryLine.ContractRevocations.AddNew();

			AssertEquals(false, contractRevocation5UL.CSI_CodeInfo.ReadOnly);
			AssertEquals(false, contractRevocation5UL.CSI_DateOfExpiryInfo.ReadOnly);
			AssertEquals(false, contractRevocation5UL.CSI_AdditionalDescriptionInfo.ReadOnly);
			AssertEquals(false, contractRevocation5UL.CSI_ReferenceNumber2Info.ReadOnly);
			AssertEquals(false, contractRevocation5UL.CSI_DescriptionInfo.ReadOnly);

			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.A;
			AssertEquals(false, contractRevocation5UL.ExportEntryNumberInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.DisposalNumberInfo.ReadOnly);
			AssertEquals(false, contractRevocation5UL.FormattedLineNoInfo.ReadOnly);

			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.B;
			AssertEquals(true, contractRevocation5UL.ExportEntryNumberInfo.ReadOnly);
			AssertEquals(false, contractRevocation5UL.DisposalNumberInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.FormattedLineNoInfo.ReadOnly);

			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.C;
			AssertEquals(true, contractRevocation5UL.ExportEntryNumberInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.DisposalNumberInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.FormattedLineNoInfo.ReadOnly);

			cusReconEntryLine.Header.ReconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.A;
			AssertEquals(true, contractRevocation5UL.ExportEntryNumberInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.DisposalNumberInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.FormattedLineNoInfo.ReadOnly);

			AssertEquals(true, contractRevocation5UL.CSI_CodeInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.CSI_DateOfExpiryInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.CSI_AdditionalDescriptionInfo.ReadOnly);
			AssertEquals(true, contractRevocation5UL.CSI_ReferenceNumber2Info.ReadOnly);
			AssertEquals(true, contractRevocation5UL.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestCaption()
		{
			var contractRevocation5UL = Factory.New<ContractRevocation5UL>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "CSI_Code", true, attrib => attrib.Caption == "Cancel Reason");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "ExportEntryNumber", true, attrib => attrib.Caption == "Export Entry Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "DisposalNumber", true, attrib => attrib.Caption == "Disposal Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "FormattedLineNo", true, attrib => attrib.ShortCaption == "EXP Entry Line No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "FormattedLineNo", true, attrib => attrib.Caption == "Export Entry Line Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "CSI_DateOfExpiry", true, attrib => attrib.Caption == "Disposal Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "CSI_AdditionalDescription", true, attrib => attrib.ShortCaption == "Goods Location Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "CSI_AdditionalDescription", true, attrib => attrib.Caption == "Goods Location Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "CSI_ReferenceNumber2", true, attrib => attrib.ShortCaption == "Residual Substance Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "CSI_ReferenceNumber2", true, attrib => attrib.Caption == "Residual Substance Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(contractRevocation5UL.GetType(), "CSI_Description", true, attrib => attrib.Caption == "Damage Situation");
		}

		public void TestMaxLength()
		{
			var contractRevocation = Factory.New<ContractRevocation5UL>();
			AssertEquals(1, contractRevocation.CSI_CodeInfo.MaxLength);
			AssertEquals(100, contractRevocation.CSI_AdditionalDescriptionInfo.MaxLength);
			AssertEquals(65, contractRevocation.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals(500, contractRevocation.CSI_DescriptionInfo.MaxLength);
			AssertEquals(20, contractRevocation.DisposalNumberInfo.MaxLength);
			AssertEquals(15, contractRevocation.ExportEntryNumberInfo.MaxLength);
			AssertEquals(100, contractRevocation.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals(3, contractRevocation.FormattedLineNoInfo.MaxLength);
		}

		public void TestCSI_Code()
		{
			var contractRevocation5UL = Factory.New<ContractRevocation5UL>();
			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.B;
			contractRevocation5UL.CSI_DateOfExpiry = ZDateTime.Today;
			contractRevocation5UL.CSI_AdditionalDescription = "폐기멸실변질손상 환급시장치장소";
			contractRevocation5UL.CSI_ReferenceNumber2 = "폐기시 잔존물의 품명,규격,수량";
			contractRevocation5UL.CSI_Description = "멸실변질손상시 피해상황 및 기타 참고사항";
			contractRevocation5UL.CSI_LineNo = 1;
			AssertProperties();

			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.C;
			AssertProperties();

			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.D;
			AssertProperties();

			contractRevocation5UL.CSI_Code = CancelReasonCodeList.Codes.A;
			AssertEquals(ZDateTime.Empty, contractRevocation5UL.CSI_DateOfExpiry);
			AssertEquals(ZString.Empty, contractRevocation5UL.CSI_AdditionalDescription);
			AssertEquals(ZString.Empty, contractRevocation5UL.CSI_ReferenceNumber2);
			AssertEquals(ZString.Empty, contractRevocation5UL.CSI_Description);
			AssertEquals(ZInt.Zero, contractRevocation5UL.CSI_LineNo);

			void AssertProperties()
			{
				AssertEquals(ZDateTime.Today, contractRevocation5UL.CSI_DateOfExpiry);
				AssertEquals("폐기멸실변질손상 환급시장치장소", contractRevocation5UL.CSI_AdditionalDescription);
				AssertEquals("폐기시 잔존물의 품명,규격,수량", contractRevocation5UL.CSI_ReferenceNumber2);
				AssertEquals("멸실변질손상시 피해상황 및 기타 참고사항", contractRevocation5UL.CSI_Description);
			}
		}

		public void TestExportEntryNumberAndDisposalNumber()
		{
			var contractRevocation5UL = Factory.New<ContractRevocation5UL>();
			contractRevocation5UL.CSI_Code = "A";
			contractRevocation5UL.CSI_ReferenceNumber = "123456789012345";
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(contractRevocation5UL.GetType(), "ExportEntryNumber", true, attrib => attrib.Member == "IsReExportReadOnly");
			AssertEquals("12345-67-89012345", contractRevocation5UL.ExportEntryNumber);
			Assert(contractRevocation5UL.DisposalNumber.IsEmpty);

			contractRevocation5UL.CSI_Code = "B";
			contractRevocation5UL.CSI_ReferenceNumber = "12345678901234567890";
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(contractRevocation5UL.GetType(), "DisposalNumber", true, attrib => attrib.Member == "IsDisposalReadOnly");
			Assert(contractRevocation5UL.ExportEntryNumber.IsEmpty);
			AssertEquals("12345678901234567890", contractRevocation5UL.DisposalNumber);

			contractRevocation5UL.CSI_Code = "C";
			Assert(contractRevocation5UL.ExportEntryNumber.IsEmpty);
			Assert(contractRevocation5UL.DisposalNumber.IsEmpty);
		}

		CusReconEntryLine SetCusReconEntryLine(BusinessObjectFactory factory)
		{
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "KR1", "TestCompany");
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			factory.Save();

			var cusReconDeclaration = factory.NewWithValidTestData<CusReconDeclaration>();
			cusReconDeclaration.CusReconEntryLines.AddNew();

			var reconEntry = cusReconDeclaration.CusReconEntries[0];
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			var cusReconEntryLine = cusReconDeclaration.CusReconEntryLines[0];
			cusReconEntryLine.CRL_OriginalEntryLineNumber = 1;
			factory.Save();

			return cusReconEntryLine;
		}
	}
}
