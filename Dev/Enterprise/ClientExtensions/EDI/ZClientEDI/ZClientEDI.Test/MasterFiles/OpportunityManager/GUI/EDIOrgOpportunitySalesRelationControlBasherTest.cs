using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDIOrgOpportunitySalesRelationControlFormForTest))]
	sealed class EDIOrgOpportunitySalesRelationControlBasherTest : ZFormBasherTest
	{
		public void TestRelatableTypeButtonCaptionPairs()
		{
			using (var form = new EDIOrgOpportunitySalesRelationControlFormForTest())
			{
				form.Show();
				Application.DoEvents();
				AssertCollectionContains(EDIRelatableActivityTypeList.Codes.Incident, form.EDIOrgOpportunitySalesRelationControl.RelatableTypeButtonCaptionPairsExposed.Select(rt => rt.Key));
			}
		}

#region Implementation
		protected override Form GetFormToBashCore()
		{
			var form = new EDIOrgOpportunitySalesRelationControlFormForTest();
			form.ControllerID = ControllerIDs.Opportunity;
			return form;
		}

#region Classes
		public class EDIOrgOpportunitySalesRelationControlForTest : EDIOrgOpportunitySalesRelationControl
		{
			internal IEnumerable<KeyValuePair<string, ResourceStringData>> RelatableTypeButtonCaptionPairsExposed => RelatableTypeButtonCaptionPairs;
		}

		public class EDIOrgOpportunitySalesRelationControlFormForTest : ZForm
		{
			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 768);
				Controls.Add(EDIOrgOpportunitySalesRelationControl);
				CaptionRenderingEnabled = true;
			}

			internal EDIOrgOpportunitySalesRelationControlForTest EDIOrgOpportunitySalesRelationControl = new EDIOrgOpportunitySalesRelationControlForTest();
		}
#endregion
#endregion
	}
}
