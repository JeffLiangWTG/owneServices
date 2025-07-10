using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.GUI
{
	public static class TransportDocumentHelper
	{
		public static void AddOrCopyTransportDocumentToMisc(JobDeclaration declaration)
		{
			if (declaration.ShouldAddOrCopyHouseBillToTransportDocuments())
			{
				var transportDocumentCode = DocumentHelper.GetDocumentByTransportType(declaration.JE_TransportMode);

				CusSupportingInfo transportDocument = null;
				if (declaration.IsExport || declaration.AreAllEntriesT2LorT2C())
				{
					transportDocument = AdditionalInfoHelper.GetAdditionalInfoByCode(declaration.AdditionalInfos, transportDocumentCode);
				}
				else if(declaration.IsImport && declaration.AreAllEntriesNoT2LNorT2C())
				{
					transportDocument = SupportingDocumentHelper.GetSupportingDocumentByCode(declaration.SupportingDocuments, transportDocumentCode);
				}

				if (transportDocument == null)
				{
					declaration.AddHouseBillTransportDocument();
				}
				else if (!DocumentHelper.IsSameDocumentReference(transportDocument, declaration.JE_HouseBill))
				{
					if (ShouldReplaceTransportDocumentReferenceByJE_HouseBill(declaration, transportDocument))
					{
						transportDocument.CSI_ReferenceNumber = declaration.JE_HouseBill;
					}
				}
			}
		}

		static bool ShouldReplaceTransportDocumentReferenceByJE_HouseBill(JobDeclaration declaration, CusSupportingInfo transportDocument)
		{
			return Globals.Message.Show(
				Res.GetString("06B2C4EE-77EB-4A7F-BD91-E13256DD02FF", @"A Transport Document ({0}) already exists and its Reference Number ""{1}"" does not match the House of Bill ""{2}"" specified.
Do you want to update the existing Transport Document with the new House Bill?", transportDocument.CSI_Code, transportDocument.CSI_ReferenceNumber, declaration.JE_HouseBill),
				Res.GetString("C77316D8-20AD-4C1E-A9B1-D69C817D58F8", "Transport Document"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.No) == DialogResult.Yes;
		}
	}
}
