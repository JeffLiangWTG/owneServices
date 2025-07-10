using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	internal class AllChargesStrategy : IChargeSpecificationStrategy
	{
		public AllChargesStrategy(IValueObjectImportContext context, bool ignorePosted)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			this.context = context;
			this.ignorePosted = ignorePosted;
			this.unMatchedCharges = new Dictionary<Job, List<Charge>>();
			this.skipHeaderCache = new Dictionary<Job, bool>();
		}

		public bool SkipJobHeader(Job header)
		{
			if (ignorePosted)
			{
				return false;
			}
			else
			{
				bool result;

				if (!skipHeaderCache.TryGetValue(header, out result))
				{
					result = HasPostedCharges(header);
					skipHeaderCache.Add(header, result);
				}

				return result;
			}
		}

		public virtual Charge MatchCharge(Job job, AccChargeCode chargeCode, GlbBranch branch, GlbDepartment department, Xsd.ChargeLine line)
		{
			List<Charge> unMatched = GetUnMatchedCharges(job);
			var charges = unMatched.Where(c => c.JR_AC == chargeCode.PK);

			if (branch != null)
			{
				charges = charges.Where(c => c.JR_GB == branch.PK);
			}

			if (department != null)
			{
				charges = charges.Where(c => c.JR_GE == department.PK);
			}

			if (line.Debtor.IsSpecified)
			{
				ZGuid debtorPK = context.FindOrCreateTempOrganisationPK(line.Debtor, job, OrganisationTypes.Debtor);

				var exactCharges = charges.Where(c => c.JR_OH_SellAccount == debtorPK);
				charges = exactCharges.Any() ? exactCharges : charges.Where(c => c.JR_OH_SellAccount.IsEmpty);
			}

			if (line.Creditor.IsSpecified)
			{
				ZGuid creditorPK = context.FindOrCreateTempOrganisationPK(line.Creditor, job, OrganisationTypes.Creditor);

				var exactCharges = charges.Where(c => c.JR_OH_CostAccount == creditorPK);
				charges = exactCharges.Any() ? exactCharges : charges.Where(c => c.JR_OH_CostAccount.IsEmpty);
			}

			Charge result = charges.FirstOrDefault();

			if (result != null)
			{
				unMatched.Remove(result);
			}

			return result;
		}

		public void RemoveUnmatchedCharges()
		{
			foreach (var pair in this.unMatchedCharges)
			{
				using (pair.Key.Charges.SuspendListChanged())
				{
					foreach (var charge in pair.Value)
					{
						if (IsChargeSafeToRemove(charge))
						{
							pair.Key.Charges.RemoveAndDelete(charge);
						}
					}
				}

				if (pair.Key.Department != null)
				{
					pair.Key.AddDepartmentCharges();
				}
			}
		}

		public void NotifySkippedHeaders()
		{
			foreach (var pair in this.skipHeaderCache)
			{
				if (pair.Value)
				{
					context.Notify(new WarningNotification(WarningType.Warning, GetSkippedHeaderReason(pair.Key)));
				}
			}
		}

		#region Implementation

		protected virtual bool IsChargeSafeToRemove(Charge charge)
		{
			return true;
		}

		string GetSkippedHeaderReason(Job header)
		{
			return Res.GetString("2606d33e-2e54-4fb8-a1b9-19f18dfe8ccf",
				"The Invoicing Job for company {0} was skipped during the Import process as it has posted charges.",
				header.Company.GC_Code);
		}

		bool HasPostedCharges(Job header)
		{
			foreach (Charge charge in header.Charges)
			{
				if (charge.IsCostPosted || charge.IsRevenuePosted)
				{
					return true;
				}
			}

			return false;
		}

		List<Charge> GetUnMatchedCharges(Job job)
		{
			List<Charge> result;

			if (!this.unMatchedCharges.TryGetValue(job, out result))
			{
				result = new List<Charge>();

				foreach (Charge charge in job.Charges)
				{
					if (charge.JR_IsRevenuePosted || charge.JR_IsCostPosted)
					{
						continue;
					}

					result.Add(charge);
				}

				unMatchedCharges.Add(job, result);
			}

			return result;
		}

		readonly bool ignorePosted;
		readonly IValueObjectImportContext context;
		readonly Dictionary<Job, List<Charge>> unMatchedCharges;
		readonly Dictionary<Job, bool> skipHeaderCache;

		#endregion
	}
}
