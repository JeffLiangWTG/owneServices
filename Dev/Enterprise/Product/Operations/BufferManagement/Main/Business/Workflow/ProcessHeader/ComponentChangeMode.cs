using System;

namespace Enterprise.BufferManagement.Business
{
	public enum ComponentChangeMode
	{
		ManualTransfer,
		Defer,
		SchematicTransfer,
		ReleaseGate,
		ResponsiveTransfer
	}

	public static class ComponentChangeModeExtensions
	{
		public static string ToCode(this ComponentChangeMode mode)
		{
			switch (mode)
			{
				case ComponentChangeMode.ManualTransfer:
					return TransferTypeList.Codes.ManualTransfer;

				case ComponentChangeMode.Defer:
					return TransferTypeList.Codes.Defer;

				case ComponentChangeMode.SchematicTransfer:
					return TransferTypeList.Codes.SchematicTransfer;

				case ComponentChangeMode.ReleaseGate:
					return TransferTypeList.Codes.ReleaseGate;

				case ComponentChangeMode.ResponsiveTransfer:
					return TransferTypeList.Codes.ResponsiveTransfer;

				default:
					throw new ArgumentException("Invalid ComponentChangeMode: " + mode);
			}
		}
	}
}
