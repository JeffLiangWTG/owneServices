using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3XAdjustmentsDocHeader))]
	sealed class B3XAdjustmentsDocumentHeaderTest : AdjustmentsDocumentHeaderTest
	{
		public override void TestAdjustmentsDocumentHeaderMembers()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			importer.OH_RL_NKClosestPort = "CA123";
			var address = importer.MainAddress;
			address.OA_Address1 = "ADDRESS1";
			address.OA_Address2 = "ADDRESS2";
			address.OA_City = "CANADA";
			address.OA_PostCode = "123";
			address.OA_State = "AB";

			var customsAddress = importer.Addresses.AddNew();
			customsAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			customsAddress.OA_Address1 = "FacilityAddress2";
			customsAddress.OA_City = "CUSTOM";
			customsAddress.OA_RL_NKRelatedPortCode = "USNYC";

			AssertEquals("CustomsAddressOfRecord Type", customsAddress.PK, importer.CustomsAddress.PK);

			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "BizNo4ImEx", Core.Constants.CountryCodes.Canada);
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "GST", Core.Constants.CountryCodes.Canada);
			b3x.JE_OH_Importer = importer.PK;
			b3x.JE_OH_NotifyParty = importer.PK;

			b3x.JE_CustomsOffice = "0019";
			b3x.CA_OriginalTransactionNo = "10207000002752";
			b3x.CA_K84AccountingDate = new ZDateTime(2012, 9, 13);
			b3x.CA_SecurityNo = "SECURITY";
			b3x.CA_OriginalAccountingDate = new ZDateTime(2012, 9, 14);

			var header = new B3XAdjustmentsDocHeader(b3x);
			AssertEquals("Customs Address of Record", @"IMPORTER
FacilityAddress2
CUSTOM
United States", header.ImporterFormatted);
			AssertEquals("BizNo4ImEx", header.BusinessNumber);
			AssertEquals("GST", header.GSTNumber);
			AssertEquals("19", header.CBSAOffice);
			AssertEquals("10207000002752", header.OriginalTransactionNo);
			AssertEquals("09", header.Month);
			AssertEquals("14", header.Day);
			AssertEquals("2012", header.Year);
			AssertEquals("SECURITY", header.SecurityNo);

			AssertEquals(ZString.Empty, header.MailToFormatted);
			var mailTo = Factory.New<OrgHeader>();
			mailTo.OH_FullName = "MAILTO";
			address = mailTo.MainAddress;
			address.OA_Address1 = "ADDRESS1";
			address.OA_Address2 = "ADDRESS2";
			address.OA_City = "CANADACITY";
			address.OA_PostCode = "123";
			address.OA_State = "AB";
			b3x.JE_OH_NotifyParty = mailTo.PK;
			header = new B3XAdjustmentsDocHeader(b3x);
			AssertEquals(@"MAILTO
ADDRESS1 ADDRESS2
CANADACITY AB 123", header.MailToFormatted);

			var mailTo2 = Factory.New<OrgHeader>();
			mailTo2.OH_FullName = "MAILTO2";
			mailTo2.OH_RL_NKClosestPort = "USCHI";
			address = mailTo2.MainAddress;
			address.OA_Address1 = "ADDRESSNY1";
			address.OA_Address2 = "ADDRESSNY2";
			address.OA_City = "NEWYORK";
			address.OA_PostCode = "12345";
			address.OA_State = "NY";
			b3x.JE_OH_NotifyParty = mailTo2.PK;
			header = new B3XAdjustmentsDocHeader(b3x);
			AssertEquals(@"MAILTO2
ADDRESSNY1 ADDRESSNY2
NEWYORK NY 12345
United States", header.MailToFormatted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			return new B3XAdjustmentsDocHeader(b3x);
		}
	}
}
