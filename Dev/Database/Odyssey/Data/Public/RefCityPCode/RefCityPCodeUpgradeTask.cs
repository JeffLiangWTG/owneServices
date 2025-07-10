using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefPostCodeSchema))]
[assembly: UsesConstants(typeof(RefCityTownSchema))]
[assembly: UsesConstants(typeof(RefCityPCodePivotSchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class RefCityPCodeUpgradeTask : EmbeddedUpgradeTask
	{
		public RefCityPCodeUpgradeTask() : base(new RefCityPCodeDataFile())
		{
		}

		protected override string ComparingColumnsNames(DataTable upgradeTable)
		{
			string result = base.ComparingColumnsNames(upgradeTable);

			switch (upgradeTable.TableName)
			{
				case RefPostCodeSchema.Constants.TableName:
					result = RefPostCodeSchema.Constants.RK_RN_NKCountry + "," + RefPostCodeSchema.Constants.RK_CityTownPostCode;
					break;

				case RefCityTownSchema.Constants.TableName:
					result = RefCityTownSchema.Constants.R9_RN_NKCountry + "," + RefCityTownSchema.Constants.R9_InternationalName + "," + RefCityTownSchema.Constants.R9_RW_NKState;
					break;

				case RefCityPCodePivotSchema.Constants.TableName:
					result = RefCityPCodePivotSchema.Constants.R0_RK + "," + RefCityPCodePivotSchema.Constants.R0_R9;
					break;
			}

			return result;
		}

		#region Insert, Update and Delete

		#region Overrides

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			bool shouldCallBase = true;

			switch (targetTable.TableName)
			{
				case RefPostCodeSchema.Constants.TableName:
					// If PK conflicts with existing one, insert record but with a new PK
					if (IsPostCodePkConflicted(sourceRow, targetTable))
					{
						Guid sourcePk = (Guid)sourceRow[RefPostCodeSchema.Constants.PK];
						Guid targetPk = Guid.NewGuid();
						((RefCityPCodeDataFile)ResourceFile).AddPostCodePkConflict(sourcePk, targetPk);

						sourceRow[RefPostCodeSchema.Constants.PK] = targetPk;
					}
					break;

				case RefCityTownSchema.Constants.TableName:
					// If PK conflicts with existing one, insert record but with a new PK
					if (IsCityTownPkConflicted(sourceRow, targetTable))
					{
						Guid sourcePk = (Guid)sourceRow[RefCityTownSchema.Constants.PK];
						Guid targetPk = Guid.NewGuid();
						((RefCityPCodeDataFile)ResourceFile).AddCityTownPkConflict(sourcePk, targetPk);

						sourceRow[RefCityTownSchema.Constants.PK] = targetPk;
					}
					break;

				case RefCityPCodePivotSchema.Constants.TableName:

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

			if (targetRow.Table.TableName == RefPostCodeSchema.Constants.TableName && !(bool)targetRow[RefPostCodeSchema.Constants.RK_IsSystem] ||
				targetRow.Table.TableName == RefCityTownSchema.Constants.TableName && !(bool)targetRow[RefCityTownSchema.Constants.R9_IsSystem] ||
				targetRow.Table.TableName == RefCityPCodePivotSchema.Constants.TableName && !(bool)targetRow[RefCityPCodePivotSchema.Constants.R0_IsSystem])
			{
				shouldCallBase = false;
				// if we don't insert PostCode OR CityTown then we should not insert pivot which reference its PK as we'd get foreign key violation
				Guid excludedPk = Guid.Empty;

				if (targetRow.Table.TableName == RefPostCodeSchema.Constants.TableName)
				{
					excludedPk = (Guid)sourceRow[RefPostCodeSchema.Constants.PK];
				}
				else if (targetRow.Table.TableName == RefCityTownSchema.Constants.TableName)
				{
					excludedPk = (Guid)sourceRow[RefCityTownSchema.Constants.PK];
				}

				if (excludedPk != Guid.Empty)
				{
					((RefCityPCodeDataFile)ResourceFile).AddPivotOrphanFK(excludedPk);
				}
			}
			else
			{
				switch (columnName)
				{
					case RefPostCodeSchema.Constants.PK:
						// Does not update PK, but adds to conflicting PK list to be handled afterwards
						Guid sourcePostCodePk = (Guid)sourceRow[RefPostCodeSchema.Constants.PK];
						Guid targetPostCodePk = (Guid)targetRow[RefPostCodeSchema.Constants.PK];
						((RefCityPCodeDataFile)ResourceFile).AddPostCodePkConflict(sourcePostCodePk, targetPostCodePk);
						shouldCallBase = false;
						break;

					case RefCityTownSchema.Constants.PK:
						Guid sourceCityTownPk = (Guid)sourceRow[RefCityTownSchema.Constants.PK];
						Guid targetCityTownPk = (Guid)targetRow[RefCityTownSchema.Constants.PK];
						((RefCityPCodeDataFile)ResourceFile).AddCityTownPkConflict(sourceCityTownPk, targetCityTownPk);
						shouldCallBase = false;
						break;

					case RefCityTownSchema.Constants.R9_IsActive:
					case RefCityTownSchema.Constants.R9_IsSystem:
					case RefPostCodeSchema.Constants.RK_IsSystem:
					case RefCityPCodePivotSchema.Constants.R0_IsSystem:
						shouldCallBase = false;
						break;
				}
			}

			if (shouldCallBase)
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		//	TODO refactor later so that it is a list of coutnries we provide base data for
		//	this is to avoid deactivation of CityTown and Postcodes for countries we don't provide data
		//	so far we supply for AU only

		bool DoWeProvideBaseDataForCountry(string countryCode)
		{
			return string.Compare(countryCode, "AU", true) == 0;
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			bool shouldCallBase = true;

			switch (targetRow.Table.TableName)
			{
				case RefPostCodeSchema.Constants.TableName:

					if (DoWeProvideBaseDataForCountry((string)targetRow[RefPostCodeSchema.Constants.RK_RN_NKCountry]))
					{
						// Do NOT delete, set ACTIVE = 0
						targetRow[RefPostCodeSchema.Constants.RK_IsActive] = 0;
					}
					shouldCallBase = false;
					break;

				case RefCityTownSchema.Constants.TableName:
					if (DoWeProvideBaseDataForCountry((string)targetRow[RefCityTownSchema.Constants.R9_RN_NKCountry]))
					{
						// Do NOT delete, set ACTIVE = 0
						targetRow[RefCityTownSchema.Constants.R9_IsActive] = 0;
					}
					shouldCallBase = false;
					break;

				case RefCityPCodePivotSchema.Constants.TableName:
					if (!(bool)targetRow[RefCityPCodePivotSchema.Constants.R0_IsSystem])
					{
						shouldCallBase = false;
					}
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

		protected bool IsPostCodePkConflicted(DataRow sourceRow, DataTable targetTable)
		{
			return IsPkConflicted(RefPostCodeSchema.Constants.PK, sourceRow[RefPostCodeSchema.Constants.PK], targetTable);
		}

		protected bool IsCityTownPkConflicted(DataRow sourceRow, DataTable targetTable)
		{
			return IsPkConflicted(RefCityTownSchema.Constants.PK, sourceRow[RefCityTownSchema.Constants.PK], targetTable);
		}

		protected bool IsCityPCodePivotPkConflicted(DataRow sourceRow, DataTable targetTable)
		{
			return IsPkConflicted(RefCityPCodePivotSchema.Constants.PK, sourceRow[RefCityPCodePivotSchema.Constants.PK], targetTable);
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
