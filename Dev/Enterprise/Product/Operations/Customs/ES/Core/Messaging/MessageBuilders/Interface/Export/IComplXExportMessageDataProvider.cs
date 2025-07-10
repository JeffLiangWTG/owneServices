using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IComplXExportMessageDataProvider : IExportMessageDataProviderCommon
	{
		ZString MessageType { get; }

		#region Fields For CST

		ZString CustomsProcedureCategory5 { get; }

		#endregion

		#region Fields For Goods

		IReadOnlyCollection<IComplXExportLine> Lines { get; }

		#endregion
	}

	public interface IComplXExportLine : IExportLineCommon
	{
		IReadOnlyCollection<IExportDocumentCommon> Documents { get; }
	}
}
