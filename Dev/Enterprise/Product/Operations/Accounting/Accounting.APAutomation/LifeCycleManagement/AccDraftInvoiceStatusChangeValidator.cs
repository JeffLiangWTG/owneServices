using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.APAutomation.LifeCycleManagement
{
	/// <summary>
	/// We will call this before updating status of a draft invoice. First step would be to check whether status can be moved to the next status
	/// If yes, status will be moved to the next status
	/// If no, status can be updated to a different value if requires and error will be logged
	/// </summary>
	[CodeAlive("Will be used in future workitems")]
	public class AccDraftInvoiceStatusChangeValidator : IAccDraftInvoiceStatusChangeValidator
	{
		public AccDraftInvoiceStatusChangeValidator()
		{
			StatusChangeMapping = new Dictionary<string, string[]>();
			PopulateStatusTransitionMappingDictionary();
		}
		readonly Dictionary<string, string[]> StatusChangeMapping;

		AccDraftInvoiceStatusUpdateValidationResult IAccDraftInvoiceStatusChangeValidator.CanChangeStatusTo(AccDraftInvoiceHeader accDraftInvoice, string newStatus)
		{
			AccDraftInvoiceStatusUpdateValidationResult result = null;
			switch (newStatus)
			{
				case AccDraftInvoiceHeaderStatus.Analyzing:
					result = CanSetToAnalysingState(accDraftInvoice);
					break;
				case AccDraftInvoiceHeaderStatus.ApprovedForPosting:
					result = CanApproveForPosting(accDraftInvoice);
					break;
				case AccDraftInvoiceHeaderStatus.Draft:
				case AccDraftInvoiceHeaderStatus.AwaitingApproval:
				case AccDraftInvoiceHeaderStatus.InReview:
					result = ValidateIfCurrentStatusAllowsTheChange(accDraftInvoice, newStatus, contains: false);
					break;
				case AccDraftInvoiceHeaderStatus.InDispute:
				case AccDraftInvoiceHeaderStatus.Discarded:
					result = ValidateIfCurrentStatusAllowsTheChange(accDraftInvoice, newStatus, contains: true);
					break;
				case AccDraftInvoiceHeaderStatus.Processed:
					result = CanMarkAsProcessed(accDraftInvoice);
					break;
				default:
					throw new Exception(Res.GetString("2a4d07ac-a099-496f-aa67-9e87490c2597", "There is no handler for Status: {0}", newStatus));
			}
			return result;
		}

		AccDraftInvoiceStatusUpdateValidationResult CanSetToAnalysingState(AccDraftInvoiceHeader accDraftInvoice)
		{
			if (!StatusChangeMapping[AccDraftInvoiceHeaderStatus.Analyzing].Contains<string>(accDraftInvoice.AIH_Status) || accDraftInvoice.IsInDatabase)
			{
				return new AccDraftInvoiceStatusUpdateValidationResult
				{
					CanUpdate = false,
					ValidationErrors = [GetErrorMessage(accDraftInvoice, AccDraftInvoiceHeaderStatus.Analyzing)]
				};
			}
			return new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true };
		}

		AccDraftInvoiceStatusUpdateValidationResult CanApproveForPosting(AccDraftInvoiceHeader accDraftInvoice)
		{
			var validationErrors = new List<string>();
			if (accDraftInvoice.AIH_Status == AccDraftInvoiceHeaderStatus.Processed)
			{
				if (!accDraftInvoice.AIH_AH_PostedTransactionHeader.IsEmpty)
				{
					validationErrors.Add(Res.GetString("689aa744-04d2-4bbd-9433-9137b7662e13", "Cannot approve a draft invoice for posting if it is already posted."));
				}
			}
			else
			{
				if (!accDraftInvoice.HasReconciliationRun)
				{
					validationErrors.Add(Res.GetString("774def5b-6324-4ba7-a3cd-8ab5df9abfa5", "Cannot approve a draft invoice for posting if reconciliation has not been completed yet"));
				}

				if (!StatusChangeMapping[AccDraftInvoiceHeaderStatus.ApprovedForPosting].Contains<string>(accDraftInvoice.AIH_Status))
				{
					validationErrors.Add(GetErrorMessage(accDraftInvoice, AccDraftInvoiceHeaderStatus.ApprovedForPosting));
				}
			}

			return new AccDraftInvoiceStatusUpdateValidationResult
			{
				CanUpdate = validationErrors.Count == 0,
				ValidationErrors = validationErrors.Count > 0 ? validationErrors.ToArray() : null
			};
		}

		AccDraftInvoiceStatusUpdateValidationResult CanMarkAsProcessed(AccDraftInvoiceHeader accDraftInvoice)
		{
			if (accDraftInvoice.PostedTransactionHeader == null)
			{
				return new AccDraftInvoiceStatusUpdateValidationResult
				{
					CanUpdate = false,
					ValidationErrors = [Res.GetString("467c1faa-be3a-42ec-9109-4505257d70dc", "Cannot mark a draft invoice as processed if it is not posted yet.")]
				};
			}

			return new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true };
		}

		AccDraftInvoiceStatusUpdateValidationResult ValidateIfCurrentStatusAllowsTheChange(AccDraftInvoiceHeader accDraftInvoice, string newStatus, bool contains)
		{
			if (contains
				? StatusChangeMapping[newStatus].Contains<string>(accDraftInvoice.AIH_Status)
				: !StatusChangeMapping[newStatus].Contains<string>(accDraftInvoice.AIH_Status))
			{
				return new AccDraftInvoiceStatusUpdateValidationResult
				{
					CanUpdate = false,
					ValidationErrors = [GetErrorMessage(accDraftInvoice, newStatus)]
				};
			}

			return new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true };
		}

		string GetErrorMessage(AccDraftInvoiceHeader accDraftInvoice, string status)
		{
			switch (status)
			{
				case AccDraftInvoiceHeaderStatus.Analyzing:
					return Res.GetString("4112c316-fdab-4cf5-a8bc-84aee3308812", "Cannot set to an invoice to analyzing state as current status is {0}", accDraftInvoice.AIH_Status);
				case AccDraftInvoiceHeaderStatus.Draft:
					return Res.GetString("5c3cca65-dd13-402d-9d1d-3b36b5618504", "Cannot mark an invoice as draft when current status is {0}", accDraftInvoice.AIH_Status);
				case AccDraftInvoiceHeaderStatus.ApprovedForPosting:
					return Res.GetString("ef34d8d9-8ad5-43e0-90da-fe3f87004dc2", "Cannot approve a draft invoice for posting when current status is {0}", accDraftInvoice.AIH_Status);
				case AccDraftInvoiceHeaderStatus.AwaitingApproval:
					return Res.GetString("dcf9f17e-141f-4b5a-a9a2-2b5295ff0aff", "Cannot send a draft invoice for approval when current status is {0}", accDraftInvoice.AIH_Status);
				case AccDraftInvoiceHeaderStatus.Discarded:
					return Res.GetString("f798d256-a52b-42f3-bafb-d48a2797b5cd", "Cannot discard a draft invoice when current status is {0}", accDraftInvoice.AIH_Status);
				case AccDraftInvoiceHeaderStatus.InDispute:
					return Res.GetString("589125e1-7ac1-4b30-9a3c-a66e39252990", "Cannot mark a draft invoice as disputed when current status is {0}", accDraftInvoice.AIH_Status);
				case AccDraftInvoiceHeaderStatus.Processed:
					return Res.GetString("467c1faa-be3a-42ec-9109-4505257d70dc", "Cannot mark a draft invoice as processed if it is not posted yet.");
				case AccDraftInvoiceHeaderStatus.InReview:
					return Res.GetString("fbd3ddae-91b4-4442-a0b4-dc4b796d4d4b", "Cannot send a draft invoice for review until reconciliation is complete.");
				default:
					return string.Empty;
			}
		}

		void PopulateStatusTransitionMappingDictionary()
		{
			StatusChangeMapping.Add(AccDraftInvoiceHeaderStatus.Analyzing, [AccDraftInvoiceHeaderStatus.Draft]);
			StatusChangeMapping.Add(AccDraftInvoiceHeaderStatus.Draft, [AccDraftInvoiceHeaderStatus.Analyzing]);
			StatusChangeMapping.Add(AccDraftInvoiceHeaderStatus.ApprovedForPosting, [AccDraftInvoiceHeaderStatus.Draft, AccDraftInvoiceHeaderStatus.InDispute, AccDraftInvoiceHeaderStatus.AwaitingApproval, AccDraftInvoiceHeaderStatus.Processed]);
			StatusChangeMapping.Add(AccDraftInvoiceHeaderStatus.AwaitingApproval, [AccDraftInvoiceHeaderStatus.Draft]);
			StatusChangeMapping.Add(AccDraftInvoiceHeaderStatus.Discarded, [AccDraftInvoiceHeaderStatus.ApprovedForPosting, AccDraftInvoiceHeaderStatus.Processed]);
			StatusChangeMapping.Add(AccDraftInvoiceHeaderStatus.InDispute, [AccDraftInvoiceHeaderStatus.ApprovedForPosting, AccDraftInvoiceHeaderStatus.Processed]);
			StatusChangeMapping.Add(AccDraftInvoiceHeaderStatus.InReview, [AccDraftInvoiceHeaderStatus.ApprovedForPosting]);
		}
	}
}
