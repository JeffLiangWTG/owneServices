using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class MovementReferenceNumberSupportingInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumberMandatoryAndGDRNorMRN() => CombineAssertions(() =>
	{
		const string MessageGDNR = "NS30118";
		const string MessageMRN = "NS30006";

		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MRN.CSI_ReferenceNumberInfo);

		MRN.CSI_ReferenceNumber = "23CH063456789012N8";
		AssertNoMessageErrorContaining("Valid GDNR", MRN.CSI_ReferenceNumberInfo, MessageGDNR);
		AssertNoMessageErrorContaining("Valid GDNR", MRN.CSI_ReferenceNumberInfo, MessageMRN);

		MRN.CSI_ReferenceNumber = "23CH003456789012N8";
		AssertHasMessageErrorContaining("Invalid GDNR", MRN.CSI_ReferenceNumberInfo, MessageGDNR);
		AssertNoMessageErrorContaining("Invalid GDNR", MRN.CSI_ReferenceNumberInfo, MessageMRN);

		MRN.CSI_ReferenceNumber = "23IT123456789012J1";
		AssertNoMessageErrorContaining("Valid MRN", MRN.CSI_ReferenceNumberInfo, MessageGDNR);
		AssertNoMessageErrorContaining("Valid MRN", MRN.CSI_ReferenceNumberInfo, MessageMRN);

		MRN.CSI_ReferenceNumber = "23XX123456789012J1";
		AssertNoMessageErrorContaining("Invalid MRN", MRN.CSI_ReferenceNumberInfo, MessageGDNR);
		AssertHasMessageErrorContaining("Invalid MRN", MRN.CSI_ReferenceNumberInfo, MessageMRN);

		MRN.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("Empty", MRN.CSI_ReferenceNumberInfo, MessageGDNR);
		AssertNoMessageErrorContaining("Empty", MRN.CSI_ReferenceNumberInfo, MessageMRN);
	});

	public void TestCheckCSI_ReferenceNumber_NZ50023() => CombineAssertions(() =>
	{
		const string mrn1 = "23IT123456789012J1";
		const string mrn2 = "23CH063456789012N8";
		const string mrn3 = "24CH202403050004J4";
		const string mrn4 = "24CH202403050005J3";

		var arrivalMovementHeader = MRN.Parent;
		var additionalMrn = arrivalMovementHeader.MovementReferenceNumbers.AddNew();
		additionalMrn.CSI_ReferenceNumber = mrn1;

		MRN.CSI_ReferenceNumber = mrn1;
		AssertHasError("MRN already added to this Arrival", MRN.CSI_ReferenceNumberInfo, PassarValidationMessages.NZ50023());
		MRN.CSI_ReferenceNumber = mrn2;
		AssertNoError("MRN is unique", MRN.CSI_ReferenceNumberInfo, PassarValidationMessages.NZ50023());

		NctsHeader otherArrival = CreateNctsHeader();
		otherArrival.ArrivalMovementHeader.MovementReferenceNumbers.AddNew().CSI_ReferenceNumber = mrn3;
		NctsHeader receivedArrival = CreateNctsHeader();
		receivedArrival.MovementReferenceNumberSetter(mrn4);
		Factory.Save();

		MRN.CSI_ReferenceNumber = mrn3;
		arrivalMovementHeader.Validation.ValidateAll();
		AssertHasRowError("MRN in another Arrival request", MRN, PassarValidationMessages.NZ50023(otherArrival.BH_JobReference));
		MRN.CSI_ReferenceNumber = mrn2;
		AssertNoRowError("MRN not in another Arrival request", MRN, PassarValidationMessages.NZ50023(otherArrival.BH_JobReference));

		MRN.CSI_ReferenceNumber = mrn4;
		arrivalMovementHeader.Validation.ValidateAll();
		AssertHasRowError("MRN in another received Arrival", MRN, PassarValidationMessages.NZ50023(receivedArrival.BH_JobReference));
		MRN.CSI_ReferenceNumber = mrn2;
		AssertNoRowError("MRN not in another received Arrival", MRN, PassarValidationMessages.NZ50023(receivedArrival.BH_JobReference));
	});

	public void TestCheckCSI_Status() => ValidationTestHelper.AssertInvalidCodeMessageError(MRN.CSI_StatusInfo, "X", "Y");

	MovementReferenceNumberSupportingInfo MRN => mrn ?? (mrn = CreateNewMRN());
	MovementReferenceNumberSupportingInfo mrn;

	MovementReferenceNumberSupportingInfo CreateNewMRN()
	{
		return CreateNctsHeader().ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
	}

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader;
	}
}
