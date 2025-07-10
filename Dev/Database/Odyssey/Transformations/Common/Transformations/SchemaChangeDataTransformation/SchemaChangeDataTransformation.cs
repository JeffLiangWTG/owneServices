using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class SchemaChangeDataTransformation : DataTransformation
	{
		#region Implementation

		/// <summary>
		/// This method is called only once to initialise the transformation SourceTableCollection
		/// See http://wiki.edi.com.au/default.aspx/EDI.DataCopyTransformation for autogeneration of the code for this method
		/// </summary>
		protected abstract SourceTableCollection GetSourceTables();
		protected abstract void PasteToTarget();

		protected virtual void BeforeTablesCopied() { }

		protected virtual void AfterTablesCopied() { }

		public ISourceTableCollection SourceTables
		{
			get
			{
				if (fSourceTables == null)
				{
					fSourceTables = GetSourceTables();
				}

				return fSourceTables;
			}
		}

		ISourceTableCollection fSourceTables;

		protected override sealed void OfflinePreUpgradeTransform()
		{
			ShowInfo("Transformation: [" + UserDescription + "]");
			CopySourceTables();
		}

		protected override sealed void OfflinePostUpgradeTransform()
		{
			if (!SourceTables.AreAllTablesCopied)
			{
				throw new InvalidOperationException("Cannot run transformation as not all source tables have been copied.");
			}

			PasteToTarget();
			DropTempSourceTablesAfterUse();
		}

		protected void CopySourceTables()
		{
			BeforeTablesCopied();

			foreach (SourceTable table in SourceTables)
			{
				table.Create();
				if (table.HasWarningsOnCopy)
				{
					ShowInfo("\t" + table.WarningMessageOnCopy);
				}
			}

			AfterTablesCopied();
		}

		protected void DropTempSourceTablesAfterUse()
		{
			foreach (SourceTable table in SourceTables)
			{
				table.Drop();
			}
		}

		public override bool IsRequired
		{
			get { return base.IsRequired && SourceExists; }
		}

		bool SourceExists
		{
			get
			{
				foreach (SourceTable table in SourceTables)
				{
					if (!DataUtils.ObjectExists(Db.Connection, table.OriginalFullyQualifiedName))
					{
						return false;
					}
				}

				return true;
			}
		}

		#endregion
	}
}
