using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(Utils))]
sealed class UtilsTest : TestCaseWithFactory
{
	public void TestGetIndianFinancialYear()
	{
		CombineAssertions(() =>
		{
			AssertEquals("2021-2022", Utils.GetIndianFinancialYear(new ZDate(2022, 3, 16)));
			AssertEquals("2022-2023", Utils.GetIndianFinancialYear(new ZDate(2022, 4, 16)));
		});
	}

	public void TestGetIndianFinancialYearRange()
	{
		CombineAssertions(() =>
		{
			var (startDate, endDate) = Utils.GetIndianFinancialYearRange(new ZDate(2022, 3, 16));
			AssertEquals(new ZDate(2021, 4, 1), startDate);
			AssertEquals(new ZDate(2022, 3, 31), endDate);

			(startDate, endDate) = Utils.GetIndianFinancialYearRange(new ZDate(2022, 4, 16));
			AssertEquals(new ZDate(2022, 4, 1), startDate);
			AssertEquals(new ZDate(2023, 3, 31), endDate);
		});
	}

	public void TestCreateMessageFromEml()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageData = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");
		var email = Utils.CreateMessageFromEml(message);

		CombineAssertions(() =>
		{
			AssertEquals("When MessageData is valid email, Subject", "Filling status - control no. 0000001, filing date 20240819, Receiver   ID INBLR4, Message ID CMCHI01", email.Subject);
			AssertEquals("When MessageData is valid email, Attachments", 0, email.Attachments.Count());

			message.EM_MessageData = null;
			email = Utils.CreateMessageFromEml(message);
			AssertNullOrEmpty("When MessageData is empty, Subject", email.Subject);
		});
	}
}
