using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ZClientEDI.Business.Registry;

namespace Enterprise.Client.EDI;

partial class EDIDataRegistry
{
	public StringRegistryItem EscrowGitHubAppId => GetItem("EscrowGitHubAppId", () =>
		new StringRegistryItem(
			"EscrowGitHubAppId",
			(NoResString)EscrowGitAuthDataCategory,
			(NoResString)"Escrow Github Application AppId",
			(NoResString)"The Id of the GitHub App used by EscrowExportSourceCodeServiceTask to clone code repositories from GitHub.",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			string.Empty));

	public BinaryRegistryItem EscrowGitHubAppPrivateKey => GetItem("EscrowGitHubAppPrivateKey", () =>
		new BinaryRegistryItem(
			"EscrowGitHubAppPrivateKey",
			(NoResString)EscrowGitAuthDataCategory,
			(NoResString)"Escrow Github Application Private Key",
			(NoResString)"The private key of the Github App used by EscrowExportSourceCodeServiceTask to clone code repositories from GitHub.",
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			Array.Empty<byte>())
		{
			EditorInfo = new CWSupportLoginTokenPrivateKeyEditorInfo()
		});

	public StringRegistryItem EscrowDevOpsToken => GetItem("EscrowDevOpsToken", () =>
		new StringRegistryItem(
			"EscrowDevOpsToken",
			(NoResString)EscrowGitAuthDataCategory,
			(NoResString)"DevOps Personal Access Token",
			(NoResString)"The personal access token used by EscrowExportSourceCodeServiceTask to clone code repositories from DevOps.",
			new StringRegistryDataType(CharacterCase.Normal),
			new TextRegistryEditorInfo(TextEditorType.Password),
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			string.Empty));

	const string EscrowGitAuthDataCategory = EscrowExporterCategory + "/Git Authentication";
}
