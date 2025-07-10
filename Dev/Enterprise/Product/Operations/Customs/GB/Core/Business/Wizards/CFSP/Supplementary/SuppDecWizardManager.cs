using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public class SuppDecWizardManager
	{
		public SuppDecWizardManager(JobDeclaration parentDec)
		{
			parentDeclaration = parentDec;
			SuppDecWizard = new SuppDecWizard(parentDeclaration);
		}

		const string PreviousDocumentOtherCode = "ZZZ";
		public const string SupplementaryDeclarationRelationshipType = "SUP";

		public JobDeclaration CreateAndShowSupplemenaryDeclaration(RelatedDeclarationHelper relatedDeclarationHelper)
		{
			var child = (JobDeclaration)relatedDeclarationHelper.CreateNewRelated(parentDeclaration, SupplementaryDeclarationRelationshipType); // new form is shown here
			child.JE_DeclarationType = SuppDecWizard.DeclarationType;
			child.JE_EntrySubStyle = SuppDecWizard.SupplementaryProcedure;
			child.JE_TotalNoOfPacks = SuppDecWizard.NumberPackagesToDeclare;
			child.JE_MasterBill = ZString.Empty;
			child.JE_HouseBill = ZString.Empty;
			child.ZG_HouseSplitReference = ZString.Empty;
			child.JE_MasterUCR = ZString.Empty;
			var invoice = (child.Invoices.Count == 0) ? child.Invoices.AddNew() : child.Invoices[0];
			var declaration = invoice.JobDeclaration;
			var pd = declaration.PreviousDocuments.AddNew();
			pd.CSI_Code = PreviousDocumentOtherCode;
			pd.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			pd.CSI_ReferenceNumber = parentDeclaration.JE_UCR; // DUCR & top-level part
			return child;
		}

		public ZString ValidateEverything()
		{
			var result = ZString.Empty;
			SuppDecWizard.Validation.ValidateAll();
			SuppDecWizard.RefreshBinding();
			if (SuppDecWizard.HasErrors)
			{
				result = SuppDecWizard.GetErrors().ToUniqueMessageListString(System.Environment.NewLine);
			}
			return result;
		}

		public SuppDecWizard SuppDecWizard { get; private set; }

		readonly JobDeclaration parentDeclaration;
	}
}
