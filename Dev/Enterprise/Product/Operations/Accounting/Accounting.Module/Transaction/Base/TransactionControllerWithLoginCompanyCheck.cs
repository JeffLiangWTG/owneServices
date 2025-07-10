using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Transaction.Base
{
	public abstract partial class TransactionControllerWithLoginCompanyCheck : ZController
	{
		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (CheckLoginCompanyNotMatch(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowViewForm(sourceEntity);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (CheckLoginCompanyNotMatch(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowEditForm(sourceEntity);
		}

		protected virtual bool ShouldCheckLoginCompanyMatch => true;

		bool CheckLoginCompanyNotMatch(BusinessObject sourceEntity)
		{
			var result = ShouldCheckLoginCompanyMatch;
			if (result)
			{
				var source = sourceEntity as TransactionHeader;
				if (source != null)
				{
					var company = source.Company;
					if (result = company != null && company.PK != Env.CurrentCompany.PK)
					{
						Globals.Message.ShowError(
								ResString.GetMultilingualString("FEF7B880-11CF-45FB-9DED-C543EA9B4E0F", "This Transaction is posted into the ledgers of another company on this database. You must login to the following company to view this transaction: {0} - {1}.", company.GC_Code, company.GC_Name),
								ResString.GetMultilingualString("B71F8FFC-91EA-4990-A212-3425686F82C0", "Access Denied: Incorrect login company")
							);
					}
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		protected sealed override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			if (sourceEntity == null)
			{
				throw new ArgumentException("SourceEntity");
			}

			var result = GetLoadedBusinessEntityInLocalFactoryCore(sourceEntity);
			return CheckControllerID(result).IsValid ? result : null;
		}

		protected void ReportInvalidDataSource(IBusiness businessEntity)
		{
			var (isValid, correctId) = CheckControllerID(businessEntity);
			if (!isValid)
			{
				TransactionControllerErrorReportHelper.ReportInvalidDataSource(this, correctId, businessEntity);
			}
		}

		(bool IsValid, ControllerID CorrectId) CheckControllerID(IBusiness businessEntity)
		{
			var controller = this as INavigationControllerIDProvider;
			if (controller != null)
			{
				var correctId = controller.GetValidControllerID(businessEntity);
				if (CheckControllerIDMismatch(correctId))
				{
					return (false, correctId);
				}
			}
			return (true, ID);
		}

		protected virtual bool CheckControllerIDMismatch(ControllerID controllerID) => false;

#if DEBUG
		public IZForm GetForm_ForTest(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}
#endif

		protected virtual IBusiness GetLoadedBusinessEntityInLocalFactoryCore(IBusiness sourceEntity)
		{
			return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}
	}
}
