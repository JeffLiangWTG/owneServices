using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class EdiOrgOpportunityValueAnalysisCollection : ActiveBusinessObjectCollection<EdiOrgOpportunityValueAnalysis>
	{
		public EdiOrgOpportunityValueAnalysisCollection(EDIOrgOpportunity master)
			: base(master.Factory, master, new ZQuery(), EdiOrgOpportunityValueAnalysisSchema.EOV_P8)
		{
			Master = master;
		}

		readonly EDIOrgOpportunity Master;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(EdiOrgOpportunityValueAnalysis newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.EOV_P8 = Master.PK;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		public void PopulateDefaultValues()
		{
			var valueAnalysisData = this.ToArray();
			var registryDefaults = EDIDataRegistry.Instance.OpportunityValueAnalysisDefaults.OfType<OpportunityValueAnalysisDefault>().ToArray();

			var missingData = registryDefaults.Where(regDef => !valueAnalysisData.Any(v => v.EOV_ModuleCode == regDef.Code)).ToArray();

			foreach (var data in missingData)
			{
				var newData = this.AddNew();
				newData.EOV_ModuleCode = data.Code;
			}
		}

		public ZDecimal TotalForeignValue
		{
			get { return this.Sum(x => x.EOV_Calc_ForeignValue); }
		}

		public ZDecimal TotalLocalValue
		{
			get { return this.Sum(x => x.EOV_Calc_LocalValue); }
		}
	}
}

