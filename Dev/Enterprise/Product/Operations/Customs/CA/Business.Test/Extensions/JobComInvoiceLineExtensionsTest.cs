using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceLineExtensionsTest : TestCaseWithFactory
	{
		public void TestGetLPCOHolderType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "HOLDER";
			var holderAddress = holder.MainAddress;
			holderAddress.OA_Address1 = "Holder Address 1";

			AssertEquals(ZString.Empty, line.GetLPCOHolderType(ZGuid.Empty));
			AssertEquals(LPCOHolderPartyTypeCodes.Codes.Broker, line.GetLPCOHolderType(GlbCompany.CurrentCompany.GC_OH_OrgProxy));

			declaration.JE_OH_Importer = holder.PK;
			AssertEquals(LPCOHolderPartyTypeCodes.Codes.Importer, line.GetLPCOHolderType(holder.PK));

			declaration.ImporterOfRecordAddress.OrganisationPK = holder.PK;
			AssertEquals(LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord, line.GetLPCOHolderType(holder.PK));

			invoice.ExporterDocumentaryAddress.OrganisationPK = holder.PK;
			AssertEquals(LPCOHolderPartyTypeCodes.Codes.Exporter, line.GetLPCOHolderType(holder.PK));

			invoice.ExporterDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord, line.GetLPCOHolderType(holder.PK));

			invoice.JZ_OH_Supplier = holder.PK;
			AssertEquals(LPCOHolderPartyTypeCodes.Codes.Supplier, line.GetLPCOHolderType(holder.PK));

			line.JI_OA_ManufacturerAddress = holderAddress.PK;
			AssertEquals(LPCOHolderPartyTypeCodes.Codes.Manufacturer, line.GetLPCOHolderType(holder.PK));
		}

		public void TestGetHolderOrgHeadersPks()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;

			var manu = Factory.New<OrgHeader>();
			manu.OH_Code = "MANUFACT";
			var manuAddress = manu.MainAddress;
			manuAddress.OA_Address1 = "Manufacturer Address 1";

			var supp = Factory.New<OrgHeader>();
			supp.OH_Code = "SUPPLIER";
			var suppAddress = supp.MainAddress;
			suppAddress.OA_Address1 = "Supplier Address 1";

			var expo = Factory.New<OrgHeader>();
			expo.OH_Code = "EXPORTER";
			var expoAddress = expo.MainAddress;
			expoAddress.OA_Address1 = "Exporter Address 1";

			var impo = Factory.New<OrgHeader>();
			impo.OH_Code = "IMPORTER";
			var impoAddress = impo.MainAddress;
			impoAddress.OA_Address1 = "Importer Address 1";

			var reco = Factory.New<OrgHeader>();
			reco.OH_Code = "RECORDER";
			var recoAddress = reco.MainAddress;
			recoAddress.OA_Address1 = "Recorder Address 1";

			var list = new List<ZGuid>(line.GetHolderOrgHeadersPks());
			AssertEquals(4, list.Count);
			Assert(!list.Contains(manu.PK));
			Assert(!list.Contains(supp.PK));
			Assert(!list.Contains(expo.PK));
			Assert(!list.Contains(impo.PK));
			Assert(!list.Contains(reco.PK));

			line.JI_OA_ManufacturerAddress = manuAddress.PK;
			list = new List<ZGuid>(line.GetHolderOrgHeadersPks());
			AssertEquals(4, list.Count);
			Assert(list.Contains(manu.PK));
			Assert(!list.Contains(supp.PK));
			Assert(!list.Contains(expo.PK));
			Assert(!list.Contains(impo.PK));
			Assert(!list.Contains(reco.PK));

			invoice.JZ_OH_Supplier = supp.PK;
			list = new List<ZGuid>(line.GetHolderOrgHeadersPks());
			AssertEquals(4, list.Count);
			Assert(list.Contains(manu.PK));
			Assert(list.Contains(supp.PK));
			Assert(!list.Contains(expo.PK));
			Assert(!list.Contains(impo.PK));
			Assert(!list.Contains(reco.PK));

			invoice.ExporterDocumentaryAddress.OrganisationPK = expo.PK;
			list = new List<ZGuid>(line.GetHolderOrgHeadersPks());
			AssertEquals(4, list.Count);
			Assert(list.Contains(manu.PK));
			Assert(list.Contains(supp.PK));
			Assert(list.Contains(expo.PK));
			Assert(!list.Contains(impo.PK));
			Assert(!list.Contains(reco.PK));

			invoice.ExporterDocumentaryAddress.E2_AddressOverride = true;
			list = new List<ZGuid>(line.GetHolderOrgHeadersPks());
			AssertEquals(4, list.Count);
			Assert(list.Contains(manu.PK));
			Assert(list.Contains(supp.PK));
			Assert(!list.Contains(expo.PK));
			Assert(!list.Contains(impo.PK));
			Assert(!list.Contains(reco.PK));

			declaration.JE_OH_Importer = impo.PK;
			list = new List<ZGuid>(line.GetHolderOrgHeadersPks());
			AssertEquals(4, list.Count);
			Assert(list.Contains(manu.PK));
			Assert(list.Contains(supp.PK));
			Assert(!list.Contains(expo.PK));
			Assert(list.Contains(impo.PK));
			Assert(!list.Contains(reco.PK));

			declaration.ImporterOfRecordAddress.OrganisationPK = reco.PK;
			list = new List<ZGuid>(line.GetHolderOrgHeadersPks());
			AssertEquals(5, list.Count);
			Assert(list.Contains(manu.PK));
			Assert(list.Contains(supp.PK));
			Assert(!list.Contains(expo.PK));
			Assert(list.Contains(impo.PK));
			Assert(list.Contains(reco.PK));
		}
	}
}
