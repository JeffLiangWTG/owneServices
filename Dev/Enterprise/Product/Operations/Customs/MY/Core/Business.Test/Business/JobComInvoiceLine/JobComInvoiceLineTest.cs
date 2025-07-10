using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be MY", Core.Constants.CountryCodes.Malaysia, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Malaysia, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestPartType()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			MasterFiles.Business.OrgSupplierPart product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			GlbCompany myCompany = Factory.New<GlbCompany>();
			myCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Malaysia;
			GlbBranch myBranch = myCompany.Branches.AddNew();
			myBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Malaysia)).RL_Code;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = myBranch.PK;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			JobDeclaration declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the right type type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);

			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		#region Implementation

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected new JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.InvoiceHeader; }
		}

		protected new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected override bool RatesAreReciprocal
		{
			get { return true; }
		}

		#endregion

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
