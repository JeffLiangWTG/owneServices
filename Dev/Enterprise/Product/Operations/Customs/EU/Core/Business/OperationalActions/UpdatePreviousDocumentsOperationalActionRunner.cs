using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.Business.OperationalActions
{
	public class UpdatePreviousDocumentsOperationalActionRunner
	{
		public void UpdatePreviousDocuments(DeclarationUpdatePreviousDocumentsApplicator applicator, JobDeclaration declaration, IOperationalActionSectionLog log)
		{
			var documentCode = applicator.DocumentCode;
			if (!documentCode.IsEmpty)
			{
				var notSuitMessage = Res.GetString("5A14C1D5-C723-4D42-B05C-E760C273860A", "Previous document code {0} doesn't suit declaration {1}.");
				var previousDocuments = declaration.PreviousDocuments;
				if (!previousDocuments.Any())
				{
					var newpreviousDocuments = previousDocuments.AddNew();
					newpreviousDocuments.CSI_Code = documentCode;
					if (newpreviousDocuments.CSI_CodeInfo.HasMessageErrors())
					{
						newpreviousDocuments.Delete();
						log.NotifyFormat(OperationalActionLogErrorLevel.Error, notSuitMessage, documentCode, declaration.JE_DeclarationReference);
					}
					else
					{
						newpreviousDocuments.CSI_ReferenceNumber = applicator.ReferenceNumber;
						newpreviousDocuments.CSI_SubType = applicator.Class;
						newpreviousDocuments.CSI_LineNo = applicator.LineNo;
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("114EF833-B59F-4900-9E89-3998478A6CE1", "Added previous document {0} with reference {1} on declaration {2}."), documentCode, applicator.ReferenceNumber, declaration.JE_DeclarationReference);
					}
				}
				else if (applicator.OverrideExisitingDocument)
				{
					foreach (Declaration.MultiLineAddInfos.PreviousDocument previousDocument in previousDocuments)
					{
						var oldCode = previousDocument.CSI_Code;
						previousDocument.CSI_Code = documentCode;
						if (previousDocument.CSI_CodeInfo.HasMessageErrors())
						{
							previousDocument.CSI_Code = oldCode;
							log.NotifyFormat(OperationalActionLogErrorLevel.Error, notSuitMessage, documentCode, declaration.JE_DeclarationReference);
						}
						else
						{
							previousDocument.CSI_ReferenceNumber = applicator.ReferenceNumber;
							previousDocument.CSI_SubType = applicator.Class;
							previousDocument.CSI_LineNo = applicator.LineNo;
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("1E65A878-91A4-4380-A754-9194B8BB8D14", "Overrode previous document {0} with reference {1} on declaration {2}."), documentCode, applicator.ReferenceNumber, declaration.JE_DeclarationReference);
						}
					}
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("17631B25-31A0-4D3A-8EEF-45A7F2129156", "A previous document already exists on declaration {0}."), declaration.JE_DeclarationReference);
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("FE93241C-6E1C-4EB1-A786-88D05F4E23B8", "Skipped previous document with empty code."));
			}
		}
	}
}
