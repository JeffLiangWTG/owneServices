using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAddInfoInvHeaderValidation : AUAddInfoHeaderValidation
	{
		public CMRAddInfoInvHeaderValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		#region Add Info Fields That Are Not Allowed At Header Level

		protected override void CheckZA_DCX()
		{
			base.CheckZA_DCX();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_DCXInfo);
		}

		protected override void CheckZA_DXT()
		{
			base.CheckZA_DXT();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_DXTInfo);
		}

		protected override void CheckZA_ELA()
		{
			base.CheckZA_ELA();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_ELAInfo);
		}

		protected override void CheckZA_FOD()
		{
			base.CheckZA_FOD();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_FODInfo);
		}

		protected override void CheckZA_ISC()
		{
			base.CheckZA_ISC();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_ISCInfo);
		}

		protected override void CheckZA_LCP()
		{
			base.CheckZA_LCP();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_LCPInfo);
		}

		protected override void CheckZA_WMC()
		{
			base.CheckZA_WMC();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_WMCInfo);
		}

		protected override void CheckZA_TRN()
		{
			base.CheckZA_TRN();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_TRNInfo);
		}

		protected override void CheckZA_PRI()
		{
			base.CheckZA_PRI();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_PRIInfo);
		}

		protected override void CheckZA_PST()
		{
			base.CheckZA_PST();
			if (!IsInPreSaveValidation)
			{
				foreach (JobComInvoiceLine line in Parent.InvoiceHeader.JobComInvoiceLines)
				{
					line.AddInfo.Validation.ValidateZA_PST();
				}
			}
		}

		protected override void CheckZA_TCI()
		{
			base.CheckZA_TCI();
			ValidateLineLevelPropertyDoesntHaveNonDefaultValue(AddInfo.ZA_TCIInfo);
		}

		#endregion

		protected override void CheckZA_VALB_Hidden()
		{
			base.CheckZA_VALB_Hidden();
			if (!IsInPreSaveValidation)
			{
				foreach (JobComInvoiceLine line in Parent.InvoiceHeader.JobComInvoiceLines)
				{
					line.AddInfo.Validation.ValidateZA_VALB_Hidden();
				}
			}
		}

		protected override void CheckZA_ORG()
		{
			base.CheckZA_ORG();
			if (AddInfo.ZA_ORG.IsEmpty)
			{
				JobComInvoiceHeader invoice = AddInfo.Parent as JobComInvoiceHeader;
				if (invoice != null && invoice.JobDeclaration.IsSACWithLines && Parent.Parent is JobComInvoiceHeader)
				{
					AddInfo.ZA_ORGInfo.AddMessageError("You have indicated that this entry is SAC with lines and Origin is mandatory.");
				}
			}
		}

		protected override void CheckZA_HeaderREL_Hidden()
		{
			base.CheckZA_HeaderREL_Hidden();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_HeaderREL_HiddenInfo, Lookups.ZA_HeaderREL_List);
		}

		protected override void CheckZA_PackCountForBond_Hidden()
		{
			base.CheckZA_PackCountForBond_Hidden();
			if (Parent.ZA_PackCountForBond_Hidden > 0 && !Parent.InvoiceHeader.JobComInvoiceLines.HasNature20)
			{
				Parent.ZA_PackCountForBond_HiddenInfo.AddMessageError("You have entered a pack count to a bond warehouse. Please enter an invoice line that is bonded.");
			}
		}

		protected override void CheckZA_EFD()
		{
			base.CheckZA_EFD();
			ValidatePropertyDoesNotCauseMultipleN20OrN30Entries(AggregateAddInfoParent as JobComInvoiceHeader, Parent.ZA_EFDInfo);
		}

		protected override void CheckZA_VAN()
		{
			base.CheckZA_VAN();
			ValidatePropertyDoesNotCauseMultipleN20OrN30Entries(AggregateAddInfoParent as JobComInvoiceHeader, Parent.ZA_VANInfo);
		}

		void ValidatePropertyDoesNotCauseMultipleN20OrN30Entries(JobComInvoiceHeader invoice, ZPropertyInfo info)
		{
			var declaration = JobDeclaration;
			if (invoice != null && declaration.IsImport && declaration.SupportsBondedWarehousing && (declaration.IsExWarehouse || declaration.HasLineGoingIntoABondedWarehouse))
			{
				var value = info.Value;
				var otherInvoice = declaration.Invoices.Cast<JobComInvoiceHeader>().FirstOrDefault(x => x.PK != invoice.PK && !value.Equals(x.AddInfo[info.Name]));
				if (otherInvoice != null)
				{
					info.AddError(string.Format("This value ({0}) is not the same value as the value ({1}) on invoice ({2}); Inventory Management Integration requires that all invoices with goods for warehousing should have the same value.", value, otherInvoice.AddInfo[info.Name], otherInvoice.JZ_InvoiceNumber));
				}
			}
		}

		protected new AUAddInfo Parent
		{
			get { return base.Parent; }
		}

		bool IsInPreSaveValidation
		{
			get { return ((IBusinessObjectInternals)Parent).IsInPreSaveValidation; }
		}
	}
}
