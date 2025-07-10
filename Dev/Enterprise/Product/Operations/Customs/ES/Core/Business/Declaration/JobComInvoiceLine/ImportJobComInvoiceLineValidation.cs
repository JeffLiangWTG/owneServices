using CargoWise.EntityFramework;
using static Enterprise.Customs.ES.Messaging.MessageSchema;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckNoPreviousDocuments();
		}

		void CheckNoPreviousDocuments()
		{
			if (IsImportEntry && Parent.EntryHeaderValidationModeIsImportNoneOrPDS && !Parent.PreviousDocuments.Any() && !Parent.InvoiceHeader.PreviousDocuments.Any() && !Parent.Declaration.PreviousDocuments.Any())
			{
				Parent.AddRowWarning(Res.GetString("87ED4D84-CB2F-4397-9267-20F50DC53529", "If you don't send [40] Previous Documents, the declaration will be a Complete Import Pre-declaration."));
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (IsImportEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
				if (Parent.JI_Description.Length > JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l)
				{
					Parent.JI_DescriptionInfo.AddWarning(Res.GetString("AD0E212A-19F9-40B9-B691-6A8AC62BC079", "When sending Import declarations, the maximum length accepted for the description is {0}, so it will be trimmed", JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l));
				}
			}
			if (IsT2LReception && Parent.JI_Description.Length > JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l)
			{
				Parent.JI_DescriptionInfo.AddWarning(Res.GetString("5156E1D4-7CB3-40C9-A261-3BC4987EEE2C", "When sending T2L Reception declarations, the maximum length accepted for the description is {0}, so it will be trimmed", JobComInvoiceLineMessageSchema.GoodsDescriptionMaxLengthImportOrT2l));
			}
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();
			if (IsImportEntry && Parent.EntryHeaderValidationModeIsImportNoneOrPDS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PrimaryPreferenceInfo);
			}
		}

		protected override void CheckJI_CountryOfOriginMandatoryValidation()
		{
			if (IsImportEntry)
			{
				base.CheckJI_CountryOfOriginMandatoryValidation();
			}
		}

		protected override bool IsJIProcedureMandatory => IsImportEntry;

		bool IsT2LReception => EntryInstruction?.IsT2L ?? false;

		bool IsImportEntry => !(IsT2LReception || (EntryInstruction?.IsT2C ?? false));
	}
}
