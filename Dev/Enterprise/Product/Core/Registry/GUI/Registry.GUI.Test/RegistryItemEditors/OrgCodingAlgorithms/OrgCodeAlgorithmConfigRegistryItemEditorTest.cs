using System;
using System.Windows.Forms;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgCodeAlgorithmConfigRegistryItemEditor))]
	sealed class OrgCodeAlgorithmConfigRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new OrgCodeAlgorithmConfigRegistryItemEditor(new OrgCodeAlgorithmRegistryDataType(OrgCodeAlgorithmType.Default), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OrgCodeAlgorithmConfigControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrgCodeAlgorithmConfigControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OrgCodeAlgorithmRegistryItem("", null, null, null, (OrgCodeAlgorithm)GetValidRegistryValues()[0]);
		}

		protected override object[] GetValidRegistryValues()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 1;
			return new object[] { algorithm };
		}
	}
}
