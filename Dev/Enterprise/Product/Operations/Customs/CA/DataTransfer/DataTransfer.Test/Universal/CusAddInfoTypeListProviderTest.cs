using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestCusAddInfoGetListForJobComInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(10, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.CADutyAndTax, "Duty And Tax", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CADutyAndTax));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader, "Canadian Food Inspection Agency PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CACNSCPGAHeader, "Canadian Nuclear Safety Commission PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CACNSCPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CADFOPGAHeader, "Department of Fisheries and Oceans PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CADFOPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader, "Environment And Climate Change Canada PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CAGACPGAHeader, "Global Affairs Canada PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CAGACPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CAHCPGAHeader, "Health Canada PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CAHCPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader, "Natural Resources Canada PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CAPHACPGAHeader, "Public Health Agency of Canada PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CAPHACPGAHeader));
			AssertEquals(CusAddInfoTypeAttribute.Codes.CATCPGAHeader, "Transport Canada PGA", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CATCPGAHeader));
		}

		public void TestCusAddInfoGetListForJobDeclaration()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.CACCN, "Cargo Control Number", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CACCN));
		}

		public void TestCusAddInfoGetListForCusAddInfo()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusAddInfoTypeList(CusAddInfoSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusAddInfoTypeAttribute.Codes.CAComponent, "Component", list.GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.CAComponent));
		}
	}
}
