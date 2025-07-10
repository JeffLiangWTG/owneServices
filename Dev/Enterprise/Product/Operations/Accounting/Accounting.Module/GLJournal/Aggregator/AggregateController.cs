using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Module.Res;

namespace Enterprise.Accounting.Aggregator
{
	public class AggregateController
	{
		static string TakeUpGLAccountsMessage => Res.GetString("Accounting|TakeUpGLAccountsMessage",
@"******* SUB-LEDGER (A/R & A/P) LEDGER TAKEUP / POSTINGS ********

If you wish to continue working with the current sub-ledger balances (for posting of GL adjustments etc) then answer 'No'.
If you wish to report on current trading / sub-ledger movements then answer 'Yes'.

Answering 'Yes' will run a potentially large process that can adversely slow system performance on large systems. Please note the following warning.

******* PERFORMANCE WARNING **********

When answering Yes to this question, there are potential performance consequences.
If this process is run during production hours on large systems it can potentially slow operational processing.
The safest approach on a larger heavily used system is to run this process out of business hours.");

		public void PerformAggregationIfRequired()
		{
			if (Env.Security.TakeUpSubLedgerTranactions.IsAllowed)
			{
				ZString lastRunMessage = GetLastGLTakeupMessage();

				if (Globals.Message.Show(TakeUpGLAccountsMessage + System.Environment.NewLine + System.Environment.NewLine + lastRunMessage, Res.GetString("2f12ae0d-e7bc-4b7d-b78b-548bda94b90b", "GL Accounts"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					PerformAggregation();
					AccountingConfigurationRegistry.Instance.LastAggregationDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime());
					AccountingConfigurationRegistry.Instance.LastAggregationStaff.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, GlbStaff.CurrentUser.PK.ToGuid());
				}
			}
			else
			{
				StringBuilder msg = new StringBuilder();

				msg.AppendLine(Res.GetString("c1ce4015-9ffc-4a93-a8a0-8be92bc28c4a", @"You do not have the necessary security rights to perform GL take up. Please contact your system administrator to grant you the following security right:
{0}"					, Env.Security.TakeUpSubLedgerTranactions.DisplayTextPathToSecurityRight));
				msg.AppendLine(GetLastGLTakeupMessage());

				var automaticSubLedgerTakeupRegistrySetting = AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (!automaticSubLedgerTakeupRegistrySetting.NextRunDateTime.IsValid)
				{
					msg.Append(Res.GetString("140d8c98-65e5-45f3-bac4-526ffda9a9b5", "It is also possible to automatically run GL Take up via the service task 'ATU'. To enable that you need to setup the following registry: {0}/{1}"
						, AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.Category
						, AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.Caption));
				}
				else
				{
					msg.Append(Res.GetString("c26195b3-74c8-42d4-8a37-54155a9a5368", "Your system is set to run an automatic GL Account take up on: {0}"
						, automaticSubLedgerTakeupRegistrySetting.NextRunDateTime));
				}

				Globals.Message.ShowInformation(msg.ToString(), Res.GetString("a9f4b0e8-7820-4e5e-be62-d16f9e758f2e", "GL take up"));
			}
		}

		ZString GetLastGLTakeupMessage()
		{
			ZString message = ZString.Empty;
			if (AccountingConfigurationRegistry.Instance.LastAggregationDate.Value != DateTime.MinValue &&
								AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value != Guid.Empty)
			{
				ZGuid staffPK = new ZGuid(AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value);
				GlbStaff lastAggregationStaff = new BusinessObjectFactory().Load<GlbStaff>(staffPK);

				string byUserLabel =
					lastAggregationStaff != null
						? Res.GetString("Accounting|TakeUpGLAccountsMessage|ByUserAppendix", "by user: {0}", lastAggregationStaff.GS_FullName)
						: string.Empty;

				message = Res.GetString("Accounting|GLAccountsTakenUpMessage|LastTakenAppendix", "GL accounts were last taken up on {0} {1}",
						(ZDateTime)AccountingConfigurationRegistry.Instance.LastAggregationDate.Value, byUserLabel);
			}

			return message;
		}

		void PerformAggregation()
		{
			var aggregator = new AggregateRunner();
			var aggregationWasSuccessful = false;

			try
			{
				Cursor.Current = Cursors.WaitCursor;
				aggregationWasSuccessful = aggregator.Aggregate();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}

			if (!aggregationWasSuccessful)
			{
				Globals.Message.ShowError(aggregator.AggregateResult, Res.GetString("a9cd2cd0-ca7f-432b-aee9-6d120403cbc7", "GL Account"));
			}
		}
	}
}
