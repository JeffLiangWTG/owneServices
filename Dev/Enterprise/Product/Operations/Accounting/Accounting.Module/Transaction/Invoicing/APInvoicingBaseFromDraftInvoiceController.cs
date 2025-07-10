using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Newtonsoft.Json;

namespace Enterprise.Accounting.Module
{
	public abstract class APInvoicingBaseFromDraftInvoiceController<T> : ZController
		where T : InvoicingBase
	{
		public override ModuleIdentifier ModuleID => null;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public sealed override Type TypeOfTopLevelBusinessObject => typeof(T);

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (!CheckPointForEdit.IsAllowed)
			{
				var message = Res.GetString("F244A58B-DBF9-4E8B-9496-C9A23BF6D484"
					, "{0}\r\n\r\n{1}"
					, SecurityCore.SecurityErrorMessage
					, CheckPointForEdit.DisplayTextPathToSecurityRight);
				var title = Res.GetString("ABDA0540-9F54-4CC3-ACE7-75F92598A592"
					, "Access Denied: {0}"
					, CheckPointForEdit.DisplayText);

				Globals.Message.ShowError(message, title);

				LastShownForm = null;
				return null;
			}

			return base.ShowEditForm(sourceEntity);
		}

		protected abstract ZString ExpectedTransactionType { get; }

		protected abstract IZForm GetEditFormCore(InvoicingBase newAP);

		protected sealed override SecurityCheckpoint CheckPointForDelete => throw new InvalidOperationException();

		protected sealed override SecurityCheckpoint CheckPointForNew => throw new InvalidOperationException();

		protected sealed override SecurityCheckpoint CheckPointForView => throw new InvalidOperationException();

		protected sealed override SecurityCheckpoint CheckPointForEdit
			=> SecuritySettings.New;

		BasicSecuritySettings SecuritySettings
		{
			get
			{
				if (securitySettings == null)
				{
					securitySettings = ObjectFactory
						.Get<IInvoiceSecurityChecker>()
						.GetBasicSecuritySettings(LedgerTypes.AccountsPayable, ExpectedTransactionType);
				}

				return securitySettings;
			}
		}
		BasicSecuritySettings securitySettings;

		protected sealed override IZForm GetForm(IBusiness businessEntity)
		{
			var draft = (AccDraftInvoiceHeader)businessEntity;
			if (draft.AIH_TransactionType != ExpectedTransactionType)
			{
				throw new InvalidOperationException($@"Incorrect AccDraftInvoiceHeader Type is allocated.
{GetDraftInvoiceDetailInfo(draft)}");
			}

			if (!TryParseDTO(ArgsForNewForm?.FirstOrDefault(), out var additionalDto))
			{
				//Maybe we can return error or something message here.
			}

			var newAP = ObjectFactory.Get<IAPReconciliationPoster>().PostFromDraftInvoice<T>(draft
				, additionalDto
				, out var validationError
				, out var reconciliationError);

			if (validationError != default)
			{
				Globals.Message.ShowError(validationError.Msg, validationError.Caption);
				LastShownForm = null;
				return null;
			}

			if (reconciliationError != default)
			{
				Globals.Message.ShowError(reconciliationError.Msg, reconciliationError.Caption);
			}

			if (newAP == null)
			{
				LastShownForm = null;
				return null;
			}

			return GetEditFormCore(newAP);
		}

		bool TryParseDTO(string jsonStr, out PosterConfigurationDTO dto)
		{
			try
			{
				dto = JsonConvert.DeserializeObject<PosterConfigurationDTO>(jsonStr);
				return dto != null;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				dto = null;
				return false;
			}
		}

		protected sealed override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var draft = factory.Load<AccDraftInvoiceHeader>(sourceEntityPK);
			if (draft == null)
			{
				Globals.Message.Show(Res.GetString("F8A2102E-AF99-46CF-A806-C12E1434E945", @"Unable to open '{0}' form for Draft Invoice.
Please ensure that CargoWise is open in the same environment as the Invoice Processing Portal, then try to post the Draft Invoice again.
If the issue persists, please raise an eRequest.", EditFormCaption));
			}
			return draft;
		}

		protected abstract string EditFormCaption { get; }

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
			=> throw new InvalidOperationException($"{GetType().Name} is not designed to have view form.");

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
			=> throw new InvalidOperationException($"{GetType().Name} is not designed to have delete form.");

		public override IZForm ShowNewForm()
			=> throw new InvalidOperationException($"{GetType().Name} is not designed to have new form.");

		protected override IZForm ShowCopyForm(BusinessObject inMemorySourceEntity, CopyOfBusinessObject returnsNewBusinessEntity)
			=> throw new InvalidOperationException($"{GetType().Name} designed to have copy form.");

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => throw new InvalidOperationException();

		string GetDraftInvoiceDetailInfo(AccDraftInvoiceHeader draft)
			=> $@"Company:{draft.Company.GC_Code} - {draft.Company.GC_Name}
Transaction Type: {draft.AIH_TransactionType}
InternalReference:{draft.AIH_InternalReference}";
	}
}
