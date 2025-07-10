using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class BulkCancelCommissionLinesAction : NonPersistentBusinessObject
	{
		public static BulkCancelCommissionLinesAction NewForDifferentFactory(BusinessObjectFactory localFactory, IEnumerable<ViewCommissionLine> commissionLinesInExternalFactory)
		{
			var commissionLinesInExternalFactoryList = commissionLinesInExternalFactory.ToList();
			foreach (var viewCommissionLine in commissionLinesInExternalFactoryList)
			{
				localFactory.AddFetchHint(AccCommissionLine.Schema.TableName, viewCommissionLine.PK);
			}

			var commissionLinesInLocalFactory = commissionLinesInExternalFactoryList.Select(x => localFactory.Load<ViewCommissionLine>(x.PK)).Where(x => x != null);
			var action = new BulkCancelCommissionLinesAction(localFactory);
			action.Initialise(commissionLinesInLocalFactory);
			return action;
		}

		public BulkCancelCommissionLinesAction(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Initialise
		public void Initialise(IEnumerable<ViewCommissionLine> commissionLinesToCancel)
		{
			using (CancelCommissionLineActionCollection.SuspendListChanged())
			{
				CancelCommissionLineActionCollection.RemoveAll();
				foreach (var line in commissionLinesToCancel)
				{
					CancelCommissionLineActionCollection.Add(new CancelCommissionLineAction(line));
				}
			}
		}

		#endregion

		#region Execute

		public void Execute()
		{
			foreach (var cancelAction in CancelCommissionLineActions)
			{
				cancelAction.Execute();
			}
		}

		#endregion

		#region RemoveErrorLines

		public int RemoveErrorLines()
		{
			var totalRemoved = 0;
			foreach (var action in CancelCommissionLineActions.ToArray())
			{
				action.RunPreSaveValidation();
				if (action.HasErrors)
				{
					CancelCommissionLineActionCollection.RemoveAndDelete(action);
					totalRemoved++;
				}
			}

			return totalRemoved;
		}

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public CancelCommissionLineActionCollection CancelCommissionLineActionCollection
		{
			get
			{
				if (cancelCommissionLineActionCollection == null)
				{
					cancelCommissionLineActionCollection = new CancelCommissionLineActionCollection(Factory);
					RegisterEditableChildObject(cancelCommissionLineActionCollection);
				}

				return cancelCommissionLineActionCollection;
			}
		}
		CancelCommissionLineActionCollection cancelCommissionLineActionCollection;

		public IEnumerable<CancelCommissionLineAction> CancelCommissionLineActions
		{
			get { return CancelCommissionLineActionCollection.Cast<CancelCommissionLineAction>(); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			CheckContainsAtLeastOne();
		}

		void CheckContainsAtLeastOne()
		{
			if (CancelCommissionLineActionCollection.Count == 0)
			{
				AddRowError(Res.GetString("ee4f40e4-1180-43b4-99a1-2be614013237", "No entity commissions have been selected to be canceled."));
			}
		}

		#endregion
	}
}
