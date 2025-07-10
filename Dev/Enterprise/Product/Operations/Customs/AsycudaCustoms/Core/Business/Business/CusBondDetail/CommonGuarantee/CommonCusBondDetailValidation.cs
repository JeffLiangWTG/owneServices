using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CommonCusBondDetailValidation : MasterFiles.Business.CusBondDetailValidation
	{
		public CommonCusBondDetailValidation(CommonCusBondDetail parent) : base(parent)
		{
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_BondTypeInfo);
		}

		protected override void CheckPW_BondNumber2()
		{
			base.CheckPW_BondNumber2();
			CheckReferenceNumberIsUnique();
		}

		void CheckReferenceNumberIsUnique()
		{
			if (!Parent.IsInDatabase || Parent.PW_BondNumber2Info.HasChanges || Parent.PW_CPH_GuaranteeInfo.HasChanges)
			{
				var referenceNumber = Parent.PW_BondNumber2;
				var linkedGuaranteePK = Parent.PW_CPH_Guarantee;
				if (!referenceNumber.IsEmpty && !linkedGuaranteePK.IsEmpty && Parent.Instruction?.JobDeclaration is JobDeclaration declaration)
				{
					if (IsDuplicateInLocal() || IsDuplicateInDb())
					{
						Parent.PW_BondNumber2Info.AddError(Res.GetString("ab8a0bad-d382-4a4a-8997-a4ee8d1761ae", "Reference Number in combination with linked guarantee must unique in the job."));
					}

					bool IsDuplicateInLocal()
					{
						var query = new ZQuery(CusBondDetailSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK)
						{
							FetchOnlyFromLocalCache = true
						};
						query.AddToFilter(CusBondDetailSchema.PW_BondNumber2, referenceNumber);
						query.AddToFilter(CusBondDetailSchema.PW_CPH_Guarantee, linkedGuaranteePK);
						query.AddToFilter(CusBondDetailSchema.PW_ParentID, declaration.CustomsEntryInstructions.GetPKs());

						return Parent.Factory.Load<CommonCusBondDetail>(query).Length > 0;
					}

					bool IsDuplicateInDb()
					{
						var result = declaration.IsInDatabase;
						if (result)
						{
							var query = new ZDBOnlyQuery(typeof(CommonCusBondDetail));
							if (Parent.IsInDatabase)
							{
								query.AddToFilter(CusBondDetailSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
							}
							query.AddToFilter(CusBondDetailSchema.PW_BondNumber2, referenceNumber);
							query.AddToFilter(CusBondDetailSchema.PW_CPH_Guarantee, linkedGuaranteePK);

							var instructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.PK);
							instructionSubQuery.AddToFilter(CusEntryInstructionSchema.CEI_ClusterKey, declaration.JE_ClusterKey);

							query.AddSubQuery(CusBondDetailSchema.PW_ParentID, instructionSubQuery, JoinCondition.And);

							result = Parent.Factory.ExistsInDatabase(CusBondDetail.Schema.TableName, query);
						}
						return result;
					}
				}
			}
		}

		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();
			MandatoryValidation.CheckNotNegative(Parent.PW_BondAmountInfo);
		}

		protected override void CheckPW_ActivityCode()
		{
			base.CheckPW_ActivityCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_ActivityCodeInfo);
		}

		protected override void CheckPW_Status()
		{
			base.CheckPW_Status();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_StatusInfo);
		}

		protected new CommonCusBondDetail Parent => (CommonCusBondDetail)base.Parent;
	}
}

