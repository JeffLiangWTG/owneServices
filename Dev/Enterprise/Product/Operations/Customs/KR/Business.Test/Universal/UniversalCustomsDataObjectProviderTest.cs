using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class UniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestTableSpecificCusCodeDataTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var list = provider.TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(CusCodeDataTypeList.Codes.VehicleNumber));
		}

		public void TestTableSpecificCusSupportingInfoTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var list = provider.TableSpecificCusSupportingInfoTypeList(JobComInvoiceHeaderSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.CertificateOfOrigin));

			list = provider.TableSpecificCusSupportingInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.CertificateOfOrigin));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.GAApproval));
			Assert(list.ContainsCode(CusSupportingInfoTypeList.Codes.PreApproval));
		}

		[TestDate(2023, 8, 25)]
		public void TestMappings()
		{
			using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
			{
				var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
				helper.CreateNewOrGetExistingCusCodeType(Messaging.Constants.ZZ.NKCodeType.OGARegulationCategory, "KR OGA Regulation Category");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
				var cusCode = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.OGARegulationCategory, "69", "마약류 관리에 관한 법률", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				Factory.SaveForTesting();

				var declaration = Factory.New<JobDeclaration>();
				declaration.Invoices.AddNew();
				declaration.InvoiceLines.AddNew();
				KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var invoice = declaration.Invoices[0];
				invoice.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
				invoice.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes.A;

				var invoiceLine = declaration.InvoiceLines[0];

				var vehicleNumber = invoiceLine.VehicleNumbers.AddNew();
				vehicleNumber.CY_Order = 1;
				vehicleNumber.CY_Data = "12345678909876543";

				invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.N;
				invoiceLine.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes.B;

				var gaApproval = invoiceLine.GAApprovalDataCollection.AddNew();
				gaApproval.CSI_Procedure = cusCode.ZZD_Code;
				gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;
				gaApproval.CSI_Code = RequirementDocumentTypeCodeList.Codes.A;
				gaApproval.CSI_ReferenceNumber = "AAA";
				gaApproval.CSI_DateOfIssue = ZDateTime.Today;
				gaApproval.CSI_ReferenceNumber2 = "BBB";
				gaApproval.CSI_Description = "APPROVAL DOCUMENT DESCRIPTION";
				gaApproval.CSI_AdditionalDescription = "APPROVAL DOCUMENT ADDITIONALDESCRIPTION";

				var preApproval = invoiceLine.PreApprovalCollection.AddNew();
				preApproval.CSI_ReferenceNumber = "철강수출번호1";
				preApproval.CSI_DateOfIssue = ZDateTime.Today.AddDays(1);
				preApproval.CSI_DateOfExpiry = ZDateTime.Today.AddDays(2);
				Factory.SaveForTesting();

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var jobData = writer.GetDataObject(declaration);

				var invoiceData = jobData.CommercialInfo.CommercialInvoiceCollection.Single();
				CombineAssertions(() =>
				{
					var invoiceCOOData = invoiceData.CustomsSupportingInformationCollection.Where(x => x.Category.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.CertificateOfOrigin).ToArray();
					AssertEquals(1, invoiceCOOData.Length);
					AssertEquals("Certificate Of Origin", invoiceCOOData[0].Category.Description);
					AssertEquals(CertificateOfOriginIssuedCodeList.Codes.Y, invoiceCOOData[0].Type.Code);
					AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.A, invoiceCOOData[0].SubType.Code);

					AssertEquals(1, invoiceData.CommercialInvoiceLineCollection.Count);
					var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];

					var vehicleNumberData = invoiceLineData.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == CusCodeDataTypeList.Codes.VehicleNumber).ToArray();
					AssertEquals(1, vehicleNumberData.Length);
					AssertEquals("Vehicle Number", vehicleNumberData[0].Type.Description);
					AssertEquals(1, vehicleNumberData[0].Order);
					AssertEquals("12345678909876543", vehicleNumberData[0].Reference);

					var invoiceLineCOOData = invoiceLineData.CustomsSupportingInformationCollection.Where(x => x.Category.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.CertificateOfOrigin).ToArray();
					AssertEquals(1, invoiceLineCOOData.Length);
					AssertEquals("Certificate Of Origin", invoiceLineCOOData[0].Category.Description);
					AssertEquals(CertificateOfOriginIssuedCodeList.Codes.N, invoiceLineCOOData[0].Type.Code);
					AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.B, invoiceLineCOOData[0].SubType.Code);

					var gaApprovalData = invoiceLineData.CustomsSupportingInformationCollection.Where(x => x.Category.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.GAApproval).ToArray();
					AssertEquals(1, gaApprovalData.Length);
					AssertEquals(cusCode.ZZD_Code, gaApprovalData[0].Procedure.Code);
					AssertEquals(cusCode.ZZD_Description, gaApprovalData[0].Procedure.Description);
					AssertEquals(RequirementTypeCodeList.Codes._1, gaApprovalData[0].SubType.Code);
					AssertEquals(RequirementTypeCodeList.Descriptions._1, gaApprovalData[0].SubType.Description);
					AssertEquals(RequirementDocumentTypeCodeList.Codes.A, gaApprovalData[0].Type.Code);
					AssertEquals(RequirementDocumentTypeCodeList.Descriptions.A, gaApprovalData[0].Type.Description);
					AssertEquals("AAA", gaApprovalData[0].ReferenceNumber);
					AssertEquals(ZDateTime.Today, gaApprovalData[0].DateOfIssue);
					AssertEquals("BBB", gaApprovalData[0].ReferenceNumberCollection[0].ReferenceNumber.Value);
					AssertEquals("APPROVAL DOCUMENT DESCRIPTION", gaApprovalData[0].Description);
					AssertEquals("APPROVAL DOCUMENT ADDITIONALDESCRIPTION", gaApprovalData[0].AdditionalDescription);

					var preApprovalData = invoiceLineData.CustomsSupportingInformationCollection.Where(x => x.Category.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.PreApproval).ToArray();
					AssertEquals(1, preApprovalData.Length);
					AssertEquals("PreApproval Details", preApprovalData[0].Category.Description);
					AssertEquals("철강수출번호1", preApprovalData[0].ReferenceNumber);
					AssertEquals(ZDateTime.Today.AddDays(1), preApprovalData[0].DateOfIssue);
					AssertEquals(ZDateTime.Today.AddDays(2), preApprovalData[0].DateOfExpiry);
				});
			}
		}
	}

	[CodeAlive("Used in UniversalCustomsDataObjectProviderTest")]
	class DeclarationDataObjectWriterForTest : DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
		{
		}
	}
}
