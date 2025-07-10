using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectInternals
	{
		void Validate(string propertyName);
		void Validate(ZPropertyInfo info);
		bool HasValidateMethod(ZPropertyInfo info);
		DataRow Row { get; }

		bool IsInPreSaveValidation { get; }
		bool IsCopying { get; set; }

		object GetColumnOriginalValue(string columnName);

		object GetValueFromRowSafely(SchemaColumn schemaColumn, DataRowVersion version);
		object GetValueFromRowSafely(ZPropertyInfo property);
		object GetValueFromRowSafely(ZPropertyInfo property, DataRowVersion version);
		object GetValueFromRowSafely(string columnName, DataRowVersion version);
		object GetValueFromRowSafely(DataColumn column, DataRowVersion version);
		IDisposable SuppressReportRowDeletedError();
		IDisposable ResumeValidationForAllDescendantsTemporarily();

		void ValidateShallowIfImprovesPreSaveValidationPerformance();
		bool AcceptChangesDelayedUntilJustBeforeSavingToDatabase { get; set; }
		bool SubscribeToDataRefreshOnInstantiation { get; }
		BusinessObjectCollection[] ParentCollections { get; }
		bool DoChildrenHaveChanges();
		IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers { get; }

		void MarkAsDeleted();

		bool IsUnCommittedRow { get; }

		void EnsureBlobField(SchemaColumn schemaColumn);
		string CreationStackTrace { get; }
	}
}
