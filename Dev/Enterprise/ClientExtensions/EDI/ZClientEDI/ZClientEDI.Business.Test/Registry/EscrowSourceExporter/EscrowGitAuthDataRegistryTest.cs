using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ZClientEDI.Business.Registry;

namespace Enterprise.Client.EDI.Test;

partial class EDIDataRegistryTest
{
	public void TestEscrowGitHubAppId()
	{
		TestRegistryItem(
			ItemSet.EscrowGitHubAppId,
			"EscrowGitHubAppId",
			"WiseTech Global Client Extensions/Escrow Source Exporter/Git Authentication",
			"Escrow Github Application AppId",
			"The Id of the GitHub App used by EscrowExportSourceCodeServiceTask to clone code repositories from GitHub.",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			TextEditorType.TextBox,
			string.Empty);
	}

	public void TestEscrowGitHubAppPrivateKey()
	{
		TestRegistryItem(ItemSet.EscrowGitHubAppPrivateKey,
			"EscrowGitHubAppPrivateKey",
			"WiseTech Global Client Extensions/Escrow Source Exporter/Git Authentication",
			"Escrow Github Application Private Key",
			"The private key of the Github App used by EscrowExportSourceCodeServiceTask to clone code repositories from GitHub.",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			Array.Empty<byte>());

		AssertEquals(typeof(CWSupportLoginTokenPrivateKeyEditorInfo), ItemSet.CWSupportLoginTokenPrivateKey.EditorInfo.GetType());
	}

	public void TestEscrowDevOpsToken()
	{
		TestRegistryItem(
			ItemSet.EscrowDevOpsToken,
			"EscrowDevOpsToken",
			"WiseTech Global Client Extensions/Escrow Source Exporter/Git Authentication",
			"DevOps Personal Access Token",
			"The personal access token used by EscrowExportSourceCodeServiceTask to clone code repositories from DevOps.",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			TextEditorType.Password,
			string.Empty);
	}
}

