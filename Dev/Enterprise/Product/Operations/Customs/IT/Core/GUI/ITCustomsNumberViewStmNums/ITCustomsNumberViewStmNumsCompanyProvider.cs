using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.IT.GUI;

public class ITCustomsNumberViewStmNumsCompanyProvider : CustomsNumberViewStmNumsCompanyProvider, ICustomsNumberViewStmNumsGuiProvider
{
	public ITCustomsNumberViewStmNumsCompanyProvider(BusinessObjectFactory factory, ZGuid ownerPk)
		: base(factory, Core.Constants.CountryCodes.Italy, ownerPk)
	{ }

	public Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper)
	{
		return new ITCustomsNumberViewStmNumsEditorForm((ITCustomsNumberViewStmNumsWrapper)wrapper);
	}

	public new ITCustomsNumberViewStmNumsWrapperCollection CustomsNumberWrappers => (ITCustomsNumberViewStmNumsWrapperCollection)base.CustomsNumberWrappers;

	protected override CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers()
	{
		return new ITCustomsNumberViewStmNumsWrapperCollection(CustomsNumbers);
	}

	public Control GetUserControl()
	{
		return new ITCustomsNumberViewStmNumsUserControl(CustomsNumberWrappers);
	}

	protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
	{
		return new ITCustomsNumberViewStmNumsSetting(Company, rangeType);
	}

	protected override CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
	{
		return new ITCustomsNumberViewStmNumsLookups(stmNums);
	}

	protected override Type WrapperType => typeof(ITCustomsNumberViewStmNumsWrapper);

	protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
	{
		return new ITCustomsNumberViewStmNumsWrapper(stmNums);
	}

	protected override ZString GetDetailCore(CustomsNumberViewStmNumsWrapper wrapper)
	{
		var itWrapper = (ITCustomsNumberViewStmNumsWrapper)wrapper;
		return Res.GetString("F0621481-5AAB-4034-B12D-0E3F06D20344", "Range Type: {0}, Year: {1}, Applies To: {2}", wrapper.SN_Type, itWrapper.YearOfApplicability, itWrapper.AppliesTo);
	}
}
