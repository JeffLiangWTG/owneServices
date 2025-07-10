using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(ClickableIrildesRequestContext))]
sealed class ClickableIrildesRequestContextTest : ClickableRequestContextAbstractTest
{
	public void TestVisible()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		AssertEquals("When NctsHeader is not departure, Visible", false, clickableContext.Visible);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		AssertEquals("When NctsHeader is departure non phase5, Visible", false, clickableContext.Visible);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertEquals("When NctsHeader is departure and phase5, Visible", true, clickableContext.Visible);
	}

	public void TestEnabled()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
		AssertEquals("When entry does not have MRN nor ReleaseCode, Enabled", false, clickableContext.Enabled);

		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";
		AssertEquals("When entry has MRN but not ReleaseCode, Enabled", false, clickableContext.Enabled);

		Factory.NewCusEntryNumber(nctsHeader, "CLR", "XYZ123", ZDateTime.Now);
		AssertEquals("When entry has both MRN and ReleaseCode, Enabled", true, clickableContext.Enabled);
	}

	public void TestExecute_CreateIrildesRequestMessage()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";

		Factory.NewCusEntryNumber(nctsHeader, "CLR", "XYZ123", ZDateTime.Now);
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		clickableContext.Execute(null);
		AssertRequestCreation(nctsHeader, EDIMessageTypeList.Codes.IrildesRequest, "IRILDES Request has been sent to customs.");
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		clickableContext = new ClickableIrildesRequestContextForTest(nctsHeader);
	}

	NctsHeader nctsHeader;
	IClickableContext clickableContext;

	protected override IClickableContext GetClickableContext() => clickableContext;
	protected override string GetExpectedName() => "IrildesRequest";
	protected override string GetExpectedCaption() => "IRILDES Request";
}

sealed class ClickableIrildesRequestContextForTest : ClickableIrildesRequestContext
{
	public ClickableIrildesRequestContextForTest(NctsHeader header) : base(header)
	{
	}

	protected override IrildesRequestMessageFactory GetNewIrildesRequestMessageFactory()
	{
		return new IrildesRequestMessageFactoryForTest();
	}
}
