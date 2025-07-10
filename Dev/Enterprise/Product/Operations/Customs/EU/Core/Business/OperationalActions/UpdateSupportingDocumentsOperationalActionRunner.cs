using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.Business.OperationalActions
{
	public class UpdateSupportingDocumentsOperationalActionRunner
	{
		public void UpdateSupportingDocuments(DeclarationUpdateSupportingDocumentsApplicator applicator, JobDeclaration declaration, IOperationalActionSectionLog log)
		{
			if (!applicator.DocumentCode.IsEmpty)
			{
				var supportingDocuments = declaration.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == applicator.DocumentCode);
				if (!supportingDocuments.Any())
				{
					var newSupportingDocument = declaration.SupportingDocuments.AddNew(applicator.DocumentCode, applicator.ReferenceNumber);
					newSupportingDocument.CSI_DateOfIssue = applicator.Date;
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("BF96FE78-281B-4B25-9069-A434F46CA9AC", "Added supporting document {0} with reference {1} on declaration {2}."), applicator.DocumentCode, applicator.ReferenceNumber, declaration.JE_DeclarationReference);
				}
				else
				{
					foreach (var supportingDocument in supportingDocuments)
					{
						if (applicator.OverrideExisitingDocument)
						{
							supportingDocument.CSI_ReferenceNumber = applicator.ReferenceNumber;
							supportingDocument.CSI_DateOfIssue = applicator.Date;
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("3CA0E6BC-9DE0-47E3-B81C-180438816509", "Updated supporting document {0} with reference {1} on declaration {2}."), applicator.DocumentCode, applicator.ReferenceNumber, declaration.JE_DeclarationReference);
						}
						else
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("A798DCA4-5E11-4059-86A3-1F8281AC5D19", "Skipped updating supporting document {0} on declaration {1} as it already exists."), applicator.DocumentCode, declaration.JE_DeclarationReference);
						}
					}
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("189178B8-BD54-4F24-88DB-726B2175265C", "Skipped supporting document with empty code."));
			}
		}
	}
}
