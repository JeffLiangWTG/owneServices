using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class ReopenPeriodKeyBusinessObject : AutoReopenPeriodKeyBusinessObject
	{
		public ReopenPeriodKeyBusinessObject(ZString message, AccPeriodManagement period)
		{
			Message = message;
			Period = period;
			RequestSelected = true;
			ReopenSelected = false;
			HasChanges = false;
		}

		#region Period

		public AccPeriodManagement Period { get; set; }

		#endregion

		#region CheckBox binding property

		public ZBool ReopenSubLedgerSelected { get; set; }
		public ZBool ReopenGeneralLedgerSelected { get; set; }
		public ZBool ReopenForAdjustmentsSelected { get; set; }

		#endregion

		public bool ValidateKey(string key)
		{
			ReopenPeriodKeyGenerator generator = new ReopenPeriodKeyGenerator();
			return generator.ValidateKey(key, ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode, GlbCompany.CurrentCompany.GC_Code, GlbStaff.CurrentUser.GS_Code, Period.AM_Period);
		}

		public ZString ReopenPeriod(string key, bool reopenSubledger, bool reopenGeneralLedger, bool reopenForAdjustments)
		{
			ZString message = ZString.Empty;

			bool validKey = false;
			try
			{
				validKey = ValidateKey(key);
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			if (!validKey)
			{
				message = Res.GetString("7bab4358-5146-4cb2-a6c9-744be1627d04", "You have entered an invalid key.\r\n\r\nNote that a key:\r\n - Can only be used within 24 hours of it being issued\r\n - Must be used to reopen the period for which it was issued\r\n - Must be used by the user it was issued to\r\n\r\nPlease contact CargoWise support for assistance.");
			}
			else
			{
				List<string> openedItems = new List<string>();
				if (reopenSubledger && Period.AM_IsSubLedgerClosed)
				{
					Period.AM_IsSubLedgerClosed = false;
					openedItems.Add(Res.GetString("2a4fc049-0774-4902-b983-8d86399ca075", "Sub ledger"));
				}

				if (reopenGeneralLedger && Period.AM_IsGeneralLedgerClosed)
				{
					Period.AM_IsGeneralLedgerClosed = false;
					openedItems.Add(Res.GetString("8b831e95-6b4e-4309-8d8f-f632ebf5e5df", "General ledger"));
				}
				if (reopenForAdjustments && Period.AM_IsSubledgerClosedForAdjustments)
				{
					Period.AM_IsSubledgerClosedForAdjustments = false;
					openedItems.Add(Res.GetString("95088778-c9bc-44fc-8d5c-46f097d4a397", "Adjustments"));
				}
				if (openedItems.Count > 0)
				{
					message = Res.GetString("ac4aa47a-2d25-45ed-baaf-561f4e810319", "Period {0} is Reopened for: {1}.", Period.AM_Period.ToString(), string.Join(", ", openedItems));
					Period.Factory.Save();
				}
				else
				{
					message = Res.GetString("c86edfa9-07eb-45e0-94e7-8597d9c971c6", "Nothing to reopen!");
				}
			}
			return message;
		}
	}
}

