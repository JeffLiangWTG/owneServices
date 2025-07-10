using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	public class MENTControlTest : TestCaseWithFactory
	{
		public void TestNoQueryExists()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "TubaDude";

			using (var control = new MENTControl())
			{
				control.SetDataBinding(band, string.Empty);
				var mentControl = control.Controls.Find("mentCollectionControl2", true).SingleOrDefault();

				AssertNotNull(mentControl);
			}
		}

		public void TestQueryExists()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "TubaDude";

			var query = MENTTestHelper.CreateQuery(Factory, "BEEF");
			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			using (var control = new MENTControl())
			{
				control.SetDataBinding(band, string.Empty);
				var mentControl = control.Controls.Find("mentCollectionControl2", true).SingleOrDefault();

				AssertNotNull(mentControl);
			}
		}

		public void TestOpenRelatedAcceptabiltyBandForm()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FC_Component = buffer.PK;
			band.BAB_Name = "TubaDude";

			var query = MENTTestHelper.CreateQuery(Factory, "BEEF");
			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			using (var control = new MENTControl())
			{
				control.SetDataBinding(band, string.Empty);

				var linkedEntityButton = control.Controls.Find("openLinkedEntityZButton", true).SingleOrDefault() as ZButton;

				AssertNotNull(linkedEntityButton);

				linkedEntityButton.PerformClick();

				AssertNull("No Error Shown", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
