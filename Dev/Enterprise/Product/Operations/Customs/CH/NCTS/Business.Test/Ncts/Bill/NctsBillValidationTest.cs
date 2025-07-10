using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsBillValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckB0_WeightErrorIfNotEntered()
	{
		DepartureNctsBill.Validation.ValidateB0_Weight();
		AssertEquals("B0_Weight does not have an 'If not Entered...' message error", false, departureNctsBill.B0_WeightInfo.Notifications.Any(e => e.Message.Contains(MandatoryValidation.YouHaveNotEntered)));
	}

	public void TestCheckNP70254() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNP70254;
		var documentTypes = new[] { "SNOT", "SWEB", "SZVE", "STRE", "SAUZ", "STAB", "SZVA", "SZWA", "NTRV" };

		foreach (var documentType in documentTypes)
		{
			AssertRowMessageError(false, "XXXX", documentType);
		}

		AssertRowMessageError(true, "XXXX");
		AssertRowMessageError(true);

		ArrivalNctsBill.Validation.ValidateAll();
		AssertNoRowMessageError("Arrival", ArrivalNctsBill, messageError);

		void AssertRowMessageError(bool expectedMessage, params string[] documentTypes)
		{
			DepartureNctsBill.PreviousDocuments.RemoveAndDeleteAll();
			foreach (var type in documentTypes)
			{
				DepartureNctsBill.PreviousDocuments.AddNew().CSI_Code = type;
			}
			DepartureNctsBill.Validation.ValidateAll();
			var assertionMessage = $"DocumentTypes={string.Join(",", documentTypes)}";
			if (expectedMessage)
			{
				AssertHasRowMessageError(assertionMessage, DepartureNctsBill, messageError);
			}
			else
			{
				AssertNoRowMessageError(assertionMessage, DepartureNctsBill, messageError);
			}
		}
	});

	NctsHeader DepartureNctsHeader => departureNctsHeader ??= CreateNctsDepartureHeader(NctsMovementType.Codes.Departure);
	NctsHeader departureNctsHeader;

	NctsHeader ArrivalNctsHeader => arrivalNctsHeader ??= CreateNctsDepartureHeader(NctsMovementType.Codes.Arrival);
	NctsHeader arrivalNctsHeader;

	NctsHeader CreateNctsDepartureHeader(string modementType)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(modementType);
		return nctsHeader;
	}

	NctsBill DepartureNctsBill => departureNctsBill ??= DepartureNctsHeader.Bills.AddNew();
	NctsBill departureNctsBill;

	NctsBill ArrivalNctsBill => arrivalNctsBill ??= ArrivalNctsHeader.Bills.AddNew();
	NctsBill arrivalNctsBill;
}
