using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class UpdateImportEntryNumberObject : NonPersistentBusinessObject
	{
		public UpdateImportEntryNumberObject(JobDeclaration declaration) : base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public static class Schema
		{
			public const string EntryNumber = "EntryNumber";
			public const string RegistrationDate = "RegistrationDate";
		}

		public JobDeclaration Declaration
		{
			get => declaration;
			private set
			{
				declaration = value;
				entryHeader = Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
				fEntryNumber = entryHeader?.MovementReferenceNumber ?? ZString.Empty;
				fRegistrationDate = entryHeader?.MovementReferenceNumberIssueDate ?? ZDateTime.Empty;
			}
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;

		public UpdateImportEntryNumberValidation Validation
		{
			get { return new UpdateImportEntryNumberValidation(this); }
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("a8a58314-224a-40c6-9aa9-d03eb28c08b0", "Update Entry Number");

		[MaxLength(10)]
		[ResourceStringData("Enterprise.Customs.BR.Business.UpdateImportEntryNumberObject|EntryNumber", Caption = "Entry Number", FullDescription = "The Entry Number.")]
		public ZString EntryNumber
		{
			get => fEntryNumber;
			set
			{
				SetNonPersistentPropertyValue(EntryNumberInfo, ref fEntryNumber, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryNumber();
				}
			}
		}

		ZString fEntryNumber;

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(Schema.EntryNumber);

		[ResourceStringData("Enterprise.Customs.BR.Business.UpdateImportEntryNumberObject|RegistrationDate", Caption = "Registration Date", FullDescription = "The Entry Number Registration Date.")]
		public ZDateTime RegistrationDate
		{
			get => fRegistrationDate;
			set
			{
				SetNonPersistentPropertyValue(RegistrationDateInfo, ref fRegistrationDate, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRegistrationDate();
				}
			}
		}

		ZDateTime fRegistrationDate;

		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		public void UpdateEntryNumber()
		{
			if (entryHeader != null)
			{
				entryHeader.MovementReferenceNumberSetter(EntryNumber, RegistrationDate);
				entryHeader.EntryNumberInfo.RefreshBinding();
				entryHeader.MovementReferenceNumberIssueDateInfo.RefreshBinding();
				Declaration.DeclarationNumberInfo.RefreshBinding();

				try
				{
					Declaration.Factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
	}
}

