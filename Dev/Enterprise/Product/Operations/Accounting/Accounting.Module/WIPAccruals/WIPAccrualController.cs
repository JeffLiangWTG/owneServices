using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract class WIPAccrualController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public WIPAccrualController()
		{
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed || Env.Security.ReverseSingleWipOrAccrual.IsAllowed)
			{
				ShowLoadedForm(sourceEntity, FormAction.Delete);
				if (IsMultipleReversing)
				{
					var sourceEntityForReversing = GetNewFactory().Load<BaseWIPAccrual>(GetCurrentBusinessEntity(sourceEntity).Identifier);
					CheckReversingErrors(sourceEntityForReversing);
					if (!sourceEntityForReversing.HasRowErrors)
					{
						sourceEntityForReversing.Reverse(true);
						sourceEntityForReversing.IsReversing = true;
						sourceEntityForReversing.Factory.SetContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing);
					}
					MultipleReversingProvider.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(sourceEntityForReversing));
				}
			}
			else
			{
				CheckPointForDelete.ShowError();
				LastShownForm = null;
			}

			return LastShownForm;
		}

		void CheckReversingErrors(BaseWIPAccrual sourceEntityForReversing)
		{
			if (sourceEntityForReversing.IsReversed)
			{
				sourceEntityForReversing.AddRowError(sourceEntityForReversing.AlreadyReversedErrorMessage);
				sourceEntityForReversing.MultipleReversingErrors.Add(sourceEntityForReversing.AlreadyReversedErrorMessage);
			}
			else if (sourceEntityForReversing.AL_LineType == TransactionLineTypes.Accrual)
			{
				var errorMessageForMissingAccruals = MultipleReversingProvider.GetMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccrualsErrorMessage(sourceEntityForReversing.PK);
				if (!errorMessageForMissingAccruals.IsEmpty)
				{
					sourceEntityForReversing.AddRowError(errorMessageForMissingAccruals);
					sourceEntityForReversing.MultipleReversingErrors.Add(errorMessageForMissingAccruals);
				}
			}
		}

		#region Multiple Reversing

		protected MultipleReversingProviderForLine MultipleReversingProvider;

		protected bool IsMultipleReversing => MultipleReversingProvider != null;

		IBusiness GetCurrentBusinessEntity(IBusiness businessEntity) => IsMultipleReversing ? MultipleReversingProvider.Current : businessEntity;

		protected sealed override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity) => sourceEntity is MultipleReversingProviderForLine ? sourceEntity : base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);

		protected override IZForm GetForm(IBusiness businessEntity) => IsMultipleReversing ? new MultipleReversingForLineForm(MultipleReversingProvider) : GetFormCore(businessEntity);

		protected abstract IZForm GetFormCore(IBusiness businessEntity);

		public sealed override ControllerID ID => IsMultipleReversing ? ControllerIDs.WIP : IDCore;

		protected abstract ControllerID IDCore { get; }

		protected override BusinessObjectFactory GetNewFactory()
		{
			return IsMultipleReversing ? MultipleReversingProvider.Factory : base.GetNewFactory();
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			MultipleReversingProvider = selectedBusinessObjects.Length == 1 ? selectedBusinessObjects[0] as MultipleReversingProviderForLine : null;
			if (IsMultipleReversing)
			{
				ShowDeleteForm(MultipleReversingProvider);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("2c133a3c-a8bf-4336-8d0e-4f3171550751", "The action for multiple objects is not implemented in this version."));
			}
		}

		#endregion
	}
}
