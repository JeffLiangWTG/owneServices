using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(InvLineFixedCollection))]
	class InvLineFixedCollectionBOCollectionViewTest : BusinessObjectCollectionViewTestCase<InvLineFixedCollection>
	{
		protected override InvLineFixedCollection GetCollectionToTest()
		{
			var lineComplete = new Customs.Business.InvoiceLineCompleteCollection(Declaration);
			return new InvLineFixedCollection(InvoiceHeader, lineComplete);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<Customs.Business.BaseJobComInvoiceLine>();
			result.JI_JZ = InvoiceHeader.PK;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
		}

		JobDeclaration fDeclaration;
		JobDeclaration Declaration => fDeclaration ?? (fDeclaration = JobDeclaration.New(Factory));
		InvHeadWithFixedInvLines fInvoiceHeader;
		InvHeadWithFixedInvLines InvoiceHeader => fInvoiceHeader ?? (fInvoiceHeader = (InvHeadWithFixedInvLines)Declaration.Invoices.AddNew());
	}
}
