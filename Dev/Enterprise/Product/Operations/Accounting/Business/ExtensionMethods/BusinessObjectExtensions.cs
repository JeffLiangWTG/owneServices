using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public static class BusinessObjectExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal error message")]
		public static void CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(this BusinessObject destinationObject, BusinessObject sourceObject, params string[] fieldsToCopyAndInValidOrder)
		{
			Argument.NotNull(destinationObject, nameof(destinationObject));
			Argument.NotNull(sourceObject, nameof(sourceObject));
			Argument.NotNull(fieldsToCopyAndInValidOrder, nameof(fieldsToCopyAndInValidOrder));
			Argument.GreaterThanZero(fieldsToCopyAndInValidOrder.Length, nameof(fieldsToCopyAndInValidOrder) + "." + nameof(fieldsToCopyAndInValidOrder.Length));

			var destinationObjectType = destinationObject.GetType();
			var sourceObjectType = sourceObject.GetType();
			if (destinationObjectType != sourceObjectType)
			{
				ErrorReporter.ReportOnce(string.Format("destinationObject '{0}' and sourceObject '{1}' must have the same type.", destinationObjectType, sourceObjectType));
			}
			else
			{
				string reportMessagePrefix = string.Format("Object type '{0}': ", destinationObjectType);

				var destinationObjectPK = destinationObject.PK;
				var sourceObjectPK = sourceObject.PK;
				if (destinationObjectPK == sourceObjectPK)
				{
					ErrorReporter.ReportOnce(reportMessagePrefix + string.Format("destinationObject and sourceObject must have different PKs. PK is '{1}'.", destinationObjectPK, sourceObjectPK));
				}
				else
				{
					var fieldNamesAndValuesToSet = new Dictionary<string, IZType>();
					foreach (var fieldName in fieldsToCopyAndInValidOrder)
					{
						var reportMessagePrefix_Field = reportMessagePrefix + string.Format("field name '{0}' ", fieldName);

						var propertyInfo = sourceObject.ZPropertyInfoHash.GetPropertySafe(fieldName);
						if (propertyInfo == null)
						{
							ErrorReporter.ReportOnce(reportMessagePrefix_Field + "is not found.");
						}
						else
						{
							fieldNamesAndValuesToSet.Add(fieldName, propertyInfo.Value);
						}
					}
					destinationObject.SetFieldsInParticularOrder(fieldNamesAndValuesToSet, fieldsToCopyAndInValidOrder);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal error message, Internal errro message")]
		public static void SetFieldsInParticularOrder(this BusinessObject destinationObject, Dictionary<string, IZType> fieldNamesAndValuesToSet, params string[] fieldsToSetAndInValidOrder)
		{
			Argument.NotNull(destinationObject, nameof(destinationObject));
			Argument.NotNull(fieldNamesAndValuesToSet, nameof(fieldNamesAndValuesToSet));
			Argument.GreaterThanZero(fieldNamesAndValuesToSet.Count, nameof(fieldNamesAndValuesToSet) + "." + nameof(fieldNamesAndValuesToSet.Count));
			Argument.NotNull(fieldsToSetAndInValidOrder, nameof(fieldsToSetAndInValidOrder));
			Argument.GreaterThanZero(fieldsToSetAndInValidOrder.Length, nameof(fieldsToSetAndInValidOrder) + "." + nameof(fieldsToSetAndInValidOrder.Length));

			var destinationObjectType = destinationObject.GetType();
			var reportMessagePrefix = string.Format("Object type '{0}': ", destinationObjectType);

			foreach (var fieldName in fieldsToSetAndInValidOrder)
			{
				var reportMessagePrefix_Field = reportMessagePrefix + string.Format("field name '{0}' ", fieldName);

				var propertyInfo = destinationObject.ZPropertyInfoHash.GetPropertySafe(fieldName);
				if (propertyInfo == null)
				{
					ErrorReporter.ReportOnce(reportMessagePrefix_Field + "is not found.");
				}
				else if (!propertyInfo.HasSetter)
				{
					ErrorReporter.ReportOnce(reportMessagePrefix_Field + "does not have a setter.");
				}
				else
				{
					IZType value;
					if (fieldNamesAndValuesToSet.TryGetValue(fieldName, out value))
					{
						try
						{
							propertyInfo.Value = value;
						}
						catch (Exception e)
						{
							throw new InvalidOperationException(reportMessagePrefix_Field, e);
						}
					}
					else
					{
						ErrorReporter.ReportOnce(reportMessagePrefix_Field + "does not have a value to set.");
					}
				}
			}
		}

		#region Calculation Note Helper

		public static StmNote CreateSystemNote(this EnterpriseBusinessObject noteParent, PredefinedNoteType logType)
		{
			var stmNote = noteParent.Notes.AddNew();
			stmNote.ST_IsCustomDescription = false;
			stmNote.ST_Description = logType.Description;

			return stmNote;
		}

		internal static StmNote CreateOrUpdateCostCalculationNote(this BusinessObject noteParent, StmNote note, ZBlob value)
		{
			return noteParent.CreateOrUpdateCalculationNote(note, value, "AUTORATE_COST");
		}

		internal static StmNote CreateOrUpdateSellCalculationNote(this BusinessObject noteParent, StmNote note, ZBlob value)
		{
			return noteParent.CreateOrUpdateCalculationNote(note, value, "AUTORATE_SELL");
		}

		static StmNote CreateOrUpdateCalculationNote(this BusinessObject noteParent, StmNote note, ZBlob value, string description)
		{
			if (value.IsEmpty)
			{
				if (note != null)
				{
					note.Delete();
				}

				return null;
			}

			if (note == null)
			{
				note = noteParent.Factory.New<StmNote>();
				note.ST_ParentID = noteParent.PK;
				note.ST_Table = noteParent.TableName;
				note.ST_IsCustomDescription = true;
			}

			var maxAllowedBytes = TypeValidation.ZBlobMaxBytesBeforeError;
			if (value.Length > maxAllowedBytes)
			{
				var noteStr = System.Text.Encoding.UTF8.GetString(value);
				var trimmednote = noteStr.TrimToFit(maxAllowedBytes / 2);
				value = ZBlob.FromUTF8(new ZString(trimmednote));
			}

			note.ST_Description = description;
			note.ST_NoteData = value;
			note.ReadOnly = true;

			return note;
		}

		internal static StmNote GetCostCalculationNote(this BusinessObject noteParent)
		{
			return noteParent.LoadCalculationNote("AUTORATE_COST");
		}

		internal static StmNote GetSellCalculationNote(this BusinessObject noteParent)
		{
			return noteParent.LoadCalculationNote("AUTORATE_SELL");
		}

		static StmNote LoadCalculationNote(this BusinessObject noteParent, string description)
		{
			if (!noteParent.IsInDatabase)
			{
				return null;
			}

			var query = new ZQuery(StmNoteSchema.ST_ParentID, noteParent.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, noteParent.TableName);
			query.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.StartsWith, description);
			query.AddToFilter(StmNoteSchema.ST_IsCustomDescription, true);

			var notes = noteParent.Factory.Load<StmNote>(query);
			if (notes.Length < 1)
			{
				return null;
			}
			else if (notes.Length > 1)
			{
				notes = notes.OrderByDescending(x => x.ST_CreatedDateUtc).ToArray();
				foreach (var notesToDelete in notes.Skip(1))
				{
					notesToDelete.Delete();
				}
			}

			return notes[0];
		}

		#endregion
	}
}

