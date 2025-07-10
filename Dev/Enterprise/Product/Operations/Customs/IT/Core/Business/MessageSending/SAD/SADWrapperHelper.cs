using System;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business;

public static class SADWrapperHelper
{
	public static ZBool IsItalianCustomsOffice(ZString customsOffice)
		=> customsOffice.StartsWith(Core.Constants.CountryCodes.Italy, StringComparison.OrdinalIgnoreCase);

	public static ZString RemoveIsoCodeIfIsItalianCustomsOffice(ZString customsOffice)
		=> IsItalianCustomsOffice(customsOffice) ? RemoveIsoCode(customsOffice) : customsOffice;

	public static ZString RemoveIsoCode(ZString customsOffice) => XmlWrapperHelper.RemoveIsoCode(customsOffice);

	public static ZString RemoveBlackListChars(ZString inputValue)
	{
		var blackAndWhiteListCharset = new (ZString BlackListChar, ZString WhiteListChar)[]
		{
			(BlackListChar: "\t", WhiteListChar: " "),
			(BlackListChar: "\r\n", WhiteListChar: " "),
			(BlackListChar: "\r", WhiteListChar: " "),
			(BlackListChar: "\n", WhiteListChar: " "),
		}.ToImmutableArray();

		foreach (var charset in blackAndWhiteListCharset)
		{
			inputValue = inputValue.Replace(charset.BlackListChar, charset.WhiteListChar);
		}
		return inputValue;
	}

	public static ZString AppendFEIfElectronicDocument(IElectronicFolderSupporter electronicFolderSupporter, ZString locationOfGoods)
	{
		Argument.NotNull(electronicFolderSupporter, nameof(electronicFolderSupporter));

		const char dash = '-';
		return electronicFolderSupporter.UseElectronicFolder ? ZString.Format("{0}{1}FE", locationOfGoods, dash).TrimStart(dash) : locationOfGoods;
	}

	public static ZDecimal? GetValueOrNullIfZero(this ZDecimal amount) => amount != ZDecimal.Zero ? amount : null;

	public static ZBool? GetTrueOrNullIfFalse(this ZBool value) => value ? value : null;

	public static ZBool IsFeeIncludedInMessageSending(IFee fee)
	{
		switch (fee.MethodOfPayment)
		{
			case DutyMethodOfPayment.ImmediatePaymentInCashA:
			case DutyMethodOfPayment.DeferredPaymentCustomsProcedureF:
			case DutyMethodOfPayment.DeferredPaymentVatProcedureG:
			case DutyMethodOfPayment.AgentGeneralGuaranteeAccountT:
				return true;
			case DutyMethodOfPayment.SecurityDepositDeferredPaymentR:
				return (fee.ChargeType == RefCusRateCodes.TemporaryAntiDumpingDuty || fee.ChargeType == RefCusRateCodes.TemportaryCountervailingDuty);
			default:
				return false;
		}
	}
}
