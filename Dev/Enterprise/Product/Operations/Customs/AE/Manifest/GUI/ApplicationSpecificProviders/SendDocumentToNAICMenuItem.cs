using System;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AE.Manifest.GUI;

public sealed class SendDocumentToNAICMenuItem(EventHandler onClickHandler) : eDocMenuItem(ResString.GetMultilingualString("F0D31923-A15E-4472-8580-E0549ADA8397", "Send this document to the NAIC"), onClickHandler)
{
	protected override bool IsDependentOnGridEditable => false;

	protected override bool IsDependentOnViewable => true;

	public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
	{
		Func<bool> elementNotDeleted = () => selectedElement != null && !selectedElement.SC_IsDeleted;
		Func<bool> isSavedInDatabase = () => Globals.IsTest || selectedElement.IsInDatabase;

		return !multipleSelected && elementNotDeleted() && isSavedInDatabase();
	}
}
