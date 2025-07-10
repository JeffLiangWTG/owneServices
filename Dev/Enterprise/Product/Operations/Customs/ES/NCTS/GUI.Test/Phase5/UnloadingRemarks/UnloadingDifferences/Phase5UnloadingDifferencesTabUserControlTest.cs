using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class Phase5UnloadingDifferencesTabUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGuaranteeGroupBoxVisibilityChangedWhenArrivalGoodsLocationValueChanged()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "Location1";

			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "Location";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			using (ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.TemporaryStorage, Core.Constants.CountryCodes.Spain, ZDateTime.Now, true))
			{
				using (var form = new ZForm(nctsHeader))
				{
					form.Controls.Add(userControl);
					userControl.SetDataBinding(nctsHeader, "");
					form.Show();

					var guaranteeGroupBox = (ZGroupBox)userControl.Controls.Find("GuaranteeGroupBox", true).SingleOrDefault();

					CombineAssertions(() =>
					{
						AssertEquals("GuaranteeGroupBox is not visible when ShouldGuaranteeForArrivalBeVisible is false", false, guaranteeGroupBox.Visible);

						arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "Location1";
						AssertEquals("GuaranteeGroupBox is visible when ShouldGuaranteeForArrivalBeVisibleCoreForTest is true, after ArrivalGoodsLocation is changed", true, guaranteeGroupBox.Visible);

						arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "Location2";
						AssertEquals("GuaranteeGroupBox is not visible when ShouldGuaranteeForArrivalBeVisibleCoreForTest is false, after ArrivalGoodsLocation is changed", false, guaranteeGroupBox.Visible);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5UnloadingDifferencesTabUserControl();
		}
		Phase5UnloadingDifferencesTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
