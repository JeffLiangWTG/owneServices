using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationRecordLoader
	{
		public DeclarationRecordLoader(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public CusEntryHeader LoadRecord(ZString masterBill, ZString houseBill, ZString voyage, ZString lloydsNumber, ZString reference, ZString entryNumber, ZString containerNumber)
		{
			CusEntryHeader result = null;
			if (!reference.IsEmpty && !entryNumber.IsEmpty && !reference.StartsWith("CE"))
			{
				result = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(factory, reference, entryNumber) as CusEntryHeader;
			}

			if (result == null)
			{
				result = LoadForBillsAndOtherDetails(masterBill, houseBill, voyage, lloydsNumber, containerNumber, reference);
			}
			return result;
		}

		CusEntryHeader LoadForBillsAndOtherDetails(ZString masterBill, ZString houseBill, ZString voyage, ZString lloydsNumber, ZString containerNumber, ZString reference)
		{
			CusEntryHeader result = null;

			// Find Bills
			if (!masterBill.IsEmpty)
			{
				var formatedVoyage = voyage.TrimStart(' ', '0');
				Bill[] bills = (Bill[])factory.Load(typeof(Bill), GetPossibleHouseBills(masterBill, houseBill, formatedVoyage, lloydsNumber, reference, containerNumber));
				if (bills.Length == 0 && !(reference.IsEmpty && containerNumber.IsEmpty))
				{
					bills = (Bill[])factory.Load(typeof(Bill), GetPossibleHouseBills(masterBill, houseBill, formatedVoyage, lloydsNumber, ZString.Empty, ZString.Empty));
				}

				// Found bill now find entry header
				if (!voyage.IsEmpty && !lloydsNumber.IsEmpty)
				{
					foreach (Bill currentBill in bills)
					{
						if (currentBill.Declaration != null)
						{
							ZString decVoyage = currentBill.Declaration.JE_VoyageFlightNo.ToUpper();
							ZString decLloydsNumber = currentBill.Declaration.VesselNumber;

							if (decVoyage.TrimStart(' ', '0') == formatedVoyage)
							{
								result = GetEntryHeaderFromDeclaration(currentBill.Declaration, currentBill);
								if (result != null)
								{
									break;
								}
							}
						}
					}
				}
				else if (bills.Length > 0)
				{
					for (int i = 0; i < bills.Length; i++)
					{
						if (bills[i].Declaration != null)
						{
							result = GetEntryHeaderFromDeclaration(bills[i].Declaration, bills[i]);
							if (result != null)
							{
								break;
							}
						}
					}
				}
			}

			return result;
		}

		#region Implementation

#if DEBUG
		public
#endif
				ZQuery GetPossibleHouseBills(ZString masterBill, ZString houseBill, ZString formattedVoyage, ZString lloydsNumber, ZString reference, ZString containerNumber)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(Bill));

			if (houseBill.IsEmpty)
			{
				query.AddToFilter(CusDecHouseBillSchema.CU_BillNum, masterBill);
				query.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
			}
			else
			{
				query.AddToFilter(CusDecHouseBillSchema.CU_BillNum, houseBill);
				query.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.HouseBill);

				ZDBOnlySubQuery masterBillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.PK);
				masterBillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, masterBill);
				masterBillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
				query.AddSubQuery(CusDecHouseBillSchema.CU_CU_ParentBill, masterBillSubQuery, JoinCondition.And);
			}

			ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusDecHouseBillSchema.CU_JE);
			if (!formattedVoyage.IsEmpty && !lloydsNumber.IsEmpty)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Contains, formattedVoyage);
				var vesselSubQuery = new ZDBOnlySubQuery(typeof(RefVessel), RefVesselSchema.RV_Code);
				vesselSubQuery.AddToFilter(RefVesselSchema.RV_LloydsNumber, lloydsNumber);
				declarationQuery.AddSubQuery(JobDeclarationSchema.JE_VesselName, vesselSubQuery, JoinCondition.And);
			}
			if (reference.IndexOf('/') != -1)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, reference.Split('/')[0]);
			}
			if (!containerNumber.IsEmpty)
			{
				var containerSubQuery = new ZDBOnlySubQuery(typeof(BaseCusContainer), CusContainerSchema.CO_JE);
				containerSubQuery.AddToFilter(JoinCondition.And, CusContainerSchema.CO_ContainerNumber, containerNumber);
				var containerDbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				containerDbOnlyResult.AddSubQuery(containerSubQuery, JoinCondition.And);
				declarationQuery.AddToFilter(containerDbOnlyResult);
			}

			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddToFilter(new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.Australia));

			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);

			declarationQuery.AddSubQuery(branchQuery, JoinCondition.And);
			query.AddSubQuery(declarationQuery, JoinCondition.And);

			return query;
		}

		CusEntryHeader GetEntryHeaderFromDeclaration(JobDeclaration declaration, Bill bill)
		{
			if (declaration.CustomsEntryHeaders.Count == 1)
			{
				return declaration.CustomsEntryHeaders[0];
			}
			else if (declaration.CustomsEntryHeaders.Count > 1)
			{
				foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
				{
					foreach (Bill currentBill in entryHeader.Bills)
					{
						if (currentBill.PK == bill.PK)
						{
							return entryHeader;
						}
					}
				}
			}

			return null;
		}

		readonly BusinessObjectFactory factory;

		#endregion
	}
}
