using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Accounting.GUI.PeriodManagement;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class PeriodManagementModule : ZEmbeddedModule
	{
		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PeriodManagement; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PeriodManagement; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		#region Implementation

		protected override Control GetNewEmbeddedControl()
		{
			PeriodManagementForm formToReturn = new PeriodManagementForm();
			formToReturn.ReAggregationFinished += new EventHandler(FormToReturn_ReAggregationFinished);
			formToReturn.SimulateAggregate += new EventHandler(FormToReturn_SimulateAggregate);
			formToReturn.AggregateAndReportAsXML += new EventHandler(FormToReturn_AggregateAndReportAsXML);

			return formToReturn;
		}

		protected void RunAggregator()
		{
			if (Globals.Message.Show(Res.GetString("b1569795-f89d-47cb-b36b-8adda2c7913e", "Do you want to take up GL Accounts?"), "Re-aggregator",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				var aggregator = ObjectFactory.New<IAggregateRunner>();
				DateTime start = Env.Time.CurrentLocalDateTime;
				var aggregatorSuccess = aggregator.Aggregate();
				TimeSpan time = Env.Time.CurrentLocalDateTime - start;

				if (aggregatorSuccess)
				{
					Globals.Message.Show(Res.GetString("48c2af9e-8033-4063-b622-0c7a3f07c5f5", "Taking up of GL finished. It took {0} minutes.", time.TotalMinutes));
				}
				else
				{
					Globals.Message.ShowError(aggregator.AggregateResult, Res.GetString("a9cd2cd0-ca7f-432b-aee9-6d120403cbc7", "GL Account"));
				}
			}
		}

		protected void RunAutoReaggregator()
		{
			var aggregator = ObjectFactory.New<IAggregateRunner>();
			aggregator.ReAggregate();
		}

		protected void RunReaggregateAndGenerateXML()
		{
			var aggregator = ObjectFactory.New<IAggregateRunner>();
			aggregator.ReAggregateAndReportAsXML();
		}

		bool isPopup;

		public override IZForm ShowPopup()
		{
			var form = base.ShowPopup();

			isPopup = true;
			OpenedFormCache.GetInstance().AdditionalFormsCount += 1;

			return form;
		}

		bool loweredAdditionalFormsCount;

		protected override void Dispose(bool isDisposing)
		{
			if (isPopup && !loweredAdditionalFormsCount)
			{
				OpenedFormCache.GetInstance().AdditionalFormsCount -= 1;
				isPopup = false;
				loweredAdditionalFormsCount = true;
			}

			base.Dispose(isDisposing);
		}

		#endregion

		void FormToReturn_ReAggregationFinished(object sender, EventArgs e)
		{
			RunAggregator();
		}

		void FormToReturn_SimulateAggregate(object sender, EventArgs e)
		{
			if (Globals.Message.Show((NoResString)@"Are you sure you want to re-aggregate transactions and send the Developer Message if there is any discrepancy?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this.", "ReAggregate", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				RunAutoReaggregator();
				Globals.Message.Show(Res.GetString("685b3eaa-00b1-406a-b208-cdff76b98c58", "Re-aggregation finished."));
			}
		}

		void FormToReturn_AggregateAndReportAsXML(object sender, EventArgs e)
		{
			if (Globals.Message.Show((NoResString)@"Are you sure you want to re-aggregate transactions and generate result as XML?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this.", "ReAggregate", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				RunReaggregateAndGenerateXML();
				Globals.Message.Show(Res.GetString("685b3eaa-00b1-406a-b208-cdff76b98c58", "Re-aggregation finished."));
			}
		}
	}
}
