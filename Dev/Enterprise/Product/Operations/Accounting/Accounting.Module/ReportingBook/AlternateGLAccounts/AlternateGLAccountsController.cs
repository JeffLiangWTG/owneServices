using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for AlternateChartofAccounts.
	/// </summary>
	public class AlternateGLAccountsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AlternateGLAccountsController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AlternateGLAccounts; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AlternateGLAccounts; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AlternateGLAccountCombineParentAccount); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AlternateGLAccountsView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AlternateGLAccountsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AlternateGLAccountsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AlternateGLAccountsDelete; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new AlternateGLAccounts(Factory);
		}

		#region GetLoadedBusinessEntityInLocalFactory

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var result = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			if (sourceEntity is AlternateGLAccountCombineParentAccount alternateGLAccountCombineParentAccount
				&& Factory.ExistsInDatabase(AccAlternateGLAccountSchema.Constants.TableName, new ZQuery(AccAlternateGLAccountSchema.PK, alternateGLAccountCombineParentAccount.AlternateGLAccount.PK)))
			{
				result = sourceEntity;
			}
			else if (sourceEntity is AccAlternateGLAccount alternateGLAccount
				&& Factory.ExistsInDatabase(AccAlternateGLAccountSchema.Constants.TableName, new ZQuery(AccAlternateGLAccountSchema.PK, alternateGLAccount.PK)))
			{
				var alternateGLAccountConbineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
				alternateGLAccountConbineParentAccount.AlternateGLAccount = alternateGLAccount;
				alternateGLAccountConbineParentAccount.GLHeaderPK = alternateGLAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_AG_GLHeader.IsValid)?.AAA_AG_GLHeader ?? ZGuid.Empty;
				result = alternateGLAccountConbineParentAccount;
			}
			else if (sourceEntity is AlternateGLAccounts alternateGLAccounts
				&& Factory.ExistsInDatabase(AccAlternateGLAccountSchema.Constants.TableName, new ZQuery(AccAlternateGLAccountSchema.PK, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PK)))
			{
				var alternateGLAccountConbineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
				alternateGLAccountConbineParentAccount.AlternateGLAccount = alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount;
				alternateGLAccountConbineParentAccount.GLHeaderPK = alternateGLAccounts.ParentGLAccountPK;
				result = alternateGLAccountConbineParentAccount;
			}
			else if (sourceEntity is AccAlternateGLAccountAttribute attirbute
				&& Factory.ExistsInDatabase(AccAlternateGLAccountSchema.Constants.TableName, new ZQuery(AccAlternateGLAccountSchema.PK, attirbute.AlternateGLAccount.PK)))
			{
				var alternateGLAccountConbineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
				alternateGLAccountConbineParentAccount.AlternateGLAccount = attirbute.AlternateGLAccount;
				alternateGLAccountConbineParentAccount.GLHeaderPK = attirbute.AAA_AG_GLHeader;
				result = alternateGLAccountConbineParentAccount;
			}

			return result;
		}

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var dataSource = businessEntity as AlternateGLAccounts ?? CreateAlternateGLAccountsWithAlternateGLAccountCombineParentAccount(businessEntity as AlternateGLAccountCombineParentAccount);
			dataSource.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);

			return new AlternateGLAccountsForm(dataSource);
		}

		AlternateGLAccounts CreateAlternateGLAccountsWithAlternateGLAccountCombineParentAccount(AlternateGLAccountCombineParentAccount alternateGLAccountCombineParentAccount)
		{
			if (alternateGLAccountCombineParentAccount != null)
			{
				var result = new AlternateGLAccounts(Factory);
				result.ParentGLAccountPK = alternateGLAccountCombineParentAccount.GLHeaderPK;
				result.OriginalParentGLAccountPK = alternateGLAccountCombineParentAccount.GLHeaderPK;
				result.OriginalAlternateGLAccountPK = alternateGLAccountCombineParentAccount.AlternateGLAccount.PK;
				result.ChartPK = alternateGLAccountCombineParentAccount.AlternateGLAccount.AGA_AAC_AlternateChart;
				result.ResetAlternateGLAccountsWithAttributeSet(result.ChartPK, result.ParentGLAccountPK, false);
				result.AccountType = alternateGLAccountCombineParentAccount.AlternateGLAccount.AGA_AccountType;
				result.CashFlowCategory = alternateGLAccountCombineParentAccount.AlternateGLAccount.CashFlowType;
				result.Unit = alternateGLAccountCombineParentAccount.AlternateGLAccount.StatisticalUnits;
				result.IsInDb = true;
				var alternateGLAccountWithAttributeSetList = result.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>();
				if (alternateGLAccountWithAttributeSetList.Any(x => x.AlternateGLAccount != null && x.AlternateGLAccount.IsInDatabase))
				{
					alternateGLAccountWithAttributeSetList.ForEach(x => x.AlternateGLAccountNum = x.AlternateGLAccount.AGA_AccountNum);
					result.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSetList.FirstOrDefault(x =>
					{
						var result = true;
						foreach (var attribute in alternateGLAccountCombineParentAccount.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>())
						{
							result = result && x.Attributes.Cast<AccAlternateGLAccountAttribute>().Any(y => y.AAA_Attribute == attribute.AAA_Attribute && y.AAA_Value == attribute.AAA_Value && y.AAA_AttributeValueID == attribute.AAA_AttributeValueID);
						}
						return result;
					});
				}
				else
				{
					result.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
					var alternateGLAccountsWithAttributeSet = result.AlternateGLAccountsWithAttributeSet.AddNew();
					var alternateGLAccount = Factory.Load<AccAlternateGLAccount>(alternateGLAccountCombineParentAccount.AlternateGLAccount.PK);
					alternateGLAccountsWithAttributeSet.AlternateGLAccount = alternateGLAccount ?? alternateGLAccountCombineParentAccount.AlternateGLAccount;
					alternateGLAccountsWithAttributeSet.AlternateGLAccountNum = alternateGLAccountsWithAttributeSet.AlternateGLAccount.AGA_AccountNum;
					result.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountsWithAttributeSet;
				}

				return result;
			}

			return null;
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.Load<AccAlternateGLAccountAttribute>(sourceEntityPK) ?? factory.Load<AccAlternateGLAccount>(sourceEntityPK) ?? base.LoadBusinessEntity(factory, sourceEntityPK);
		}

		public event EventHandler RefreshGrid;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully && RefreshGrid != null)
			{
				RefreshGrid(this, EventArgs.Empty);
			}
		}
	}
}
