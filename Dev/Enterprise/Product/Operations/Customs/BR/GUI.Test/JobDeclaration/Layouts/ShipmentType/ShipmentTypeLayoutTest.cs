using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayout))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		public void TestIsMultimodalCheckBoxVisibility()
		{
			var subTypesList = declaration.Lookups.MessageSubTypeList.GetAllCodes();
			var subTypesMultimodalNotAvailable = new string[] { MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15, MessageSubTypeList.Codes._16, MessageSubTypeList.Codes._17, MessageSubTypeList.Codes._18, MessageSubTypeList.Codes._20, MessageSubTypeList.Codes._21 };
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_MessageSubType = MessageSubTypeList.Codes._02;
				AssertEquals($"IsMultimodalCheckBox NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.Export} and MessageSubType = {MessageSubTypeList.Codes._13}", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				foreach (var subType in subTypesList)
				{
					declaration.JE_MessageSubType = subType;
					if (subTypesMultimodalNotAvailable.Contains(subType))
					{
						AssertEquals($"IsMultimodalCheckBox NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.Import}, TransportMode = {TransportTypeList.Codes.Sea} and MessageSubType = {subType}", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));
					}
					else
					{
						AssertEquals($"IsMultimodalCheckBox Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.Import}, TransportMode = {TransportTypeList.Codes.Sea} and MessageSubType = {subType}", true, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));
					}
				}

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				foreach (var subType in subTypesList)
				{
					declaration.JE_MessageSubType = subType;
					if (subTypesMultimodalNotAvailable.Contains(subType))
					{
						AssertEquals($"IsMultimodalCheckBox NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex}, TransportMode = {TransportTypeList.Codes.Sea} and MessageSubType = {subType}", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));
					}
					else
					{
						AssertEquals($"IsMultimodalCheckBox Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex}, TransportMode = {TransportTypeList.Codes.Sea} and MessageSubType = {subType}", true, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));
					}
				}

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				declaration.JE_MessageSubType = MessageSubTypeList.Codes._02;
				AssertEquals($"IsMultimodalCheckBox NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.ImportLicense} and MessageSubType = {MessageSubTypeList.Codes._13}", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Own;
				declaration.JE_MessageSubType = MessageSubTypeList.Codes._02;
				AssertEquals($"IsMultimodalCheckBox NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.Export} and MessageSubType = {MessageSubTypeList.Codes._13}", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = MessageSubTypeList.Codes._02;
				AssertEquals($"IsMultimodalCheckBox NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.Import} and MessageSubType = {MessageSubTypeList.Codes._13}", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				declaration.JE_MessageSubType = MessageSubTypeList.Codes._02;
				AssertEquals($"IsMultimodalCheckBox NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.ImportLicense} and MessageSubType = {MessageSubTypeList.Codes._13}", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, declaration));
			});
		}

		public void TestSpecialTransportDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("SpecialTransportDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.SpecialTransportDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("SpecialTransportDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.SpecialTransportDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("SpecialTransportDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.SpecialTransportDropEdit, declaration));
			});
		}

		public void TestMessageSubTypeDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("MessageSubTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("MessageSubTypeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("MessageSubTypeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("MessageSubTypeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, true, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, declaration));
			});
		}

		public void TestDeclarantTypeDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("TypeOfOperationExportDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, true, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("TypeOfOperationExportDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("TypeOfOperationExportDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("TypeOfOperationExportDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));
			});
		}

		public void TestAlternativeDeclarantTypeDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("DeclarantTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("DeclarantTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("DeclarantTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("DeclarantTypeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, declaration));
			});
		}

		public void TestOperationTypeDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("OperationTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.OperationTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("OperationTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.OperationTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("OperationTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.OperationTypeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("OperationTypeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.OperationTypeDropEdit, declaration));

				declaration.DeclarantType = DeclarantTypeList.Codes.DoorToDoor;
				AssertEquals("OperationTypeDropEdit NOT Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex + " and if DeclarantType " + DeclarantTypeList.Codes.DoorToDoor, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.OperationTypeDropEdit, declaration));

				declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;
				AssertEquals("OperationTypeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex + " and if DeclarantType " + DeclarantTypeList.Codes.LegalPerson, true, Layout.IsVisible(ShipmentTypeControlBag.Instance.OperationTypeDropEdit, declaration));

				declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;
				AssertEquals("OperationTypeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex + " and if DeclarantType " + DeclarantTypeList.Codes.DiplomaticMission, false, Layout.IsVisible(ShipmentTypeControlBag.Instance.OperationTypeDropEdit, declaration));
			});
		}

		public void TestDispatchModalityDropEditVisibility()
		{
			var subTypesList = declaration.Lookups.MessageSubTypeList.GetAllCodes();
			var subTypesNotAvailable = new[] { MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15, MessageSubTypeList.Codes._16, MessageSubTypeList.Codes._17,
				MessageSubTypeList.Codes._18, MessageSubTypeList.Codes._19, MessageSubTypeList.Codes._20, MessageSubTypeList.Codes._21 }.ToList();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				foreach (var subType in subTypesList)
				{
					declaration.JE_MessageSubType = subType;
					if (subTypesNotAvailable.Contains(subType))
					{
						Assert($"DispatchModalityDropEdit NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex} and MessageSubType = {subType}", !Layout.IsVisible(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration));
					}
					else
					{
						Assert($"DispatchModalityDropEdit Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex} and MessageSubType = {subType}", Layout.IsVisible(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration));
					}
				}

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				Assert($"DispatchModalityDropEdit NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.Export}", !Layout.IsVisible(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.LPCO;
				Assert($"DispatchModalityDropEdit NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.LPCO}", !Layout.IsVisible(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				Assert($"DispatchModalityDropEdit Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.Import}", Layout.IsVisible(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				Assert($"DispatchModalityDropEdit NOT Visible, if MessageType = {Common.BR.BRJobMessageTypeList.Codes.ImportLicense}", !Layout.IsVisible(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration));
			});
		}
		public void TestDispatchModalityDropEditCaption()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				Layout.TryGetCaption(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration, out var resourceStringData);
				AssertEquals("DispatchModalityDropEdit Caption, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, "Dispatch Modality", resourceStringData.Caption);
				AssertEquals("DispatchModalityDropEdit FullDescription, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Import, "The Special Dispatch Modality.", resourceStringData.FullDescription);

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
				Layout.TryGetCaption(ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, declaration, out resourceStringData);
				AssertEquals("DispatchModalityDropEdit Caption, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, "Dispatch Modality", resourceStringData.Caption);
				AssertEquals("DispatchModalityDropEdit FullDescription, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, "Modality Adopted for Customs Clearance.", resourceStringData.FullDescription);
			});
		}

		public void TestContainerModeDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("ContainerModeDropEdit Visible, if messagetype " + Common.BR.BRJobMessageTypeList.Codes.Export, true, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("ContainerModeDropEdit Visible, if MessageType " + Common.BR.BRJobMessageTypeList.Codes.Import + " and TransportMode " + TransportTypeList.Codes.Sea, true, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("ContainerModeDropEdit NOT Visible, if MessageType " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Own;
				AssertEquals("ContainerModeDropEdit NOT Visible, if MessageType " + Common.BR.BRJobMessageTypeList.Codes.Export, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("ContainerModeDropEdit NOT Visible, if MessageType " + Common.BR.BRJobMessageTypeList.Codes.Import + " and TransportMode " + TransportTypeList.Codes.Own, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));

				declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("ContainerModeDropEdit NOT Visible, if MessageType " + Common.BR.BRJobMessageTypeList.Codes.ImportLicense, false, Layout.IsVisible(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstAndOnlyColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		IEnumerable<(ControlReference, ControlWidthClass)> FirstAndOnlyColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.OperationTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.BRTransportModeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.IsMultimodalCheckBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.SpecialTransportDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.DispatchModalityDropEdit, ControlWidthClass.Long);
			}
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ShipmentTypeLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<JobDeclaration>();

		PanelLayout layout;
	}
}
