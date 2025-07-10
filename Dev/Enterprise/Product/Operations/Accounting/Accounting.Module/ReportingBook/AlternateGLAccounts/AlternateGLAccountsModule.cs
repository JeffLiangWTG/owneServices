using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AlternateGLAccountsModule : ZFilterGridModule
	{
		public AlternateGLAccountsModule()
		{
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewActionMenuItems());
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.AlternateGLAccounts.BulkCreateAlternateGLAccounts", "Bulk Create Alternate GL Accounts"), new EventHandler(BulkCreateAlternateGLAccounts)));
			return menu.ToArray();
		}

		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AlternateGLAccounts; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var newController = new AlternateGLAccountsController();
			newController.RefreshGrid += new EventHandler(AlternateGLAccountsModule_RefreshGrid);
			return newController;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AlternateGLAccountsFilterControl(GridCollection, (AlternateGLAccountsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AlternateGLAccountCombineParentAccountCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AlternateGLAccountsFilterBusinessObject();
		}

		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox)
		{
			return new AlternateGLAccountModuleDecisionProvider(findbox);
		}

		protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter)
		{
			return true;
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var filterHelper = ((AlternateGLAccountsFilterBusinessObject)FilterBusinessObject).AlternateGLAccountFilter;
			filterHelper.MaximumRows = query.MaximumRows;

			try
			{
				var collection = new AlternateGLAccountCombineParentAccountCollection(factory);
				collection.LoadFilterHelper(factory, filterHelper);
				return PerformSearchResult.Success(factory, query, collection.Cast<AlternateGLAccountCombineParentAccount>().ToArray(), true);
			}
			finally
			{
				filterHelper.MaximumRows = null;
			}
		}

		void BulkCreateAlternateGLAccounts(object sender, EventArgs e)
		{
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AlternateGLAccounts; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowUniversalCopy => false;

		protected override bool AllowAdvancedDataAutomationWizard => false;

		protected override void DeleteMultiple(BusinessObject[] selectedBusinessObjects)
		{
			var alternateGLAccountCombineParentAccounts = selectedBusinessObjects.Cast<AlternateGLAccountCombineParentAccount>();
			var canDeleteHashSet = new HashSet<AccAlternateGLAccount>();

			var canDeleteDetailMessage = string.Empty;
			var cannotDeleteDetailMessage = string.Empty;
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
			dbOnlyQuery.AddToFilter(AccAlternateGLAccountSchema.PK, alternateGLAccountCombineParentAccounts.Select(x => x.AlternateGLAccount.PK));
			var readonlyFactory = new ReadOnlyBusinessObjectFactory();
			var alternateGLAccountPKInDB = readonlyFactory.Load<AccAlternateGLAccount>(dbOnlyQuery).Select(x => x.PK).ToList();

			foreach (var alternateGLAccountWithParent in alternateGLAccountCombineParentAccounts)
			{
				var alternateGLAccount = alternateGLAccountWithParent.AlternateGLAccount;

				if (alternateGLAccountPKInDB.Contains(alternateGLAccountWithParent.AlternateGLAccount.PK))
				{
					if (alternateGLAccount.CanDelete)
					{
						if (canDeleteHashSet.Add(alternateGLAccount))
						{
							if (alternateGLAccount.AlternateGLAccountAttributes.Any())
							{
								var attributes = alternateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>();

								canDeleteDetailMessage += string.Join("\r\n", attributes.Select(x => $"Alternate Chart: {alternateGLAccount.AlternateChart.AAC_Code} - Parent Account: {x.GLHeader.AG_AccountNum} - Alternate Account: {alternateGLAccount.AGA_AccountNum}"));
								canDeleteDetailMessage += "\r\n";
							}
							else
							{
								canDeleteDetailMessage += $"Alternate Chart: {alternateGLAccount.AlternateChart.AAC_Code} - Alternate Account: {alternateGLAccount.AGA_AccountNum}";
								canDeleteDetailMessage += "\r\n";
							}
						}
					}
					else
					{
						cannotDeleteDetailMessage += $"Alternate Chart: {alternateGLAccount.AlternateChart.AAC_Code} - Alternate Account: {alternateGLAccount.AGA_AccountNum}";
						cannotDeleteDetailMessage += "\r\n";
					}
				}
			}

			if (alternateGLAccountCombineParentAccounts.Count() == canDeleteHashSet.Count && canDeleteHashSet.Where(x => x.AlternateGLAccountAttributes != null).All(x => x.AlternateGLAccountAttributes.Count <= 1))
			{
				base.DeleteMultiple(canDeleteHashSet.ToArray());
				PerformSearch();
				return;
			}

			var totalMessage = new StringBuilder();

			if (alternateGLAccountPKInDB.Count < alternateGLAccountCombineParentAccounts.Count())
			{
				totalMessage.AppendLine(Res.GetString("5155ED7F-3C8B-4294-B47F-F41C69AD7666", @"Some Alternate Accounts are already deleted, no action will be performed on them."));
			}

			if (!string.IsNullOrEmpty(canDeleteDetailMessage))
			{
				totalMessage.AppendLine(Res.GetString("C16C3C07-0E40-4F98-91DA-9655996F8B25", @"The selected Alternate Account(s) and it's related will be deleted:

{0}", canDeleteDetailMessage));
			}

			if (!string.IsNullOrEmpty(cannotDeleteDetailMessage))
			{
				totalMessage.AppendLine(Res.GetString("EB123D55-5CC8-4918-92A5-51CAA33FB920",
					@"The selected Alternate Account(s) cannot be deleted because it is used as either Alternate Number, Consolidate, Percent Number or Total Reference in Alternate Account:

{0}", cannotDeleteDetailMessage));
			}

			if (!string.IsNullOrEmpty(canDeleteDetailMessage))
			{
				totalMessage.Append(Res.GetString("2BD21F49-CF1B-4DDD-A80D-639135A367D5", @"Click 'Yes' to proceed, click 'No' to cancel the action."));

				if (Globals.Message.Show(totalMessage.ToString(), Res.GetString("a1c61881-4fc4-42a3-ac8f-4a6962fe0afb", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					base.DeleteMultiple(canDeleteHashSet.ToArray());
				}
			}
			else
			{
				Globals.Message.ShowInformation(totalMessage.ToString().Trim(), Res.GetString("726f6b7d-d615-4226-bdf6-834cb5e439aa", "Information"));
			}

			PerformSearch();
		}

		protected void AlternateGLAccountsModule_RefreshGrid(object sender, EventArgs e)
		{
			if (!this.DisplayGrid.IsDisposed)
			{
				PerformSearch();
			}
		}

		#endregion
	}
}
