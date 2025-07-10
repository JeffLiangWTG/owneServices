using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public partial class OrganisationalUnitPickerColumnStyle : ZCodeFindBoxColumnStyle
	{
		public OrganisationalUnitPickerColumnStyle(OrganisationalUnitPickerColumnStyleInfo info)
			: base(() => new OrganisationalUnitPickerFindBox(), info)
		{
		}
	}

	public class OrganisationalUnitPickerColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(OrganisationalUnitPickerColumnStyle);
	}
}
