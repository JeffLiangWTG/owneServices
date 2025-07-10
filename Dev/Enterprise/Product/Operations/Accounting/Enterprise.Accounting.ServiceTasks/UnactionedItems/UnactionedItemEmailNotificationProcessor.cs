using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ServiceTasks
{
	public abstract class UnactionedItemEmailNotificationProcessor<T> where T : BusinessObject
	{
		protected UnactionedItemEmailNotificationProcessor(ILogger logger)
		{
			ServiceLogger = logger;
		}

		readonly ILogger ServiceLogger;

		protected BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		protected abstract IEnumerable<T> GetUnactionedItems();
		protected abstract AccountingHtmlEmailDef GetNewNotificationEmail(IEnumerable<T> items, ZStringBuilder errorMessages, ZGuid companyPK);
		protected abstract ZBool ShouldSendEmailForCompany(ZGuid companyPK);

		protected abstract ZGuid GetItemPK(T item);
		protected abstract ZGuid GetItemCompanyPK(T item);
		protected abstract ZString GetItemCompanyName(T item);
		protected abstract ZGuid GetItemBranchPK(T item);

		public void SendEmail()
		{
			var unactionedItems = GetUnactionedItems();
			if (unactionedItems.Any())
			{
				foreach (var unactionedItemsGroupedByCompany in unactionedItems.GroupBy(GetItemCompanyPK))
				{
					var companyPK = unactionedItemsGroupedByCompany.Key;
					if (ShouldSendEmailForCompany(companyPK))
					{
						var companyName = GetItemCompanyName(unactionedItemsGroupedByCompany.First());

						using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), (GetItemBranchPK(unactionedItemsGroupedByCompany.First())).ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
						{
							var errorMessages = new ZStringBuilder();
							var email = GetNewNotificationEmail(unactionedItemsGroupedByCompany, errorMessages, companyPK);
							var result = email.Send();
							if (result == EmailSendResult.Successful)
							{
								ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "{0} was sent successfully for {1}.", email.EmailDescription, companyName));
							}
							else
							{
								ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "{0} was not sent for {1}. {2}", email.EmailDescription, companyName, errorMessages.ToStringWithNewLineBetweenAppends()));
							}
						}
					}
				}
			}
		}
	}
}
