using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CSARevenueSummaryFormController : StatementController
	{
		public override ControllerID ID => ControllerIDs.Customs.CA.CACSARevenueSummaryForm;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.CACSARevenueSummaryForm;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CACSARevenueSummaryFormView;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = Factory.New<CusStatementHeader>();
			result.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity) => new CSARevenueSummaryFormForm((CusStatementHeader)businessEntity);
	}
}
