using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public abstract class CusEntryNumbersProvider : NonPersistentBusinessObject
{
	protected CusEntryNumbersProvider(BusinessObjectFactory factory, BusinessObject parentBizObj, ZString countryCode) : base(factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.parentBizObj = Argument.NotNull(parentBizObj, nameof(parentBizObj));
		this.countryCode = Argument.NotNullOrEmpty(countryCode, nameof(countryCode));
	}

	readonly BusinessObjectFactory factory;
	readonly BusinessObject parentBizObj;
	readonly ZString countryCode;

	public CusEntryNumber RegistrationInfo => factory.GetValue(ref registrationInfoCache, () => CusEntryNumber.Load(parentBizObj, CusEntryNumberConstants.EntryTypes.RegistrationNumber, countryCode));
	CachedProperty<CusEntryNumber> registrationInfoCache;

	public RegCusEntryNumberWrapper RegistrationInfoWrapper => factory.GetValue(ref registrationInfoWrapperCache, () => RegCusEntryNumberWrapper.Load(RegistrationInfo));
	CachedProperty<RegCusEntryNumberWrapper> registrationInfoWrapperCache;

	public CusEntryNumber Irildes => factory.GetValue(ref irildesCache, () => CusEntryNumber.Load(parentBizObj, CusEntryNumberConstants.EntryTypes.Irildes, countryCode));
	CachedProperty<CusEntryNumber> irildesCache;

	public ExitAndReleaseCusEntryNumberWrapper IrildesWrapper => factory.GetValue(ref irildesWrapperCache, () => ExitAndReleaseCusEntryNumberWrapper.Load(Irildes));
	CachedProperty<ExitAndReleaseCusEntryNumberWrapper> irildesWrapperCache;

	public CusEntryNumber Ivisto => factory.GetValue(ref ivistoCache, () => CusEntryNumber.Load(parentBizObj, CusEntryNumberConstants.EntryTypes.Ivisto, countryCode));
	CachedProperty<CusEntryNumber> ivistoCache;

	public ExitAndReleaseCusEntryNumberWrapper IvistoWrapper => factory.GetValue(ref ivistoWrapperCache, () => ExitAndReleaseCusEntryNumberWrapper.Load(Ivisto));
	CachedProperty<ExitAndReleaseCusEntryNumberWrapper> ivistoWrapperCache;

	public CusEntryNumber ReleaseInfo => factory.GetValue(ref releaseInfoCache, () => CusEntryNumber.Load(parentBizObj, CusEntryNumberConstants.EntryTypes.ClereanceCode, countryCode));
	CachedProperty<CusEntryNumber> releaseInfoCache;

	public CusEntryNumber InsertOrUpdateReleaseCode(ZString releaseCode, ZDateTime releaseDate)
	{
		return InsertOrUpdateEntryNum(CusEntryNumberConstants.EntryTypes.ClereanceCode, releaseCode.Left(CusEntryNumber.Schema.CE_EntryNumMaxLength), releaseDate);
	}

	public CusEntryNumber InsertOrUpdateManualReleaseCode(ZString releaseCode, ZDateTime releaseDate)
	{
		var cusEntryNumber = InsertOrUpdateReleaseCode(releaseCode, releaseDate);
		cusEntryNumber.CE_EntryIsSystemGenerated = false;

		return cusEntryNumber;
	}

	public CusEntryNumber InsertOrUpdateEntryNum(ZString entryType, ZString entryNum, ZDateTime releaseDate)
	{
		var cusEntryNumber = factory.Load<CusEntryNumber>(GetCusEntryNumberQuery(entryType))?.LastOrDefault();
		if (cusEntryNumber == null)
		{
			cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_ParentID = parentBizObj.PK;
			cusEntryNumber.CE_ParentTable = parentBizObj.TableName;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_RN_NKCountryCode = countryCode;
		}
		cusEntryNumber.CE_EntryNum = entryNum;
		cusEntryNumber.CE_IssueDate = releaseDate;
		return cusEntryNumber;
	}

	ZQuery GetCusEntryNumberQuery(ZString entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, parentBizObj.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
		query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
		query.OrderBy = CusEntryNumSchema.CE_IssueDate.Name;
		return query;
	}
}
