using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRMessageManager : SingleMessageManager
	{
		#region Overrides
		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			var result = new List<EDIMessage>();
			CMRMessageBuilder[] builders = GetBuilder(bizo);

			foreach (CMRMessageBuilder builder in builders)
			{
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
				builder.Messages = GetMessages(bizo);
				result.Add(builder.PopulateMessagesReturningResult());
			}

			return result.ToArray();
		}

		protected override sealed EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			var result = new List<EDIMessage>();
			CMRMessageBuilder[] builders = GetBuilder(bizo);
			foreach (CMRMessageBuilder builder in builders)
			{
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
				builder.Messages = GetMessages(bizo);
				result.Add(builder.PopulateMessagesReturningResult());
			}

			return result.ToArray();
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return GetAmendmentManager(bizo).GenerateAmendmentMessageSet();
		}

		public bool SendingAmendment
		{
			get { return sendingAmendment; }
			set { sendingAmendment = value; }
		}
		bool sendingAmendment;

		public override bool CanSendOriginal => ValidCanSendOriginalStatusCodes.Contains(GetStatus());

		public override bool CanSendWithdrawal => ValidCanSendWithdrawalStatusCodes.Contains(GetStatus());

		protected override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();

			foreach (var error in CheckCMRMessageSendingEnvironment(BusinessObject.Factory))
			{
				result.Add(new MessageSendingError(error));
			}

			var serviceWarning = CheckMessageSendingServiceTaskIsRunning();
			if (!serviceWarning.IsEmpty)
			{
				result.AddWarning(serviceWarning);
			}

			if (ShouldValidateCurrentCompanyLocalBusNumCharacters)
			{
				ValidateCurrentCompanyLocalBusNumCharacters(result);
			}

			return result;
		}

		protected override void ResetToOriginalCore()
		{
			base.ResetToOriginalCore();
			foreach (ICalculatedCusStatusCalculator statusCalculator in StatusCalculators)
			{
				statusCalculator.ResetToOriginal();
				if (statusCalculator is ICMRCargoReportEventsLogger)
				{
					IParentForCargoReporter parentForCargoReportingEvents = ((ICMRCargoReportEventsLogger)statusCalculator).ParentForCargoReportingEvents;
					if (parentForCargoReportingEvents != null)
					{
						Logs parentForCargoReportingEventsLogs = parentForCargoReportingEvents.Logs;
						if (parentForCargoReportingEventsLogs != null)
						{
							LogsForNominatedEvent cargoReportLogs = new LogsForNominatedEvent(parentForCargoReportingEventsLogs, parentForCargoReportingEvents.CargoReportSentEvent);
							cargoReportLogs.CancelAll();
							cargoReportLogs = new LogsForNominatedEvent(parentForCargoReportingEventsLogs, parentForCargoReportingEvents.CargoReportAcceptedEvent);
							cargoReportLogs.CancelAll();
							cargoReportLogs = new LogsForNominatedEvent(parentForCargoReportingEventsLogs, parentForCargoReportingEvents.CargoReportRejectedEvent);
							cargoReportLogs.CancelAll();
							cargoReportLogs = new LogsForNominatedEvent(parentForCargoReportingEventsLogs, parentForCargoReportingEvents.CargoReportWithdrawEvent);
							cargoReportLogs.CancelAll();
							parentForCargoReportingEventsLogs.AddNew(Events.Cancelled, "All Cargo Report events cancelled");
						}
					}
				}
			}
		}

		protected override bool ShouldSendMessagesInTestMode
		{
			get { return Env.Registry.CMRTestMode; }
		}

		#endregion

		public override bool IsWaitingForResponse
		{
			get
			{
				ZString status = GetStatus();
				return status == CMRBaseStatuses.Codes.AwaitingResponseToOriginal || status == CMRBaseStatuses.Codes.AwaitingResponseToAmendment || status == CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			}
		}

		#region ABN Validation

		protected virtual bool ShouldValidateCurrentCompanyLocalBusNumCharacters
		{
			get { return false; }
		}

		void ValidateCurrentCompanyLocalBusNumCharacters(MessageSendingNotificationCollection notifications)
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			if (orgProxy != null)
			{
				var abnNumber = orgProxy.PrimaryRegistrationNumber.Number;
				if (abnNumber.KeepAlphanumericCharacters().Length > 11)
				{
					notifications.AddWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN cannot be greater than 11 characters.");
				}
				else if (abnNumber.IsEmpty)
				{
					notifications.AddWarning("The current company (" + GlbCompany.CurrentCompany.GC_Name + ") ABN has not been entered.");
				}
			}
			else
			{
				notifications.AddWarning("The Organisation Proxy of current company (" + GlbCompany.CurrentCompany.GC_Name + ") has not been entered. Please enter it in Company form.");
			}
		}

		#endregion

		#region Implementation

		protected virtual ArrayList ValidCanSendOriginalStatusCodes
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(CMRBaseStatuses.Codes.NotSent);
				result.Add(CMRBaseStatuses.Codes.OriginalRejected);
				result.Add(CMRBaseStatuses.Codes.WithdrawalAccepted);
				result.Add(CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn);
				result.Add(CustomsEntryStatus.ScheduledLodgeWithPayment.Code);
				result.Add(CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code);
				result.Add(CustomsEntryStatus.ScheduledPayment.Code);
				return result;
			}
		}

		protected virtual ArrayList ValidCanSendWithdrawalStatusCodes
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(CMRBaseStatuses.Codes.AmendmentAccepted);
				result.Add(CMRBaseStatuses.Codes.AmendmentRejected);
				result.Add(CMRBaseStatuses.Codes.OriginalAccepted);
				result.Add(CMRBaseStatuses.Codes.WithdrawalRejected);
				result.Add(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms);
				result.Add(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine);
				result.Add(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
				result.Add(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement);
				result.Add(CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
				result.Add(CMRConsolidatedCargoStatuses.Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo);
				result.Add(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl);
				result.Add(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed);
				result.Add(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus);
				result.Add(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement);
				result.Add(CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction);
				result.Add(CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived);
				result.Add(CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived);
				result.Add(CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived);
				result.Add(CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived);
				return result;
			}
		}

		#endregion

		#region Static Methods

		public static StringCollection CheckCMRMessageSendingEnvironment(BusinessObjectFactory factory)
		{
			var result = new StringCollection();

			if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
			{
				result.Add("Please enter a Customs Registration Number before sending CMR messages");
			}

			foreach (var certificateError in new CMRUtilities().GetCertificateErrors(factory))
			{
				result.Add(certificateError);
			}

			return result;
		}

		public static ZString CheckMessageSendingServiceTaskIsRunning()
		{
			var result = ZString.Empty;
			if (AUCustomsDataRegistry.Instance.EnableAUServiceTaskCheckForSendingMessage.Value)
			{
				if (ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask("AUS") <= ServiceTaskStatus.ServiceTaskIsInactive)
				{
					result = "Please have your Administrator check the tasks with these codes. AUS: ServiceTaskIsInactive";
				}
			}
			return result;
		}

		#endregion

		internal abstract ICalculatedCusStatusCalculator[] StatusCalculators { get; }
		internal abstract string GetStatus();
		internal abstract CMRMessageBuilder[] GetBuilder(BusinessObject bizo);
		internal abstract EDIMessageCollection GetMessages(BusinessObject bizo);
		internal abstract CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo);
	}
}
