using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsCancellationMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When NctsHeader is null", () => new NctsCancellationMessageWrapper(nctsHeader: null, sendingObject));
		AssertExceptionThrown<ArgumentNullException>("When NctsHeader.MovementHeader is null", () => new NctsCancellationMessageWrapper(Factory.New<NctsHeader>(), sendingObject));
		AssertExceptionThrown<ArgumentNullException>("When sendingObject is null", () => new NctsCancellationMessageWrapper(nctsHeader, sendingObject: null));
	}

	public void TestMrn()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Mrn), "", cancellationMessageWrapper.Mrn);

		var mrn = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.Branch.Company.GC_RN_NKCountryCode);
		mrn.CE_EntryNum = "22CH00000294926586";
		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Mrn), "22CH00000294926586", cancellationMessageWrapper.Mrn);
	}

	public void TestCustomsOffice()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.CustomsOffice), "", cancellationMessageWrapper.CustomsOffice);

		var departureCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew();
		departureCustomsOffice.CY_Code = "DEP";
		departureCustomsOffice.CY_Data = "IT137100";

		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.CustomsOffice), "137100", cancellationMessageWrapper.CustomsOffice);
	}

	public void TestReason()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Reason), "", cancellationMessageWrapper.Reason);

		sendingObject.Reason = "A";
		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.Reason), "A", cancellationMessageWrapper.Reason);
	}

	public void TestLegislativeReference()
	{
		var cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.LegislativeReference), "", cancellationMessageWrapper.LegislativeReference);

		sendingObject.LegislativeReference = "1";
		cancellationMessageWrapper = GetNewCancellationMessageWrapper();
		AssertEquals(nameof(ICancellation.LegislativeReference), "1", cancellationMessageWrapper.LegislativeReference);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
	}

	NctsHeader nctsHeader;
	NctsHeaderMessageSendingObject sendingObject;

	ICancellation GetNewCancellationMessageWrapper() => new NctsCancellationMessageWrapper(nctsHeader, sendingObject);
}
