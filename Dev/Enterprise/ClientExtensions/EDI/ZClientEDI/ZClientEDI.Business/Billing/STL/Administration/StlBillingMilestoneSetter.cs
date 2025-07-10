using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlBillingMilestoneSetter : AutoStlBillingMilestoneSetter
	{
		public StlBillingMilestoneSetter() : base(new BusinessObjectFactory()) { }

		[List(nameof(Databases))]

		[RelatedBusinessObject("Database")]
		public override ZGuid DatabasePk { get => base.DatabasePk; set => base.DatabasePk = value; }

		public LicenceDatabase Database => Factory.Load<LicenceDatabase>(DatabasePk);

		public LicenceDatabaseNonDependentCollection Databases => new LicenceDatabaseNonDependentCollection(Factory);

		public ZDate BillingPeriodAsZDate => ZDateTime.TryParseExact(BillingPeriod.ToString(), out var result, "yyyyMM") ? result.Date : ZDate.Empty;

		public void SetMilestone(ILogger logger)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var milestoneQuery = new ZQuery(ClientChargeableUsageSchema.U1_PeriodStart, BillingPeriodAsZDate)
				.AddToFilter(ClientChargeableUsageSchema.U1_LD, DatabasePk)
				.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.STL)
				.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, DatabaseUsage.MilestoneUsageCode);

			var billedUsageQuery = new ZQuery(ClientChargeableUsageSchema.U1_PeriodStart, BillingPeriodAsZDate)
				.AddToFilter(ClientChargeableUsageSchema.U1_LD, DatabasePk)
				.AddToFilter(ClientChargeableUsageSchema.U1_AH_Invoice, SQLComparisonOperator.NotEqual, DBNull.Value);

			var milestones = factory.Load<ClientChargeableUsage>(milestoneQuery);
			if (!milestones.Any())
			{
				logger.Error((NoResString)"The specified milestone could not be found.");
			}
			else if (milestones.All(x => x.U1_UnitCount == 1))
			{
				logger.Error((NoResString)"The milestone is complete.");
			}
			else if (factory.ExistsInDatabase(ClientChargeableUsageSchema.Constants.TableName, billedUsageQuery))
			{
				logger.Error((NoResString)"Billing for this client for the specified period has already been completed.");
			}
			else
			{
				foreach (var item in milestones.Where(x => x.U1_UnitCount != 1).ToArray())
				{
					item.U1_UnitCount = 1;
				}
				factory.Save();
				logger.Information((NoResString)"The milestone was successfully set.");
			}
		}
	}

	public class StlBillingMilestoneSetterValidation : AutoStlBillingMilestoneSetterValidation
	{
		public StlBillingMilestoneSetterValidation(AutoStlBillingMilestoneSetter parent)
			: base(parent) { }

		public new StlBillingMilestoneSetter Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (StlBillingMilestoneSetter)base.Parent; }
		}

		protected override void CheckBillingPeriod()
		{
			if (Parent.BillingPeriodAsZDate.IsEmpty)
			{
				Parent.BillingPeriodInfo.AddError((NoResString)"Invalid input. Please enter the date in the format yyyyMM (e.g., 202402).");
			}
		}

		protected override void CheckDatabasePk()
		{
			MandatoryValidation.CheckEntered(Parent.DatabasePkInfo);
			ListValidation.ErrorIfInvalidPK(Parent.DatabasePkInfo);
		}
	}
}
