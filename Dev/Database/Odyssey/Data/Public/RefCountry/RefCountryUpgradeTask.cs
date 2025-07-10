using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefCountrySchema))]
[assembly: UsesConstants(typeof(RefCurrencySchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class RefCountryUpgradeTask : EmbeddedUpgradeTask
	{
		public RefCountryUpgradeTask() : base(new RefCountryDataFile())
		{
		}

		internal RefCountryUpgradeTask(RefCountryDataFile resourceFile) : base(resourceFile)
		{
		}

		protected override string ComparingColumnsNames(DataTable upgradeTable)
		{
			string result = base.ComparingColumnsNames(upgradeTable);

			switch (upgradeTable.TableName)
			{
				case RefCountrySchema.Constants.TableName:
					result = RefCountrySchema.Constants.RN_Code;
					break;

				case RefCurrencySchema.Constants.TableName:
					result = RefCurrencySchema.Constants.RX_Code;
					break;
			}

			return result;
		}
		public override bool IsRequired
		{
			get
			{
				return (base.IsRequired && ResourceFile.VersionInDatabase == 0);
			}
		}

		#region Insert, Update and Delete

		#region Overrides

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			bool shouldCallBase = true;

			switch (targetTable.TableName)
			{
				case RefCountrySchema.Constants.TableName:
					// If PK conflicts with existing one, inserts record but with a new PK
					if (IsCountryPkConflicted(sourceRow, targetTable))
					{
						Guid sourcePk = (Guid)sourceRow[RefCountrySchema.Constants.PK];
						Guid targetPk = Guid.NewGuid();
						((RefCountryDataFile)ResourceFile).AddCountryPkConflict(sourcePk, targetPk);

						sourceRow[RefCountrySchema.Constants.PK] = targetPk;
					}
					break;

				case RefCurrencySchema.Constants.TableName:
					// If PK conflicts with existing one, inserts record but with a new PK
					if (IsCurrencyPkConflicted(sourceRow, targetTable))
					{
						sourceRow[RefCurrencySchema.Constants.PK] = Guid.NewGuid();
					}
					break;
			}

			if (shouldCallBase)
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			bool shouldCallBase = true;

			if (shouldCallBase)
			{
				switch (columnName)
				{
					case RefCountrySchema.Constants.PK:
						// Does not update PK, but adds to conflicting PK list to be handles afterwards
						Guid sourceCountryPk = (Guid)sourceRow[RefCountrySchema.Constants.PK];
						Guid targetCountryPk = (Guid)targetRow[RefCountrySchema.Constants.PK];
						((RefCountryDataFile)ResourceFile).AddCountryPkConflict(sourceCountryPk, targetCountryPk);
						shouldCallBase = false;
						break;

					case RefCurrencySchema.Constants.PK:
						// Does not update PK
						shouldCallBase = false;
						break;

					case RefCountrySchema.Constants.RN_PostcodeValidationRule:
					case RefCountrySchema.Constants.RN_StateProvinceValidationRule:
					case RefCountrySchema.Constants.RN_AddressFormattingRule:
					case RefCurrencySchema.Constants.RX_IsActive:
						shouldCallBase = false;
						break;
				}
			}

			if (shouldCallBase)
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			bool shouldCallBase = true;

			switch (targetRow.Table.TableName)
			{
				case RefCountrySchema.Constants.TableName:
					// Do NOT delete, set ACTIVE = N if IsSystem
					if ((bool)targetRow[RefCountrySchema.Constants.RN_IsSystem])
					{
						targetRow[RefCountrySchema.Constants.RN_IsActive] = false;
					}

					shouldCallBase = false;
					break;

				case RefCurrencySchema.Constants.TableName:
					// If SYSTEM => set ACTIVE = N
					// If USER   => do NOT delete (do nothing)
					if ((bool)targetRow[RefCurrencySchema.Constants.RX_IsSystem])
					{
						targetRow[RefCurrencySchema.Constants.RX_IsActive] = false;
					}
					shouldCallBase = false;
					break;
			}

			if (shouldCallBase)
			{
				base.DoDelete(targetRow, ref targetIndex);
			}
			else
			{
				targetIndex++;
			}
		}

		#endregion

		protected bool IsCountryPkConflicted(DataRow sourceRow, DataTable targetTable)
		{
			return IsPkConflicted(RefCountrySchema.Constants.PK, sourceRow[RefCountrySchema.Constants.PK], targetTable);
		}

		protected bool IsCurrencyPkConflicted(DataRow sourceRow, DataTable targetTable)
		{
			return IsPkConflicted(RefCurrencySchema.Constants.PK, sourceRow[RefCurrencySchema.Constants.PK], targetTable);
		}

		protected bool IsPkConflicted(string pkField, object sourcePkValue, DataTable targetTable)
		{
			string filterExpression = String.Format("{0} = '{1}'", pkField, sourcePkValue.ToString());
			DataRow[] matchingPkTargetRows = targetTable.Select(filterExpression);

			return (matchingPkTargetRows.Length > 0);
		}

		#endregion
	}
}
