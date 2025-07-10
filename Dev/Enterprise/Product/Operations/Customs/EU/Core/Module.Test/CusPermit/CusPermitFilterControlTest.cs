using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	class CusPermitFilterControlTest : Customs.Module.Testing.CusPermitFilterControlTest
	{
		[RequiresSTA]
		public void TestNewZFilterStrip_ReturnType()
		{
			var filterRules = new Dictionary<CodeDescriptionPair, ZString>();
			var collection = PermitFindBoxCollection.GetCachedCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, null, "", "", filterRules, ZDate.Today);
			var filterBizO = new CusPermitFilterStripBusinessObject();
			using (var form = new Form())
			{
				var filterControl = new CusPermitFilterControl_ForTest(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();
				AssertType<CusPermitModuleStrip>(filterControl.Strips.Single());
			}
		}

		class CusPermitFilterControl_ForTest : CusPermitFilterControl
		{
			public CusPermitFilterControl_ForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
			{
			}

			public new List<ZFilterStrip> Strips => base.Strips;
		}
	}
}
