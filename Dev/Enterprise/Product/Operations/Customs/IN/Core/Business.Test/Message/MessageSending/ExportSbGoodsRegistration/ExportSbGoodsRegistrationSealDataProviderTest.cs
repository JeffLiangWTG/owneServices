using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationSealDataProviderTest : ExportSbGoodsRegistrationSealDataProviderAbstractClassBase
{
	public override void TestCommissionerate()
	{
		AssertEquals("TBA", CreateDataProvider().Commissionerate);
	}

	public override void TestCustomHouseCode()
	{
		AssertEquals("TBA", CreateDataProvider().CustomHouseCode);
	}

	public override void TestDivision()
	{
		AssertEquals("TBA", CreateDataProvider().Division);
	}

	public override void TestExaminationDate()
	{
		AssertEquals("TBA", CreateDataProvider().ExaminationDate);
	}

	public override void TestExaminingOfficerDesignation()
	{
		AssertEquals("TBA", CreateDataProvider().ExaminingOfficerDesignation);
	}

	public override void TestExaminingOfficerName()
	{
		AssertEquals("TBA", CreateDataProvider().ExaminingOfficerName);
	}

	public override void TestItemValuesVerifiedByExaminingOfficer()
	{
		AssertEquals("TBA", CreateDataProvider().ItemValuesVerifiedByExaminingOfficer);
	}

	public override void TestMessageType()
	{
		AssertEquals("TBA", CreateDataProvider().MessageType);
	}

	public override void TestRange()
	{
		AssertEquals("TBA", CreateDataProvider().Range);
	}

	public override void TestSampleForwarded()
	{
		AssertEquals("TBA", CreateDataProvider().SampleForwarded);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	public override void TestSealNo()
	{
		AssertEquals("TBA", CreateDataProvider().SealNo);
	}

	public override void TestSupervisingOfficerDesignation()
	{
		AssertEquals("TBA", CreateDataProvider().SupervisingOfficerDesignation);
	}

	public override void TestSupervisingOfficerName()
	{
		AssertEquals("TBA", CreateDataProvider().SupervisingOfficerName);
	}

	protected override SealDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Goodsregistration.Seal.First();
	}
}
