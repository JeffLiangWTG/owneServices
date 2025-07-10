using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture.Business.Internal
{
	internal class StmDataGridLayoutStorage : IGridLayoutStorage
	{
		public static bool IsDefaultLayout(string layoutName)
		{
			return string.IsNullOrEmpty(layoutName) ||
				layoutName.ToLower() == DefaultLayoutName.GetUnresolvedString().ToLower();
		}

		public static StmDataGridLayoutStorage New(StmData stmData)
		{
			StmDataGridLayoutStorage result = null;

			if (stmData != null)
			{
				result = new StmDataGridLayoutStorage(stmData);
			}

			return result;
		}

		StmDataGridLayoutStorage(StmData stmData)
		{
			this.stmData = stmData;
		}

		readonly StmData stmData;

		#region IColumnLayoutStorage Members

		public string GridLayoutKey
		{
			get { return stmData.SD_Name; }
		}

		public string ColumnLayoutName
		{
			get { return DefaultLayoutName.GetUnresolvedString(); }
			set
			{
				throw new NotSupportedException();
			}
		}

		public string ColumnLayoutDisplayName => DefaultLayoutName;

		[ThreadSafe]
		public static MultilingualString DefaultLayoutName = ResString.GetMultilingualString("875fcd16-6a34-4ff4-bf6f-e5f80c6dca54", "Default");

		public byte[] ColumnLayoutData
		{
			get { return stmData.SD_BinaryValue; }
		}

		public bool IsRenameAllowed
		{
			get { return false; }
		}

		public bool SaveColumnLayout
		{
			get { return false; }
			set
			{
				throw new NotSupportedException();
			}
		}

		public bool SaveGridColourLayout
		{
			get { return false; }
			set
			{
				throw new NotSupportedException();
			}
		}

		public ZGuid GridColourLayoutID
		{
			get { return ZGuid.Empty; }
			set
			{
				throw new NotSupportedException();
			}
		}

		public bool IsSystemDefined
		{
			get { return false; }
		}

		public bool IsPublished
		{
			get { return false; }
		}

		public bool IsPublishedAcrossAllCompanies
		{
			get { return false; }
		}

		public bool IsDeleteAllowed
		{
			get { return false; }
		}

		public bool IsDeleted
		{
			get { return stmData.IsDeleted; }
		}

		void IGridLayoutStorage.Delete()
		{
			throw new NotSupportedException();
		}

		#endregion

		public ZGuid PK
		{
			get { return stmData.PK; }
		}
	}
}
