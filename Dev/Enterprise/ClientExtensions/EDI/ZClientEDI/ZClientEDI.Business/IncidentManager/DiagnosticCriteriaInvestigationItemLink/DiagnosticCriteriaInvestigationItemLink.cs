using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[DependentBusinessObject(typeof(IncidentDiagnosticCriteria), "InvestigationItemPivots")]
	public class DiagnosticCriteriaInvestigationItemLink : AutoDiagnosticCriteriaInvestigationItemLink
	{
		public DiagnosticCriteriaInvestigationItemLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public InvestigationItem InvestigationItem => Factory.Load<InvestigationItem>(DIL_INV_InvestigationItem);

		[RelatedBusinessObject("InvestigationItem")]
		public override ZGuid DIL_INV_InvestigationItem { get => base.DIL_INV_InvestigationItem; set => base.DIL_INV_InvestigationItem = value; }

		public IncidentDiagnosticCriteria IncidentDiagnosticCriteria => Factory.Load<IncidentDiagnosticCriteria>(DIL_IMD_DiagnosticCriteria);

		[RelatedBusinessObject("IncidentDiagnosticCriteria")]
		public override ZGuid DIL_IMD_DiagnosticCriteria { get => base.DIL_IMD_DiagnosticCriteria; set => base.DIL_IMD_DiagnosticCriteria = value; }

		[ChildEditable]
		public DiagnosticCriteriaLinkResponseResultPivotCollection InvestigationResultPivots
		{
			get
			{
				if (investigationResultPivots == null)
				{
					investigationResultPivots = new DiagnosticCriteriaLinkResponseResultPivotCollection(this, Factory);
					RegisterEditableChildObject(investigationResultPivots);
					investigationResultPivots.Load();
				}

				return investigationResultPivots;
			}
		}

		DiagnosticCriteriaLinkResponseResultPivotCollection investigationResultPivots;

		public ZString ConfirmOption
		{
			get
			{
				confirmOption = string.Empty;
				GetOption();

				return confirmOption;
			}
		}

		void GetOption()
		{
			var confirmOptions = InvestigationResultPivots
				.OfType<DiagnosticCriteriaInvestigationResult>()
				.Where(item => item.DCR_ResponseResult == "con")
				.Select(item => item.INR_Sequence)
				.ToList();

			var negateOptions = InvestigationResultPivots
				.OfType<DiagnosticCriteriaInvestigationResult>()
				.Where(item => item.DCR_ResponseResult == "neg")
				.Select(item => item.INR_Sequence)
				.ToList();

			confirmOption = confirmOptions.Count == 1 ? confirmOptions.First().ToString() : string.Join(",", confirmOptions);
			negateOption = negateOptions.Count == 1 ? negateOptions.First().ToString() : string.Join(",", negateOptions);
		}

		ZString confirmOption;

		public ZPropertyInfo ConfirmOptionInfo
		{
			get { return GetZPropertyInfo(nameof(ConfirmOption)); }
		}

		public ZString NegateOption
		{
			get
			{
				negateOption = string.Empty;
				GetOption();

				return negateOption;
			}
		}

		ZString negateOption;

		public ZPropertyInfo NegateOptionInfo
		{
			get { return GetZPropertyInfo(nameof(NegateOption)); }
		}
	}
}
