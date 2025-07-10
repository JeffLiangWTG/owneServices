using System;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.GUI
{
	[SuppressCheckControlModuleId]
	[SuppressCheckControlLookupList]
	public class PlaceOfUseOrProcessingColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(PlaceOfUseOrProcessingColumnStyle);
	}
}
