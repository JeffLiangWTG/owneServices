using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Business.OperationalAction
{
	public class MucrFunctionUpdaterOperationalActionRunner
	{
		readonly IOperationalActionSectionLog log;
		readonly BusinessObject[] targets;

		public MucrFunctionUpdaterOperationalActionRunner(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			this.log = log;
			this.targets = targets;
		}

		public void PerformMucrFunctionOperationalAction(bool actionAssociate, bool actionDisassociate, bool actionClose,
				string mucrManual, string mawp, string mawn, string airport, string shed)
		{
			if (!CheckMucrGenerationStylesMatchUnlessUsingManualMucrOrUnlessDoingNoMessaging(mucrManual, actionAssociate, actionDisassociate, actionClose))
			{
				return; // cannot go on
			}

			int numberOfActions = 1;  // update new values for mucr
			if (actionAssociate)
			{
				numberOfActions++;
			}
			if (actionDisassociate)
			{
				numberOfActions++;
			}

			int numberOfProgressSteps = targets.Length * numberOfActions;
			if (actionClose)
			{
				numberOfProgressSteps++;
			}
			log.SetSectionProgressMax(numberOfProgressSteps);

			foreach (BusinessObject target in targets)
			{
				JobDeclaration dec = target as JobDeclaration;
				if (dec != null)
				{
					var notifier = new SendsMessagesToCustomsShutterUpperer(false);
					LogControllerLink link = new LogControllerLink(dec.JE_DeclarationReference, ControllerIDs.Customs.JobDeclaration, dec.PK);
					OperationalActionBulkMucrMessageSender sender = new OperationalActionBulkMucrMessageSender(dec);
					OperationalActionMucrUpdater updater = new OperationalActionMucrUpdater(dec);

					if (actionDisassociate)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Disassociating {0} from existing MUCR {1}", link, dec.JE_MasterUCR);
						sender.OperationalActionSendMucrMessage(new GbDes242MessageFunction.MucrDisAssociate(), notifier);
						if (ShowUserAnyUnsuccessfulActionAndReturnShouldAbort(log, notifier))
						{
							continue;
						}

						log.BumpSectionProgress();
					}

					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Updating MUCR on {0}", link);
					updater.OperationaActionUpdateMucr(mucrManual, mawp, mawn, airport, shed);
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "MUCR on {0} is now '{1}'", link, dec.JE_MasterUCR);
					log.BumpSectionProgress();

					if (actionAssociate)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Associating {0} to MUCR '{1}'", link, dec.JE_MasterUCR);
						sender.OperationalActionSendMucrMessage(new GbDes242MessageFunction.MucrAssociate(), notifier);
						if (ShowUserAnyUnsuccessfulActionAndReturnShouldAbort(log, notifier))
						{
							continue;
						}

						log.BumpSectionProgress();
					}
				}
			}

			// Close need be done only ONCE, against any dec.
			if (actionClose)
			{
				JobDeclaration decForClosing = targets[0] as JobDeclaration;
				if (decForClosing != null)
				{
					var notifier = new SendsMessagesToCustomsShutterUpperer(false);
					LogControllerLink link = new LogControllerLink(decForClosing.JE_DeclarationReference, ControllerIDs.Customs.JobDeclaration, decForClosing.PK);
					OperationalActionBulkMucrMessageSender senderForClosing = new OperationalActionBulkMucrMessageSender(decForClosing);
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Closing {0} using {1} for messaging", decForClosing.JE_MasterUCR, link);
					senderForClosing.OperationalActionSendMucrMessage(new GbDes242MessageFunction.MucrClose(), notifier);
					if (ShowUserAnyUnsuccessfulActionAndReturnShouldAbort(log, notifier))
					{
						return;
					}

					log.BumpSectionProgress();
				}
			}
		}

		bool ShowUserAnyUnsuccessfulActionAndReturnShouldAbort(IOperationalActionSectionLog log, SendsMessagesToCustomsShutterUpperer notifier)
		{
			bool shouldAbort = false;
			if (!String.IsNullOrEmpty(notifier.InvalidOperationText))
			{
				log.Notify(OperationalActionLogErrorLevel.Error, notifier.InvalidOperationText);
				shouldAbort = true;
			}
			if (!String.IsNullOrEmpty(notifier.LastErrorsAsString))
			{
				log.Notify(OperationalActionLogErrorLevel.Error, notifier.LastErrorsAsString);
				shouldAbort = true;
			}
			if (!String.IsNullOrEmpty(notifier.Warning))
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, notifier.Warning);
			}
			return shouldAbort;
		}

		bool CheckMucrGenerationStylesMatchUnlessUsingManualMucrOrUnlessDoingNoMessaging(ZString mucrManual, bool actionAssociate, bool actionDisassociate, bool actionClose)
		{
			if (this.targets.Length == 1)
			{
				return true;
			}

			// If we are not using a manual MUCR, must ensure that all declarations have the same generation style. Otherwise we could end up with many different MUCRs.
			if (mucrManual.IsEmpty && (actionAssociate || actionClose || actionDisassociate))
			{
				// If autogenerating and messaging, check the badges

				// Get a list of the badges in question:
				List<string> badges = new List<string>();
				foreach (BusinessObject bo in this.targets)
				{
					if (bo is JobDeclaration dec)
					{
						if (!badges.Contains(dec.JE_CustomsProfile))
						{
							badges.Add(dec.JE_CustomsProfile);
						}
					}
				}

				// Get a list of the badges stored in the rego:
				BadgeCodeSettingCollection badgesInRego = GBCustomsDataRegistry.Instance.BadgeCodes.Value;
				List<string> badgeGenerationStyles = new List<string>();

				foreach (string badge in badges)
				{
					// Pull out the badge (by code) from the collection in the rego
					BadgeCodeSetting badgeCodeSetting = badgesInRego.FindByBadgeCodeOnly(badge);
					if (badgeCodeSetting != null)
					{
						if (!badgeGenerationStyles.Contains(badgeCodeSetting.MasterUcrCalculationMode))
						{
							badgeGenerationStyles.Add(badgeCodeSetting.MasterUcrCalculationMode);
						}
					}
				}

				if (badgeGenerationStyles.Count > 1)
				{
					this.log.Notify(OperationalActionLogErrorLevel.Error, "Cannot proceed - the selected declarations' badges' MUCR generation styles differ. Select only declarations with matching badge MUCR generation styles (e.g. badges), or use the manual MUCR generation option, or for automatic generation untick all EAC messaging options.");
					return false;
				}
			}
			return true;
		}
	}
}
