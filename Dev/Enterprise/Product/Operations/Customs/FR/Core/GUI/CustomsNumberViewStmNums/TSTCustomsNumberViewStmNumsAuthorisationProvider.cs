using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public class TSTCustomsNumberViewStmNumsAuthorisationProvider : CustomsNumberViewStmNumsAuthorisationProvider, ICustomsNumberViewStmNumsGuiProvider
	{
		public TSTCustomsNumberViewStmNumsAuthorisationProvider(BusinessObjectFactory factory, ZGuid ownerPk) : base(factory, CusAuthorisationHeaderCustomsNumberProviderKeyList.Codes.TemporaryStorage, ownerPk)
		{
		}

		public Control GetUserControl()
		{
			return new TSTCustomsNumberViewStmNumsUserControl(CustomsNumberWrappers);
		}

		public Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper)
		{
			return new TSTCustomsNumberViewStmNumsEditorForm(wrapper);
		}

		protected override CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
		{
			return new TSTCustomsNumberViewStmNumsLookups(stmNums);
		}

		protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
		{
			return new TSTCustomsNumberViewStmNumsSetting(Parent, rangeType);
		}

		public new TSTCustomsNumberViewStmNumsWrapperCollection CustomsNumberWrappers => (TSTCustomsNumberViewStmNumsWrapperCollection)base.CustomsNumberWrappers;

		protected override CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers()
		{
			return new TSTCustomsNumberViewStmNumsWrapperCollection(CustomsNumbers);
		}

		protected override Type WrapperType => typeof(TSTCustomsNumberViewStmNumsWrapper);

		protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
		{
			return new TSTCustomsNumberViewStmNumsWrapper(stmNums);
		}

		protected override CustomsNumberViewStmNumsValidation GetNewValidation(CustomsNumberViewStmNums stmNums)
		{
			return new TSTCustomsNumberViewStmNumsValidation(stmNums);
		}
	}
}
