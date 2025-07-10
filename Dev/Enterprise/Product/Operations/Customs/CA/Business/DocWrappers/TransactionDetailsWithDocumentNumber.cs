using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.MessageProcessors;

namespace Enterprise.Customs.CA.Business;

[TestExcludeBusinessObjectsAllHaveTestCases]
public abstract class TransactionDetailsWithDocumentNumber : NonPersistentBusinessObject
{
	internal TransactionDetailsWithDocumentNumber(BusinessObjectFactory factory) : base(factory)
	{
	}

	#region Properties

	public virtual ZString JobNumber
	{
		get
		{
			var result = ZString.Empty;
			var declarationLink = Declaration != null ? EmailDefBuilder.GetJobLink(Declaration, Declaration.JE_DeclarationReference) : string.Empty;
			var relatedB3DeclarationLink = RelatedB3Declaration != null ? EmailDefBuilder.GetJobLink(RelatedB3Declaration, RelatedB3Declaration.JE_DeclarationReference) : string.Empty;
			if (!string.IsNullOrEmpty(declarationLink) && !string.IsNullOrEmpty(relatedB3DeclarationLink))
			{
				result = declarationLink + " / " + relatedB3DeclarationLink;
			}
			else
			{
				result = declarationLink + relatedB3DeclarationLink;
			}
			return result;
		}
	}

	public abstract ZString B3RelatedDocumentNumber { get; }

	public abstract ZString DocumentNumber { get; }

	public JobDeclaration Declaration
	{
		get
		{
			if (declaration == null && !DocumentNumber.IsEmpty)
			{
				declaration = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(Factory, DocumentNumber, ZString.Empty);
			}
			return declaration;
		}
	}
	JobDeclaration declaration;

	public JobDeclaration RelatedB3Declaration
	{
		get
		{
			if (relatedB3Declaration == null && !B3RelatedDocumentNumber.IsEmpty)
			{
				relatedB3Declaration = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(Factory, B3RelatedDocumentNumber, ZString.Empty);
			}
			return relatedB3Declaration;
		}
	}
	JobDeclaration relatedB3Declaration;

	#endregion
}
