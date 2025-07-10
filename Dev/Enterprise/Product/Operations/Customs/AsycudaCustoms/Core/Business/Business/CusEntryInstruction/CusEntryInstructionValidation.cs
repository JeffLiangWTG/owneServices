using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryInstructionValidation : Customs.Business.CusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
			isRiskManagementEnabled = Parent?.JobDeclaration?.IsRiskManagementEnabled ?? false;
		}
		readonly bool isRiskManagementEnabled;

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRemainingCustomsValue();
			ValidateRemainingNetWeightKilograms();
			ValidateRemainingCustomsQuantity();
			ValidateASY_PortOfExit();
			ValidateASY_LocalReferenceNumber();
		}

		public void ValidateRemainingCustomsValue()
		{
			ValidateCalculatedProperty(Parent.RemainingCustomsValueInfo);
		}

		protected void CheckRemainingCustomsValue()
		{
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.CheckNotNegative(Parent.RemainingCustomsValueInfo);
			}
		}

		public void ValidateRemainingNetWeightKilograms()
		{
			ValidateCalculatedProperty(Parent.RemainingNetWeightKilogramsInfo);
		}

		protected void CheckRemainingNetWeightKilograms()
		{
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.CheckNotNegative(Parent.RemainingNetWeightKilogramsInfo);
			}
		}

		public void ValidateRemainingCustomsQuantity()
		{
			ValidateCalculatedProperty(Parent.RemainingCustomsQuantityInfo);
		}

		protected void CheckRemainingCustomsQuantity()
		{
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.CheckNotNegative(Parent.RemainingCustomsQuantityInfo);
			}
		}

		public void ValidateASY_PortOfExit()
		{
			ValidateCalculatedProperty(Parent.ASY_PortOfExitInfo);
		}

		protected void CheckASY_PortOfExit()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ASY_PortOfExitInfo);
		}

		public void ValidateASY_LocalReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.ASY_LocalReferenceNumberInfo);
		}

		protected void CheckASY_LocalReferenceNumber()
		{
			var localReferenceNumber = Parent.ASY_LocalReferenceNumber;
			var localReferenceNumberInfo = Parent.ASY_LocalReferenceNumberInfo;
			var jobDeclaration = Parent.JobDeclaration;
			if (!localReferenceNumber.IsEmpty && jobDeclaration != null)
			{
				var instructions = jobDeclaration.CustomsEntryInstructions;
				var parentPK = Parent.PK;
				if (instructions.Count > 1 && instructions.Cast<CusEntryInstruction>().Any(x => x.ASY_LocalReferenceNumber.EqualsIgnoringCase(localReferenceNumber) && x.PK != parentPK))
				{
					localReferenceNumberInfo.AddError(Res.GetString("786A7536-6A24-45CB-B341-EE8D239AC85F", "This Local Reference Number found on other Entry Instruction of this declaration."));
				}
				else if (jobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.CH_BGMReference.EqualsIgnoringCase(localReferenceNumber) && x.CH_CEI_Instruction != parentPK))
				{
					localReferenceNumberInfo.AddError(Res.GetString("DC008CEF-EAFB-4434-8858-B631E1826E6F", "This Local Reference Number is duplicate to the entry reference of the entry header linked to other Entry Instruction of this declaration."));
				}
				else
				{
					var duplicateDeclaration = GetDuplicateDeclaration(jobDeclaration, GetDuplicateLocalReferenceNumberQuery(localReferenceNumber));
					if (duplicateDeclaration != null)
					{
						localReferenceNumberInfo.AddError(Res.GetString("4DDB83E0-D89F-4BB3-BFA1-C9BE49C023F6", "This Local Reference Number found on Declaration Job: {0}.", duplicateDeclaration.JE_DeclarationReference));
					}
					else
					{
						duplicateDeclaration = GetDuplicateDeclaration(jobDeclaration, GetDuplicateEntryBGMReferenceNumberQuery(localReferenceNumber));
						if (duplicateDeclaration != null)
						{
							localReferenceNumberInfo.AddError(Res.GetString("6C85F36A-5773-430C-92C1-12F83639DA53", "This Local Reference Number found on an Entry for Declaration Job: {0}.", duplicateDeclaration.JE_DeclarationReference));
						}
					}
				}
			}
		}

		JobDeclaration GetDuplicateDeclaration(JobDeclaration declaration, ZDBOnlySubQuery subQuery)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddSubQuery(subQuery, JoinCondition.And);
			query.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, declaration.JE_MessageType);
			query.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);

			return Parent.Factory.LoadTop1<JobDeclaration>(query);
		}

		static ZDBOnlySubQuery GetDuplicateLocalReferenceNumberQuery(ZString localReferenceNumber)
		{
			var genAddOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CusEntryInstruction.Schema.ASY_LocalReferenceNumber);
			genAddOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, localReferenceNumber);

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);
			entryInstructionSubQuery.AddSubQuery(genAddOnColumnSubQuery, JoinCondition.And);

			return entryInstructionSubQuery;
		}

		static ZDBOnlySubQuery GetDuplicateEntryBGMReferenceNumberQuery(ZString localReferenceNumber)
		{
			var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entryHeaderSubQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, localReferenceNumber);

			return entryHeaderSubQuery;
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_StyleInfo);
			EnsureThatAllInvoiceLinesHaveSameProcedureDetails();
		}

		void EnsureThatAllInvoiceLinesHaveSameProcedureDetails()
		{
			var lineHasProcedure = Parent.InvoiceLines.Where(line => line.CusProcedure != null);
			var sourceLine = lineHasProcedure.FirstOrDefault();

			if (sourceLine != null)
			{
				var procedure = sourceLine.CusProcedure;
				var sourcePropertyInfos = new[]
				{
					procedure.ZZ6_IntoWarehouseInfo,
					procedure.ZZ6_OutOfWarehouseInfo,
					procedure.ZZ6_IntoTemporaryImportInfo,
					procedure.ZZ6_OutOfTemporaryImportInfo,
					procedure.ZZ6_IntoTemporaryExportInfo,
					procedure.ZZ6_OutOfTemporaryExportInfo,
					procedure.ZZ6_IntoInwardProcessingInfo,
					procedure.ZZ6_OutOfInwardProcessingInfo,
					procedure.ZZ6_IntoOutwardProcessingInfo,
					procedure.ZZ6_OutofOutwardProcessingInfo,
					procedure.ZZ6_IsGuaranteeConsumedInfo,
					procedure.ZZ6_IsGuaranteeReleasedInfo,
					procedure.ZZ6_IsTransitInfo
				};

				var sourceProcedureLineNo = sourceLine.JI_LineNo;
				var sourceProcedureProcedureCode = sourceLine.JI_Procedure;

				var messageBuilder = new ZStringBuilder();
				foreach (var targetLine in lineHasProcedure.Where(line => line.JI_Procedure != sourceProcedureProcedureCode))
				{
					if (!MatchProcedureProperties(sourcePropertyInfos, sourceProcedureLineNo, targetLine, messageBuilder))
					{
						Parent.CEI_StyleInfo.AddMessageError(Res.GetString("ab77a912-9511-4988-9dcf-854753c7a811", "All invoice lines should have matching procedure attributes:\r\n{0}", messageBuilder));
						break;
					}
				}
			}
		}

		bool MatchProcedureProperties(IEnumerable<ZPropertyInfo> sourcePropertyInfos, ZShort sourceLineNo, BaseJobComInvoiceLine targetLine, ZStringBuilder messageBuilder)
		{
			bool result = true;
			foreach (var sourcePropertyInfo in sourcePropertyInfos)
			{
				var targetPropertyInfo = targetLine.CusProcedure.FindPropertyInfo(sourcePropertyInfo.Name);
				var targetPropertyValue = targetPropertyInfo.Value;
				var sourcePropertyValue = sourcePropertyInfo.Value;

				if (targetPropertyValue.CompareTo(sourcePropertyValue) != 0)
				{
					messageBuilder.AppendLine(Res.GetString("e2844a6d-453a-44af-b64f-9ecd2ac3b5b3", "{0} is {1} on Invoice Line {2}, but is {3} on Invoice Line {4}",
						targetPropertyInfo.HumanReadableName, sourcePropertyValue, sourceLineNo, targetPropertyValue, targetLine.JI_LineNo));

					result = false;
				}
			}
			return result;
		}

		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();
			if ((Parent.CEI_OA_Warehouse.IsEmpty || Parent.Warehouse?.GetWhsWarehouse() == null)
				&& Parent.JobDeclaration is JobDeclaration declaration && declaration.IsBondedWarehouseAutomationOn && Parent.IsOutOfWarehouseWarehousing)
			{
				Parent.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("BDD24263-40F8-4EE8-998C-852563DE5CDE", "The {0}, must be entered and match a Warehouse record.", Parent.CEI_OA_WarehouseInfo.HumanReadableName));
			}
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			if ((Parent.CEI_OA_Warehouse2.IsEmpty || Parent.Warehouse2?.GetWhsWarehouse() == null)
				&& Parent.JobDeclaration is JobDeclaration declaration && declaration.IsBondedWarehouseAutomationOn && Parent.IsIntoWarehouseWarehousing)
			{
				Parent.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("D1701015-F7E0-4A7E-9CA9-84AA670C7D05", "The {0} must be entered and match a Warehouse record.", Parent.CEI_OA_Warehouse2Info.HumanReadableName));
			}
		}
	}
}
