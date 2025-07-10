using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveHeaderValueSetStrategy : IValueSetStrategy
	{
		public CusInBondMoveHeaderValueSetStrategy(CusInBondMoveHeader moveHeader)
		{
			MoveHeader = moveHeader;
			Factory = moveHeader.Factory;
		}
		CusInBondMoveHeader MoveHeader { get; }
		BusinessObjectFactory Factory { get; }

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		protected void ValueSetCore(ZPropertyInfo info, IZType oldValue)
		{
			switch (info.Name)
			{
				case CusInBondMoveHeader.Schema.BM_Calc_PermitNumber:
					if (info.Value != oldValue)
					{
						DefaultBM_Calc_PermitNumberChanged();
					}
					break;
			}
		}

		protected virtual void DefaultBM_Calc_PermitNumberChanged()
		{
			var moveHeaderWithSameNumber = GetFirstSameTransitPermit();
			if (moveHeaderWithSameNumber != null)
			{
				using (MoveHeader.SuspendChangeValidationForPermitDates())
				{
					MoveHeader.BM_Calc_IssueDate = moveHeaderWithSameNumber.BM_Calc_IssueDate;
					MoveHeader.BM_ArrivalDate = moveHeaderWithSameNumber.BM_ArrivalDate;
					MoveHeader.BM_Calc_ValidityDate = moveHeaderWithSameNumber.BM_Calc_ValidityDate;
				}
			}
		}

		CusInBondMoveHeader GetFirstSameTransitPermit()
		{
			var permitNumber = MoveHeader.BM_Calc_PermitNumber;
			if (permitNumber.IsEmpty || MoveHeader.EntryInstruction?.JobDeclaration?.Company == null)
			{
				return null;
			}

			var entryInstructionPk = MoveHeader.EntryInstruction.PK;
			var result = MoveHeader.EntryInstruction.JobDeclaration.CustomsEntryInstructions
				.Cast<CusEntryInstruction>()
				.SelectMany(instruction => instruction.CusInBondPermitsHeaders)
				.FirstOrDefault(moveHeader => moveHeader.EntryInstruction.PK != entryInstructionPk
												&& moveHeader.BM_Calc_PermitNumber == permitNumber);

			if (result == null)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
				var sql = @"
					BM_PK IN
					(
						SELECT BM_PK FROM dbo.CusInBondMoveHeader
						INNER JOIN dbo.CusInBondHeader ON  BH_PK = BM_BH
						INNER JOIN dbo.CusEntryInstruction ON CEI_PK = BH_ParentID
						INNER JOIN dbo.JobDeclaration ON  JE_ClusterKey = CEI_ClusterKey
						INNER JOIN dbo.CusEntryNum ON CE_ParentID = BM_PK
						WHERE JE_GC = @CompanyPK
							AND CE_EntryNum = @PermitNumber
							AND CE_EntryType = 'PMT'
							AND CE_RN_NKCountryCode = @CountryCode
							AND JE_PK <> @JobPK
					)"; // Part of a SQL expression
				var parameters = new ZSqlParameterCollection
				{
					{ "@CompanyPK", MoveHeader.EntryInstruction.JobDeclaration.JE_GC, JobDeclarationSchema.JE_GC },
					{ "@CountryCode", MoveHeader.EntryInstruction.JobDeclaration.CountryCode, CusEntryNumSchema.CE_RN_NKCountryCode },
					{ "@PermitNumber", MoveHeader.BM_Calc_PermitNumber, CusEntryNumSchema.CE_EntryNum },
					{ "@JobPK", MoveHeader.EntryInstruction.JobDeclaration.PK, JobDeclarationSchema.PK }
				};
				query.AddFilterAndZSQLParameterCollection(sql, parameters);
				result = Factory.LoadTop1<CusInBondMoveHeader>(query);
			}
			return result;
		}
	}
}
