using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class XmlColumnElementRenameTransformation : DataTransformation
	{
		protected XmlColumnElementRenameTransformation()
		{
		}

		protected sealed override void OfflinePostUpgradeTransform()
		{
			var sql = GetTransformSql();
			Db.Connection.ExecuteNonQuery(sql);
		}

		string GetTransformSql()
		{
			var elements = ElementsToRename.ToArray();
			if (elements.Length == 0)
			{
				throw new InvalidOperationException("At least one element rename detail must be specified");
			}

			var replaceSql = GetReplaceSql("cast(XmlData as nvarchar(max))", elements[0]);

			for (int i = 1; i < elements.Length; i++)
			{
				replaceSql = GetReplaceSql(replaceSql, elements[i]);
			}

			return string.Format(@"
				WITH data as (
					SELECT
						{0},
						TRY_CONVERT(xml, {1}) XmlData
					FROM {2}
				)
				UPDATE {2}
				SET {1} = cast({3} as xml)
				{4}
				FROM {2}
				JOIN data on {2}.{0} = data.{0}
				WHERE XmlData is not null;",
				/*0*/ XmlColumn.TableSchema.PK.Name,
				/*1*/ XmlColumn.Name,
				/*2*/ XmlColumn.TableName,
				/*3*/ replaceSql,
				/*4*/ SetAuditColumnsExpression);
		}

		static string GetReplaceSql(string expression, ElementRenameDetails element)
		{
			return string.Format("Replace(Replace(Replace({0}, '<{1}>', '<{2}>'), '</{1}>', '</{2}>'), '<{1}/>', '<{2}/>')", expression, element.OldElementName, element.NewElementName);
		}

		protected internal abstract SchemaColumn XmlColumn { get; }
		protected abstract IEnumerable<ElementRenameDetails> ElementsToRename { get; }
		protected internal virtual string SetAuditColumnsExpression => "";

		protected internal class ElementRenameDetails
		{
			public ElementRenameDetails(string oldElementName, string newElementName)
			{
				OldElementName = oldElementName;
				NewElementName = newElementName;
			}

			internal string OldElementName { get; private set; }
			internal string NewElementName { get; private set; }
		}
	}
}
