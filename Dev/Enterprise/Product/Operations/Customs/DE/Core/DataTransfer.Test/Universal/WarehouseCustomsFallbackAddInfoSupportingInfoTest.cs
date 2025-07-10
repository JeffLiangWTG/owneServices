using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsFallbackAddInfoSupportingInfoTest : TestCase
	{
		public void TestWarehouseCustomsFallbackAddInfoAddInfoMember()
		{
			var supportingInfoInvoiceHeader = new CustomsSupportingInformation()
			{
				Type = new CodeDescriptionPair6Char() { Code = "C014" },
				DateOfIssue = ZDateTime.BrettsBirthday,
				ReferenceNumber = "header-support1"
			};

			IWarehouseCustomsLineAddInfo lineAddInfo = new WarehouseCustomsFallbackAddInfoSupportingInfo(supportingInfoInvoiceHeader);
			AssertEquals("Type", "SDH", lineAddInfo.Type);
			AssertEquals("AddInfoData", "Type=C014*Reference=header-support1*DateOfIssue=1971-09-18 00:00:00.000", lineAddInfo.AddInfoData);
			AssertEquals("NAddInfoData", ZString.Empty, lineAddInfo.NAddInfoData);
		}
	}
}
