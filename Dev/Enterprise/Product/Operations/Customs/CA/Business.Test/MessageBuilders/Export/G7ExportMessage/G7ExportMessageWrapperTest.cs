using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class G7ExportMessageWrapperTest : TestCaseWithFactory
	{
		public void TestExportLicenceProxy()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = caCompany.Branches.AddNew();
			var companyOrg = Factory.NewWithValidTestData<OrgHeader>();
			companyOrg.OH_Code = "~GCORG";
			caCompany.GC_OH_OrgProxy = companyOrg.PK;
			Factory.Save();

			Declaration.JE_GB = caBranch.PK;
			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1234";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_PartNo = "23232";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			var g7ExportMessageWrapper = new G7ExportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
			AssertNull("No licence set", g7ExportMessageWrapper.ExportLicenceProxy);
			companyOrg.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICENSE", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			AssertEquals("Company proxy applies", companyOrg.PK, g7ExportMessageWrapper.ExportLicenceProxy.PK);
		}

		public void TestPortOfExitWhenEnteredInvalid()
		{
			Declaration.CA_PortOfExit = "5555";
			Declaration.CA_PlaceOfReport = "5555";
			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1234";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_PartNo = "23232";

			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			Factory.Save();

			var g7ExportMessageWrapper = new G7ExportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
			AssertEquals("Invalid selection results in an empty string", ZString.Empty, g7ExportMessageWrapper.PortOfExit);
			AssertEquals("Invalid selection results in an empty string", ZString.Empty, g7ExportMessageWrapper.PlaceOfReport);
		}

		public void TestExporterBusinessNumber()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = caCompany.Branches.AddNew();
			var companyOrg = Factory.NewWithValidTestData<OrgHeader>();
			companyOrg.OH_Code = "~GCORG";
			caCompany.GC_OH_OrgProxy = companyOrg.PK;
			Factory.Save();

			Declaration.JE_GB = caBranch.PK;
			Declaration.JE_OH_Supplier = companyOrg.PK;
			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1234";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_PartNo = "23232";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			var g7ExportMessageWrapper = new G7ExportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
			AssertEquals("No Business Number is set", ZString.Empty, g7ExportMessageWrapper.ExporterBusinessNumber);
			companyOrg.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RM0001", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			AssertEquals("Only BusinessNumberImportExport is set", "123456789RM0001", g7ExportMessageWrapper.ExporterBusinessNumber);
			companyOrg.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForExport, "123456789RM0002", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada));
			AssertEquals("BusinessNumberExport is set", "123456789RM0002", g7ExportMessageWrapper.ExporterBusinessNumber);
		}

		[TestDate(2008, 2, 15, 10, 30, 25)]
		public void TestG7ExportMessageContent()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value))
			{
				CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "98765");
				CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				var uhelper = new UniversalReferenceTestDataHelper(Factory);
				uhelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
				var portOfExit = uhelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0009", "AB-Port Of Exit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				var placeOfReport = uhelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0351", "AB-Place of Report", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

				var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
				brokerLicence.XZ_RefNumber = "21311X";
				brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

				var helper = new DeclarationTestHelper(Factory, false);
				var declaration = Declaration;
				declaration.CA_PortOfExit = portOfExit.ZZD_Code;
				declaration.CA_PlaceOfReport = placeOfReport.ZZD_Code;
				declaration.CA_ReasonForExportCode = "26";
				declaration.CA_TransportDocumentNumber = "9991KZKICARGO-01";
				declaration.JE_OA_SupplierAddress = helper.Consignor.MainAddress.PK;
				declaration.JE_OA_ImporterAddress = helper.Consignee.MainAddress.PK;
				var vendor = declaration.VendorDocAddress;
				vendor.E2_CompanyName = "VENDOR NAME";
				vendor.E2_Address1 = "VENDOR ADDRESS 1";
				vendor.E2_Address2 = "VENDOR ADDRESS 2";
				vendor.E2_City = "VENDOR CITY";
				vendor.E2_Postcode = "VENDOR PC";
				vendor.E2_State = "VENDOR STATE";
				vendor.E2_RN_NKCountryCode = "CA";
				declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
				helper.ShippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "9991");
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_DeclarationReference = "B00123457";
				declaration.JE_VesselName = helper.VesselWithCountry.RV_Code;
				declaration.JE_RL_NKPortOfLoading = helper.CAVAR.Code;
				declaration.JE_RL_NKPortOfArrival = helper.AUSYD.Code;
				declaration.JE_ExportDate = new ZDateTime(2008, 7, 30, 1, 2, 3);
				declaration.JE_RL_NKOrigin = helper.CATOR.Code;
				declaration.JE_RL_NKFinalDestination = helper.NZAKL.Code;
				declaration.JE_TotalWeight = 142.30m;
				declaration.JE_TotalNoOfPacks = 10;
				declaration.JE_TotalNoOfPacksPackType = "PAL";
				declaration.Permits.AddNew("PERMITNUMBER1");
				declaration.Permits.AddNew("PERMITNUMBER2");

				helper.Consignor.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "KC3333");
				var container1 = helper.CreateCusContainer(declaration, "TURE1111111", "SEAL1", helper.Container40US, "FCL");
				container1.CA_RN_NKCountryOfRegistration = "DE";
				helper.CreateCusContainer(declaration, "TURE2222222", "SEAL2", helper.Container20US, "FCL");
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_Weight = 62.30m;
				invoice1.JZ_InvoiceNumber = "INV123456";
				invoice1.JZ_InvoiceDate = new ZDateTime(2007, 10, 23);
				invoice1.JZ_InvoiceAmount = 7000.00m;
				invoice1.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
				var invoice1Charge = invoice1.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 500m);
				invoice1Charge.J7_RX_NKCurrency = "CAD";
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line1.JI_Tariff = "87011010";
				invoice1Line1.JI_LinePrice = 5000m;
				invoice1Line1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				invoice1Line1.JI_CustomsQuantity = 10;
				invoice1Line1.JI_CustomsUnitQty = UnitOfMeasureListForDLM.Codes.Number;
				invoice1Line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
				invoice1Line1.JI_StateOrRegionOfOrigin = CanadianProvinceList.Codes.Alberta;
				invoice1Line1.CA_ConveyanceIdentificationNumber = "VIN1, VIN2 VIN3";
				invoice1Line1.JI_Description = "BIGCARS";
				invoice1Line1.Permits.AddNew("PERMITNUMBER2");
				invoice1Line1.Permits.AddNew("PERMITNUMBER3");

				var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line2.JI_Tariff = "8701101299";
				invoice1Line2.JI_LinePrice = 123.45m;
				invoice1Line2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				invoice1Line2.JI_CustomsQuantity = 2;
				invoice1Line2.JI_CustomsUnitQty = "BOX";
				invoice1Line2.JI_CountryOfOrigin = "AU";
				invoice1Line2.JI_Description = "BLUE THINGS";

				var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line3.JI_Tariff = "4101909000";
				invoice1Line3.JI_InvoiceQuantity = 10;
				invoice1Line3.JI_InvoiceUQ = "BOX";
				invoice1Line3.JI_CustomsUnitQty = ZString.Empty;
				invoice1Line3.JI_LinePrice = 123.45m;
				invoice1Line3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				invoice1Line3.JI_CountryOfOrigin = "AU";
				invoice1Line3.JI_Description = "BLUE THINGS";

				declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				var g7ExportMessageWrapper = new G7ExportMessageWrapper(declaration.CustomsEntryHeaders[0]);
				var builder = new G7ExportMessageBuilder(g7ExportMessageWrapper, MessageSubTypes.Create);
				foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
				{
					var message = (EDIMessage)builderResult.Message;
					AssertMultilineASCIIEquals("G7ExportMessageContent", G7ExportDeclarationMessage, message.EM_FormattedMessageText);
				}

				CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
				g7ExportMessageWrapper = new G7ExportMessageWrapper(declaration.CustomsEntryHeaders[0]);
				AssertEquals("21311X", g7ExportMessageWrapper.BrokerSecurityNumber);

				invoice1.JZ_RW_NKOriginState = ZString.Empty;
				invoice1Line1.JI_StateOrRegionOfOrigin = ZString.Empty;
				var detailLine = g7ExportMessageWrapper.Details.First(x => x.ProductDescription == "BIGCARS");
				AssertEquals("Origin province defaulted from exporter", "QC", detailLine.ProvinceOfOrigin);
				invoice1.JZ_RW_NKOriginState = "ON";
				AssertEquals("Origin province defaulted from invoice header", "ON", detailLine.ProvinceOfOrigin);
				invoice1Line1.JI_StateOrRegionOfOrigin = "AB";
				AssertEquals("Origin province defaulted from invoice line", "AB", detailLine.ProvinceOfOrigin);
			}
		}

		internal const string G7ExportDeclarationMessage = @"UNH+<<MSGNO PLACEHOLDER>>+GSIMEX:D:00A:CC:EX1STP
