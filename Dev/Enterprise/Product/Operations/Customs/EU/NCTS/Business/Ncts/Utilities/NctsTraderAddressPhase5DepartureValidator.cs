using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsTraderAddressPhase5DepartureValidator
	{
		internal NctsTraderAddressPhase5DepartureValidator(JobDocAddress address, string traderName)
		{
			this.address = Argument.NotNull(address, nameof(address));
			this.traderName = Argument.NotNullOrEmpty(traderName, nameof(traderName));
		}

		internal void CheckRuleE1104_1(NctsHeader nctsHeader, ZPropertyInfo targetPropertyInfo)
		{
			Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));
			if ((nctsHeader?.Configuration.ValidationRuleConfiguration?.IsRuleE1104_1Active ?? false) && nctsHeader.IsPhase5Departure)
			{
				ValidateMaximumLengthCustomsFields(nctsHeader, targetPropertyInfo, ValidationRuleCodeConstants.E1104_1.GetRuleCodeMessagePrefix());
			}
		}

		void ValidateMaximumLengthCustomsFields(NctsHeader nctsHeader, ZPropertyInfo targetPropertyInfo, string ruleCode)
		{
			ValidateCompanyNameMaxLength(nctsHeader, targetPropertyInfo, ruleCode);
			ValidateAddressMaxLength(nctsHeader, targetPropertyInfo, ruleCode);
		}

		void ValidateCompanyNameMaxLength(NctsHeader nctsHeader, ZPropertyInfo targetPropertyInfo, string ruleCode)
		{
			var nameCaption = Res.GetString("F182FC11-6EE8-410E-9636-D6D6B4CF72EC", "name");
			AddWarningIfExceedsCustomsMaxLength(targetPropertyInfo, address.E2_CompanyName, GetCompanyNameMaxLength(nctsHeader), nameCaption, ruleCode);
		}

		void ValidateAddressMaxLength(NctsHeader nctsHeader, ZPropertyInfo targetPropertyInfo, string ruleCode)
		{
			var addressCaption = Res.GetString("DEFD4A97-FDDC-483B-ABE5-1DADD9612BA7", "address");
			AddWarningIfExceedsCustomsMaxLength(targetPropertyInfo, GetAddressAsASingleLineForCustomsMessage(address), GetAddressMaxLength(nctsHeader), addressCaption, ruleCode);
		}

		void AddWarningIfExceedsCustomsMaxLength(ZPropertyInfo targetPropertyInfo, ZString value, int customsMaxLength, string fieldName, string ruleCode)
		{
			if (value.Length > customsMaxLength)
			{
				targetPropertyInfo.AddWarning(FieldIsLongerThanMaximumLength(ruleCode, traderName, fieldName, customsMaxLength));
			}
		}

		int GetCompanyNameMaxLength(NctsHeader header) => header.IsInPhase5TransitionPeriod
			? NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.Name
			: NctsConstants.CustomsFieldMaxLength.Trader.Name;

		int GetAddressMaxLength(NctsHeader header) => header.IsInPhase5TransitionPeriod
			? NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.Address
			: NctsConstants.CustomsFieldMaxLength.Trader.Address;

		string FieldIsLongerThanMaximumLength(string ruleCode, string traderName, string fieldName, int maximumLength)
			=> Res.GetString("AACA8369-DFC0-4BF5-BD33-67EDB29A9736", "{0} {1} {2} is longer than {3} characters, it will be truncated in the message.", ruleCode, traderName, fieldName, maximumLength);

		string GetAddressAsASingleLineForCustomsMessage(IDocAddress docAddress)
			=> FormattableString.Invariant($"{docAddress.E2_Address1.Trim()} {docAddress.E2_Address2.Trim()}");

		readonly JobDocAddress address;
		readonly string traderName;
	}
}
