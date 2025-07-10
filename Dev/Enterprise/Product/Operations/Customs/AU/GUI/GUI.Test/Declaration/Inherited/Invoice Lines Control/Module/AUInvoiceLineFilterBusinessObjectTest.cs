using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AUInvoiceLineFilterBusinessObject))]
	sealed class AUInvoiceLineFilterBusinessObjectTest : InvoiceLineFilterBusinessObjectTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTextFilter_HasCustomsError()
		{
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var invoice1 = Declaration.Invoices.AddNew();
			var invoice2 = Declaration.Invoices.AddNew();
			var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine12 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine21 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine22 = invoice2.JobComInvoiceLines.AddNew();
			var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
			var mergedLine1 = entryHeader.MergedLines.AddNew();
			mergedLine1.CL_LineNumber = 1;
			var mergedLine2 = entryHeader.MergedLines.AddNew();
			mergedLine2.CL_LineNumber = 2;
			invoiceLine11.JI_CL = mergedLine1.PK;
			invoiceLine12.JI_CL = mergedLine1.PK;
			invoiceLine21.JI_CL = mergedLine2.PK;
			invoiceLine22.JI_CL = mergedLine2.PK;
			Factory.Save();
			AssertEquals(4, Declaration.FilteredInvoiceLines.Count);
			AssertEquals(false, invoiceLine11.HasChanges);
			AssertEquals(false, invoiceLine12.HasChanges);
			AssertEquals(false, invoiceLine21.HasChanges);
			AssertEquals(false, invoiceLine22.HasChanges);
			var responseWithOneLineError = Factory.New<EDIMessage>();
			responseWithOneLineError.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Received;
			responseWithOneLineError.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseWithOneLineError.EM_MessageType = "CMR";
			responseWithOneLineError.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\AU\Business\Business.Test\MessageProcessors\CMR\TestFiles\" + "CMRCUSRESRejection.txt").Replace("\r\n", "");
			responseWithOneLineError.EM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader.Messages.Add(responseWithOneLineError);
			AssertEquals("Line 1 has no error", false, mergedLine1.HasMessageResponseError);
			AssertEquals("Line 2 has error", true, mergedLine2.HasMessageResponseError);
			var filter = (ModuleTextFilter)FilterBO[AUInvoiceLineFilterConstants.HasCustomsError];
			filter.IsActive = true;
			var collectionFilter = Declaration.FilteredInvoiceLines;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = string.Empty;
			FilterBO.Search();
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine11, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine12, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine21, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Yes";
			FilterBO.Search();
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine11, collectionFilter);
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine12, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine21, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine22, collectionFilter);
			filter.Property = "No";
			FilterBO.Search();
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine11, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine22, collectionFilter);
			filter.Property = "All";
			FilterBO.Search();
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine11, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine12, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine21, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine22, collectionFilter);
			// filter AND filter2
			var filterStrip = FilterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = filter.Description;
			var filter2 = (ModuleTextFilter)FilterBO[AUInvoiceLineFilterConstants.HasCustomsError + " (1)"];
			filter2.Property = "No";
			FilterBO.Search();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine11, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine22, collectionFilter);
			// filter OR filter2
			filter.OrCategory = FilterOrCategory.Red;
			filter2.OrCategory = FilterOrCategory.Red;
			FilterBO.Search();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine11, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine12, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine21, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine22, collectionFilter);
			// (filter OR filter2) AND filter3
			var filterStrip3 = FilterBO.FilterStrips.AddNew();
			filterStrip3.FilterDescription = filter.Description;
			var filter3 = (ModuleTextFilter)FilterBO[AUInvoiceLineFilterConstants.HasCustomsError + " (2)"];
			filter3.Property = "No";
			filter3.OrCategory = FilterOrCategory.None;
			FilterBO.Search();
			AssertEquals(3, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine11, collectionFilter);
			AssertCollectionContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(AUInvoiceLineFilterConstants.HasCustomsError, invoiceLine22, collectionFilter);
		}

		protected override InvoiceLineFilterBusinessObject CreateNewInvoiceLineFilterBusinessObject(Func<Customs.Business.IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
			=> new AUInvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);

		protected override BaseJobDeclaration GetDeclaration() => Factory.New<JobDeclaration>();

		JobDeclaration Declaration => (JobDeclaration)declaration;

		AUInvoiceLineFilterBusinessObject FilterBO => (AUInvoiceLineFilterBusinessObject)filterBO;

		protected override bool ShouldBeLocalizable => false;
	}
}
