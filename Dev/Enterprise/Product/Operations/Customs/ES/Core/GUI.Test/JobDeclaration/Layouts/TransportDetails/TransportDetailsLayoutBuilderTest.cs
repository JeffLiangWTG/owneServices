using System;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayoutBuilder))]
	sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder, JobDeclaration, Customs.GUI.TransportDetailsControlBag>
	{
		protected override TransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new TransportDetailsLayoutBuilder();

		protected override int ExpectedMaxColumns => 1;

		public void TestInlandModeOfTransportDropEditVisibility()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, x => false, true);
		}

		public void TestTransportInlandModeAndTypeOfIdUserControlVisibility()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, x => x.IsUCC6AndIsExport);
		}

		public void TestInlandTransportDetailsUserControlVisibility()
		{
			AssertUCC6ControlVisibility(EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, x => x.IsUCC6AndIsExport && !x.IsRailInland && !x.IsRoadInland, true);
		}

		public void TestMasterBillAndIATAUserControlVisibility()
		{
			AssertControlVisibility(TransportDetailsControlBag.Instance.MasterBillAndIATAUserControl, x => x.IsAir);
		}

		public void TestVesselUserControlVisibility()
		{
			AssertControlVisibility(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, x => x.IsSea);
		}

		public void TestTransportInlandRailUserControlVisibility()
		{
			AssertUCC6ControlVisibility(TransportDetailsControlBag.Instance.TransportInlandRailUserControl, x => x.IsRailInland);
		}

		public void TestAdditionalWagonNumbersUserControlVisibility()
		{
			AssertUCC6ControlVisibility(TransportDetailsControlBag.Instance.TransportInlandRailUserControl, x => x.IsRailInland);
		}

		public void TestPortOfLoadingUserControlVisibility()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, x => true);
		}

		void AssertControlVisibility(ControlReference controlReference, Func<JobDeclaration, bool> visible)
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				foreach (var messageType in messageTypesWithoutWAD)
				{
					declaration.JE_MessageType = messageType;
					foreach (var transportMode in allTransportModes)
					{
						declaration.JE_TransportMode = transportMode;
						AssertEquals($"MessageType: {messageType}; TransportMode: {transportMode}", visible(declaration), Layout.IsVisible(controlReference, declaration));
					}
				}
			});
		}

		void AssertUCC6ControlVisibility(ControlReference controlReference, Func<JobDeclaration, bool> visible, bool visibleWhenNotUCC6 = false)
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					AssertEquals("Is Non UCC6", false, declaration.Configuration.IsUCC6(declaration));
					AssertControlVisibility("Non UCC6", (x) => visibleWhenNotUCC6);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertEquals("Is UCC6", true, declaration.Configuration.IsUCC6(declaration));
					AssertControlVisibility("UCC6", visible);
				}
			});

			void AssertControlVisibility(string description, Func<JobDeclaration, bool> isVisible)
			{
				foreach (var transportMode in allTransportModes)
				{
					declaration.JE_TransportMode = transportMode;
					foreach (var inlandTransportMode in allTransportModes)
					{
						declaration.JE_TransportModeInland = inlandTransportMode;
						AssertEquals($"{description}: TransportMode: {transportMode}; InlandTransportMode: {inlandTransportMode}", isVisible(declaration), Layout.IsVisible(controlReference, declaration));
					}
				}
			}
		}

		string[] messageTypesWithoutWAD => new[] { DEJobMessageTypeList.Codes.Import, DEJobMessageTypeList.Codes.Export, DEJobMessageTypeList.Codes.MiscellaneousCustoms };

		string[] allTransportModes => new[]
		{
			TransportModes.Air,
			TransportModes.FixedTransportInstallations,
			TransportModes.InlandWaterwayTransport,
			TransportModes.OwnPropulsion,
			TransportModes.Mail,
			TransportModes.Rail,
			TransportModes.Road,
			TransportModes.Sea,
			string.Empty
		};

		PanelLayout Layout => layout ?? (layout = new TransportDetailsLayout().Layout);
		PanelLayout layout;
	}
}
