using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public abstract class CustomsMessageFountainProvider : ICustomsMessageFountainProvider
{
	public ZString FountainPrefix => fountainPrefix ?? (fountainPrefix = GetFountainPrefixForDate(ZDate.Today));
	string fountainPrefix;

	public ZString DeclarantTaxNumber => declarantTaxNumber ?? (declarantTaxNumber = CustomsCredentialHelper.GetAccountFromNode(Node)?.DeclarantTaxNumber ?? ZString.Empty);
	string declarantTaxNumber;

	public ZString Node => node ?? (node = NodeCore);
	string node;

	public ZString FountainType => fountainType ?? (fountainType = FountainTypeCore);
	string fountainType;

	public ITCustomsNumberViewStmNumsWrapper Wrapper
	{
		get
		{
			if (wrapper == null && !DeclarantTaxNumber.IsEmpty)
			{
				var foundNumberRange = GetNumberRanges(FountainPrefix).Where(x => x.SN_Value < x.SN_MaximumValue).FirstOrDefault();
				wrapper = foundNumberRange != null ? new ITCustomsNumberViewStmNumsWrapper(foundNumberRange) : null;
			}
			return wrapper;
		}
	}
	ITCustomsNumberViewStmNumsWrapper wrapper;

	public GlbCompany Company => company ?? (company = CompanyCore ?? GlbCompany.CurrentCompany);
	GlbCompany company;

	public INumberFountainProxy TryGetNumberFountain() => Wrapper?.StmNums?.TryGetNumberFountain();

	public ZBool HasClonableNumberRanges
	{
		get
		{
			if (!hasClonableNumberRanges.HasValue)
			{
				hasClonableNumberRanges = !DeclarantTaxNumber.IsEmpty && ClonableNumberRanges.Any();
			}
			return hasClonableNumberRanges.Value;
		}
	}
	bool? hasClonableNumberRanges;

	public void TryCloneLastYearNumberRanges()
	{
		if (HasClonableNumberRanges)
		{
			var separateFactory = new BusinessObjectFactory();
			var companyOnSeparateFactory = separateFactory.Load<GlbCompany>(Company.PK);
			foreach (var numberRangeToClone in ClonableNumberRanges)
			{
				var clonedNumberRange = companyOnSeparateFactory.CustomsNumberProvider.CustomsNumbers.AddNew();
				clonedNumberRange.SN_Type = fountainType;
				clonedNumberRange.SN_MinimumValue = numberRangeToClone.SN_MinimumValue;
				clonedNumberRange.SN_MaximumValue = numberRangeToClone.SN_MaximumValue;
				clonedNumberRange.SN_Value = numberRangeToClone.SN_MinimumValue;
				clonedNumberRange.SN_FountainName = ITCustomsNumberViewStmNumsWrapper.GenerateFountainName(ZDate.Today.Year, DeclarantTaxNumber);
				clonedNumberRange.SN_Prefix = clonedNumberRange.SN_FountainName;
			}
			separateFactory.Save();
			ResetCachedValues();
		}
	}

	#region Implementation

	protected abstract ZString NodeCore { get; }
	protected abstract ZString FountainTypeCore { get; }
	protected abstract GlbCompany CompanyCore { get; }

	CustomsNumberViewStmNums[] AllNumberRanges => allNumberRanges ?? (allNumberRanges = Company.CustomsNumberProvider?.CustomsNumbers?.Cast<CustomsNumberViewStmNums>()?.ToArray() ?? Array.Empty<CustomsNumberViewStmNums>());
	CustomsNumberViewStmNums[] allNumberRanges;

	CustomsNumberViewStmNums[] ClonableNumberRanges => clonableNumberRanges ?? (clonableNumberRanges = GetNumberRanges(LastYearFountainPrefix));
	CustomsNumberViewStmNums[] clonableNumberRanges;

	ZString LastYearFountainPrefix => lastYearFountainPrefix ?? (lastYearFountainPrefix = GetFountainPrefixForDate(ZDate.Today.AddYears(-1)));
	string lastYearFountainPrefix;

	CustomsNumberViewStmNums[] GetNumberRanges(ZString snPrefix) => AllNumberRanges.Where(x => x.SN_Prefix.StartsWith(snPrefix, StringComparison.OrdinalIgnoreCase) && x.SN_Type == FountainType).ToArray();

	ZString GetFountainPrefixForDate(ZDate date) => FormattableString.Invariant($"{date.Year}:{DeclarantTaxNumber}");

	void ResetCachedValues()
	{
		allNumberRanges = null;
		clonableNumberRanges = null;
		wrapper = null;
		company?.Reload();
	}

	#endregion
}
