using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class PeriodReopenedEmail : AccountingEmailDef
	{
		public PeriodReopenedEmail(AccPeriodManagement period)
		{
			this.AM_Period = period.AM_Period;
			this.Body = GetBodyCore(period);
		}
		new readonly string Body;
		protected ZInt AM_Period;

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.PeriodReopenNotifyGroup; }
		}

		protected override string GetSubject()
		{
			return Res.GetString("7D3C3926-2780-449f-8187-805E89C0892F", "Accounting Period {0} of {1} ({2}) has been reopened.", AM_Period.ToString(), GlbCompany.CurrentCompany.GC_Name, GlbCompany.CurrentCompany.GC_Code);
		}

		string GetBodyCore(AccPeriodManagement periodVar)
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append(Res.GetString("0DA115C3-52EF-4f29-902D-87A57528F4ED", "Accounting Period {0} of {1} ({2}) has been reopened by {3} ({4}).", periodVar.AM_Period.ToString(), GlbCompany.CurrentCompany.GC_Name, GlbCompany.CurrentCompany.GC_Code, GlbStaff.CurrentUser.GS_FullName, GlbStaff.CurrentUser.GS_Code));

			if (!periodVar.AM_IsSubLedgerClosed && periodVar.AM_IsSubLedgerClosedInfo.HasChanges)
			{
				result.Append(Res.GetString("D4381F0B-50DA-4998-B4B6-22128046BCD1", "- The Sub Ledger has been reopened"));
			}
			if (!periodVar.AM_IsGeneralLedgerClosed && periodVar.AM_IsGeneralLedgerClosedInfo.HasChanges)
			{
				result.Append(Res.GetString("C9EA2CE7-8451-4ee4-B4DD-9E68DEE8964C", "- The General Ledger has been reopened"));
			}
			if (!periodVar.AM_IsSubledgerClosedForAdjustments && periodVar.AM_IsSubledgerClosedForAdjustmentsInfo.HasChanges)
			{
				result.Append(Res.GetString("E363289D-54BB-4a21-BD76-BEE76CCC5005", "- This period has been reopened for Adjustments"));
			}

			string yes = Res.GetString("c7c49dc1-8e2d-4d41-883a-241c12d57a9d", "Yes");
			string no = Res.GetString("91967d1d-695e-4858-b43d-b8ab191135fc", "No");
			string subLedgerIsClosed = periodVar.AM_IsSubLedgerClosed ? yes : no;
			string generalLedgerIsClosed = periodVar.AM_IsGeneralLedgerClosed ? yes : no;
			string adjustmentsLedgerIsClosed = periodVar.AM_IsSubledgerClosedForAdjustments ? yes : no;

			result.Append(ZString.Empty);
			result.Append(Res.GetString("31860092-5659-4d9b-8433-2BD402F4248E", "Current Settings:\r\nPeriod: {0}\r\nStart Date: {1}\r\nEnd Date: {2}\r\nSub Ledger is Closed: {3}\r\nGeneral Ledger is closed: {4}\r\nClosed for Adjustments: {5}",
				periodVar.AM_Period.ToString(),
				periodVar.AM_StartDate.Date.ToString(),
				periodVar.AM_EndDate.Date.ToString(),
				subLedgerIsClosed,
				generalLedgerIsClosed,
				adjustmentsLedgerIsClosed));

			return result.ToStringWithNewLineBetweenAppends();
		}

		protected override string GetBody()
		{
			return Body;
		}
	}
}
