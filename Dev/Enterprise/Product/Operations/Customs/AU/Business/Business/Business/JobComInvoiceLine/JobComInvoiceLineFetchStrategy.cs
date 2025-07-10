using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine line)
			: base(line)
		{
		}

		JobComInvoiceLine line
		{
			get { return BusinessObject as JobComInvoiceLine; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, line.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, line.PK);
			Factory.AddFetchHint(CusStorageDocPivotSchema.CSD_ParentID, line.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			if (!line.JI_Tariff.IsEmpty)
			{
				if (line.IsExport)
				{
					if (AUCAHECCWrapper.EnableCWRefForAHECC)
					{
						Factory.AddFetchHint(AUCAHECCWrapper.GetTariffFetchHint(Factory, line.JI_Tariff));
					}
					else
					{
						var query = new ZQuery(AUCAHECCSchema.UA_AHECC, line.JI_Tariff);
						query.OrderBy = AUCAHECCSchema.UA_DateStart.Name + OrderByClause.Descending;
						Factory.AddFetchHint(AUCAHECCSchema.Instance, query);
					}
				}
				else if (AUCClassWrapper.UseCustomsReferenceData)
				{
					Factory.AddFetchHint(AUCClassWrapper.GetTariffFetchHint(Factory, line.JI_Tariff));
				}
				else
				{
					var isImport = line.IsImport;
					var isDrawback = line.Declaration?.IsDrawback ?? false;
					if (isImport || isDrawback)
					{
						ZString tariffClassification = line.TariffNumber.Replace(" ", "").Replace(".", "");
						Factory.AddFetchHint(CMRStatisticalClassificationPeriodSnapshotSchema.SC_TariffClassificationNumber, tariffClassification);
						if (isImport)
						{
							Factory.AddFetchHint(CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, tariffClassification);
							for (int i = 2; i <= tariffClassification.Length; i++)
							{
								Factory.AddFetchHint(typeof(CMRCommunityProtectionProfile), CMRCommunityProtectionProfileSchema.CP_TariffClassificationNumberfield, tariffClassification.Left(i));
								ZString notTariff = CMRCommunityProtectionProfile.NotConstants + tariffClassification.Left(i);
								Factory.AddFetchHint(typeof(CMRCommunityProtectionProfile), CMRCommunityProtectionProfileSchema.CP_TariffClassificationNumberfield, notTariff);
							}
						}
					}
				}
			}
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(QuarantineExDocLineSchema.QL_JI, BusinessObject.PK);
			Factory.AddFetchHint(CusStorageDocPivotSchema.CSD_ParentID, line.PK);
		}
	}
}
