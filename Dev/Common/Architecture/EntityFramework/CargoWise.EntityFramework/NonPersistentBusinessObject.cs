#pragma warning disable 0809

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class NonPersistentBusinessObject<TValidation> : NonPersistentBusinessObject
		where TValidation : ZValidation
	{
		protected NonPersistentBusinessObject()
		{
		}

		protected NonPersistentBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected NonPersistentBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public TValidation Validation
		{
			get { return GetNewValidation(); }
		}

		public abstract TValidation GetNewValidation();
	}

	/// <summary>
	/// Business Object that is not stored in the DB
	/// Add your very own properties for fun and profit.
	/// </summary>
	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection]
	public abstract class NonPersistentBusinessObject : BusinessObject
	{
		protected NonPersistentBusinessObject()
			: this(null, null)
		{
		}

		protected NonPersistentBusinessObject(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		protected NonPersistentBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetInstantiationTime()
		{
		}

		#region PK Management
		public override SchemaGuidColumn PKSchemaColumn
		{
			get
			{
				return CargoWise.Schema.Schema.GenericPkColumn;
			}
		}

		protected internal override void SetPKAndDefaults()
		{
			if (Row != null)
			{
				base.SetPKAndDefaults();
			}
			else
			{
				AddToFactoryCacheInternal();
				SetDefaultsSuspendingAndResumingHasChanges();
			}
		}

		internal override ZGuid GetPKInternal()
		{
			if (Row != null)
			{
				return base.GetPKInternal();
			}
			else
			{
				if (fPK.IsEmpty)
				{
					fPK = GetPK();
				}
				return fPK;
			}
		}
		ZGuid fPK;

		protected virtual ZGuid GetPK()
		{
			return ZGuid.NewZGuid();
		}
		#endregion

		public override string TablePrefix
		{
			get { return ""; }
		}

		public override sealed void OnLoaded()
		{
			return;
		}

		public override void Delete()
		{
			fIsDeleted = true;
			foreach (BusinessObjectCollection parentCollection in ParentCollections)
			{
				if (!parentCollection.IsNonCommittedCollectionElement(this))
				{
					((IBusinessObjectCollectionInternals)parentCollection).HasChangesFromDelete = true;
				}
			}
		}

		public override bool IsDeleted
		{
			get
			{
				if (this is IWrapPersistentBizO wrapper && wrapper.Parent != null)
				{
					return wrapper.Parent.IsDeleted || fIsDeleted;
				}
				return fIsDeleted;
			}
		}
		bool fIsDeleted;

		public virtual void ResetState()
		{
			fIsDeleted = false;
		}

		public override bool IsInDatabase
		{
			get
			{
				if (this is IWrapPersistentBizO wrapper && wrapper.Parent != null)
				{
					return wrapper.Parent.IsInDatabase;
				}
				return false;
			}
		}

		protected override bool ShouldCheckIsInDatabaseCore => false;

		public override bool IsNotExcludedFromSavingByFactory
		{
			get { return true; }
		}

		public override bool IsSavedByFactory
		{
			get
			{
				if (this is IWrapPersistentBizO wrapper && wrapper.Parent != null)
				{
					return wrapper.Parent.IsSavedByFactory;
				}
				return false;
			}
		}

		public override string TableName
		{
			get { return Table == null ? "" : base.TableName; }
		}

		internal override void InitialiseRowWrapper(DataRow row)
		{
			if (row != null)
			{
				base.InitialiseRowWrapper(row);
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			// Don't call base..
		}
#endif

		/// <summary>
		/// This property will not work with ActiveBusinessObjectCollections. Consider defining the collection your
		/// business object is a part of to perform unique validation.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ICollection<BusinessObjectCollection> ParentCollections
		{
			get { return base.ParentCollections; }
		}

		#region MatchesFilter

		protected override bool MatchesFilterCore(ZQuery filter, DataRow row, DataTable table, string tableName, string identifier)
		{
			if (filter == null || filter.IsEmpty)
			{
				return true;
			}

			if (row == null)
			{
				row = NonPersistentRow;
				if (row != null && row.Table != null)
				{
					table = row.Table;
					tableName = row.Table.TableName;
				}
			}

			return row != null && base.MatchesFilterCore(filter, row, table, tableName, identifier);
		}

		protected override Type TypeForMatchesFilter
		{
			get
			{
				if (this is IWrapPersistentBizO wrapper && wrapper.Parent != null)
				{
					return wrapper.Parent.GetType();
				}
				return GetType();
			}
		}

		internal DataRow NonPersistentRow
		{
			get
			{
				if (Row != null)
				{
					return Row;
				}

				var nonPersistentTable = ParentCollections.OfType<INonPersistentBusinessObjectCollectionInternal>().Select(collection => collection.GetNonPersistentTable(this)).FirstOrDefault(table => table != null)
					?? GetIndividualNonPersistentTable();

				if (nonPersistentTable != null && nonPersistentTable.Columns.Contains(PKSchemaColumn.Name))
				{
					DataRow nonPersistentRow = nonPersistentTable.Rows.Cast<DataRow>().FirstOrDefault(row => PK.Equals(row[PKSchemaColumn.Name]));

					if (nonPersistentRow == null)
					{
						nonPersistentRow = nonPersistentTable.NewRow();
						nonPersistentRow[PKSchemaColumn.Name] = ((IZTypeInternals)PK).GetValueForLogicalDataLayer(false);
						nonPersistentTable.Rows.Add(nonPersistentRow);
					}

					return nonPersistentRow;
				}

				return null;
			}
		}

		internal DataTable GetIndividualNonPersistentTable()
		{
			return individualNonPersistentTable ?? (individualNonPersistentTable = CreateNewNonPersistentTable());
		}
		DataTable individualNonPersistentTable;

		internal DataTable CreateNewNonPersistentTable()
		{
			var table = new DataTable(GetType().Name);
			table.Columns.Add(PKSchemaColumn.Name, PK.BaseDataType);
			return table;
		}

		#endregion

		#region Unsupported Overridden Members
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Cannot use this member on a non-persistent business object", true)]
		protected sealed override void CheckCanCopyPersistentValuesFrom()
		{
			throw new NotSupportedException("Can't copy persistent values to a Non-Persistent object!");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Cannot use this member on a non-persistent business object", true)]
		protected sealed override void ReloadCore()
		{
			throw new NotSupportedException("Can't reload a non-persistent object!");
		}

		#endregion

		protected string GeographySerializationErrorMessage => Res.GetString("031AF28A - FE9A - 4786 - AC83 - B1A10841378C", "Unable to serialize Geography");
	}
}
#pragma warning restore 0809
