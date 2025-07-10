using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public sealed class RefDataTestHelper
{
	public RefDataTestHelper(BusinessObjectFactory factory)
	{
		Factory = factory;
		Helper = new UniversalReferenceTestDataHelper(factory);
	}

	BusinessObjectFactory Factory { get; }

	UniversalReferenceTestDataHelper Helper { get; }

	const string DefaultDataGrouping = Core.Constants.CountryCodes.Switzerland;

	public CodeListBuilder CreateCodeList(string codeType, string dataGrouping = DefaultDataGrouping)
	{
		return new CodeListBuilder(this, codeType, dataGrouping);
	}

	public abstract class BaseBuilder
	{
		protected BaseBuilder(RefDataTestHelper helper, string dataGrouping)
		{
			Helper = helper;
			DataGrouping = dataGrouping;
		}

		protected RefDataTestHelper Helper { get; }
		protected string DataGrouping { get; }
		protected UniversalReferenceTestDataHelper UniveralHelper => Helper.Helper;

		public void Save()
		{
			Helper.Factory.Save();
		}
	}

	public sealed class CodeListBuilder : BaseBuilder
	{
		internal CodeListBuilder(RefDataTestHelper helper, string codeType, string dataGrouping) : base(helper, dataGrouping)
		{
			CodeType = codeType;
			UniveralHelper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			UniveralHelper.CreateNewOrGetExistingCusCodeType(codeType, $"CodeType-{codeType}", dataGrouping);
		}
		string CodeType { get; }

		RefCusCodeList currentCodeList;

		public CodeListBuilder CreateCode(string code)
		{
			currentCodeList = UniveralHelper.CreateCusCodeList(DataGrouping, CodeType, code, $"Code-{code}", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			return this;
		}

		public CodeListBuilder WithDescription(string description)
		{
			currentCodeList.ZZD_Description = description;
			return this;
		}

		public CodeListBuilder WithAttribute(string name, string value)
		{
			UniveralHelper.CreateCusCodeListAttribute(currentCodeList.PK, name, value);
			return this;
		}

		public CodeListBuilder WithValidity(ZDateTime startDate, ZDateTime endDate)
		{
			currentCodeList.ZZD_StartDate = startDate;
			currentCodeList.ZZD_EndDate = endDate;
			return this;
		}
	}
}
