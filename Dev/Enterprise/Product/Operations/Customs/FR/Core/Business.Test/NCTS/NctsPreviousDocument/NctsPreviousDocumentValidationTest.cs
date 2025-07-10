using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsPreviousDocumentValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			previousDocument.CSI_ReferenceNumber = "";
			AssertHasMessageErrorContaining("Reference Number is mandatory but the value is provided.", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_ReferenceNumber = "RefNumber";
			AssertNoMessageErrorContaining("Reference Number is mandatory but the value is provided.", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_SubType()
		{
			previousDocument.CSI_SubType = "";
			AssertNoNotifications("no validation on csi_subtype", previousDocument.CSI_SubTypeInfo);
			previousDocument.CSI_SubType = ";:,ccac*$";
			AssertNoNotifications("no validation on csi_subtype", previousDocument.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_Reference_ShouldShowMessageError_WhenReferenceCannotIndexRegTempHeader()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ000046";
			var cusEntryNum = CusEntryNumber.New(ist, CusEntryNumberTypes.France.DDT, Core.Constants.CountryCodes.France);
			cusEntryNum.CE_EntryNum = "A000024";

			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes._337;
			previousDocument.CSI_ReferenceNumber = "FRJ000045";
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, "Could not find matching temporary storage register using reference FRJ000045.");
			previousDocument.CSI_ReferenceNumber = "FRJ000046";
			AssertNoMessageErrors(previousDocument.CSI_ReferenceNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousDocument previousDocument;
	}
}
