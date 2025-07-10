using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CACusPermitFilterControlTest : Customs.Module.Testing.CusPermitFilterControlTest
	{
		public void TestFilteredGridFields()
		{
			var filterRules = new Dictionary<CodeDescriptionPair, ZString>() {
					{ new CodeDescriptionPair("CODE1", "CODE1 Description"), "VALUE1" },
					{ new CodeDescriptionPair("CODE2", "CODE2 Description"), "VALUE2" },
					{ new CodeDescriptionPair("CODE3", "CODE3 Description"), "VALUE3" },
				};
			var collection = PermitFindBoxCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, null, "", "", filterRules, ZDate.Today);
			var filterBizO = new CACusPermitFilterStripBusinessObject();

			using (var filterControl = new CACusPermitFilterControl(collection, filterBizO))
			{
				filterControl.Show();
				AssertEquals("DIFURNs", "DIF URN", filterControl.FilteredGrid.GetColumnStyle("DIFURNs").CaptionResourceString.Caption);
				AssertEquals("DIFMessageStatus", "DIF Message Status", filterControl.FilteredGrid.GetColumnStyle("DIFMessageStatus").CaptionResourceString.Caption);
			}
		}
	}
}
