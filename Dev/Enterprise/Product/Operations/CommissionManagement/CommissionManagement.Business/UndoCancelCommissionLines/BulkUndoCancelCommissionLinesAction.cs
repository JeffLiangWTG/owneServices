using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class BulkUndoCancelCommissionLinesAction : NonPersistentBusinessObject
	{
		public static BulkUndoCancelCommissionLinesAction NewForDifferentFactory(BusinessObjectFactory localFactory, IEnumerable<ViewCommissionLine> commissionLinesInExternalFactory)
		{
			var commissionLinesInExternalFactoryList = commissionLinesInExternalFactory.ToList();
			foreach (var viewCommissionLine in commissionLinesInExternalFactoryList)
			{
				localFactory.AddFetchHint(AccCommissionLine.Schema.TableName, viewCommissionLine.PK);
			}

			var commissionLinesInLocalFactory = commissionLinesInExternalFactoryList.Select(x => localFactory.Load<ViewCommissionLine>(x.PK)).Where(x => x != null);
			var action = new BulkUndoCancelCommissionLinesAction(localFactory);
			action.Initialise(commissionLinesInLocalFactory);
			return action;
		}

		public BulkUndoCancelCommissionLinesAction(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Initialise

		public void Initialise(IEnumerable<ViewCommissionLine> commissionLinesToCancel)
		{
			using (UndoCancelCommissionLineActionCollection.SuspendListChanged())
			{
				UndoCancelCommissionLineActionCollection.RemoveAll();
				foreach (var line in commissionLinesToCancel)
				{
					UndoCancelCommissionLineActionCollection.Add(new UndoCancelCommissionLineAction(line));
				}
			}
		}

		#endregion

		#region Execute

		public void Execute()
		{
			foreach (var action in UndoCancelCommissionLineActions)
			{
				action.Execute();
			}
		}

		#endregion

		#region RemoveErrorLines

		public int RemoveErrorLines()
		{
			var totalRemoved = 0;
			foreach (var action in UndoCancelCommissionLineActions.ToArray())
			{
				action.RunPreSaveValidation();
				if (action.HasErrors)
				{
					UndoCancelCommissionLineActionCollection.RemoveAndDelete(action);
					totalRemoved++;
				}
			}

			return totalRemoved;
		}

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public UndoCancelCommissionLineActionCollection UndoCancelCommissionLineActionCollection
		{
			get
			{
				if (undoCancelCommissionLineActionCollection == null)
				{
					undoCancelCommissionLineActionCollection = new UndoCancelCommissionLineActionCollection(Factory);
					RegisterEditableChildObject(undoCancelCommissionLineActionCollection);
				}

				return undoCancelCommissionLineActionCollection;
			}
		}
		UndoCancelCommissionLineActionCollection undoCancelCommissionLineActionCollection;

		public IEnumerable<UndoCancelCommissionLineAction> UndoCancelCommissionLineActions
		{
			get { return UndoCancelCommissionLineActionCollection.Cast<UndoCancelCommissionLineAction>(); }
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
			if (UndoCancelCommissionLineActionCollection.Count == 0)
			{
				AddRowError(Res.GetString("0256cfe9-b34e-480b-8333-6bf1543e065f", "No entity commissions have been selected to be undo canceled."));
			}
		}

		#endregion
	}
}
