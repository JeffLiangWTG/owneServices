using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class CusEntryInstructionJobDocAddressValidation : JobDocAddressValidation
	{
		public CusEntryInstructionJobDocAddressValidation(JobDocAddress address, CusEntryInstruction entryInstruction)
			: base(address)
		{
			this.entryInstruction = entryInstruction;
		}

		readonly CusEntryInstruction entryInstruction;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			ValidateE2_Contact();
		}

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();

			if (Parent.E2_AddressType == DocAddressTypes.Codes.JustificationContactDetailAddress && entryInstruction.IsJustificationContactDetailAddressAvailable && Parent.OrganisationPK.IsValid)
			{
				var targetInfo = Parent.E2_ContactInfo;
				if (Parent.E2_Contact.Length > 100)
				{
					targetInfo.AddMessageError(Res.GetString("5b614f8e-44dd-43dc-ad15-fdb0849625d3", "Contact Name should have a maximum length of 100 characters."));
				}

				if (Parent.E2_Email.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("9998948C-99D0-4B84-80D7-149335EFB1C2", "You have entered a Justification Organization. Contact's Email is required"));
				}
				else if (Parent.E2_Email.Length > 77)
				{
					targetInfo.AddMessageError(Res.GetString("cd9fb7d0-322c-40d8-9932-cd6a753c8897", "Contact E-mail should have a maximum length of 77 characters."));
				}

				if (Parent.E2_Phone.IsEmpty && Parent.E2_Mobile.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("5E3F0FAA-DD39-480B-B771-C82D14F6985B", "You have entered a Justification Organization. Contact's Mobile or Contact Phone is required"));
				}
			}
		}
	}
}
