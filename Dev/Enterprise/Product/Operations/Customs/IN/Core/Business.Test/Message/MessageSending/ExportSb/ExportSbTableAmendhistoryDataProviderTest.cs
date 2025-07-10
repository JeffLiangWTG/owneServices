using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableAmendhistoryDataProviderTest : ExportSbTableAmendhistoryDataProviderAbstractClassBase
{
	public override void TestAmendmentDate()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentDate);
	}

	public override void TestAmendmentNumber()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentNumber);
	}

	public override void TestAmendmentStatus()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentStatus);
	}

	public override void TestIndicateTypeOfAmendment()
	{
		AssertEquals("TBA", CreateDataProvider().IndicateTypeOfAmendment);
	}

	public override void TestMessageType()
	{
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertEquals(DeclarationMessageTypeList.Codes.Fresh, CreateDataProvider().MessageType);
	}

	public override void TestReasonForAmendment()
	{
		AssertEquals("TBA", CreateDataProvider().ReasonForAmendment);
	}

	public override void TestRequestDate()
	{
		AssertEquals("TBA", CreateDataProvider().RequestDate);
	}

	public override void TestRequestLetterNumber()
	{
		AssertEquals("TBA", CreateDataProvider().RequestLetterNumber);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	public override void TestSiteid()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().Siteid);
	}

	protected override TableAmendhistoryDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Sb.TableAmendhistory.First();
	}
	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryHeaders.Add(header);
	}

	JobDeclaration declaration;
}
