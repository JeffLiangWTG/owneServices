#if DEBUG
using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DbUpgrader.Data;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldManager : NonPersistentBusinessObject
	{
		public StmSystemDefinedFieldManager(IDocumentSupportable documentSupportable)
		{
			this.DocumentSupportable = documentSupportable;
		}

		public void SaveForCheckIn()
		{
			Controller.FullSave();
			SetFieldsReadOnly(true);
		}

		public bool Checkout(out string errorMessage)
		{
			bool result = true;
			errorMessage = "";

			try
			{
				Controller.FullCheckOut();
				ReloadFields(false);
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				result = false;
			}
			return result;
		}

		public void UndoCheckout()
		{
			Controller.FullUndoCheckOut();
			ReloadFields(true);
		}

		public BusinessObjectFactory FactoryForSaving
		{
			get { return Fields.Factory; }
		}

		void ReloadFields(bool readOnly)
		{
			Fields.SwapFactoryAndRemoveAll(new BusinessObjectFactory());
			Fields.Load();
			SetFieldsReadOnly(readOnly);
		}

		readonly IDocumentSupportable DocumentSupportable;

		#region Fields

		public StmSystemDefinedFieldDependentCollection Fields
		{
			get
			{
				if (fFields == null)
				{
					var lFields = new StmSystemDefinedFieldDependentCollection(DocumentSupportable, new BusinessObjectFactory());
					lFields.Load();
					fFields = lFields;
					fFields.SetReadOnlyIncludingChildren(true);
					fFields.Sort(StmSystemDefinedFieldSchema.Constants.S1_Order, ListSortDirection.Ascending);
					RegisterEditableChildObject(fFields);
				}
				return fFields;
			}
		}

		void SetFieldsReadOnly(bool readOnly)
		{
			Fields.SetReadOnlyIncludingChildren(readOnly);

			if (!readOnly)
			{
				foreach (StmSystemDefinedField field in Fields)
				{
					if (!field.IsGrid)
					{
						field.FieldColumns.SetReadOnlyIncludingChildren(true);
					}
				}
			}
		}

		StmSystemDefinedFieldDependentCollection fFields;

		#endregion

		#region DataUpgradeSetupController

		ISetupController Controller
		{
			get
			{
				if (fController == null)
				{
					fController = GetNewController();
				}
				return fController;
			}
		}

		protected virtual ISetupController GetNewController()
		{
			UpgradeTask[] tasks = new UpgradeTask[] { new StmSystemDefinedFieldUpgradeTask() };
			return new DataUpgradeSetupController(tasks);
		}

		ISetupController fController;

		#endregion

		#region Validation

		public ZValidation Validation
		{
			get { return new StmSystemDefinedFieldManagerValidation(this); }
		}

		#region class StmSystemDefinedFieldManagerValidation

		class StmSystemDefinedFieldManagerValidation : ZValidation
		{
			public StmSystemDefinedFieldManagerValidation(StmSystemDefinedFieldManager parent)
				: base(parent)
			{
			}

			public override Type AutoValidationType
			{
				get { return null; }
			}

			public override void ValidateAll()
			{
			}
		}

		#endregion

		#endregion

		#region For Testing

		public void EditWithoutCheckout()
		{
			EditWithoutCheckoutCore();
		}

		public void SaveWithoutCheckIn()
		{
			SaveWithoutCheckInCore();
		}

		protected virtual void EditWithoutCheckoutCore()
		{
			SetFieldsReadOnly(false);
		}

		protected virtual void SaveWithoutCheckInCore()
		{
			Fields.Factory.Save();
			SetFieldsReadOnly(true);
		}

		#endregion
	}
}
#endif
