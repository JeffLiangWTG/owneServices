using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineAddInfoSupportingInfoTest : TestCase
	{
		public void TestWarehouseLineAddInfoMember()
		{
			var supportingInfoInvoiceHeader = new CustomsSupportingInformation()
			{
				Type = new CodeDescriptionPair6Char() { Code = "C015" },
				DateOfIssue = ZDateTime.BrettsBirthday,
				ReferenceNumber = "header-support1",
				Quantity = 666,
				UnitOfQuantity = new CodeDescriptionPair4Char() { Code = "NAR" },
				Status = new CodeDescriptionPair() { Code = "J" },
			};

			IWarehouseCustomsLineAddInfo lineAddInfo = new WarehouseCustomsLineAddInfoSupportingInfo(supportingInfoInvoiceHeader);
			AssertEquals("Type", "SDL", lineAddInfo.Type);
			AssertEquals("AddInfoData", "Type=C015*Reference=header-support1*DateOfIssue=1971-09-18 00:00:00.000*Available=J*Quantity=666*UnitofMeasure=NAR", lineAddInfo.AddInfoData);
			AssertEquals("NAddInfoData", ZString.Empty, lineAddInfo.NAddInfoData);
		}
	}
}
