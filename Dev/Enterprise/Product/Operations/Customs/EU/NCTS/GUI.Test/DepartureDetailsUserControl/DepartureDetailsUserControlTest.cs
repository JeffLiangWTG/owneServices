using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DepartureDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestPresentationDateTimeOffsetEdit()
		{
			var presentationDateTimeOffsetEdit = control.PresentationDateTimeOffsetEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateTimeOffsetEdit>("Type", presentationDateTimeOffsetEdit);
				AssertEquals("BindTo", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_PresentationDateTime), presentationDateTimeOffsetEdit.BindTo);
				AssertEquals("DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Long, presentationDateTimeOffsetEdit.DateTimeFormat);
				AssertEquals("AutoCompleteYear", 12, presentationDateTimeOffsetEdit.TabIndex);
			});
		}

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestCustomerReferenceNumberTextBox()
		{
			var customerReferenceNumberTextBox = control.CustomerReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", customerReferenceNumberTextBox);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_PaperlessInbondNum), customerReferenceNumberTextBox.GetBindingMember());
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, customerReferenceNumberTextBox.CharacterCasing);
			});
		}

		public void TestDeclarationTypeDropEdit()
		{
			var declarationTypeDropEdit = control.DeclarationTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", declarationTypeDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_InBondEntryType), declarationTypeDropEdit.GetBindingMember());
			});
		}

		public void TestAdditionalDeclarationTypeDropEdit()
		{
			var additionalDeclarationTypeDropEdit = control.AdditionalDeclarationTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", additionalDeclarationTypeDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_AdditionalDeclarationType), additionalDeclarationTypeDropEdit.GetBindingMember());
			});
		}

		public void TestTirCarnetNumberTextBox()
		{
			var tirCarnetNumberTextBox = control.TirCarnetNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", tirCarnetNumberTextBox);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.TirCarnetNumber), tirCarnetNumberTextBox.GetBindingMember());
			});
		}

		public void TestCountryOfDispatchDropEdit()
		{
			var countryOfDispatchDropEdit = control.CountryOfDispatchDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", countryOfDispatchDropEdit);
				AssertEquals("GetBindingMember", $"{nameof(NctsHeader.MovementHeader)}.{nameof(NctsArrivalMovementHeader.BM_RN_NKCountryOfDispatch)}", countryOfDispatchDropEdit.GetBindingMember());
			});
		}

		public void TestCountryOfDestinationDropEdit()
		{
			var countryOfDestinationDropEdit = control.CountryOfDestinationDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", countryOfDestinationDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_RL_NKDestinationPort), countryOfDestinationDropEdit.GetBindingMember());
			});
		}

		public void TestSimplifiedProcedureAndReducedDataSetUserControl()
		{
			AssertType<SimplifiedProcedureAndReducedDataSetUserControl>("Type", control.SimplifiedProcedureAndReducedDataSetUserControl);
		}

		public void TestSecurityDropEdit()
		{
			var securityDropEdit = control.SecurityDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", securityDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_TypeOfSecurity), securityDropEdit.GetBindingMember());
			});
		}

		public void TestGrossWeightCalcDropEdit()
		{
			var grossWeightCalcDropEdit = control.GrossWeightCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", grossWeightCalcDropEdit);
				AssertEquals("BindToAmount", $"{nameof(NctsHeader.MovementHeader)}.{nameof(NctsDepartureMovementHeader.BM_GrossWeight)}", grossWeightCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", $"{nameof(NctsHeader.MovementHeader)}.{nameof(NctsDepartureMovementHeader.BM_GrossWeightUQ)}", grossWeightCalcDropEdit.BindToUnit);
				AssertEquals("Decimals", 6, grossWeightCalcDropEdit.Decimals);
			});
		}

		public void TestLocationOfGoodsUserControl()
		{
			var locationOfGoodsUserControl = control.LocationOfGoodsUserControl;
			CombineAssertions(() =>
			{
				AssertType<LocationOfGoodsUserControl>("Type", locationOfGoodsUserControl);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader), locationOfGoodsUserControl.GetBindingMember());
			});
		}

		public void TestDateLimitDateEdit()
		{
			var dateLimitDateEdit = control.DateLimitDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", dateLimitDateEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_ExportDate), dateLimitDateEdit.GetBindingMember());
			});
		}

		public void TestCommunicationLanguageDropEdit()
		{
			var communicationLanguageDropEdit = control.CommunicationLanguageDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", communicationLanguageDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.BH_CommunicationLanguage), communicationLanguageDropEdit.GetBindingMember());
			});
		}

		public void TestTimeLimitForTransitCalcEdit()
		{
			var timeLimitForTransitCalcEdit = control.TimeLimitForTransitCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZIntEdit>("Type", timeLimitForTransitCalcEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsHeader.MovementHeader.BM_ExportTimeLimit), timeLimitForTransitCalcEdit.GetBindingMember());
			});
		}

		public void TestOverrideFreightDetailsCheckBox()
		{
			var overrideFreightDetailsCheckBox = control.OverrideFreightDetailsCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", overrideFreightDetailsCheckBox);
				AssertEquals("GetBindingMember", nameof(NctsHeader.BH_OverrideFreightDefaults), overrideFreightDetailsCheckBox.GetBindingMember());
			});
		}

		public void TestCommercialReferenceNumberTextBox()
		{
			var commercialReferenceNumberTextBox = control.CommercialReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", commercialReferenceNumberTextBox);
				AssertEquals("BindTo", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_UniqueConsignmentReference), commercialReferenceNumberTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, commercialReferenceNumberTextBox.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DepartureDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		DepartureDetailsUserControl control;
	}
}