BGM+914+B00123457+9
LOC+42+0009::96
LOC+172+0351::96
DTM+129:200807300102:203
MEA+WT+AAD+KGM:142
EQD+CN+TURE1111111DE4000::5
EQD+CN+TURE2222222
TDT+11++1++9991::96+++:::VESSEL WITH COUNTRY
RFF+ABT:<<ENTRY NUMBER PLACE HOLDER>>
RFF+AAS:9991KZKICARGO-01
PAC+10++PAL
RFF+AIJ:KC3333
CST++661:117:96
NAD+EX+++CONSIGNOR NAME+STREET 1+CITY+QC+43422+CA
NAD+AG+98765::96
NAD+DP+++CONSIGNEE NAME+STREET 1+CITY+NSW++AU
MOA+39:7000.00:USD
MOA+64:500:CAD
UNS+D
SEQ++1
DMS+INV123456
GEI+3+26
NAD+SE+++VENDOR NAME+VENDOR ADDRESS 1:VENDOR ADDRESS 2+VENDOR CITY+VENDOR ST+VENDOR PC+CA
NAD+CN+++CONSIGNEE NAME+STREET 1+CITY+NSW++AU
LIN+1
LOC+27+CA::5+AB::96
RFF+AKG:VIN1
RFF+AKG:VIN2
RFF+AKG:VIN3
DOC+811::96+PERMITNUMBER1
DOC+811::96+PERMITNUMBER2
DOC+811::96+PERMITNUMBER3
IMD+++:::BIGCARS
GID+1
CST++87011010:169:96
MEA+AAR++NMB:10
MOA+40:5000.00:USD
LIN+2
LOC+27+AU::5+QC::96
DOC+811::96+PERMITNUMBER1
DOC+811::96+PERMITNUMBER2
IMD+++:::BLUE THINGS
GID+1
CST++8701101299:169:96
MEA+AAR++BOX:2
MOA+40:123.45:USD
LIN+3
LOC+27+AU::5+QC::96
DOC+811::96+PERMITNUMBER1
DOC+811::96+PERMITNUMBER2
IMD+++:::BLUE THINGS
GID+1
CST++4101909000:169:96
MEA+AAR++BOX:10
MOA+40:123.45:USD
UNS+S
UNT+58+<<MSGNO PLACEHOLDER>>";

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		#endregion Implementation
	}
}
