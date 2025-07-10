using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Testing
{
	[TestedType(typeof(SWLOrganisationForm))]
	class SWLOrganisationFormTest : OrganisationFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingConsortium = true;
			Factory.Save();
			return new SWLOrganisationForm(carrier);
		}

		protected override void SetUp()
		{
			SWLDataRegistry.Instance.IsShipnetEnable.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
		}
	}
}
