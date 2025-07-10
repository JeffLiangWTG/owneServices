using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Mapping;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	[DefaultField("TitleText")]
	public class TableWrapper : GenericWrapper
	{
		public TableWrapper(MapTable mapTable, BusinessObjectFactory factory)
			: base(null, factory)
		{
			MapTable = mapTable ?? new MapTable("", "", "", 0, "", 0, false);
			List<MapTable.Line> lines = MapTable.GetLines();
			ZStringBuilder leftColumnText = new ZStringBuilder();
			ZStringBuilder rightColumnText = new ZStringBuilder();
			foreach (MapTable.Line line in lines)
			{
				leftColumnText.Append(line.LeftColumnValue);
				rightColumnText.Append(line.RightColumnValue);
			}
			fLeftColumnText = leftColumnText.ToStringWithNewLineBetweenAppends();
			fRightColumnText = rightColumnText.ToStringWithNewLineBetweenAppends();
		}
		readonly MapTable MapTable;
		readonly ZString fLeftColumnText;
		readonly ZString fRightColumnText;

		public ZString TitleText
		{
			get { return MapTable.TypeTitle; }
		}

		public ZString TitleSupplement
		{
			get { return MapTable.DefaultFieldIncludingTitle; }
		}

		public ZString LeftColumnTitle
		{
			get { return MapTable.LeftColumnTitle; }
		}

		public ZString LeftColumnText
		{
			get { return fLeftColumnText; }
		}

		public ZString RightColumnTitle
		{
			get { return MapTable.RightColumnTitle; }
		}

		public ZString RightColumnText
		{
			get { return fRightColumnText; }
		}

		public ZString DataSourceType
		{
			get { return (MapTable.IsNonGeneric ? (NoResString)"Non" : (NoResString)"") + (NoResString)"Generic"; }
		}
	}
}
