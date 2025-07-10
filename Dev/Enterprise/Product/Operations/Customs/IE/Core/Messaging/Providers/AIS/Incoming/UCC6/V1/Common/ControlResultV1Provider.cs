using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public sealed class ControlResultV1Provider
	{
		readonly ControlsType controlsType;

		public ControlResultV1Provider(ControlsType controlsType)
		{
			this.controlsType = controlsType;
		}

		public ZString ControlResultCode => controlsType.ControlResultCode;

		public ZDateTime ControlDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(controlsType.ControlDate);

		public ZString Remarks => controlsType.Remarks;
	}
}
