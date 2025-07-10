using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class ArrivalNotificationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestNationalInfoSeparatorUserControl() => CombineAssertions(() =>
		{
			var control = this.control.NationalInfoSeparatorUserControl;
			AssertType<SeparatorUserControl>("Type", control);
			AssertEquals("Caption", "National Info", control.CaptionResourceString.Caption);
		});

		public void TestArrivalDateDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.BM_ArrivalDate), control.ArrivalDateDateTimeOffsetEdit.GetBindingMember());
				AssertType<ZDateTimeOffsetEdit>("Type", control.ArrivalDateDateTimeOffsetEdit);
			});
		}

		[RequiresSTA]
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestAuthorizationCodeDropEdit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.AuthorizationCode), control.AuthorizationCodeDropEdit.GetBindingMember());
				AssertType<ZDropEdit>("Type", control.AuthorizationCodeDropEdit);
			});
		}

		[RequiresSTA]
		public void TestNumberCodeFindBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.AuthorizationNumber), control.NumberCodeFindBox.GetBindingMember());
				AssertType<ZCodeFindBox>("Type", control.NumberCodeFindBox);
			});
		}

		public void TestOwnerZGuidFindBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.AuthorizationOwner), control.OwnerZGuidFindBox.GetBindingMember());
				AssertType<ZGuidFindBox>("Type", control.OwnerZGuidFindBox);
			});
		}

		[RequiresSTA]
		public void TestDestinationTraderDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.DestinationTraderDocAddressControl);
		}

		public void TestMrnTextBox()
		{
			AssertType<ZTextBox>(control.MrnTextBox);
		}

		public void TestLocalReferenceNumberTextBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.BM_PaperlessInbondNum), control.LocalReferenceNumberTextBox.GetBindingMember());
				AssertType<ZTextBox>(control.LocalReferenceNumberTextBox);
			});
		}

		[RequiresSTA]
		public void TestDestinationCustomsOfficeCodeCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.DestinationCustomsOfficeCodeCodeFindBox);
		}

		public void TestDischargeTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.DischargeTypeDropEdit);
		}

		public void TestCarnetTotalPagesDropEdit()
		{
			AssertType<ZDropEdit>(control.CarnetTotalPagesDropEdit);
		}

		public void TestIncidentFlagDropEdit()
		{
			var incidentFlagDropEdit = control.IncidentFlagDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(control.IncidentFlagDropEdit);
				AssertEquals("BindingMember", nameof(NctsHeader.BH_ExportFlag), incidentFlagDropEdit.GetBindingMember());
			});
		}

		public void TestLocationOfGoodsUserControl()
		{
			var locationOfGoodsUserControl = control.LocationOfGoodsUserControl;
			CombineAssertions(() =>
			{
				AssertType<LocationOfGoodsUserControl>("Type", locationOfGoodsUserControl);
				AssertEquals("BindingMember", nameof(NctsHeader.ArrivalMovementHeader), locationOfGoodsUserControl.GetBindingMember());
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

		[RequiresSTA]
		public void TestTransportMeansLabel() => CombineAssertions(() =>
		{
			var transportMeansLabel = control.TransportMeansLabel;
			AssertType<ZLabel>("Type", transportMeansLabel);
			AssertEquals("Caption", "Transport Means", transportMeansLabel.CaptionResourceString.Caption);
		});

		public void TestCommunicationLanguageDropEdit() => CombineAssertions(() =>
		{
			var communicationLanguageDropEdit = control.CommunicationLanguageDropEdit;
			AssertType<ZDropEdit>("Type", communicationLanguageDropEdit);
			AssertEquals("GetBindingMember", nameof(NctsHeader.BH_CommunicationLanguage), communicationLanguageDropEdit.GetBindingMember());
		});

		[RequiresSTA]
		public void TestTransportAtArrivalTypeDropEdit() => CombineAssertions(() =>
		{
			var transportAtArrivalTypeDropEdit = control.TransportAtArrivalTypeDropEdit;
			AssertType<ZDropEdit>("Type", transportAtArrivalTypeDropEdit);
			AssertEquals("GetBindingMember", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.BM_TransportAtArrivalType), transportAtArrivalTypeDropEdit.GetBindingMember());
		});

		public void TestTransportAtArrivalIDTextBox() => CombineAssertions(() =>
		{
			var transportAtArrivalIDTextBox = control.TransportAtArrivalIDTextBox;
			AssertType<ZTextBox>("Type", transportAtArrivalIDTextBox);
			AssertEquals("GetBindingMember", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.BM_TransportAtArrivalID), transportAtArrivalIDTextBox.GetBindingMember());
		});

		public void TestTransportNationalityCodeFindBox() => CombineAssertions(() =>
		{
			var transportNationalityCodeFindBox = control.TransportNationalityCodeFindBox;
			AssertType<ZCodeFindBox>("Type", transportNationalityCodeFindBox);
			AssertEquals("GetBindingMember", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality), transportNationalityCodeFindBox.GetBindingMember());
		});

		[RequiresSTA]
		public void TestSealsStateValidUserControl()
		{
			var sealsStateValidDropEdit = control.StateOfSealsDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(control.StateOfSealsDropEdit);
				AssertEquals("BindingMember", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsHeader.ArrivalMovementHeader.BM_StateOfSeals), sealsStateValidDropEdit.GetBindingMember());
			});
		}

		public void TestAdditionalTextUserControl()
		{
			var additionalTextTextBox = control.AdditionalTextTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(control.AdditionalTextTextBox);
				AssertEquals("BindingMember", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsHeader.ArrivalMovementHeader.BM_AdditionalText), additionalTextTextBox.GetBindingMember());
			});
		}

		[RequiresSTA]
		public void TestGoodsLocationFromAuthorizationCodeFindBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.AuthorizationLocation), control.GoodsLocationFromAuthorizationCodeFindBox.GetBindingMember());
				AssertType<ZCodeFindBox>("Type", control.GoodsLocationFromAuthorizationCodeFindBox);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ArrivalNotificationDetailsUserControl();
		}

		public void TestDestinationCustomsOfficeCodeCodeFindBoxisBindToDestinationCustomsOfficeCodeForArrival()
		{
			var desCusFindBox = control.FindSingleOrDefault<ZCodeFindBox>("DestinationCustomsOfficeCodeCodeFindBox");
			AssertNotNull(desCusFindBox);

			AssertEquals("ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival", desCusFindBox.BindTo);
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ArrivalNotificationDetailsUserControl control;
	}
}
