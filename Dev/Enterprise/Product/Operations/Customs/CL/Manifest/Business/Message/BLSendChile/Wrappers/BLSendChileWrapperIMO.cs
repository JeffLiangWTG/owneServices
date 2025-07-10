using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperIMO : IIMO
	{
		internal BLSendChileWrapperIMO(UNDGDataItem uNDGDataItem)
		{
			undg = Argument.NotNull(uNDGDataItem, "UNDGDataItem cannot be null");
		}
		readonly UNDGDataItem undg;

		string IIMO.Class => undg.DI_IMOClass;

		string IIMO.Number => undg.SubstanceCode;
	}
}
