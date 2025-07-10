using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TempStorageRegisterLinesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.TempStorageRegisterLines;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TempStorageRegisterLinesFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TempStorageRegLinesFilterStripControl(GridCollection, (TempStorageRegisterLinesFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusTempStorageRegLineSelCollection(Factory, GetFilterByApplicationCodeQuery(ApplicationCodeFilter));

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EUTempStorageRegister;

		protected virtual ZString ApplicationCodeFilter => ZString.Empty;

		ZDBOnlyQuery GetFilterByApplicationCodeQuery(ZString appCode)
		{
			var query = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
			if(!appCode.IsEmpty)
			{
				var headerByAppCodeQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegHeader), CusTempStorageRegLineSchema.SRL_SRH);
				headerByAppCodeQuery.AddToFilter(CusTempStorageRegHeaderSchema.SRH_AppCode, appCode);
				query.AddSubQuery(headerByAppCodeQuery, JoinCondition.And);
			}
			return query;
		}
	}
}
