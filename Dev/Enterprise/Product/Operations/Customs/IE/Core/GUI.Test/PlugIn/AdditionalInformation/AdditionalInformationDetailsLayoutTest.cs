using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(AdditionalInformationDetailsLayout))]
	sealed class AdditionalInformationDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestKindDropEditCaption()
		{
			var info = Factory.New<AdditionalInfo>();
			var layout = ((IPanelLayoutProvider)new AdditionalInformationDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				info.CSI_SubType = "^_^";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.KindDropEdit, info, out var resourceStringData);
				AssertEquals("Default caption", "Kind", resourceStringData.Caption);

				info.CSI_SubType = "INF";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.KindDropEdit, info, out resourceStringData);
				AssertEquals("Caption", "Kind", resourceStringData.Caption);
				AssertEquals("FullDescription", "Additional Information – Kind", resourceStringData.FullDescription);

				info.CSI_SubType = "TRA";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.KindDropEdit, info, out resourceStringData);
				AssertEquals("Caption", "Kind", resourceStringData.Caption);
				AssertEquals("FullDescription", "Transport Document – Kind", resourceStringData.FullDescription);

				info.CSI_SubType = "REF";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.KindDropEdit, info, out resourceStringData);
				AssertEquals("Caption", "Kind", resourceStringData.Caption);
				AssertEquals("FullDescription", "Additional Reference – Kind", resourceStringData.FullDescription);
			});
		}

		public void TestFullTypeCodeFindBoxCaption()
		{
			var info = Factory.New<AdditionalInfo>();
			var layout = ((IPanelLayoutProvider)new AdditionalInformationDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				info.CSI_SubType = "^_^";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, info, out var resourceStringData);
				AssertEquals("Default caption", "Full Type", resourceStringData.Caption);

				info.CSI_SubType = "INF";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, info, out resourceStringData);
				AssertEquals("Caption", "Full Type", resourceStringData.Caption);
				AssertEquals("FullDescription", "Additional Information – [12 12 008 000] Code", resourceStringData.FullDescription);

				info.CSI_SubType = "TRA";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, info, out resourceStringData);
				AssertEquals("Caption", "Full Type", resourceStringData.Caption);
				AssertEquals("FullDescription", "Transport Document – [12 05 002 000] Type", resourceStringData.FullDescription);

				info.CSI_SubType = "REF";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, info, out resourceStringData);
				AssertEquals("Caption", "Full Type", resourceStringData.Caption);
				AssertEquals("FullDescription", "Additional Reference – [12 04 002 000] Type", resourceStringData.FullDescription);
			});
		}

		public void TestReferenceTextBoxCaption()
		{
			var info = Factory.New<AdditionalInfo>();
			var layout = ((IPanelLayoutProvider)new AdditionalInformationDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				info.CSI_SubType = "INF";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, info, out var resourceStringData);
				AssertEquals("Default Caption", "Reference", resourceStringData.Caption);

				info.CSI_SubType = "TRA";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, info, out resourceStringData);
				AssertEquals("Caption", "Reference", resourceStringData.Caption);
				AssertEquals("FullDescription", "Transport Document – [12 05 001 000] Reference Number", resourceStringData.FullDescription);

				info.CSI_SubType = "REF";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, info, out resourceStringData);
				AssertEquals("Caption", "Reference", resourceStringData.Caption);
				AssertEquals("FullDescription", "Additional Reference – [12 04 001 000] Reference Number", resourceStringData.FullDescription);
			});
		}

		public void TestDescriptionTextBoxCaption()
		{
			var info = Factory.New<AdditionalInfo>();
			var layout = ((IPanelLayoutProvider)new AdditionalInformationDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				info.CSI_SubType = "^_^";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.DescriptionTextBox, info, out var resourceStringData);
				AssertEquals("Default Caption", "Description", resourceStringData.Caption);

				info.CSI_SubType = "INF";
				layout.TryGetCaption(AdditionalInformationDetailsControlBag.Instance.DescriptionTextBox, info, out resourceStringData);
				AssertEquals("Caption", "Description", resourceStringData.Caption);
				AssertEquals("FullDescription", "Additional Information – [12 02 009 000] Text", resourceStringData.FullDescription);
			});
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInformationDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalInformationDetailsControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
				yield return (AdditionalInformationDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			}
		}
	}
}
