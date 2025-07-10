using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2AsClaimForLineViewCollection))]
	sealed class B2AsClaimForLineViewCollectionTest : BusinessObjectCollectionViewTestCase<B2AsClaimForLineViewCollection>
	{
		public void TestIsThisPartOfTheCollection()
		{
			AsAccountedForInvoice.JZ_InvoiceNumber = "INV1";
			AsAccountForInvoiceLine.CA_OriginalLineNo = "Test01";
			var asClaimedForInvoice = AsAccountedForInvoice.CorrespondingAsClaimedForInvoice;
			var invoiceLine1 = asClaimedForInvoice.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_IsAccountForLine = false;
			invoiceLine1.JI_ParentID = AsAccountForInvoiceLine.PK;
			var invoiceLine2 = asClaimedForInvoice.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_IsAccountForLine = true;
			invoiceLine2.JI_ParentID = AsAccountForInvoiceLine.PK;
			var invoiceLine3 = asClaimedForInvoice.JobComInvoiceLines.AddNew();
			invoiceLine3.CA_IsAccountForLine = false;

			AsAccountForInvoiceLine.ReadOnlyAsClaimForFilteredInvoiceLines.Rebuild();
			AssertEquals(2, AsAccountForInvoiceLine.ReadOnlyAsClaimForFilteredInvoiceLines.Count);
			AsAccountForInvoiceLine.CA_IsAccountForLine = false;
			AsAccountForInvoiceLine.ReadOnlyAsClaimForFilteredInvoiceLines.Rebuild();
			AssertEquals(0, AsAccountForInvoiceLine.ReadOnlyAsClaimForFilteredInvoiceLines.Count);
		}

		public void TestAllowNew()
		{
			Assert(!((IBindingList)Collection).AllowNew);
		}

		protected override B2AsClaimForLineViewCollection GetCollectionToTest()
		{
			return new B2AsClaimForLineViewCollection(AsAccountForInvoiceLine, Declaration.InvoiceLines);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AsAccountedForInvoice.JZ_InvoiceNumber = "INV1";
			var asClaimedForInvoice = AsAccountedForInvoice.CorrespondingAsClaimedForInvoice;
			var asClaimForLine = asClaimedForInvoice.JobComInvoiceLines.AddNew();
			asClaimForLine.JI_ParentID = AsAccountForInvoiceLine.PK;
			asClaimForLine.CA_IsAccountForLine = false;
			return asClaimForLine;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		JobComInvoiceHeader AsAccountedForInvoice
		{
			get { return fAsAccountedForInvoice ?? (fAsAccountedForInvoice = Declaration.B2AsAccountedForInvoices.AddNew()); }
		}
		JobComInvoiceHeader fAsAccountedForInvoice;

		JobComInvoiceLine AsAccountForInvoiceLine
		{
			get
			{
				if (fAsAccountForInvoiceLine == null)
				{
					fAsAccountForInvoiceLine = AsAccountedForInvoice.AsAccountForFilteredInvoiceLines.AddNew();
				}
				return fAsAccountForInvoiceLine;
			}
		}
		JobComInvoiceLine fAsAccountForInvoiceLine;
	}
}
