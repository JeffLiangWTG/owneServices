using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(Component))]
	sealed class ComponentTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<Component>
	{
		public void TestSupportsNotes()
		{
			var bo = (Component)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestPGAHeaderCA_NameFieldType()
		{
			var cnscHeader = Factory.New<CNSCPGAHeader>();
			var hcHeader = Factory.New<HCPGAHeader>();
			var ecccHeader = Factory.New<ECCCPGAHeader>();
			var cnscCom = cnscHeader.Components.AddNew();
			var hcCom = hcHeader.Components.AddNew();
			var ecccCom = ecccHeader.Components.AddNew();

			cnscHeader.CA_Category = CNSCCategories.Codes.CNS;
			AssertEquals(nameof(FieldType.TextDropEdit), cnscCom.CA_NameFieldType);
			cnscHeader.CA_Category = CNSCCategories.Codes.NE;
			AssertEquals(nameof(FieldType.Text), cnscCom.CA_NameFieldType);
			cnscHeader.CA_Category = CNSCCategories.Codes.NS;
			AssertEquals(nameof(FieldType.Text), cnscCom.CA_NameFieldType);
			cnscHeader.CA_Category = CNSCCategories.Codes.RD;
			AssertEquals(nameof(FieldType.Text), cnscCom.CA_NameFieldType);
			AssertEquals(nameof(FieldType.Text), hcCom.CA_NameFieldType);
			AssertEquals(nameof(FieldType.Text), ecccCom.CA_NameFieldType);
		}

		protected override IEnumerable<Component> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = "Y";
			var cNSCPGAHeader = invoiceLine.CNSCPGAHeader;
			yield return cNSCPGAHeader.Components.AddNew();

			invoiceLine.CA_HCInd = "Y";
			var hCPGAHeader = invoiceLine.HCPGAHeader;
			yield return hCPGAHeader.Components.AddNew();
		}
	}
}
