using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestStatisticalValueManualOverrideIsSetForImports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var collection = new InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(true, invoiceLine.ZG_StatisticalValueManualOverride);
		}

		public void TestStatisticalValueManualOverrideIsNOTsetForExports()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var collection = new InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(false, invoiceLine.ZG_StatisticalValueManualOverride);
		}

		public void TestTypedIndexer()
		{
			var collection = new InvoiceLineCompleteCollection(Declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestCountryOfDestinationIsSetForImports()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var collection = new InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
				var invoiceLine = collection.AddNew();
				AssertEquals(declaration.CountryCode, invoiceLine.ZG_CountryOfDestination);
				AssertEquals(CountryCodes.UnitedKingdom, invoiceLine.ZG_CountryOfDestination);
			}
		}

		public void TestCountryOfDestinationIsNotSetForExports()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				var collection = new InvoiceLineViewCollection<JobComInvoiceLine>(declaration);
				var invoiceLine = collection.AddNew();
				AssertEquals(ZString.Empty, invoiceLine.ZG_CountryOfDestination);
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			return dec;
		}

		protected override void PrepareCharge(JobComInvCharge charge)
		{
			base.PrepareCharge(charge);
			charge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
		}
	}
}
