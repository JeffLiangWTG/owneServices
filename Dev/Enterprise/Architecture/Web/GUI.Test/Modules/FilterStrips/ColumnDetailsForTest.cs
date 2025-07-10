using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	public sealed class ColumnDetailsForTest
	{
		public ColumnDetailsForTest(ZString headerText, int columnIndex, Type columnType)
		{
			this.HeaderText = headerText;
			this.ColumnIndex = columnIndex;
			this.ColumnType = columnType;
		}

		public ZString HeaderText
		{
			get { return fHeaderText; }
			set { fHeaderText = value; }
		}
		ZString fHeaderText;

		public int ColumnIndex
		{
			get { return fColumnIndex; }
			set { fColumnIndex = value; }
		}
		int fColumnIndex;

		public Type ColumnType
		{
			get { return fColumnType; }
			set { fColumnType = value; }
		}
		Type fColumnType;
	}
}
