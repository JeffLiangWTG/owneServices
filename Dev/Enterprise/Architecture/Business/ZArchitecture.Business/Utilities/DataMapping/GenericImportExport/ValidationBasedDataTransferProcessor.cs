using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class ValidationBasedDataTransferProcessor<TFlattenedRecord, TFlattenedCollectionInfo, TImportedItem> : DataTransferProcessor
		where TFlattenedRecord : NonPersistentBusinessObject
		where TFlattenedCollectionInfo : ImportCollectionInfoImpl
		where TImportedItem : EnterpriseBusinessObject
	{
		public class ValidationErrorToResultingErrorMapping
		{
			public ValidationErrorToResultingErrorMapping(string importedItemError, Func<TFlattenedRecord, string> resultingErrorGetter)
			{
				ImportedItemError = importedItemError;
				ResultingErrorGetter = resultingErrorGetter;
			}

			public string ImportedItemError { get; }
			public Func<TFlattenedRecord, string> ResultingErrorGetter { get; }
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected ValidationBasedDataTransferProcessor(TFlattenedCollectionInfo flattenedCollectionInfo, IEnumerable<ValidationErrorToResultingErrorMapping> validationErrorToResultingErrorMappings)
		{
			flattenedCollection = (NonPersistentBusinessObjectCollection<TFlattenedRecord>)((IImportCollectionInfo)flattenedCollectionInfo).Collection;
			Factory = flattenedCollection.Factory;
			this.validationErrorToResultingErrorMappings = validationErrorToResultingErrorMappings;
		}

		readonly protected BusinessObjectFactory Factory;
		readonly NonPersistentBusinessObjectCollection<TFlattenedRecord> flattenedCollection;
		readonly IEnumerable<ValidationErrorToResultingErrorMapping> validationErrorToResultingErrorMappings;

		public override void Import()
		{
			Factory.ActivateStringInterning(); // leave activated for the life of the factory (ie: until the Form is closed)

			var recordNumber = 0;
			foreach (var flattenedRecord in flattenedCollection.Cast<TFlattenedRecord>())
			{
				recordNumber++;

				if (IsCanceled)
				{
					break;
				}

				if (!OnProgressChanged(recordNumber * 100 / flattenedCollection.Count, Res.GetString("7EF79AB8-031E-44AD-9AE4-E44B8317B4AB", "{0} ({1} of {2})", ProgressMessage, recordNumber, flattenedCollection.Count)))
				{
					break;
				}

				var importedItem = GetImportedItem(flattenedRecord);
				ValidateImportedItem(importedItem);

				if (importedItem.HasErrors)
				{
					ErrorCount++;
					var errors = importedItem.NotificationsIncludingChildren.Where(n => n.Type == CargoWise.ComponentModel.NotificationType.Error).ToList();

					foreach (var mapping in validationErrorToResultingErrorMappings)
					{
						if (CheckHasErrorsWithMessageAndRemoveFromErrorList(errors, mapping.ImportedItemError))
						{
							NotifyError(recordNumber, mapping.ResultingErrorGetter(flattenedRecord));
						}
					}

					// all other unprocessed validation errors - just in case
					foreach (var error in errors)
					{
						NotifyError(recordNumber, error.Message);
					}

					importedItem.Delete();
					continue;
				}

				NewCount++;
			}
		}

		protected abstract string ProgressMessage { get; }

		protected abstract TImportedItem GetImportedItem(TFlattenedRecord flattenedRecord);

		protected abstract void ValidateImportedItem(TImportedItem importedItem);

		public override void Rollback()
		{
			(new ActiveBusinessObjectCollection<TImportedItem>(Factory)).DeleteAll();
		}

		#region Validation

		bool CheckHasErrorsWithMessageAndRemoveFromErrorList(List<INotification> errors, string message)
		{
			var foundErrors = errors.Where(e => e.Message.Contains(message)).ToArray();

			if (!foundErrors.Any())
			{
				return false;
			}

			foreach (var error in foundErrors)
			{
				errors.Remove(error);
			}

			return true;
		}

		#endregion

		#region Logging

		void NotifyError(int recordNumber, string message)
		{
			var log = Res.GetString("F3C7402D-79B9-4276-A79E-4A04AC158634", "Line {0}: {1}", recordNumber, message);
			LogList.Add(log);
		}

		public int ItemsToImport { get { return flattenedCollection.Count; } }

		public int NewCount { get; protected set; }
		public int ErrorCount { get; protected set; }

		public IEnumerable<string> Logs => LogList;

		protected List<string> LogList = new List<string>();

		#endregion
	}
}
