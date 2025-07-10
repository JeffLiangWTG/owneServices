using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class UpdateImportEntryNumberValidation : ZValidation
	{
		public UpdateImportEntryNumberValidation(UpdateImportEntryNumberObject updateImportEntryNumber) : base(updateImportEntryNumber)
		{
			parent = updateImportEntryNumber;
		}
		readonly UpdateImportEntryNumberObject parent;

		public override Type AutoValidationType => typeof(UpdateImportEntryNumberValidation);

		public override void ValidateAll()
		{
			ValidateEntryNumber();
			ValidateRegistrationDate();
		}

		public void ValidateEntryNumber()
		{
			ValidateCalculatedProperty(parent.EntryNumberInfo);
		}

		protected void CheckEntryNumber()
		{
			if (!parent.EntryNumber.IsEmpty)
			{
				if (!parent.EntryNumber.IsNumbersOnlyOrEmpty)
				{
					parent.EntryNumberInfo.AddError(Res.GetString("73923752-aaa1-425f-b0ca-f3f2ff16831a", "Entry Number allows numeric characters."));
				}
				else
				{
					var duplicateEntryNumber = parent.Factory.LoadTop1<JobDeclaration>(GetDuplicateEntryNumberQuery(parent.Declaration, parent.EntryNumber));

					if (duplicateEntryNumber != null)
					{
						parent.EntryNumberInfo.AddWarning(Res.GetString("7bfc3d87-a89d-4d44-92e4-f1628ebe7287", "The Entry Number {0} is already contained in the job '{1}'.", parent.EntryNumber, duplicateEntryNumber.JE_DeclarationReference));
					}
				}
			}
		}

		public void ValidateRegistrationDate()
		{
			ValidateCalculatedProperty(parent.RegistrationDateInfo);
		}

		protected void CheckRegistrationDate()
		{
			TypeValidation.CheckValidSmallDateTime(parent.RegistrationDateInfo);
		}

		ZDBOnlyQuery GetDuplicateEntryNumberQuery(JobDeclaration declaration, ZString entryNumber)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_GC, declaration.JE_GC);
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, declaration.JE_MessageType);
			result.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);

			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			entryNumberQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, entryNumber);
			entryHeaderQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);
			result.AddSubQuery(entryHeaderQuery, JoinCondition.And);

			return result;
		}
	}
}

