using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public abstract class TSCustomsNumberViewStmNumsBusinessProvider : CustomsNumberViewStmNumsBusinessProvider
{
	protected TSCustomsNumberViewStmNumsBusinessProvider(BusinessObjectFactory factory, ZString countryCode, ZGuid ownerPk) : base(factory, countryCode, ownerPk)
	{
	}
	public CusTempStorageRegPremises Premises => (CusTempStorageRegPremises)Parent;

	public new TSCustomsNumberViewStmNumsWrapperCollection CustomsNumberWrappers => (TSCustomsNumberViewStmNumsWrapperCollection)base.CustomsNumberWrappers;

	public TSCustomsNumberViewStmNumsWrapper ActiveWrapper
	{
		get
		{
			if (activeWrapper == null || !activeWrapper.IsActive)
			{
				activeWrapper = CustomsNumberWrappers.Cast<TSCustomsNumberViewStmNumsWrapper>()
					.FirstOrDefault(x => x.IsActive);
			}
			return activeWrapper;
		}
	}
	TSCustomsNumberViewStmNumsWrapper activeWrapper;

	protected override CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers()
		=> new TSCustomsNumberViewStmNumsWrapperCollection(CustomsNumbers);

	protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
		=> new TSCustomsNumberViewStmNumsSetting(Premises, rangeType);

	protected override CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
		=> new TSCustomsNumberViewStmNumsLookups(stmNums);

	protected override ICustomsNumberViewStmNumsParent GetParentCore(BusinessObjectFactory factory, ZGuid ownerPk)
		=> factory.Load<CusTempStorageRegPremises>(ownerPk);

	protected override BusinessObject GetOwnerCore(BusinessObjectFactory factory, ZGuid ownerPk)
		=> factory.Load<CusTempStorageRegPremises>(ownerPk);

	protected override ZString GetOwnerTypeCore(CustomsNumberViewStmNums stmNums)
		=> Res.GetString("37DAD23A-A5BE-419F-AA70-7B0A1F582A45", "Premises");

	protected override ZString GetOwnerForDisplayCore(BusinessObject owner)
		=> owner is CusTempStorageRegPremises premises ? premises.SRP_Code : ZString.Empty;

	protected override ZString GetProviderKeyCore() => ProviderKey;

	protected override Type WrapperType => typeof(TSCustomsNumberViewStmNumsWrapper);

	protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
		=> new TSCustomsNumberViewStmNumsWrapper(stmNums);

	protected override IBusinessObjectCollection GetOwnerCollectionCore(BusinessObjectFactory factory, CustomsNumberViewStmNums stmNums)
		=> new CusTempStorageRegPremisesCollection(factory);
}
