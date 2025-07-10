using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderDocumentSupporter : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderDocumentSupporter
	{
		public CusTempStorageRegHeaderDocumentSupporter(CusTempStorageRegHeader parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;
	}
}
