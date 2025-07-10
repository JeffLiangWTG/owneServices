using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AccountingService : IAccountingService
	{
		public string PostTransactions(Guid jobPK, JobInvoicingPostingOption postingOption, bool shouldPerformBackDating, DateTime postDate, DateTime invoiceDate)
		{
			string result = Res.GetString("F30A4FC5-D24B-45A0-B803-929D8F3F10DB", "Charges successfully posted");
			try
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Post Transactions" };

				// Dirty hack until reimplemented (Denys says 4 months ;)
				var type = Type.GetType("Enterprise.Accounting.GUI.JobInvoicing.GLOWInvoicingPostManagerGUIWrapper, Enterprise.Accounting.GUI");
				var obj = Activator.CreateInstance(type, jobPK, postingOption, shouldPerformBackDating, postDate, invoiceDate, factory);
				var methodInfo = obj.GetType().GetMethod("Post");
				methodInfo.Invoke(obj, null);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				else
				{
					result = ex.Message;
				}
			}
			return result;
		}

		public PreviewInvoicesData PreviewTransactions(Guid jobPK, bool shouldPerformBackDating, DateTime postDate, DateTime invoiceDate)
		{
			try
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Preview Transactions" };

				// Dirty hack until reimplemented (Denys says 4 months ;)
				var type = Type.GetType("Enterprise.Accounting.GUI.JobInvoicing.GLOWInvoicingPostManagerGUIWrapper, Enterprise.Accounting.GUI");
				var obj = Activator.CreateInstance(type, jobPK, JobInvoicingPostingOption.All, shouldPerformBackDating, postDate, invoiceDate, factory);
				var methodInfo = obj.GetType().GetMethod("GetPreviewInvoicesData");
				return (PreviewInvoicesData)methodInfo.Invoke(obj, null);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				else
				{
					PreviewInvoicesData data = new PreviewInvoicesData();
					data.ErrorMessage = ex.Message;
					return data;
				}
			}
		}

		public BackPostingData GetBackPostingData(Guid jobPK, JobInvoicingPostingOption postingOption)
		{
			try
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Get Back Posting Data" };

				// Dirty hack until reimplemented (Denys says 4 months ;)
				var type = Type.GetType("Enterprise.Accounting.GUI.JobInvoicing.GLOWInvoicingPostManagerGUIWrapper, Enterprise.Accounting.GUI");
				var obj = Activator.CreateInstance(type, jobPK, postingOption, false, DateTime.MinValue, DateTime.MinValue, factory);
				var methodInfo = obj.GetType().GetMethod("GetBackPostingData");
				return (BackPostingData)methodInfo.Invoke(obj, null);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				else
				{
					BackPostingData data = new BackPostingData();
					data.ErrorMessage = ex.Message;
					return data;
				}
			}
		}

		public string RecognizeRevenue(Guid jobPK)
		{
			string result = Res.GetString("D9A7241F-9B3A-4C9B-AF72-B93952302618", "Revenue recognized successfully");
			try
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Recognize Revenue" };
				factory.Load<ChargeWithCost>(new ZQuery(JobChargeSchema.JR_JH, jobPK));
				factory.Save();
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				else
				{
					result = ex.Message;
				}
			}
			return result;
		}

		public JobCalcPropertiesData GetJobCalcPropertiesData(Guid jobPK)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Recognize Revenue" };
			Job job = factory.Load<Job>(jobPK);
			if (job != null)
			{
				JobCalcPropertiesData jobCalcPropertiesData = new JobCalcPropertiesData();

				jobCalcPropertiesData.LocalCurrency = job.JH_LocalCurrency;
				jobCalcPropertiesData.ChargeableWgtVol = job.ChargeableWgtVol;
				jobCalcPropertiesData.ProfitRevenueMargin = job.JH_ProfitRevenueMargin;

				return jobCalcPropertiesData;
			}
			else
			{
				return null;
			}
		}

		public JobChargeCalcPropertiesData GetJobChargeCalcPropertiesData(Guid chargePK)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Recognize Revenue" };
			ChargeWithCost charge = factory.Load<ChargeWithCost>(chargePK);
			if (charge != null)
			{
				JobChargeCalcPropertiesData jobChargeCalcPropertiesData = new JobChargeCalcPropertiesData();

				jobChargeCalcPropertiesData.IsRevenuePosted = charge.IsRevenuePosted;
				jobChargeCalcPropertiesData.IsApportioned = charge.JR_IsApportioned;
				jobChargeCalcPropertiesData.IsApproved = charge.IsApproved;
				jobChargeCalcPropertiesData.IsCostPosted = charge.IsCostPosted;
				jobChargeCalcPropertiesData.ChargeType = charge.ChargeType;
				jobChargeCalcPropertiesData.SellRecognition = charge.SellRecognition;
				jobChargeCalcPropertiesData.CostRecognition = charge.CostRecognition;
				jobChargeCalcPropertiesData.MarginPercentage = charge.MarginPercentage;
				jobChargeCalcPropertiesData.CFXAmtReverseSign = charge.JR_CFXAmtReverseSign;
				jobChargeCalcPropertiesData.OSSellAmtWithGST = charge.JR_Calc_OSSellAmtWithGST;
				jobChargeCalcPropertiesData.CFXAmt = charge.JR_CFXAmt;
				jobChargeCalcPropertiesData.AgentDeclaredRevenueLocal = charge.JR_AgentDeclaredSellAmtLocal;
				jobChargeCalcPropertiesData.AgentDeclaredCostLocal = charge.JR_AgentDeclaredCostAmtLocal;

				return jobChargeCalcPropertiesData;
			}
			else
			{
				return null;
			}
		}

		public JobHeaderDefaultFields GetJobHeaderDefaultFields(Guid shipmentPK, string loginName, Guid branchPK, Guid departmentPK)
		{
			using (Env.SetTemporaryUserContext(new UserContext(loginName, branchPK, departmentPK)))
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Recognize Revenue" };
				ForwardingShipment shipment = factory.Load<ForwardingShipment>(shipmentPK);
				Job job = (Job)new JobHeader.Loader(factory, shipment).TryCreate();
				job.JH_ParentTableCode = "JS";
				job.JH_ParentID = shipmentPK;
				job.PlugInData = shipment;
				JobHeaderDefaultFields jobHeaderDefaultFields = new JobHeaderDefaultFields();

				jobHeaderDefaultFields.JH_JobBufferPercentOverride = job.JH_JobBufferPercentOverride;
				jobHeaderDefaultFields.JH_A_JCL = job.JH_A_JCL.IsEmpty ? null : job.JH_A_JCL.ToDateTime();
				jobHeaderDefaultFields.JH_A_JOP = job.JH_A_JOP.IsEmpty ? null : job.JH_A_JOP.ToDateTime();
				jobHeaderDefaultFields.JH_RevenueRecognizedDate = job.JH_RevenueRecognizedDate.IsEmpty ? null : job.JH_RevenueRecognizedDate.ToDateTime();
				jobHeaderDefaultFields.JH_SystemLastEditTimeUtc = job.JH_SystemLastEditTimeUtc.IsEmpty ? null : job.JH_SystemLastEditTimeUtc.ToDateTime();
				jobHeaderDefaultFields.JH_JobPlannedStartDate = job.JH_JobPlannedStartDate.IsEmpty ? null : job.JH_JobPlannedStartDate.ToDateTime();
				jobHeaderDefaultFields.JH_SystemCreateTimeUtc = job.JH_SystemCreateTimeUtc.IsEmpty ? null : job.JH_SystemCreateTimeUtc.ToDateTime();
				jobHeaderDefaultFields.JH_UniqueJobInvoiceNumber = job.JH_UniqueJobInvoiceNumber;
				jobHeaderDefaultFields.JH_AgentChargesCFX = job.JH_AgentChargesCFX;
				jobHeaderDefaultFields.JH_LocalChargesCFX = job.JH_LocalChargesCFX;
				jobHeaderDefaultFields.JH_GB = !job.JH_GB.IsValid ? Guid.Empty : job.JH_GB.ToGuid();
				jobHeaderDefaultFields.JH_GC = !job.JH_GC.IsValid ? Guid.Empty : job.JH_GC.ToGuid();
				jobHeaderDefaultFields.JH_GE = !job.JH_GE.IsValid ? Guid.Empty : job.JH_GE.ToGuid();
				jobHeaderDefaultFields.JH_JH_ParentJob = !job.JH_JH_ParentJob.IsValid ? Guid.Empty : job.JH_JH_ParentJob.ToGuid();
				jobHeaderDefaultFields.JH_OA_AgentCollectAddr = !job.JH_OA_AgentCollectAddr.IsValid ? Guid.Empty : job.JH_OA_AgentCollectAddr.ToGuid();
				jobHeaderDefaultFields.JH_OA_LocalChargesAddr = !job.JH_OA_LocalChargesAddr.IsValid ? Guid.Empty : job.JH_OA_LocalChargesAddr.ToGuid();
				jobHeaderDefaultFields.JH_OC_LocalBillingContact = !job.JH_OC_LocalBillingContact.IsValid ? Guid.Empty : job.JH_OC_LocalBillingContact.ToGuid();
				jobHeaderDefaultFields.JH_ProfitShareInvoice = !job.JH_ProfitShareInvoice.IsValid ? Guid.Empty : job.JH_ProfitShareInvoice.ToGuid();
				jobHeaderDefaultFields.JH_Description = job.JH_Description;
				jobHeaderDefaultFields.JH_ExcludeFromPeriodicRating = job.JH_ExcludeFromPeriodicRating.ToString();
				jobHeaderDefaultFields.JH_GS_NKRepOps = job.JH_GS_NKRepOps;
				jobHeaderDefaultFields.JH_GS_NKRepSales = job.JH_GS_NKRepSales;
				jobHeaderDefaultFields.JH_HeaderType = job.JH_HeaderType;
				jobHeaderDefaultFields.JH_HoldReason = job.JH_HoldReason;
				jobHeaderDefaultFields.JH_IsProfitSharePosted = job.JH_IsProfitSharePosted.ToString();
				jobHeaderDefaultFields.JH_JobLocalReference = job.JH_JobLocalReference;
				jobHeaderDefaultFields.JH_LocalClientInvoicingStyle = job.JH_LocalClientInvoicingStyle;
				jobHeaderDefaultFields.JH_Name = job.JH_Name;
				jobHeaderDefaultFields.JH_PaymentCollectionStatus = job.JH_PaymentCollectionStatus;
				jobHeaderDefaultFields.JH_ProfitLossReasonCode = job.JH_ProfitLossReasonCode;
				jobHeaderDefaultFields.JH_RatingHasBeenRun = job.JH_RatingHasBeenRun.ToString();
				jobHeaderDefaultFields.JH_SingleAgentsInvoicePerConsol = job.JH_SingleAgentsInvoicePerConsol.ToString();
				jobHeaderDefaultFields.JH_Status = job.JH_Status;
				jobHeaderDefaultFields.JH_SystemCreateUser = job.JH_SystemCreateUser;
				jobHeaderDefaultFields.JH_SystemLastEditUser = job.JH_SystemLastEditUser;
				jobHeaderDefaultFields.JH_TH_NKQuoteNumber = job.JH_TH_NKQuoteNumber;

				return jobHeaderDefaultFields;
			}
		}

		public string CreateProfitShareCharges(Guid jobPK)
		{
			string result = Res.GetString("B6800DD3-49BF-411A-BFFF-727356825978", "Create Profit Share Charges completed successfully");
			try
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Create Profit Share Charges" };
				Job job = factory.Load<Job>(jobPK);
				if (job != null && job.Parent is ForwardingShipment)
				{
					new ProfitShareShipmentChargeCreator(new ProfitShareCalculator(factory, new IJobInvoicingPlugIn[] { (IJobInvoicingPlugIn)job.Parent }).CreateProfitShares(), job).CreateCharges();
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				else
				{
					result = ex.Message;
				}
			}
			return result;
		}

		public string ReverseInvoiceRedoBilling(Guid jobPK, string reversingReason)
		{
			string result = Res.GetString("4183059F-F65D-4318-B221-2F530766AE4E", "Reverse Invoice Redo Billing completed successfully");
			try
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Reverse Invoice Redo Billing" };
				Job job = factory.Load<Job>(jobPK);
				if (job != null)
				{
					JobInvoicingSecurityHelper securityHelper = new JobInvoicingSecurityHelper(job.PlugInData != null ? job.PlugInData.InvoicingSupporter.JobInvoicingSecurity : Env.Security.None);
					if (!securityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ReverseBilling))
					{
						throw new Exception(securityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.ReverseBilling));
					}
					else
					{
						bool result1 = true;
						ISecurityOverrideProvider paidRelatedInvoicesSecurityProvider = null;

						if (job != null)
						{
							JobInvoicingReverser reverser1 = new JobInvoicingReverser(job, delegate(InvoicingBase[] transactions)
							{
								var arInvoices = transactions.Where(x => x is ARInvoice);
								foreach (var invoice in arInvoices)
								{
									if (paidRelatedInvoicesSecurityProvider == null)
									{
										paidRelatedInvoicesSecurityProvider = invoice.SecurityOverrideProvider;
									}
									else
									{
										invoice.SecurityOverrideProvider = paidRelatedInvoicesSecurityProvider;
									}

									ReversingFactory reversingFactory = new ReversingFactory();
									ReversingBase reverser = reversingFactory.NewReversing(invoice, securityHelper);
									bool canReverse = reverser.CanReverseTransaction;
									result1 &= canReverse;

									if (!canReverse)
									{
										string message = reverser.CantReverseErrorMessage;
										string arInvoiceMessage = Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.ErrorMessageForNotAllowed;
										if (message.Contains(arInvoiceMessage))
										{
											message = message.Replace(arInvoiceMessage, securityHelper.GetInvSecurity(SecurityCore.AllowReversalWhenRelatedAPTrArePaid).ErrorMessageForNotAllowed);
										}

										throw new Exception(message);
									}
								}
							}, null);
							string validationErrors = reverser1.IsValidToReverseAllInvoices();
							if (!string.IsNullOrEmpty(validationErrors))
							{
								throw new Exception(validationErrors);
							}
							else
							{
								if (job.IsClosed && !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job))
								{
									result1 = false;
								}

								if (result1)
								{
									if (!string.IsNullOrEmpty(reversingReason))
									{
										reverser1.ReverseAllInvoices(reversingReason, "TXT");
										if (reverser1.ContinueWithSave)
										{
											try
											{
												reverser1.ReversingFactory.Save();
												if (job.PlugInData != null)
												{
													job.PlugInData.InvoicingSupporter.PostedStateChanged();
												}
											}
											catch (ZSaveConcurrencyException ex)
											{
												throw new Exception(Res.GetString("19c87d9c-738c-4212-9cd3-ee04ad544479", "While you were working, another user has modified this job. Please try again."), ex);
											}
										}
										else if (reverser1.Errors.Count > 0)
										{
											throw new Exception(reverser1.Errors.ToString());
										}
									}
									else
									{
										throw new Exception(Res.GetString("D818FDE2-0BCE-47FA-A447-A1DF3A9022DA", "You haven't entered a reversing reason. Cannot proceed with reversing."));
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				else
				{
					result = ex.Message;
				}
			}
			return result;
		}
	}
}