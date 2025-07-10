using System;
using System.Collections;
using System.Data;
using CargoWise.Data;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase
{
	[CodeAlive("AddInfo will need to be parsed in the future?")]
	abstract class AddInfoKeyDataPairExportTransformation : DataTransformation
	{
		protected override void OfflinePostUpgradeTransform()
		{
			ArrayList results = new ArrayList();

			DbCommand cmd = Db.Connection.Command(FindAddInfosSQL);
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					results.Add(new PKAddInfoPair(reader.GetGuid(0), reader.GetString(1)));
				}
			}

			foreach (PKAddInfoPair pair in results)
			{
				MoveDataFromAddInfoToDesiredDestination(pair);
			}

			RunConcludingUpdate();
		}

		public virtual void RunConcludingUpdate()
		{
		}

		protected class PKAddInfoPair
		{
			public PKAddInfoPair(Guid pK, string addInfo)
			{
				this.PK = pK;
				this.AddInfo = addInfo;
			}

			public readonly Guid PK;
			public readonly string AddInfo;
		}

		string FindAddInfosSQL
		{
			get
			{
				return "select " + TablePKName + ", " + TableAddInfoFieldName + " from " + TableName + " " +
					" where " + TableAddInfoFieldName + " like '%" + AddInfoDataToSearchByAfterLike + "%'" + ExtraFindClause;
			}
		}

		protected virtual string ExtraFindClause
		{
			get { return ""; }
		}

		protected virtual string AddInfoDataToSearchByAfterLike
		{
			get { return AddInfoKey + "="; }
		}

		protected virtual object ConvertStringAddInfoRepresentationToCorrectType(string valueAsString)
		{
			return valueAsString;
		}

		protected virtual SqlDbType TypeOfValue
		{
			get { return SqlDbType.VarChar; }
		}

		internal abstract string TableName { get; }
		internal abstract string TableAddInfoFieldName { get; }
		internal abstract string TablePKName { get; }
		internal abstract string AddInfoKey { get; }
		protected abstract void MoveDataFromAddInfoToDesiredDestination(PKAddInfoPair pair);
	}
}
