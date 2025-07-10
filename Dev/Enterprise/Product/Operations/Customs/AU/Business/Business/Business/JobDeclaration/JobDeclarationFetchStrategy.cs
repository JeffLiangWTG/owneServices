using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration declaration
		{
			get { return BusinessObject as JobDeclaration; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusEntryHeaderSchema.CH_ClusterKey, declaration.JE_ClusterKey);
			Factory.AddFetchHint(QuarantineColsHeaderSchema.QCH_ClusterKey, declaration.JE_ClusterKey);
		}

		protected override void FetchForViewDeclaration(TableColumn[] columns)
		{
			foreach (TableColumn tableColumn in columns)
			{
				if (tableColumn.ColumnName == JobDeclaration.Schema.JE_EntryStatusDescription)
				{
					Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.PK);
					foreach (CusEntryHeader header in declaration.CustomsEntryHeaders)
					{
						Factory.AddFetchHint(StmALogSchema.SL_Parent, header.PK);
					}
				}
				else if (tableColumn.ColumnName == JobDeclaration.Schema.ConsolidatedCargoStatusDescription)
				{
					Factory.AddFetchHint(typeof(Bill), CusDecHouseBillSchema.CU_ClusterKey, declaration.JE_ClusterKey);
				}
			}

			base.FetchForViewDeclaration(columns);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			var packingGroups = BusinessObject.PackingGroups; // Add to children for child validation
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
		}

		protected override void FetchForMergeCore()
		{
			communityProtectionProfileDetails = new List<Tuple<ZQuery, ZQuery>>();
			base.FetchForMergeCore();
			communityProtectionProfileDetails = null;
		}

		List<Tuple<ZQuery, ZQuery>> communityProtectionProfileDetails;

		protected override void AddMergeFetchHintsAfterInvoiceLines()
		{
			base.AddMergeFetchHintsAfterInvoiceLines();
			foreach (var communityProtectionProfileDetail in communityProtectionProfileDetails)
			{
				var profiles = Factory.Load<CMRCommunityProtectionProfile>(communityProtectionProfileDetail.Item1);
				if (profiles.Length > 0)
				{
					var profilesMatchingNotInTariffAndStat = Factory.Load<CMRCommunityProtectionProfile>(communityProtectionProfileDetail.Item2);
					foreach (var profile in profiles.Where(x => !profilesMatchingNotInTariffAndStat.Contains(x)))
					{
						Factory.AddFetchHint(CMRCommunityProtectionRiskSchema.CK_Identifier, profile.CP_CommunityProtectionRiskIdentifier);
					}
				}
			}
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			var auInvoiceLine = (JobComInvoiceLine)invoiceLine;
			Factory.AddFetchHint(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLine.PK); // Move this to base if other countries use it as well
			var tariffNumber = auInvoiceLine.TariffNumber;
			if (!tariffNumber.IsEmpty)
			{
				var minimumLength = 2;
				var filterString = auInvoiceLine.TariffNumber.Replace(".", "").Replace(" ", "");
				var maximumLength = Math.Min(filterString.Length, 8);

				var tariffBits = new string[maximumLength - minimumLength + 1];
				for (int length = minimumLength; length <= maximumLength; length++)
				{
					tariffBits[length - minimumLength] = filterString.Left(length);
				}
				var candidatesProfileQuery = new ZQuery(CMRCommunityProtectionProfileSchema.CP_TariffClassificationNumberfield, tariffBits);
				Factory.AddFetchHint(CMRCommunityProtectionProfileSchema.Instance, candidatesProfileQuery);
				var profilesMatchingNotInTariffAndStatFilter = CMRCommunityProtectionRisk.GetCPProfileTariffWithNotFilter(tariffNumber, auInvoiceLine.StatCode);
				Factory.AddFetchHint(CMRCommunityProtectionProfileSchema.Instance, profilesMatchingNotInTariffAndStatFilter);
				communityProtectionProfileDetails.Add(new Tuple<ZQuery, ZQuery>(candidatesProfileQuery, profilesMatchingNotInTariffAndStatFilter));
			}
		}

		#region Document Supporter Fetch Hints

		protected override void AddDocumentSupporterFetchHintsForJobDeclaration()
		{
			base.AddDocumentSupporterFetchHintsForJobDeclaration();
			Factory.AddFetchHint(new FetchHint(CusEntryCPDecSchema.ON_JE, declaration.PK));
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetJobComInvoiceHeaderRelatedFetchHints(row))
			{
				yield return hint;
			}

			var invoicePK = row.GetValue(JobComInvoiceHeaderSchema.PK);
			yield return new FetchHint(QuarantineExDocHeaderSchema.QH_JZ, invoicePK);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return hint;
			}

			var tariffNumber = row.GetValue(JobComInvoiceLineSchema.JI_Tariff);
			if (!tariffNumber.IsEmpty)
			{
				if (AUCAHECCWrapper.EnableCWRefForAHECC)
				{
					yield return AUCAHECCWrapper.GetTariffFetchHint(Factory, tariffNumber);
				}
				else
				{
					yield return new FetchHint(AUCAHECCSchema.UA_AHECC, tariffNumber);
				}

				if (AUCClassWrapper.UseCustomsReferenceData)
				{
					yield return AUCClassWrapper.GetTariffFetchHint(Factory, tariffNumber);
				}
				else
				{
					yield return new FetchHint(CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, tariffNumber);
				}
			}
		}

		protected override IEnumerable<IFetchHint> GetCusEntryHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetCusEntryHeaderRelatedFetchHints(row))
			{
				yield return hint;
			}

			var entryHeaderPK = row.GetValue(CusEntryHeaderSchema.PK);
			yield return new FetchHint(CusEntryCPDecSchema.ON_CH, entryHeaderPK);
			yield return new FetchHint(EDIMessageSchema.EM_LinkUniqueID, entryHeaderPK);
		}

		protected override IEnumerable<IFetchHint> GetCusEntryLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetCusEntryLineRelatedFetchHints(row))
			{
				yield return hint;
			}

			var entryLinePK = row.GetValue(CusEntryLineSchema.PK);
			yield return new FetchHint(CusEntryCPDecSchema.ON_CL, entryLinePK);
		}

		#endregion
	}
}
