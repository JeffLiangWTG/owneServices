using CargoWise.Types;
using Enterprise.MailManager.ExternalMailInterface;
using MimeKit;

namespace Enterprise.Customs.IN.Business;

public static class Utils
{
	public static string GetIndianFinancialYear(ZDate date)
	{
		var range = GetIndianFinancialYearRange(date);
		return $"{range.startDate.Year}-{range.endDate.Year}";
	}

	public static (ZDate startDate, ZDate endDate) GetIndianFinancialYearRange(ZDate date)
	{
		var startYear = date.Month >= FinancialYearStartMonth ? date.Year : date.AddYears(-1).Year;
		var startDate = new ZDate(startYear, FinancialYearStartMonth, 1);
		return (startDate, startDate.AddYears(1).AddDays(-1));
	}

	static int FinancialYearStartMonth => 4;

	public static MimeMessage CreateMessageFromEml(EDIMessage message)
	{
		var messageData = message.EM_MessageData;
		return messageData.IsEmpty
			? MimeMessageExtensions.CreateMessageFromEml(message.EM_MessageText)
			: MimeMessageExtensions.CreateMessageFromEml(messageData);
	}
}
