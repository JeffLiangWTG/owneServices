using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CusEntryInstructionCollection : CusEntryInstructionCollection<CusEntryInstruction>
	{
		public CusEntryInstructionCollection(JobDeclaration parentBO)
			: base(parentBO)
		{
		}

		JobDeclaration JobDeclaration => Master as JobDeclaration;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var entryInstruction = (CusEntryInstruction)child;
			var previousEntryInstruction = this.LastOrDefault<CusEntryInstruction>();
			SetDefaultsForInstruction(previousEntryInstruction, entryInstruction);
		}

		public void SetDefaultsForInstruction(CusEntryInstruction previousInstruction, CusEntryInstruction entryInstruction)
		{
			base.SetDefaultsForNewChild(entryInstruction);

			JobDeclaration.TransportDataHelper.UpdateBillOfLoading(entryInstruction);

			using (entryInstruction.GetValidationSuspender())
			using (entryInstruction.SuspendSettingHasChanges())
			{
				if (previousInstruction == null)
				{
					SetDefaultsForFirstChild(entryInstruction);
				}
				else
				{
					SetDefaultsFromPreviousEntryInstruction(previousInstruction, entryInstruction);
				}

				if (!JobDeclaration.OfficeOfDestination.IsEmpty && entryInstruction.IsCIQRequiresEditableAndFalse)
				{
					entryInstruction.CEI_CIQRequires = true;
				}

				var linkAddInfo = JobDeclaration.SupplierImporterLink.GetAddInfo();
				if (linkAddInfo != null)
				{
					if (!linkAddInfo.ZO_ManualNo.IsEmpty)
					{
						entryInstruction.CEI_ManualNo = linkAddInfo.ZO_ManualNo;
					}
					if (!linkAddInfo.ZO_ProcedureCode.IsEmpty)
					{
						entryInstruction.CEI_Style = linkAddInfo.ZO_ProcedureCode;
					}
					if (!linkAddInfo.ZO_LevyType.IsEmpty)
					{
						entryInstruction.CEI_LevyType = linkAddInfo.ZO_LevyType;
					}
				}

				var jobOrgAddInfo = JobDeclaration.OrgImpAddInfo;
				if (jobOrgAddInfo != null)
				{
					if (!entryInstruction.WillGenerateRecordListing)
					{
						if (jobOrgAddInfo.ZO_IsConsolidatedDutyCollection)
						{
							entryInstruction.OperationMatters.AddNew().CY_Code = OperationMatterList.Codes.ConsolidatedDutyCollection;
						}
					}
					if (jobOrgAddInfo.ZO_IsAssuredInspectClearance)
					{
						entryInstruction.OperationMatters.AddNew().CY_Code = OperationMatterList.Codes.AssuredInspectClearance;
					}
					if (!jobOrgAddInfo.ZO_IntelligentDeclarationType.IsEmpty)
					{
						entryInstruction.CEI_SubStyle = jobOrgAddInfo.ZO_IntelligentDeclarationType;
					}
				}

				entryInstruction.CEI_EnterprisePromised = true;
			}
		}

		protected void SetDefaultsForFirstChild(CusEntryInstruction firstEntryInstruction)
		{
			firstEntryInstruction.CEI_Packages = JobDeclaration.JE_TotalNoOfPacks;
			firstEntryInstruction.CEI_PackageUQ = PackageType.GetMappedPackageType(JobDeclaration.JE_TotalNoOfPacksPackType);
			firstEntryInstruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms;
		}

		protected void SetDefaultsFromPreviousEntryInstruction(CusEntryInstruction previousEntryInstruction, CusEntryInstruction entryInstruction)
		{
			entryInstruction.CEI_PackageUQ = previousEntryInstruction.CEI_PackageUQ;
			entryInstruction.CEI_DocumentSubmissionType = previousEntryInstruction.CEI_DocumentSubmissionType;
			if (JobDeclaration.WillGenerateBothEntries && !previousEntryInstruction.IsParent && !previousEntryInstruction.IsChild)
			{
				entryInstruction.CEI_CEI_Parent = previousEntryInstruction.PK;
			}
		}
	}
}
