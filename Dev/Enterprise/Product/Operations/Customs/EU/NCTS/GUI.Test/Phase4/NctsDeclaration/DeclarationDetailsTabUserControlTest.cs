using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Design;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class DeclarationDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestRepresentativeField()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var frm = new ZForm(header))
			{
				frm.Controls.Add(new DeclarationDetailsTabUserControl());
				frm.Show();

				Application.DoEvents();

				var representativeAddressControl = (MasterFiles.GUI.ZDocAddressControl)frm.Controls.Find("RepresentativeDocAddressControl", true).First();

				AssertEquals(true, representativeAddressControl.IsVisibleForBinding);
				AssertEquals(false, representativeAddressControl.IsDisposed);
			}
		}

		[RequiresSTA]
		public void TestBrokerFindBoxWhenCountryIsGermany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				using (var frm = new ZForm(header))
				{
					frm.Controls.Add(new DeclarationDetailsTabUserControl());
					frm.Show();

					Application.DoEvents();

					var brokerFindBox = (ZCodeFindBox)frm.Controls.Find("BrokerFindBox", true).First();

					AssertEquals(true, brokerFindBox.IsVisibleForBinding);
					AssertEquals(false, brokerFindBox.IsDisposed);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				using (var frm = new ZForm(header))
				{
					frm.Controls.Add(new DeclarationDetailsTabUserControl());
					frm.Show();

					Application.DoEvents();

					var brokerFindBox = (ZCodeFindBox)frm.Controls.Find("BrokerFindBox", true).First();

					AssertEquals(false, brokerFindBox.IsVisibleForBinding);
					AssertEquals(false, brokerFindBox.IsDisposed);
				}
			}
		}

		[RequiresSTA]
		public void TestVisibilityTIRData()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
			Factory.Save();

			using (var frm = new ZForm(header))
			{
				frm.Controls.Add(new DeclarationDetailsTabUserControl());
				frm.Show();

				CombineAssertions(() =>
				{
					var tirCarnetNumberTextBox = (ZTextBox)frm.Controls.Find("TirCarnetNumberTextBox", true).Single();
					var tirCarnetExpiryDateEdit = (ZDateEdit)frm.Controls.Find("TirCarnetExpiryDateEdit", true).Single();
					AssertEquals("TirCarnetNumber is invisible initially.", false, tirCarnetNumberTextBox.Visible);
					AssertEquals("TirExpiryDate is invisible initially.", false, tirCarnetExpiryDateEdit.Visible);

					header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
					AssertEquals("TirCarnetNumber is visible when declaration Type is TIR.", true, tirCarnetNumberTextBox.Visible);
					AssertEquals("TirExpiryDate is visible when declaration Type is TIR.", true, tirCarnetExpiryDateEdit.Visible);
				});
			}
		}

		public void TestSealsShouldBeSubclassOfBusinessObjectCollection()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var sealsInfo = header.GetType().GetProperty("Seals", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				?? header.GetType().GetProperty("Seals", BindingFlags.Public | BindingFlags.Instance);
			var sealsType = sealsInfo.PropertyType;
			Assert(TypeUtilities.IsSubclassOfBusinessObjectCollection(sealsType));
		}

		[RequiresSTA]
		public void TestLrnTextBoxAllowsNormalCase()
		{
			using (var control = new DeclarationDetailsTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("LrnTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestMeansOfTransportAtDepartureIdentityTextBoxAllowsNormalCase()
		{
			using (var control = new DeclarationDetailsTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("MeansOfTransportAtDepartureIdentityTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestMeansOfTransportCrossingBorderIdentityTextBoxAllowsNormalCase()
		{
			using (var control = new DeclarationDetailsTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("MeansOfTransportCrossingBorderIdentityTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestAgreedLocationOfGoodsNormalTextBoxAllowsNormalCase()
		{
			using (var control = new DeclarationDetailsTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("AgreedLocationOfGoodsNormalTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestCustomsSubPlaceNormalTextBoxAllowsNormalCase()
		{
			using (var control = new DeclarationDetailsTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("CustomsSubPlaceNormalTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestCustomsOfficesZDynamicUserControl()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new ZForm(header))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var customsOfficesZDynamicUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("CustomsOfficesZDynamicUserControl");
				AssertType<CustomsOfficesUserControl>(customsOfficesZDynamicUserControl.HostedControl);
			}
		}

		public void TestContainersAndSealsZDynamicUserControl()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new ZForm(header))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var customsOfficesZDynamicUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("ContainersAndSealsZDynamicUserControl");
				AssertType<ContainersAndSealsUserControl>(customsOfficesZDynamicUserControl.HostedControl);
			}
		}

		public void TestPortOfDispatchIsVisible()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			nctsConfigurationMock.Protected().Setup<ZBool>("FullLoadPortSupportCore", ItExpr.IsAny<bool>()).Returns(new ZBool(true)).Verifiable();
			header.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(nctsConfigurationMock.Object);

			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				using (var frm = new ZForm(header))
				{
					frm.Controls.Add(new DeclarationDetailsTabUserControl());
					frm.Show();

					CombineAssertions(() =>
					{
						var portDispatchFindBox = (ZCodeFindBox)frm.Controls.Find("PortOfDispatchFindBox", true).Single();
						AssertEquals("portDispatchFindBox is visible when FullLoadPortSupportCore = true.", true, portDispatchFindBox.Visible);

						var countryOfDispatchDropEdit = (ZDropEdit)frm.Controls.Find("CountryOfDispatchDropEdit", true).Single();
						AssertEquals("countryOfDispatchDropEdit is invisible when FullLoadPortSupportCore = true.", false, countryOfDispatchDropEdit.Visible);
					});
				}
			}
		}

		public void TestCountryOfDispatchIsVisible()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			nctsConfigurationMock.Protected().Setup<ZBool>("FullLoadPortSupportCore", ItExpr.IsAny<bool>()).Returns(new ZBool(false)).Verifiable();
			header.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(nctsConfigurationMock.Object);

			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				using (var frm = new ZForm(header))
				{
					frm.Controls.Add(new DeclarationDetailsTabUserControl());
					frm.Show();

					CombineAssertions(() =>
					{
						var portDispatchFindBox = (ZCodeFindBox)frm.Controls.Find("PortOfDispatchFindBox", true).Single();
						AssertEquals("portDispatchFindBox is invisible when FullLoadPortSupportCore = false.", false, portDispatchFindBox.Visible);

						var countryOfDispatchDropEdit = (ZDropEdit)frm.Controls.Find("CountryOfDispatchDropEdit", true).Single();
						AssertEquals("countryOfDispatchDropEdit is visible when FullLoadPortSupportCore = false.", true, countryOfDispatchDropEdit.Visible);
					});
				}
			}
		}

		public void TestValuationDateDateEdit()
		{
			using (var control = new DeclarationDetailsTabUserControl())
			{
				var valuationDateDateEdit = control.FindSingle<ZDateEdit>("ValuationDateDateEdit");
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_ValuationDate), valuationDateDateEdit.GetBindingMember());
			}
		}

		public void TestValuationDateEnabled()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();

			using (var frm = new ZForm(header))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var valuationDateDateEdit = control.FindSingle<ZDateEdit>("ValuationDateDateEdit");
				AssertEquals("ValuationDateDateEdit enabled when MRN not set", true, valuationDateDateEdit.Enabled);
			}

			var mrn = header.MovementReferenceEntryNumber;
			mrn.CE_EntryNum = "1234";
			Factory.Save();

			using (var frm = new ZForm(header))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var valuationDateDateEdit = control.FindSingle<ZDateEdit>("ValuationDateDateEdit");
				AssertEquals("ValuationDateDateEdit not enabled after MRN set", false, valuationDateDateEdit.Enabled);
			}
		}
	}
}
