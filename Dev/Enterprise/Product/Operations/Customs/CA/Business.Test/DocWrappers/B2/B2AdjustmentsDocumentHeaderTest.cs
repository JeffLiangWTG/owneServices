using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2AdjustmentsDocHeader))]
	sealed class B2AdjustmentsDocumentHeaderTest : AdjustmentsDocumentHeaderTest
	{
		public override void TestAdjustmentsDocumentHeaderMembers()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			var address = importer.MainAddress;
			address.OA_Address1 = "ADDRESS1";
			address.OA_Address2 = "ADDRESS2";
			address.OA_City = "CANADACITY";
			address.OA_PostCode = "123";
			address.OA_State = "AB";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "BizNo4ImEx", Core.Constants.CountryCodes.Canada);
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "GST", Core.Constants.CountryCodes.Canada);

			var customsAddress = importer.Addresses.AddNew();
			customsAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			customsAddress.OA_Address1 = "FacilityAddress2";
			customsAddress.OA_City = "CUSTOM";
			customsAddress.OA_RL_NKRelatedPortCode = "USNYC";

			AssertEquals("CustomsAddressOfRecord Type", customsAddress.PK, importer.CustomsAddress.PK);

			b2.JE_OH_Importer = importer.PK;
			b2.JE_OH_NotifyParty = importer.PK;

			b2.JE_CustomsOffice = "0019";
			b2.CA_OriginalTransactionNo = "10207000002752";
			b2.CA_K84AccountingDate = new ZDateTime(2012, 9, 13);
			b2.CA_SecurityNo = "SECURITY";
			b2.CA_OriginalAccountingDate = new ZDateTime(2012, 9, 14);

			var header = new B2AdjustmentsDocHeader(b2);
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
			address.OA_State = "TR";
			b2.JE_OH_NotifyParty = mailTo.PK;
			header = new B2AdjustmentsDocHeader(b2);
			AssertEquals("MAILTO\r\nADDRESS1 ADDRESS2\r\nCANADACITY TR 123", header.MailToFormatted);

			var mailTo2 = Factory.New<OrgHeader>();
			mailTo2.OH_FullName = "MAILTO2";
			mailTo2.OH_RL_NKClosestPort = "USCHI";
			address = mailTo2.MainAddress;
			address.OA_Address1 = "ADDRESS1";
			address.OA_Address2 = "ADDRESS2";
			address.OA_City = "NEWYORKCITY";
			address.OA_PostCode = "12345";
			address.OA_State = "NY";
			b2.JE_OH_NotifyParty = mailTo2.PK;
			header = new B2AdjustmentsDocHeader(b2);
			AssertEquals(@"MAILTO2
ADDRESS1 ADDRESS2
NEWYORKCITY NY 12345
United States", header.MailToFormatted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			return new B2AdjustmentsDocHeader(b2);
		}
	}
}
