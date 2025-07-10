using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperDate : IDocDates
	{
		internal AWBSendChileWrapperDate(AsycudaManifestHeader header, ZString dateName, ZString fArvFromPartial)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.dateName = dateName;
			this.fArvFromPartial = fArvFromPartial;
		}
		readonly AsycudaManifestHeader header;
		readonly string dateName;
		readonly ZString fArvFromPartial;

		string IDocDates.Name => dateName;

		string IDocDates.Value
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
					case WrappersConstants.DateName.Farribo:
						result = fArvFromPartial;
						break;
				}
				return result;
			}
		}
	}
}
