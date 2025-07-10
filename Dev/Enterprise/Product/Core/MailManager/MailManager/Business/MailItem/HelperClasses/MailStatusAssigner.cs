using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager.Business
{
	public class MailStatusAssigner
	{
		public MailStatusAssigner(BusinessObject[] selectedElements, string newStatus)
		{
			fSelectedElements = selectedElements;
			fNewStatus = newStatus;

			SavingFactory = new BusinessObjectFactory();
		}

		public
#if DEBUG
			virtual
#endif
			string Errors
		{
			get { return fErrors; }
		}

		public
#if DEBUG
			virtual
#endif
			bool HasDBChanged
		{
			get { return fHasDBChanged; }
		}

		public
#if DEBUG
			virtual
#endif
			int ItemsAffected
		{
			get { return fItemsAffected; }
		}

		public virtual void Assign()
		{
			fErrors = "";
			fHasDBChanged = false;
			fItemsAffected = 0;

			ZQuery filter = new ZQuery();
			ZGuid[] selectedItems = new ZGuid[fSelectedElements.Length];
			for (int i = 0; i < fSelectedElements.Length; i++)
			{
				selectedItems[i] = fSelectedElements[i].PK;
			}
			filter.AddToFilter(MailDBItemsSchema.PK, SQLComparisonOperator.Equal, selectedItems);

			MailItem[] itemsToAssign = (MailItem[])SavingFactory.Load(typeof(MailItem), filter);

			if (itemsToAssign.Length == fSelectedElements.Length)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(
					//action
					() =>
					{
						foreach (MailItem item in itemsToAssign)
						{
							if ((item.MI_Status != fNewStatus) &&
								!((fNewStatus == MailStatus.Queued) && (item.MI_Status == MailStatus.QueuedWithAck) && item.IsInDatabase))
							{
								item.MI_Status = fNewStatus;
								item.RunPreSaveValidation();

								if (item.HasErrors)
								{
									fErrors += item.Notifications.GetErrors().ToUniqueMessageListString();
								}
								else
								{
									fItemsAffected++;
								}
							}
						}

						if (fErrors.Length == 0)
						{
							SavingFactory.Save();
						}
						else
						{
							fItemsAffected = 0;
						}
					},
					//recoveryAction
					() =>
					{
						foreach (var item in itemsToAssign)
						{
							item.ReloadSafe();
						}
						fItemsAffected = 0;
					});
			}
			else
			{
				fHasDBChanged = true;
			}
		}

#if DEBUG
		public
#endif
		readonly BusinessObjectFactory SavingFactory;
		readonly BusinessObject[] fSelectedElements;
		readonly string fNewStatus;
		string fErrors;
		bool fHasDBChanged;
		int fItemsAffected;
	}
}
