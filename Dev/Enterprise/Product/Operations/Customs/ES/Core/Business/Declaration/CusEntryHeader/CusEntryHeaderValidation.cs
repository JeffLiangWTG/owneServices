using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEntryHeaderValidation : EU.Business.Declaration.CusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(EU.Business.Declaration.CusEntryHeader parent) : base(parent)
		{
		}

		public new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckT2LReceptionEntry();
			CheckAnnexesInEntry();
			ValidateMovementReferenceNumber();
		}

		static string T2LReceptionEntryError(bool isPOUS2) =>
			isPOUS2
			? ResString.GetMultilingualString("CD9B6F52-11FD-4D67-8B01-39E68420C86F", "Cannot send T2L Reception message without MRN; please enter those fields under the Entries grid")
			: ResString.GetMultilingualString("09DE8485-98F3-4C28-8DB6-D1420E98F3B6", "Cannot send T2L Reception message without MRN or Issue Date; please enter those fields under the Entries grid");

		void CheckT2LReceptionEntry()
		{
			var isPOUS2 = Parent.ZG_POUSVersion > 1;

			Parent.RemoveRowMessageError(T2LReceptionEntryError(isPOUS2));
			if (Parent.IsT2L && Parent.IsImport && (isPOUS2 ? Parent.MovementReferenceNumber.IsEmpty : !Parent.HasMRNAndIssueDate))
			{
				Parent.AddRowMessageError(T2LReceptionEntryError(isPOUS2));
			}
		}

		static string AnnexesInEntryError => ResString.GetMultilingualString("D063D6EA-9522-4E03-BE7C-8D9DDFC23242", "At least one annex should be added when sending a message");

		void CheckAnnexesInEntry()
		{
			Parent.RemoveRowMessageError(AnnexesInEntryError);
			if ((Parent.RequiresT2LAnnexes() || Parent.RequiresAESAnnexes || Parent.RequiresH1Annexes) && !Parent.HasAnnexes)
			{
				Parent.AddRowMessageError(AnnexesInEntryError);
			}
		}

		static string NoEntryInstructionError => ResString.GetMultilingualString("690FA732-F011-4CE7-A9A5-1ED1D76CA606", "You have not entered an Entry Instruction, please check Inv. Lines.");

		protected override void CheckCH_CEI_Instruction()
		{
			base.CheckCH_CEI_Instruction();

			Parent.RemoveRowMessageError(NoEntryInstructionError);
			var entryInstructionCode = Parent.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
			if (entryInstructionCode.IsEmpty)
			{
				Parent.AddRowMessageError(NoEntryInstructionError);
			}
		}

		public void ValidateMovementReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.MovementReferenceNumberInfo);
		}

		protected void CheckMovementReferenceNumber()
		{
			if ((Parent.EntryInstruction?.IsT2C ?? false) && Parent.MovementReferenceNumber.IsEmpty)
			{
				Parent.MovementReferenceNumberInfo.AddMessageError(NoMovementReferenceNumberT2CError); 
			}
		}

		static string NoMovementReferenceNumberT2CError => ResString.GetMultilingualString("5146718C-F1D6-4290-9E5B-C55EB43CBFCE", "T2L(F) MRN is mandatory");
	}
}
