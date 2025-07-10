using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class TariffBoxNomenclatureSelectionModeBehaviourTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestUpdateBehaviour()
		{
			using (var form = new ZForm(goodsItem))
			{
				var controlBag = GoodsItemDetailsControlBag.Instance;
				var allControls = new Dictionary<ControlReference, Control>();
				controlBag.CreateControls(form, allControls);
				var tariffFindBoxControl = allControls[controlBag.CommodityCodeTariffFindBox] as Universal.GUI.TariffFindBox;
				AssertNotNull("TariffFindBoxControl", tariffFindBoxControl);

				var behaviour = new TariffBoxNomenclatureSelectionModeBehaviour();
				behaviour.UpdateBehaviour(tariffFindBoxControl, goodsItem);

				var actualModes = tariffFindBoxControl.GetSelectNomenclatureModes?.Invoke();
				AssertArrayEqualsByElements($"Selection Modes", new[] { SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff }, actualModes.ToArray());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			Factory.Save();
		}

		NctsHeader header;
		NctsDepartureCargoDesc goodsItem;
	}
}
