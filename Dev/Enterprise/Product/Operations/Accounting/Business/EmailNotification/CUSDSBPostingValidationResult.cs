using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChargesSummary
	{
		public ZString ChargeCode { get; set; }
		public ZString Description { get; set; }
		public ZString AdditionalDescription { get; set; }
		public ZDecimal ExistingExTaxAmount { get; set; }
		public ZDecimal ExistingTaxAmount { get; set; }
		public ZDecimal NewExTaxAmount { get; set; }
		public ZDecimal NewTaxAmount { get; set; }
	}

	public class CUSDSBPostingValidationResult
	{
		public CUSDSBPostingValidationResult(ZString uniqueNumber, AutoPostingNotification autoPostingNotification)
		{
			arDiscrepancyCharges = new List<ChargesSummary>();
			apDiscrepancyCharges = new List<ChargesSummary>();
			warnings = new List<string>();
			errors = new List<string>();
			this.UniqueNumber = uniqueNumber;
			this.AutoPostingNotification = autoPostingNotification;
		}

		readonly List<ChargesSummary> apDiscrepancyCharges;
		readonly List<ChargesSummary> arDiscrepancyCharges;
		readonly List<string> warnings;
		readonly List<string> errors;
		public readonly ZString UniqueNumber;
		public readonly AutoPostingNotification AutoPostingNotification;

		public ZString APInvoiceDetail
		{
			get;
			set;
		}

		public ZString ARInvoiceDetail
		{
			get;
			set;
		}

		public void Merge(CUSDSBPostingValidationResult passedResult)
		{
			if (!passedResult.UniqueNumber.IsEmpty &&
				!UniqueNumber.IsEmpty &&
				passedResult.UniqueNumber != UniqueNumber)
			{
				throw new InvalidOperationException();
			}

			apDiscrepancyCharges.AddRange(passedResult.apDiscrepancyCharges);
			apDiscrepancyCharges.AddRange(passedResult.arDiscrepancyCharges);
			warnings.AddRange(passedResult.warnings);
			errors.AddRange(passedResult.errors);
		}

		public ChargesSummary[] APDiscrepancyCharges
		{
			get { return apDiscrepancyCharges.ToArray(); }
		}

		public void AddAPDiscrepancyCharge(ChargesSummary[] chargeSummary)
		{
			apDiscrepancyCharges.AddRange(chargeSummary);
		}

		public ChargesSummary[] ARDiscrepancyCharges
		{
			get { return arDiscrepancyCharges.ToArray(); }
		}

		public void AddARDiscrepancyCharge(ChargesSummary[] chargeSummary)
		{
			arDiscrepancyCharges.AddRange(chargeSummary);
		}

		public IEnumerable<string> Messages
		{
			get
			{
				foreach (string error in errors)
				{
					yield return error;
				}

				foreach (string warning in warnings)
				{
					yield return warning;
				}
			}
		}

		public string MessageIncludingDiscrepancyDetails
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				if (HasMessages)
				{
					result.Append(Res.GetString("bf9d940d-af37-463b-9208-b6407f38074e", "Processing result for {0}", UniqueNumber));
				}

				foreach (string message in Messages)
				{
					result.Append(message);
				}

				if (apDiscrepancyCharges.Count > 0)
				{
					result.Append(Res.GetString("f1a09414-da59-446c-998b-20909bc6db72", "There is discrepancy between Customs amount and an existing AP invoice, {0}", APInvoiceDetail));
				}

				if (arDiscrepancyCharges.Count > 0)
				{
					result.Append(Res.GetString("a8b41d44-ab0e-4f10-92f1-745b84a7b013", "There is discrepancy between Customs amount and an existing AR invoice, {0}", ARInvoiceDetail));
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public void AddError(string message)
		{
			errors.Add(message);
		}

		public void AddWarning(string message)
		{
			warnings.Add(message);
		}

		/// <summary>
		/// AP Discrepancy or AR Discrepancy does not deem as error. When posting happens, it does not repost what has been posted.
		/// When posting AP with AR Discrepancy, AP posting should proceed with an email notification to users with AR discrepancy or vice versa
		/// </summary>
		public bool HasErrors
		{
			get { return errors.Count > 0; }
		}

		public bool HasMessages
		{
			get { return HasErrors || warnings.Count > 0 || apDiscrepancyCharges.Count > 0 || arDiscrepancyCharges.Count > 0; }
		}

		public bool ContainsMessage(string partialMessage)
		{
			foreach (string message in Messages)
			{
				if (message.Contains(partialMessage))
				{
					return true;
				}
			}
			return false;
		}
	}
}
