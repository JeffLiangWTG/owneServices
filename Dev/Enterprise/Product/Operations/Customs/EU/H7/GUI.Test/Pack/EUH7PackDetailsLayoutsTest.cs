using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7PackDetailsLayouts))]
	class EUH7PackDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EUH7PackDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Medium);
				yield return (EUH7PackDetailsControlBag.Instance.PackQtyCalcEdit, ControlWidthClass.Medium);
				yield return (EUH7PackDetailsControlBag.Instance.PackUQDropEdit, ControlWidthClass.Medium);
				yield return (EUH7PackDetailsControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Medium);
				yield return (EUH7PackDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (EUH7PackDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EUH7PackDetailsControlLayoutBuilder<AsycudaPack>();

		public void TestCaptions()
		{
			AssertCaption(EUH7PackDetailsControlBag.Instance.GoodsDescriptionTextBox, "Description (on Pack)", "Description (on Pack)", "Description (on Pack)", "Description of goods, as stipulated on the pack.");
			AssertCaption(EUH7PackDetailsControlBag.Instance.MarksAndNumbersTextBox, "Marks (on Pack)", "Marks (on Pack)", "Marks (on Pack)", "Free form description of the marks and numbers stipulated on the pack.");
		}

		void AssertCaption(ControlReference controlReference, string expectedCaption, string expectedShortCaption, string expectedMediumCaption, string expectedFullDescription)
		{
			LayoutForTesting.TryGetCaption(controlReference, null, out var captionData);
			AssertEquals($"Caption for {controlReference.ControlName}", expectedCaption, captionData?.Caption);
			AssertEquals($"ShortCaption for {controlReference.ControlName}", expectedShortCaption, captionData?.ShortCaption);
			AssertEquals($"MediumCaption for {controlReference.ControlName}", expectedMediumCaption, captionData?.MediumCaption);
			AssertEquals($"FullDescription for {controlReference.ControlName}", expectedFullDescription, captionData?.FullDescription);
		}
	}
}
