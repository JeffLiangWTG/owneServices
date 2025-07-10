using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EntryInstructionBasicDetailsLayoutProvider))]
	sealed class EntryInstructionBasicDetailsLayoutProviderTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.DetailsLabel, ControlWidthClass.LongNoCaption);
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseUserControl, ControlWidthClass.LongNoCaption);
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.BondHolderOrganisationControl, ControlWidthClass.LongNoCaption);
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.RemoverOrganisationControl, ControlWidthClass.LongNoCaption);
				yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 2;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
	}
}
