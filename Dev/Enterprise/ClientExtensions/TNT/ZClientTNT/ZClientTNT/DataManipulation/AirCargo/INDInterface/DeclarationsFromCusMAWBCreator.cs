using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.AirCargo
{
	public class DeclarationsFromCusMAWBCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DeclarationsFromCusMAWBCreator(CusMAWB masterBill)
			: base(masterBill.Factory)
		{
			if (masterBill == null)
			{
				throw new ArgumentNullException(nameof(masterBill));
			}
			this.MasterBill = masterBill;
			fNoOfDeclarationCreated = 0;
			fProgressLog = ZString.Empty;
			((IZPropertyInfoObsolete)ProgressLogInfo).ReadOnly = true;
		}

		public readonly CusMAWB MasterBill;
		public event TNTProgressEventHandler Progress;

		public void StopCreation(object sender, EventArgs e)
		{
			Stop = true;
		}

		public void CreateDeclarations(INotifications notify)
		{
			int i = 0;
			int totalCount = MasterBill.ChildBills.Count;
			notify.Notify(new InfoNotification(string.Format("Start Checking {0} Housebill(s) for {1}", totalCount.ToString(), MasterBill.UnderbondHumanReadableName)));
			Stop = false;

			Factory.SuspendValidation();
			foreach (CusHAWB houseBill in MasterBill.ChildBills)
			{
				if (Stop)
				{
					notify.Notify(new WarningNotification("Process stopped by user"));
					break;
				}
				ShowProgress(++i, totalCount, "Checking " + houseBill.UnderbondHumanReadableName);
				CreateDeclarationIfNeeded(houseBill, notify, i, totalCount);
			}

			if (NoOfDeclarationCreated > 0)
			{
				ShowProgress(totalCount, totalCount, "Saving Declarations");
				try
				{
					Factory.Save();
				}
				catch (ZSaveConcurrencyException ex)
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, "Failed to save Records due to Concurrency Exception: " + ex.Message));
				}
			}
			ShowProgress(100, 100, "Process completed.");
		}

		#region ProgressLog

		[MaxLength(ProgressLogMaximumLength)]
		public ZString ProgressLog
		{
			get { return fProgressLog; }
			set
			{
				if (fProgressLog != value)
				{
					SetNonPersistentPropertyValue(ProgressLogInfo, ref fProgressLog, value);
				}
			}
		}

		ZString fProgressLog;

		public ZPropertyInfo ProgressLogInfo
		{
			get { return GetZPropertyInfo(nameof(ProgressLog)); }
		}

		internal const int ProgressLogMaximumLength = 320000;

		#endregion

		#region NoOfDeclarationCreated

		public int NoOfDeclarationCreated
		{
			get { return fNoOfDeclarationCreated; }
		}

		int fNoOfDeclarationCreated;

		#endregion

		#region CreateDeclarationIfNeeded

		void CreateDeclarationIfNeeded(CusHAWB houseBill, INotifications notify, int current, int total)
		{
			if (IsDeclarationNeedToBeCreated(houseBill, notify))
			{
				fNoOfDeclarationCreated++;
				TNTDeclarationFromAirCargoCreator declarationCreator = new TNTDeclarationFromAirCargoCreator(houseBill);
				JobDeclaration declaration = (JobDeclaration)declarationCreator.Create(notify);
				notify.Notify(new InfoNotification(string.Format("Declaration {0} created for {1}", declaration.JE_DeclarationReference, houseBill.UnderbondHumanReadableName)));
			}
		}

		bool IsDeclarationNeedToBeCreated(CusHAWB houseBill, INotifications notify)
		{
			bool result = false;
			if (!houseBill.CS_IsSelfAssessedClearance)
			{
				if (!IsLinkedToShipment(houseBill))
				{
					if (!IsLinkedToDeclaration(houseBill))
					{
						if (!IsSameDeclartionExist(houseBill))
						{
							if (IsLocalDestination(houseBill))
							{
								result = true;
							}
							else
							{
								notify.Notify(new InfoNotification(string.Format("{0} is a transhipment; Declaration will not be created for it", houseBill.UnderbondHumanReadableName)));
							}
						}
						else
						{
							notify.Notify(new InfoNotification(string.Format("A Declaration with the same Housebill number is already exists; No new Declaration will be created for {0}", houseBill.UnderbondHumanReadableName)));
						}
					}
					else
					{
						notify.Notify(new InfoNotification(string.Format("{0} is already linked to a Declaration", houseBill.UnderbondHumanReadableName)));
					}
				}
				else
				{
					notify.Notify(new InfoNotification(string.Format("{0} is already linked to a Shipment", houseBill.UnderbondHumanReadableName)));
				}
			}
			else
			{
				notify.Notify(new InfoNotification(string.Format("No Declaration created for {0} as it is a SAC", houseBill.UnderbondHumanReadableName)));
			}
			return result;
		}

		void ShowProgress(int current, int total, string message)
		{
			if (Progress != null)
			{
				Progress(this, new TNTProgressEventArgs(current, total, message));
			}
		}

		bool IsLinkedToShipment(CusHAWB houseBill)
		{
			return houseBill.CS_JS.IsValid;
		}

		bool IsLinkedToDeclaration(CusHAWB houseBill)
		{
			return houseBill.CS_JE_CustomsFormalEntry.IsValid;
		}

		bool IsSameDeclartionExist(CusHAWB houseBill)
		{
			ZQuery filter = new ZQuery(JobDeclarationSchema.JE_MasterBill, houseBill.CS_MasterBillNum);
			filter.AddToFilter(JobDeclarationSchema.JE_HouseBill, houseBill.CS_HAWB);
			return (Factory.Load(typeof(JobDeclaration), filter)).Length > 0;
		}

		bool IsLocalDestination(CusHAWB houseBill)
		{
			return houseBill.CS_RL_NKDestination.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		#endregion

		bool Stop;
	}
}
