using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosPreExportCheck
	{
		public CognosPreExportCheck(ICognosNotificationSubscriber notifications)
		{
			this.Notifications = notifications;
		}

		public virtual bool EnsureCanExport()
		{
			bool result = true;

			using (new ProcessStartFinishNotifier(Notifications, "Pre-Export Check", false, true))
			{
				result = result && EnsureAllBSHAndPnLAccountsAreMapped();
				result = result && EnsureUserGroupsForStatisticsAreAssigned();
			}

			return result;
		}

		bool EnsureAllBSHAndPnLAccountsAreMapped()
		{
			bool result = true;

			StringBuilder warningMessageBuilder = new StringBuilder();
			foreach (AccGLHeader gLHeader in GetGLAccountsThatAreNotMapped())
			{
				warningMessageBuilder.AppendFormat("- {0} {1}\r\n", gLHeader.AG_AccountNum, gLHeader.AG_DescriptionMultilingual);
			}

			if (warningMessageBuilder.Length > 0)
			{
				warningMessageBuilder.Insert(0, "The following GL Accounts are not mapped.\r\n");
				Notifications.Notify(new WarningNotification(WarningType.Warning, warningMessageBuilder.ToString()));
			}

			return result;
		}

		AccGLHeader[] GetGLAccountsThatAreNotMapped()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(AccGLHeader));

			ZQuery accountTypeFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			accountTypeFilter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, Core.Constants.AccountType.ProfitAndLossAccount);
			filter.AddToFilter(accountTypeFilter);
			filter.AddToFilter(AccGLHeaderSchema.AG_IsActive, true);

			ZDBOnlySubQuery mappedPivotFilter = new ZDBOnlySubQuery(typeof(AccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AG, true);

			ZDBOnlySubQuery mappedToCognosAccountFilter = new ZDBOnlySubQuery(typeof(CognosAccGLAccountDescriptor), AccGLAccountDescriptorSchema.PK);
			mappedToCognosAccountFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger);
			mappedToCognosAccountFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, SQLComparisonOperator.Equal, CognosAccGLAccountDescriptor.ReportTypeCOA);
			mappedPivotFilter.AddSubQuery(AccGLDescriptorPivotSchema.YJ_AJ, mappedToCognosAccountFilter, JoinCondition.And);

			filter.AddSubQuery(mappedPivotFilter, JoinCondition.And);

			filter.OrderBy = AccGLHeaderSchema.Constants.AG_AccountNum;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			return factory.Load<AccGLHeader>(filter);
		}

		bool EnsureUserGroupsForStatisticsAreAssigned()
		{
			bool result = true;
			StringBuilder warningMessageBuilder = new StringBuilder();

			if (JASDataRegistry.Instance.CognosAdminGroup.IsEmpty)
			{
				warningMessageBuilder.AppendLine("- Administration Group");
			}

			if (JASDataRegistry.Instance.CognosSalesGroup.IsEmpty)
			{
				warningMessageBuilder.AppendLine("- Sales Group");
			}

			if (JASDataRegistry.Instance.CognosShipmentControlGroup.IsEmpty)
			{
				warningMessageBuilder.AppendLine("- Shipment Control Group");
			}

			if (warningMessageBuilder.Length > 0)
			{
				warningMessageBuilder.Insert(0, "The following user groups have not been mapped in the Cognos registry. Please configure through Registry > JAS Client Extension > Cognos\r\n");
				Notifications.Notify(new WarningNotification(WarningType.Warning, warningMessageBuilder.ToString()));
			}
			return result;
		}

		public readonly ICognosNotificationSubscriber Notifications;
	}
}

#region Implementation
#endregion
