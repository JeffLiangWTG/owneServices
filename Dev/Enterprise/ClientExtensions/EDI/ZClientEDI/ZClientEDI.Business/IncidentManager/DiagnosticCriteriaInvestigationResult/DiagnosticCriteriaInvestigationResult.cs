using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[DependentBusinessObject(typeof(DiagnosticCriteriaInvestigationItemLink), "InvestigationResultPivots")]
	public class DiagnosticCriteriaInvestigationResult : AutoDiagnosticCriteriaInvestigationResult
	{
		public DiagnosticCriteriaInvestigationResult(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DiagnosticCriteriaInvestigationItemLink DiagnosticCriteriaLink => Factory.Load<DiagnosticCriteriaInvestigationItemLink>(DCR_DIL_ParentLink);

		[RelatedBusinessObject("DiagnosticCriteriaLink")]
		public override ZGuid DCR_DIL_ParentLink { get => base.DCR_DIL_ParentLink; set => base.DCR_DIL_ParentLink = value; }

		public InvestigationItemResponseOption ResponseOption => Factory.Load<InvestigationItemResponseOption>(DCR_INR_ResponseOption);

		public ZString INR_ResponseOption => ResponseOption?.INR_ResponseOption ?? "";

		public virtual ZShort INR_Sequence
		{
			get => ResponseOption?.INR_Sequence ?? 0;
			set
			{
				ResponseOption.INR_Sequence = value;
			}
		}

		[RelatedBusinessObject("ResponseOption")]
		public override ZGuid DCR_INR_ResponseOption { get => base.DCR_INR_ResponseOption; set => base.DCR_INR_ResponseOption = value; }

		#region DCR_ResponseResult

		[List("Lookups.Results")]
		[MaxLength(3)]
		public override ZString DCR_ResponseResult
		{
			get => base.DCR_ResponseResult;
			set
			{
				base.DCR_ResponseResult = value;
				DCR_ResponseResultInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateDCR_ResponseResult();
				}
			}
		}

		[List("Lookups.Results")]
		[BusinessObjectTestExclude]
		public ZString DCR_ResponseResultDescription
		{
			get => Lookups.Results.GetDescriptionFromCode(DCR_ResponseResult);
			set
			{
				DCR_ResponseResult = Lookups.Results.GetCodeFromDescription(value);
				DCR_ResponseResultDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DCR_ResponseResultDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(DCR_ResponseResultDescription)); }
		}

		#endregion
	}
}
