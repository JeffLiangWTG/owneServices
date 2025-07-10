using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ClientSharedComponents;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow.Testing
{
	public class WowTestHelper : SharedTestHelper
	{
		public JobCharge CreateJobCharge(ChargeCollection charges, ZDecimal sellAmt, ZGuid chargePK)
		{
			var jobCharge = charges.AddNew();
			jobCharge.JR_AC = chargePK;
			jobCharge.JR_LocalSellAmt = sellAmt;
			return jobCharge;
		}

		public AccChargeCode CreateChargeCode(ZString chargeCode, ZString chargeGroup)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_ChargeGroup, chargeGroup);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var charge = Factory.LoadTop1<AccChargeCode>(query);
			if (charge == null)
			{
				charge = Factory.New<AccChargeCode>();
				charge.AC_Code = chargeCode;
				charge.AC_ChargeGroup = chargeGroup;
				charge.SetGLAccountDataForTesting();
				Factory.Save();
			}

			return charge;
		}

		public JobDeclaration CreateJobDecWithJobCharges(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_JobNum = "123";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var originChargeCode = CreateChargeCode("CONSOL", ChargeCodeGroupList.Codes.Origin);
			CreateJobCharge(job.Charges, 9.45m, originChargeCode.PK);
			var destinChargeCode = CreateChargeCode("DES", ChargeCodeGroupList.Codes.Destination);
			CreateJobCharge(job.Charges, 11m, destinChargeCode.PK);
			var otherImportChargeCode = CreateChargeCode("NGRP", ChargeCodeGroupList.Codes.NotGrouped);
			CreateJobCharge(job.Charges, 12m, otherImportChargeCode.PK);
			var quarantineChargeCode = CreateChargeCode("QUARANT", ChargeCodeGroupList.Codes.Brokerage);
			CreateJobCharge(job.Charges, 13m, quarantineChargeCode.PK);
			var detentChargeCode = CreateChargeCode("DET", ChargeCodeGroupList.Codes.Freight);
			CreateJobCharge(job.Charges, 14m, detentChargeCode.PK);
			Factory.Save();
			return declaration;
		}
	}
}
