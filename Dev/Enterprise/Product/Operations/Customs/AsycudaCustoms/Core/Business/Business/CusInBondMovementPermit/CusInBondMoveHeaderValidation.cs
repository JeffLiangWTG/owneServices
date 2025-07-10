using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveHeaderValidation : Customs.Business.CusInBondMoveHeaderValidation
	{
		public CusInBondMoveHeaderValidation(CusInBondMoveHeader parent)
			: base(parent)
		{
			Factory = parent.Factory;
		}

		protected new CusInBondMoveHeader Parent => (CusInBondMoveHeader)base.Parent;

		BusinessObjectFactory Factory { get; }

		public void ValidateBM_Calc_PermitNumber()
		{
			ValidateCalculatedProperty(Parent.BM_Calc_PermitNumberInfo);
		}

		protected void CheckBM_Calc_PermitNumber()
		{
			MandatoryValidation.CheckEntered(Parent.BM_Calc_PermitNumberInfo);
			var permitNumber = Parent.BM_Calc_PermitNumber;
			var permitPK = Parent.PK;
			var hasDuplicatedPermitNumberInSameEntryInstruction = Parent.EntryInstruction?.CusInBondPermitsHeaders
				.Any(bm => bm.BM_Calc_PermitNumber == permitNumber && bm.PK != permitPK) ?? false;
			if (hasDuplicatedPermitNumberInSameEntryInstruction)
			{
				Parent.BM_Calc_PermitNumberInfo.AddError(Res.GetString("4859C060-C6A8-43E9-AFD8-844BE377792A", "Permit # should be unique per Entry Instruction."));
			}
		}

		public void ValidateBM_Calc_IssueDate()
		{
			ValidateCalculatedProperty(Parent.BM_Calc_IssueDateInfo);
		}

		protected void CheckBM_Calc_IssueDate()
		{
			var issueDateInfo = Parent.BM_Calc_IssueDateInfo;
			MandatoryValidation.MessageErrorIfNotEntered(issueDateInfo);
			var issueDate = Parent.BM_Calc_IssueDate;
			if (issueDate.IsValid)
			{
				if (Parent.BM_ArrivalDate.IsValid && issueDate > Parent.BM_ArrivalDate)
				{
					issueDateInfo.AddError(Res.GetString("4fdbed42-fd4d-40bf-b39b-001b10634dbd", "Issue Date should be before or equal to Arrival Date."));
				}
				if (Parent.BM_Calc_ValidityDate.IsValid && issueDate > Parent.BM_Calc_ValidityDate)
				{
					issueDateInfo.AddError(Res.GetString("b6c470de-28ca-41a4-a7b9-2a95b245bfab", "Issue Date should be before or equal to Validity Date."));
				}
				if (issueDate > ZDateTime.Now)
				{
					issueDateInfo.AddError(Res.GetString("8d66f5a0-8230-4b2f-905f-af19fc857619", "Issue Date should be before or equal to Today."));
				}

				ValidateTransitPermitValueChanged(issueDateInfo, CusEntryNumSchema.CE_IssueDate);
			}
		}

		public void ValidateBM_Calc_ValidityDate()
		{
			ValidateCalculatedProperty(Parent.BM_Calc_ValidityDateInfo);
		}

		protected void CheckBM_Calc_ValidityDate()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_Calc_ValidityDateInfo);
			if (Parent.BM_Calc_ValidityDate.IsValid)
			{
				if (Parent.BM_Calc_IssueDate.IsValid && Parent.BM_Calc_IssueDate > Parent.BM_Calc_ValidityDate)
				{
					Parent.BM_Calc_ValidityDateInfo.AddError(Res.GetString("26d9b959-7bd7-4061-b4c1-a31c8a647451", "Validity Date should be after or equal to Issue Date."));
				}

				ValidateTransitPermitValueChanged(Parent.BM_Calc_ValidityDateInfo, CusEntryNumSchema.CE_ExpiryDate);
			}
		}

		protected override void CheckBM_ArrivalDate()
		{
			base.CheckBM_ArrivalDate();
			if (Parent.BM_ArrivalDate.IsValid)
			{
				if (Parent.BM_Calc_IssueDate.IsValid && Parent.BM_Calc_IssueDate > Parent.BM_ArrivalDate)
				{
					Parent.BM_ArrivalDateInfo.AddError(Res.GetString("87055bcf-75dd-4319-a249-1e73dfbe98c2", "Arrival Date should be after or equal to Issue Date."));
				}
				ValidateTransitPermitValueChanged(Parent.BM_ArrivalDateInfo, CusInBondMoveHeaderSchema.BM_ArrivalDate);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBM_Calc_PermitNumber();
			ValidateBM_Calc_IssueDate();
			ValidateBM_Calc_ValidityDate();
		}

		void ValidateTransitPermitValueChanged(ZPropertyInfo info, SchemaColumn schemaColumn)
		{
			if (!Parent.IsPermitDateChangeValidationSuspended)
			{
				var diffMoveHeader = GetFirstTransitPermitWithDifferentDate(info, schemaColumn);
				if (diffMoveHeader != null)
				{
					info.AddMessageError(
						Res.GetString("285D1D6A-036C-4BA5-AA15-8F39A34B0ABB",
							"{0} on {1} has a different {2} ({3}) for this {4}.",
						diffMoveHeader.EntryInstruction.HumanReadableName,
									diffMoveHeader.EntryInstruction.JobDeclaration.HumanReadableName,
									info.HumanReadableName,
									((ZDateTime)diffMoveHeader[info.Name]).ToShortDateString(),
									Parent.BM_Calc_PermitNumberInfo.HumanReadableName));
				}
			}
		}

		CusInBondMoveHeader GetFirstTransitPermitWithDifferentDate(ZPropertyInfo info, SchemaColumn schemaColumn)
		{
			var permitNumber = Parent.BM_Calc_PermitNumber;
			if (permitNumber.IsEmpty || Parent.EntryInstruction?.JobDeclaration?.Company == null)
			{
				return null;
			}

			var entryInstructionPk = Parent.EntryInstruction.PK;
			var value = info.Value;
			var result = Parent.EntryInstruction.JobDeclaration.CustomsEntryInstructions
				.Cast<CusEntryInstruction>()
				.SelectMany(instruction => instruction.CusInBondPermitsHeaders)
				.FirstOrDefault(moveHeader => moveHeader.EntryInstruction.PK != entryInstructionPk
												&& moveHeader.BM_Calc_PermitNumber == permitNumber
												&& !value.Equals(moveHeader[info.Name]));

			if (result == null)
			{
				var query = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
				var sql = string.Format(CultureInfo.InvariantCulture, @"
				BM_PK IN
				(
					SELECT BM_PK FROM dbo.CusInBondMoveHeader
					INNER JOIN dbo.CusInBondHeader ON BH_PK = BM_BH
					INNER JOIN dbo.CusEntryInstruction ON CEI_PK = BH_ParentID
					INNER JOIN dbo.JobDeclaration ON JE_ClusterKey = CEI_ClusterKey
					INNER JOIN dbo.CusEntryNum ON CE_ParentID = BM_PK
					WHERE JE_GC = @CompanyPK
						AND CE_EntryNum = @PermitNumber
						AND CE_EntryType = 'PMT'
						AND CE_RN_NKCountryCode = @CountryCode
						AND JE_PK <> @JobPK
						AND {0} <> @DateValue
				)", schemaColumn.Name);
				var parameters = new ZSqlParameterCollection
				{
					{ "@CompanyPK", Parent.EntryInstruction.JobDeclaration.JE_GC, JobDeclarationSchema.JE_GC },
					{ "@CountryCode", Parent.EntryInstruction.JobDeclaration.CountryCode, CusEntryNumSchema.CE_RN_NKCountryCode },
					{ "@PermitNumber", Parent.BM_Calc_PermitNumber, CusEntryNumSchema.CE_EntryNum },
					{ "@JobPK", Parent.EntryInstruction.JobDeclaration.PK, JobDeclarationSchema.PK },
					{ "@DateValue", info.Value, schemaColumn }
				};
				query.AddFilterAndZSQLParameterCollection(sql, parameters);
				result = Factory.LoadTop1<CusInBondMoveHeader>(query);
			}
			return result;
		}
	}
}
