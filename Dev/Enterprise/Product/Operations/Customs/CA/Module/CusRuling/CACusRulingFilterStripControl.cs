using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Module;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CACusRulingFilterStripControl : ZZRefCusRulingFilterStripControl
	{
		public CACusRulingFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				FilteredGrid.SetColumnCaption(CACusRuling.Schema.ZZX_RulingNumber, Res.GetString("C4FE8E0D-BEE8-4E10-9D9D-71831CBDE982", "Remission Number"));
				FilteredGrid.SetColumnCaption(CACusRuling.Schema.ZZX_RulingType, Res.GetString("F565993C-9EE7-41E6-BA41-8EBF3D8905F8", "Remission Type"));
				FilteredGrid.SetColumnCaption(CACusRuling.Schema.RulingTypeDescription, Res.GetString("A5B7BFF7-3FC3-4EEC-89B2-2D64529AC93B", "Remission Type Description"));
			}
		}
	}
}
