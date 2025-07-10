using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class ConvertToStandAloneDeclarationMenuItem : ZMenuItem
	{
		public ConvertToStandAloneDeclarationMenuItem(Func<BusinessObject[]> getSelectedElements, bool shouldAutomaticallySaveAddressChanges = false)
			: base(ConvertToStandAloneDeclarationCaption)
		{
			this.getSelectedElements = getSelectedElements;
			this.shouldAutomaticallySaveAddressChanges = shouldAutomaticallySaveAddressChanges;
			Click += new EventHandler((_, _) => MenuAction());
		}

		readonly Func<BusinessObject[]> getSelectedElements;
		readonly bool shouldAutomaticallySaveAddressChanges;

		public Action MenuAction => () =>
		{
			var selectedBills = getSelectedElements().Cast<AsycudaBill>();
			if (CanProceedWithConversion(selectedBills) && PromptToMatchOrCreateConsigneeAndShipperOrgsIfNotExist(selectedBills))
			{
				var convertedBillCount = 0;

				if (selectedBills.Count() == 1)
				{
					StandAloneDeclarationConverter.OnCompleted -= StandAloneDeclarationConverter_OpenForm;
					StandAloneDeclarationConverter.OnCompleted += StandAloneDeclarationConverter_OpenForm;
				}
				else
				{
					StandAloneDeclarationConverter.OnCompleted -= TrackConversionProgress;
					StandAloneDeclarationConverter.OnCompleted += TrackConversionProgress;
				}

				StandAloneDeclarationConverter.TryConvert(selectedBills);

				void TrackConversionProgress(object sender, EventArgs e)
				{
					convertedBillCount++;

					if (convertedBillCount == selectedBills.Count())
					{
						OpenDeclarationCreatedNotificationWindow(sender, e);
						StandAloneDeclarationConverter.OnCompleted -= TrackConversionProgress;
					}
				}
			}
		};

		bool CanProceedWithConversion(IEnumerable<AsycudaBill> bills)
		{
			return HasBills(bills) && (!(HasAnyChanges(bills) || HasExistingDeclarations(bills) || HasReleasedStatus(bills)));
		}

		bool HasBills(IEnumerable<AsycudaBill> bills)
		{
			if (bills == null || !bills.Any())
			{
				Globals.Message.ShowError(Res.GetString("a37babb4-0516-4037-8f85-ba1087674075", "No bills have been selected. Please select bill(s) before creating Stand Alone Declaration."));
				return false;
			}

			return true;
		}

		bool HasAnyChanges(IEnumerable<AsycudaBill> bills)
		{
			return HasBillsFailedOnValidation(bills.Where(b => b.HasChanges || b.Header.HasChanges), SaveAnyChangesMessage, (numbers, msg) => msg);
		}

		bool HasExistingDeclarations(IEnumerable<AsycudaBill> bills)
		{
			return HasBillsFailedOnValidation(bills.Where(b => b.HasBeenConvertedToStandaloneDeclaration), ExistingDeclarationMessage, FormulateErrorMessageWithBillNumbers);
		}

		bool HasReleasedStatus(IEnumerable<AsycudaBill> bills)
		{
			return HasBillsFailedOnValidation(bills.Where(b => b.ABL_BillStatus == b.GetReleasedStatus()), GetReleasedStatusMessage(bills.First()?.GetReleasedStatusDescription()), FormulateErrorMessageWithBillNumbers);
		}

		bool HasBillsFailedOnValidation(IEnumerable<AsycudaBill> invalidBills, string errorMessage, Func<List<ZString>, string, string> formulateErrorMessage)
		{
			if (invalidBills.Any())
			{
				var billNumbers = invalidBills.Select(b => b.ABL_BillNumber).ToList();
				Globals.Message.ShowError(formulateErrorMessage(billNumbers, errorMessage));
				return true;
			}

			return false;
		}

		string FormulateErrorMessageWithBillNumbers(List<ZString> billNumbers, string errorMessage)
		{
			errorMessage += System.Environment.NewLine;
			if (billNumbers.Count <= MaximumBillNumbersInErrorMessage)
			{
				errorMessage += string.Join(System.Environment.NewLine, billNumbers);
			}
			else
			{
				errorMessage += $"{string.Join(System.Environment.NewLine, billNumbers.GetRange(0, MaximumBillNumbersInErrorMessage - 2))}{System.Environment.NewLine}...{System.Environment.NewLine}{billNumbers.LastOrDefault()}";
			}

			return errorMessage;
		}

		bool PromptToMatchOrCreateConsigneeAndShipperOrgsIfNotExist(IEnumerable<AsycudaBill> bills)
		{
			var canContinueWithConversion = true;
			if (IsAnyBillWithoutImporterOrExporterOrgs(bills) && UserChooseToConvertToOrganization(bills))
			{
				using (var viewModels = new AsycudaBillSimilarAddressViewModelCollection(bills))
				using (var similarAddressSelectionForm = new EUH7SimilarAddressesSelectionForm(viewModels))
				{
					ZFormModaliser.ShowDialogWithoutDispose(similarAddressSelectionForm);
					if (similarAddressSelectionForm.DialogResult == DialogResult.OK)
					{
						if (viewModels.AnyBillLinkedWithOrg)
						{
							canContinueWithConversion = false;
							try
							{
								if (shouldAutomaticallySaveAddressChanges)
								{
									bills.First().Factory.Save();
									canContinueWithConversion = true;
								}
								else
								{
									Globals.Message.Show(OrgLinkedMessage);
								}
							}
							catch (Exception e) when (!e.IsCriticalException())
							{
								ZExceptionReporting.HandleSaveException(e, new NotificationHandlerWithMessageOverride());
							}
						}
					}
				}
			}

			return canContinueWithConversion;
		}

		bool IsAnyBillWithoutImporterOrExporterOrgs(IEnumerable<AsycudaBill> bills)
		{
			return bills.Any(b => b.ABL_OA_Consignee.IsEmpty || b.ABL_OA_Shipper.IsEmpty);
		}

		bool UserChooseToConvertToOrganization(IEnumerable<AsycudaBill> bills)
		{
			var message = string.Empty;

			if (bills.Count() == 1)
			{
				if (bills.First().ABL_OA_Consignee.IsEmpty && bills.First().ABL_OA_Shipper.IsEmpty)
				{
					message = ImporterAndExporterOrgNotFoundPrompt;
				}
				else if (bills.First().ABL_OA_Consignee.IsEmpty)
				{
					message = ImporterOrgNotFoundPrompt;
				}
				else if (bills.First().ABL_OA_Shipper.IsEmpty)
				{
					message = ExporterOrgNotFoundPrompt;
				}
			}
			else
			{
				message = ImporterOrExporterOrgNotFoundPrompt;
			}

			return Globals.Message.Show(message, OrgsNotFoundCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}

		void OpenDeclarationCreatedNotificationWindow(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(BillConversionSuccessMessage);
		}

		void StandAloneDeclarationConverter_OpenForm(object sender, EventArgs e)
		{
			if (sender is IBusiness declaration)
			{
				DeclarationController.ShowFormForNewEntity(declaration);
			}
			StandAloneDeclarationConverter.OnCompleted -= StandAloneDeclarationConverter_OpenForm;
		}

		void StandAloneDeclarationConverter_ShowError(object sender, EventArgs e)
		{
			if (sender is string errorMessage)
			{
				Globals.Message.ShowError(errorMessage);
			}
		}

		StandAloneDeclarationConverter StandAloneDeclarationConverter
		{
			get
			{
				if (standAloneDeclarationConverter == null)
				{
					standAloneDeclarationConverter = new StandAloneDeclarationConverter();
					standAloneDeclarationConverter.OnError += StandAloneDeclarationConverter_ShowError;
				}

				return standAloneDeclarationConverter;
			}
		}

		StandAloneDeclarationConverter standAloneDeclarationConverter;

		ZController DeclarationController
		{
			get
			{
				if (declarationController == null)
				{
					declarationController = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
					declarationController.ShowChildrenAsDialog = true;
				}

				return declarationController;
			}
		}

		ZController declarationController;

		static MultilingualString ConvertToStandAloneDeclarationCaption => ResString.GetMultilingualString("10f6b3c3-a6e2-47ba-8def-38fc12c0ec6d", "Convert to Stand Alone Declaration");

		static string SaveAnyChangesMessage => Res.GetString("5db68703-175e-41a6-a94e-60f6d60fa07d", "Please save any changes made before creating Stand Alone Declaration.");

		static string ExistingDeclarationMessage => Res.GetString("182ba0ce-1340-4a42-8360-98d9e5c6aee0", "A Stand Alone Declaration cannot be created for a Bill that already has an existing Stand Alone Declaration.");

		static string ImporterOrExporterOrgNotFoundPrompt => Res.GetString("358bfbd5-e0c9-4d91-b7a9-178f1beb3cad", "Importer or Exporter organization not found for at least one bill selected, would you like to convert them into organizations?");

		static string ImporterAndExporterOrgNotFoundPrompt => Res.GetString("cec33050-021e-42d1-a8ee-ff34d09332d8", "Importer and Exporter organizations not found, would you like to convert to organizations with details defaulted from 'Importer' and 'Exporter' fields?");

		static string ImporterOrgNotFoundPrompt => Res.GetString("75d2c67c-8d50-4e4f-8cee-aa4225f45c6a", "Importer organization not found, would you like to convert to organization with details defaulted from 'Importer' fields?");

		static string ExporterOrgNotFoundPrompt => Res.GetString("5d4a5b58-6461-4ed6-8bd4-093cede124fb", "Exporter organization not found, would you like to convert to organization with details defaulted from 'Exporter' fields?");

		static string OrgsNotFoundCaption => Res.GetString("6877ec74-2174-4f8f-a610-b71a17a6d992", "Organizations not found");

		static string OrgLinkedMessage => Res.GetString("35e2a7a9-5600-4bee-aa96-2965a53b1d6f", "New organization(s) have been linked to the selected bills. Please click 'Convert' again to create Stand Alone Declarations.");

		static string BillConversionSuccessMessage => Res.GetString("2bf88dca-9d66-4aff-bb77-cd4aa81ec212", "Bills have been converted to Stand Alone Declarations.");

		string GetReleasedStatusMessage(ZString releasedStatusDescription) => Res.GetString("6a96fd54-f9e1-4728-bd31-b89ef6abc3fe", @"A Stand Alone Declaration cannot be created for a Bill with a '{0}' Customs Status.", releasedStatusDescription);

		const int MaximumBillNumbersInErrorMessage = 10;
	}

	class NotificationHandlerWithMessageOverride : INotificationHandlerWithMessageOverride
	{
		#region INotificationHandler Members

		void INotificationHandler.ReportInformation(string message, string caption)
		{
			Globals.Message.ShowInformation(message, caption);
		}

		void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
		{
			Globals.Message.ShowError(message, caption);
		}

		#endregion

		#region INotificationHandlerWithMessageOverride Members
		string INotificationHandlerWithMessageOverride.MergeWarningMessage => ConcurrencyMessage;
		string INotificationHandlerWithMessageOverride.CriticalWarningMessage => null;
		string INotificationHandlerWithMessageOverride.CannotDeleteMessage => null;
		string INotificationHandlerWithMessageOverride.DeletedObjectsHeader => null;
		string INotificationHandlerWithMessageOverride.MergedObjectsHeader => ConcurrencyHeader;
		string INotificationHandlerWithMessageOverride.CriticalObjectsHeader => null;
		string INotificationHandlerWithMessageOverride.CannotDeleteObjectsHeader => null;
		#endregion

		string ConcurrencyMessage => Res.GetString("44c97952-3895-4b1c-a906-cb433cb2c6e2", "One or more selected bills have been changed in another session. Please click 'Convert' again to create Stand Alone Declarations.");
		string ConcurrencyHeader => Res.GetString("43b2e60a-866d-49a7-8662-f63f6194ce46", "The following objects had changes:");
	}
}

