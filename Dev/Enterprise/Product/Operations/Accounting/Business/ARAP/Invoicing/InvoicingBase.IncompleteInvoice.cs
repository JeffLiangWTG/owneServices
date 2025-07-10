using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingBase
	{
		void PrepareAttributesOfImportedChargesAndCostsForSavingIncompleteInvoice()
		{
			if (Factory.HasContext(BusinessContext.SavingAsIncomplete))
			{
				SyncImportedConsolCostAttrib();

				SyncImportedChargeAttrib();
			}

			void SyncImportedConsolCostAttrib()
			{
				var importedCosts = GetImportedConsolCostDetailsFromIncompleteInvoice();
				var costsThatAreNewlyImported = new List<JobConsolCost>();
				var costsThatWereImportedOriginallyButDeleted = new List<JobConsolCost>();

				if (ConsolCostsOriginallyImportedToIncompleteInvoice?.Any() ?? false)
				{
					//identify costs that were not in the originally imported cost list
					foreach (var importedConsolCost in importedCosts)
					{
						if (!ConsolCostsOriginallyImportedToIncompleteInvoice.Any(originalConsolCost => importedConsolCost.PK == originalConsolCost.PK))
						{
							costsThatAreNewlyImported.Add(importedConsolCost);
						}
					}

					//identify costs that were in the originally imported cost list, but have been removed now
					foreach (var originalConsolCost in ConsolCostsOriginallyImportedToIncompleteInvoice)
					{
						if (!importedCosts.Any(importedConsolCost => importedConsolCost.PK == originalConsolCost.PK))
						{
							costsThatWereImportedOriginallyButDeleted.Add(originalConsolCost);
						}
					}
				}
				else
				{
					costsThatAreNewlyImported.AddRange(importedCosts);
				}

				foreach (var newImportedCost in costsThatAreNewlyImported)
				{
					newImportedCost.Attributes.Add(JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice, PK.ToString());
				}

				if (costsThatWereImportedOriginallyButDeleted.Any())
				{
					DeleteINVJobConsolCostAttributesByJobConsolCostPK(Factory, costsThatWereImportedOriginallyButDeleted.Select(c => c.PK));
				}
			}

			void SyncImportedChargeAttrib()
			{
				var importedCharges = GetImportedChargeDetailsFromIncompleteInvoice();
				var chargesThatAreNewlyImported = new List<JobCharge>();
				var chargesThatWereImportedOriginallyButDeleted = new List<JobCharge>();

				if (ChargesOriginallyImportedToIncompleteInvoice?.Any() ?? false)
				{
					//identify charges that were not in the originally imported charge list
					foreach (var importedCharge in importedCharges)
					{
						if (!ChargesOriginallyImportedToIncompleteInvoice.Any(originalCharge => originalCharge.PK == importedCharge.PK))
						{
							chargesThatAreNewlyImported.Add(importedCharge);
						}
					}

					//identify charges that were in the originally imported charge list, but have been removed now
					foreach (var originalCharge in ChargesOriginallyImportedToIncompleteInvoice)
					{
						if (!importedCharges.Any(importedCharge => importedCharge.PK == originalCharge.PK))
						{
							chargesThatWereImportedOriginallyButDeleted.Add(originalCharge);
						}
					}
				}
				else
				{
					chargesThatAreNewlyImported.AddRange(importedCharges);
				}

				foreach (var newImportedCharge in chargesThatAreNewlyImported)
				{
					var attrib = newImportedCharge.JobChargeAttributes.AddNew();
					attrib.EC_JR = newImportedCharge.PK;
					attrib.EC_Name = JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice;
					attrib.EC_Value = PK.ToString();
				}

				if (chargesThatWereImportedOriginallyButDeleted.Any())
				{
					DeleteINVJobChargeAttributesByJobChargePK(Factory, chargesThatWereImportedOriginallyButDeleted.Select(c => c.PK).ToArray());
				}
			}
		}

		void DeleteINVAttributeOfChargesAndCostsImportedToIncompleteInvoice(BusinessObjectFactory factory)
		{
			var delJobChargeAttribQuery = new ZDBOnlyQuery(typeof(JobChargeAttrib));
			delJobChargeAttribQuery.AddToFilter(JobChargeAttribSchema.EC_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, AH_SystemCreateTimeUtc);
			delJobChargeAttribQuery.AddToFilter(JobChargeAttribSchema.EC_SystemLastEditTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, AH_SystemLastEditTimeUtc);
			delJobChargeAttribQuery.AddToFilter(JobChargeAttribSchema.EC_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);
			delJobChargeAttribQuery.AddToFilter(JobChargeAttribSchema.EC_Value, PK.ToString());
			var jobChargeAttribs = factory.Load<JobChargeAttrib>(delJobChargeAttribQuery);
			if (jobChargeAttribs.Length > 0)
			{
				jobChargeAttribs.DeleteAll();
			}

			var delJobConsolCostAttribQuery = new ZDBOnlyQuery(typeof(JobConsolCostAttrib));
			delJobConsolCostAttribQuery.AddToFilter(JobConsolCostAttribSchema.E6A_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, AH_SystemCreateTimeUtc);
			delJobConsolCostAttribQuery.AddToFilter(JobConsolCostAttribSchema.E6A_SystemLastEditTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, AH_SystemLastEditTimeUtc);
			delJobConsolCostAttribQuery.AddToFilter(JobConsolCostAttribSchema.E6A_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);
			delJobConsolCostAttribQuery.AddToFilter(JobConsolCostAttribSchema.E6A_Value, PK.ToString());
			var jobConsolCostAttribs = factory.Load<JobConsolCostAttrib>(delJobConsolCostAttribQuery);
			if (jobConsolCostAttribs.Length > 0)
			{
				jobConsolCostAttribs.DeleteAll();
			}
		}

		void DeleteINVJobConsolCostAttributesByJobConsolCostPK(BusinessObjectFactory factory, IEnumerable<ZGuid> jobConsolCostPKs)
		{
			foreach (var pkBatch in AccountingUtils.ChunksOf(jobConsolCostPKs, 25))
			{
				var invAttribs = LoadINVJobConsolCostAttributes(factory ?? Factory, pkBatch.ToArray());
				invAttribs.DeleteAll();
			}
		}

		void DeleteINVJobChargeAttributesByJobChargePK(BusinessObjectFactory factory, IEnumerable<ZGuid> jobChargePKs)
		{
			foreach (var pkBatch in AccountingUtils.ChunksOf(jobChargePKs, 25))
			{
				var invAttribs = LoadINVJobChargeAttributes(factory ?? Factory, pkBatch.ToArray());
				invAttribs.DeleteAll();
			}
		}

		List<JobCharge> GetImportedChargeDetailsFromIncompleteInvoice()
		{
			var importedCharges = new List<JobCharge>();
			foreach (InvoicingLineBase line in Lines)
			{
				if (line.IsPopulatedFromImportedJobCharge && line.OriginalJobCharge.IsInDatabase && !line.OriginalJobCharge.JR_E6.IsValid)
				{
					if (line.OriginalJobCharge != null)
					{
						importedCharges.Add(line.OriginalJobCharge);
					}
				}
			}
			return importedCharges;
		}

		List<JobConsolCost> GetImportedConsolCostDetailsFromIncompleteInvoice()
		{
			return ConsolCosting.ConsolCosts.Where(c => c.IsImportedConsolCost && c.RelatedConsolCostFromDatabase != null)
												.Select(c => c.RelatedConsolCostFromDatabase)
												.OfType<JobConsolCost>()
												.ToList();
		}

		JobChargeAttrib[] LoadINVJobChargeAttributes(BusinessObjectFactory factory, ZGuid[] jobChargePKs)
		{
			var invAttribsToDeleteQuery = new ZQuery(JobChargeAttribSchema.EC_JR, jobChargePKs);
			invAttribsToDeleteQuery.AddToFilter(JobChargeAttribSchema.EC_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);
			var invAttribs = factory.Load<JobChargeAttrib>(invAttribsToDeleteQuery);
			return invAttribs;
		}

		JobConsolCostAttrib[] LoadINVJobConsolCostAttributes(BusinessObjectFactory factory, ZGuid[] jobConsolCostPKs)
		{
			var invAttribsToDeleteQuery = new ZQuery(JobConsolCostAttribSchema.E6A_E6_JobConsolCost, jobConsolCostPKs);
			invAttribsToDeleteQuery.AddToFilter(JobConsolCostAttribSchema.E6A_Name, JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice);
			var invAttribs = factory.Load<JobConsolCostAttrib>(invAttribsToDeleteQuery);
			return invAttribs;
		}

		List<JobCharge> ChargesOriginallyImportedToIncompleteInvoice { get; set; }

		List<JobConsolCost> ConsolCostsOriginallyImportedToIncompleteInvoice { get; set; }
	}
}
