using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperDate : IDocumentDate
	{
		internal BLSendChileWrapperDate(AsycudaManifestHeader header, ZString dateName)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.dateName = dateName;
		}
		readonly AsycudaManifestHeader header;
		readonly string dateName;

		string IDocumentDate.Name => dateName;

		string IDocumentDate.Value
		{
			get
			{
				var result = ZDate.Today.ToString(WrappersConstants.DateFormatLong);
				switch (dateName)
				{
					case WrappersConstants.DateName.Fpres:
						result = Env.Time.CurrentLocalDate.ToString(WrappersConstants.DateFormatLong);
						break;
					case WrappersConstants.DateName.Fem:
						result = header.AMA_MasterBillIssueDate.ToString(WrappersConstants.DateFormatShort);
						break;
					case WrappersConstants.DateName.Fzarpe:
						result = header.AMA_E_DEP.ToString(WrappersConstants.DateFormatLong);
						break;
					case WrappersConstants.DateName.Femb:
						result = header.AMA_E_DEP.ToString(WrappersConstants.DateFormatLong);
						break;
				}
				return result;
			}
		}
	}
}
