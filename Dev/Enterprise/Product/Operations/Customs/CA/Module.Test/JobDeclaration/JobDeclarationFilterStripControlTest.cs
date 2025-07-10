using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestFilterGridColorContextKey()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var module = new JobDeclarationModule())
			using (var filterControl = new JobDeclarationFilterStripControl(module, declarations, module.FilterBusinessObject))
			{
				filterControl.OnLoad_Exposed();
				AssertEquals(ModuleIDs.Customs.JobDeclaration.Name, filterControl.FilteredGrid.ColorContextKey);
			}
		}

		public void TestFilteredGridFields()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				filterControl.Show();
				CombineAssertions(() =>
				{
					AssertEquals(JobDeclaration.Schema.JE_EntrySubmittedDate, "Submitted Date", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_EntrySubmittedDate));
					AssertEquals(JobDeclaration.Schema.JE_OH_Forwarder, "Service Provider", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_OH_Forwarder));
					AssertEquals(JobDeclaration.Schema.JE_OH_ShippingLine, "Carrier", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_OH_ShippingLine));
					AssertEquals(JobDeclaration.Schema.JE_OH_Importer, "Importer/Consignee", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_OH_Importer));
					AssertEquals(JobDeclaration.Schema.JE_OH_Supplier, "Vendor/Exporter", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_OH_Supplier));
					AssertEquals(JobDeclaration.Schema.JE_MessageSubType, "Entry Type", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_MessageSubType));
					AssertEquals(JobDeclaration.Schema.DeclarationNumber, "Transaction Number", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.DeclarationNumber));
					AssertEquals(JobDeclaration.Schema.JE_EntryAuthorisationDate, "Release Date", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_EntryAuthorisationDate));
					AssertEquals(JobDeclaration.Schema.JE_WarehouseReleaseDate, "Sub Location ETD", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_WarehouseReleaseDate).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.JE_CCNsAsAString, "CCNs", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_CCNsAsAString).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_K84AccountingDate, "Accounting Date", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_K84AccountingDate).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_K84StatementDate, "Statement Date", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_K84StatementDate).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_PlaceOfReport, "Place Of Report", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_PlaceOfReport).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_PortOfExit, "Port Of Exit", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_PortOfExit).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_ReasonForExportCode, "Reason For Export", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_ReasonForExportCode).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_RX_DeclaredCurr, "Declared Currency", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_RX_DeclaredCurr).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_TransportDocumentNumber, "Trans. Doc. No.", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_TransportDocumentNumber).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.JE_DateOfFirstArrival, "Date of First Arrival", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_DateOfFirstArrival).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_ServiceOption, "Service Option", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_ServiceOption).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_AssesmentOption, "Assessment Option", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_AssesmentOption).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_UnladingOffice, "Port of Unlading", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_UnladingOffice).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.JE_CarrierCode, "Carrier Code", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_CarrierCode).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.JE_LocationOfGoods, "Sub-Location", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_LocationOfGoods).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.JE_CustomsOffice, "Port of Clearance", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_CustomsOffice).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.EstimatedPaymentDueDate, "Entry Due Date", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.EstimatedPaymentDueDate).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalCustomsValueInLocalCurrency, "Customs Value", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalCustomsValueInLocalCurrency).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalBilledAmount, "Total Billed (Duty and Tax)", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalBilledAmount).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalOutstandingAmount, "Total Outstanding", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalOutstandingAmount).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalInvoicedAmount, "Total Invoiced (DSB)", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalInvoicedAmount).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalNormalDuty, "Duty Amount", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalNormalDuty).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalSimaDuty, "SIMA", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalSimaDuty).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalGST, "GST", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalGST).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalExciseTax, "Excise Tax", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalExciseTax).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalDutyAndTax, "Total Duty & Tax", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalDutyAndTax).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.TotalAmountPayable, "Total Payable", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.TotalAmountPayable).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.JE_EntryStatus, "Release Status", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_EntryStatus));
					AssertEquals(JobDeclaration.Schema.JE_EntryStatusDescription, "Release Status Description", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_EntryStatusDescription));
					AssertEquals(JobDeclaration.Schema.JE_MessageStatus, "Last Message", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_MessageStatus));
					AssertEquals(JobDeclaration.Schema.JE_MessageStatusDescription, "Last Message Description", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.JE_MessageStatusDescription));
					AssertEquals("ReleaseEntryHeader+CH_Status", "Release Message Status", filterControl.FilteredGrid.GetColumnStyle("ReleaseEntryHeader+CH_Status").CaptionResourceString.Caption);
					AssertEquals("ReleaseEntryHeader+MessageStatusDescription", "Release Message Status Desc.", filterControl.FilteredGrid.GetColumnStyle("ReleaseEntryHeader+MessageStatusDescription").CaptionResourceString.Caption);
					AssertEquals("B3EntryHeader+CH_EntryStatus", "Entry Status", filterControl.FilteredGrid.GetColumnStyle("B3EntryHeader+CH_EntryStatus").CaptionResourceString.Caption);
					AssertEquals("B3EntryHeader+EntryHeaderStatusDescription", "Entry Status Desc.", filterControl.FilteredGrid.GetColumnStyle("B3EntryHeader+EntryHeaderStatusDescription").CaptionResourceString.Caption);
					AssertEquals("B3EntryHeader+CH_Status", "Entry Message Status", filterControl.FilteredGrid.GetColumnStyle("B3EntryHeader+CH_Status").CaptionResourceString.Caption);
					AssertEquals("B3EntryHeader+MessageStatusDescription", "Entry Message Status Desc.", filterControl.FilteredGrid.GetColumnStyle("B3EntryHeader+MessageStatusDescription").CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_AccountingAge, "Days Since Release", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_AccountingAge).CaptionResourceString.Caption);
					AssertEquals("ImporterOfRecordAddress+OrganisationPK", "Importer of Record", filterControl.FilteredGrid.GetColumnStyle("ImporterOfRecordAddress+OrganisationPK").CaptionResourceString.Caption);
					AssertEquals("B3EntrySubmittedDate", "Entry Submission Date", filterControl.FilteredGrid.GetColumnStyle("B3EntrySubmittedDate").CaptionResourceString.Caption);
					AssertEquals("DIFURNs", "eDocs DIF URN", filterControl.FilteredGrid.GetColumnStyle("DIFURNs").CaptionResourceString.Caption);
					AssertEquals("DIFMessageStatus", "eDocs DIF Message Status", filterControl.FilteredGrid.GetColumnStyle("DIFMessageStatus").CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_DeclarationException, "Exception Code", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_DeclarationException).CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.WarehouseTransactionStatusDescription, "WHS Status", filterControl.FilteredGrid.GetColumnCaption(JobDeclaration.Schema.WarehouseTransactionStatusDescription));
					AssertEquals("B3AcceptedDate", "Entry Accepted Date", filterControl.FilteredGrid.GetColumnStyle("B3AcceptedDate").CaptionResourceString.Caption);
					AssertEquals(JobDeclaration.Schema.CA_CSAEntry, "CSA Release Only", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_CSAEntry).CaptionResourceString.Caption);
					AssertEquals("G7ExportEntryHeader+CH_Status", "EXP Status", filterControl.FilteredGrid.GetColumnStyle("G7ExportEntryHeader+CH_Status").CaptionResourceString.Caption);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var groupName = "Bond Information";
				var bondTypeColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_BondType);
				AssertEquals(JobDeclaration.Schema.CA_BondType, "Bond Type", bondTypeColumn.CaptionResourceString.Caption);
				AssertEquals(groupName, bondTypeColumn.GroupName.Caption);
				AssertEquals(false, bondTypeColumn.IsUnavailable);

				var bondNumColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_BondNo);
				AssertEquals(JobDeclaration.Schema.CA_BondNo, "Bond Number", bondNumColumn.CaptionResourceString.Caption);
				AssertEquals(groupName, bondNumColumn.GroupName.Caption);
				AssertEquals(false, bondNumColumn.IsUnavailable);

				var bondSuretyColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_SuretyCode);
				AssertEquals(JobDeclaration.Schema.CA_SuretyCode, "Bond Surety", bondSuretyColumn.CaptionResourceString.Caption);
				AssertEquals(groupName, bondSuretyColumn.GroupName.Caption);
				AssertEquals(false, bondSuretyColumn.IsUnavailable);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var bondColumns = filterControl.FilteredGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().Where(x => x.GroupName.Caption == "Bond Information");
				AssertEquals(3, bondColumns.Count());
				AssertEquals(true, bondColumns.All(x => x.IsUnavailable));
			}
		}
	}
}
