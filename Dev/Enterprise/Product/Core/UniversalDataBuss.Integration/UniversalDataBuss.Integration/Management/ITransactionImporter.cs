using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ITransactionImporter
	{
		bool ImportTransaction(IEDIMessage message, ITopLevelDataObject dataObject, IXmlImportLogger logger, IUniversalObjectFactory factory);
		IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlImportLogger logger, IUniversalObjectFactory factory);

		void ImportTransactionLines(ZString xml, IAccTransactionHeader transaction, bool isCrossLedgerImport);

		Tuple<ITopLevelDataObject, ICodeMappingManager> ImportUniversalTransactionFromXml(ZString xml, BusinessObjectFactory factory, bool isCrossLedgerImport);

		void TrySetMappedValueDirectlyFromSourceCode(ITopLevelDataObject universalTransaction);

		void TrySetMappedValueDirectlyFromSourceCode(IDataObject universalLine, BusinessObject line);

		IOrgHeader GetOrganization(ITopLevelDataObject universalTransaction, IUniversalObjectFactory universalFactory, IXmlImportLogger logger, bool isCrossLedgerImport);

		IJobHeader GetJob(BusinessObjectFactory factory, ITopLevelDataObject universalTransaction, IDataObject universalLine);

		BusinessObject GetConsol(ITopLevelDataObject universalTransaction, ZString consolNumber, ZString? consolType);
	}
}
