using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(InvHeadWithFixedInvLines))]
	internal class InvHeadWithFixedInvLinesTest : BaseJobComInvoiceHeaderAbstractTest<InvHeadWithFixedInvLines, BaseJobComInvoiceLine>
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.BaseJobComInvoiceHeader);
		public void TestBaseJobComInvoiceLineViewCollection()
		{
			AssertEquals("pre-condition", TestHelper.InvHeadWithFixedInvLines.CreateNewJobComInvoiceLineCollection(), null);
			TestHelper.InvHeadWithFixedInvLines.JZ_JE = Factory.NewWithValidTestData<JobDeclaration>().PK;
			BaseJobComInvoiceLineViewCollection actualCollection = TestHelper.InvHeadWithFixedInvLines.CreateNewJobComInvoiceLineCollection();
			AssertEquals("Type is InvLineFixedCollection", typeof(InvLineFixedCollection), actualCollection.GetType());
		}

		WCBTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new WCBTestHelper(Factory));
			}
		}

		WCBTestHelper testHelper;
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.Invoices.AddNew();
		}
	}
}
