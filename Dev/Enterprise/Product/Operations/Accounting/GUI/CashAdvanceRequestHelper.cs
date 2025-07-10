using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
	public static class CashAdvanceRequestHelper
	{
		static List<AccCashAdvanceRequestHeader> GetCashAdvanceRequestHeaderList(BusinessObject[] businessObjects)
		{
			var factory = new BusinessObjectFactory();
			var requestList = factory.Load<AccCashAdvanceRequestHeader>(new ZQuery(AccCashAdvanceRequestHeaderSchema.PK, businessObjects.Select(bizO => bizO.PK).ToArray()));
			return requestList.ToList();
		}

		public static void MarkAsCancelRequest(SecurityCheckpoint securityCheckPoint, BusinessObject[] businessObjects)
		{
			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				var cashAdvanceRequests = GetCashAdvanceRequestHeaderList(businessObjects);
				if (cashAdvanceRequests.Count == 0)
				{
					Globals.Message.ShowError(Res.GetString("bba2ede0-2a12-4108-acc1-055942309360", "Please select an Advance Payment Request to perform this action."));
				}
				else
				{
					var errorMessageBuilder = new ZStringBuilder();
					var successfulMessageBuilder = new ZStringBuilder();
					var onlyError = true;
					var updatedCAHs = new List<AccCashAdvanceRequestHeader>();
					foreach (var cashAdvance in cashAdvanceRequests)
					{
						var result = cashAdvance.CancelRequest();
						if (result.IsEmpty)
						{
							updatedCAHs.Add(cashAdvance);
							successfulMessageBuilder.Append(cashAdvance.CAH_RequestReferenceNumber);
							onlyError = false;
						}
						else
						{
							errorMessageBuilder.AppendLine(FormattableString.Invariant($"{cashAdvance.CAH_RequestReferenceNumber}-{result}"));
						}
					}
					try
					{
						if (updatedCAHs.Count > 0)
						{
							updatedCAHs.First().Factory.Save();
						}
						var fullMessageBuilder = new ZStringBuilder();
						if (!successfulMessageBuilder.IsEmpty)
						{
							fullMessageBuilder.AppendLine(Res.GetString("9854f1c4-2aa7-416d-9e7c-b9b1c771459f", "Following Advance Payment requests are marked as {0}-{1}{2}", "Cancelled", System.Environment.NewLine, successfulMessageBuilder.ToStringWithDelimiterBetweenAppends(", ")));
						}
						if (!errorMessageBuilder.IsEmpty)
						{
							fullMessageBuilder.AppendLine(Res.GetString("1cc0843a-c599-49e5-be98-16bb738d2354", "Following Advance Payment requests could not be marked as {0}.{1}{2}", "Cancelled", System.Environment.NewLine, errorMessageBuilder.ToString()));
						}
						if (!fullMessageBuilder.IsEmpty)
						{
							string caption = Res.GetString("2f96bfb9-2584-4318-ac70-bb6ff163a79e", "Mark as {0}", "Cancel");
							if (onlyError)
							{
								Globals.Message.ShowError(fullMessageBuilder.ToString(), caption);
							}
							else
							{
								Globals.Message.ShowInformation(fullMessageBuilder.ToString(), caption);
							}
						}
					}
					catch (ZSaveConcurrencyException ex)
					{
						Globals.Message.ShowError(new ConcurrencyExceptionHandler(ex).UserFriendlyMessage);
					}
					catch (ZCannotSaveException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}

		public static void MarkAsPaidOrUnpaid(SecurityCheckpoint securityCheckPoint, BusinessObject[] businessObjects, Func<AccCashAdvanceRequestHeader, (bool IsSuccessful, string ErrorMessage)> action, string operationName)
		{
			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				var cashAdvanceRequests = GetCashAdvanceRequestHeaderList(businessObjects);
				if (cashAdvanceRequests.Count > 0)
				{
					if (cashAdvanceRequests.Any())
					{
						var errorMessageBuilder = new ZStringBuilder();
						var successfulMessageBuilder = new ZStringBuilder();
						var onlyError = true;
						var updatedCAHs = new List<AccCashAdvanceRequestHeader>();
						foreach (var cashAdvance in cashAdvanceRequests)
						{
							var result = action(cashAdvance);
							if (result.IsSuccessful)
							{
								updatedCAHs.Add(cashAdvance);
								successfulMessageBuilder.Append(cashAdvance.CAH_RequestReferenceNumber);
								onlyError = false;
							}
							else
							{
								errorMessageBuilder.AppendLine(FormattableString.Invariant($"{cashAdvance.CAH_RequestReferenceNumber}-{result.ErrorMessage}")); // Advance Payment request number is appended to the error message.
							}
						}
						try
						{
							if (updatedCAHs.Count > 0)
							{
								updatedCAHs.First().Factory.Save();
							}
							var fullMessageBuilder = new ZStringBuilder();
							if (!successfulMessageBuilder.IsEmpty)
							{
								fullMessageBuilder.AppendLine(Res.GetString("e7a7da5f-525d-4d86-90a7-839e075134ee", "Following Advance Payment requests are marked as {0}-{1}{2}", operationName, System.Environment.NewLine, successfulMessageBuilder.ToStringWithDelimiterBetweenAppends(", ")));
							}
							if (!errorMessageBuilder.IsEmpty)
							{
								fullMessageBuilder.AppendLine(Res.GetString("24075f24-65c0-4033-a654-a8decf52c5dc", "Following Advance Payment requests could not be marked as {0}.{1}{2}", operationName, System.Environment.NewLine, errorMessageBuilder.ToString()));
							}

							if (!fullMessageBuilder.IsEmpty)
							{
								string caption = Res.GetString("c4632bdb-ab0f-4243-ae63-d10987feb06a", "Mark as {0}", operationName);
								if (onlyError)
								{
									Globals.Message.ShowError(fullMessageBuilder.ToString(), caption);
								}
								else
								{
									Globals.Message.ShowInformation(fullMessageBuilder.ToString(), caption);
								}
							}
						}
						catch (ZSaveConcurrencyException ex)
						{
							Globals.Message.ShowError(new ConcurrencyExceptionHandler(ex).UserFriendlyMessage);
						}
						catch (ZCannotSaveException ex)
						{
							Globals.Message.ShowError(ex.Message);
						}
					}
				}
				else
				{
					string message = Res.GetString("0f5b0601-00f7-481b-9f50-812121c8da98", "Please select at least one Advance Payment request to mark as {0}.", operationName);
					string caption = Res.GetString("e5907330-d074-45ac-9ba0-943c2e5e6c5d", "Select an Advance Payment request");
					Globals.Message.ShowInformation(message, caption);
				}
			}
		}
	}
}
