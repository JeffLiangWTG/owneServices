using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.FetchStrategies
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration BusinessObject => (JobDeclaration)base.BusinessObject;

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(GuaranteeForDeclaration), new ZQuery(CusBondDetailSchema.PW_ParentID, BusinessObject.PK));
			Factory.AddFetchHint(typeof(CusEntryInstruction), new ZQuery(CusEntryInstructionSchema.CEI_JE, BusinessObject.PK));
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(typeof(CusEntryInstruction), new ZQuery(CusEntryInstructionSchema.CEI_JE, BusinessObject.PK));
			var declarantAddress = BusinessObject.DeclarantAddress;
			if (declarantAddress != null)
			{
				Factory.AddFetchHint(typeof(OrgHeader), declarantAddress.OA_OH);
			}
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.BaseJobComInvoiceHeader invoice)
		{
			base.AddMergeFetchHintsFor(invoice);
			Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoice.PK);
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, invoiceLine.PK);
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, invoiceLine.PK);
			Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, invoiceLine.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, invoiceLine.PK);
			Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
		}

		protected override void FetchForMergeCore()
		{
			base.FetchForMergeCore();
			Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, BusinessObject.PK);
		}
	}
}
