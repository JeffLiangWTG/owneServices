using System;
using Enterprise.ChangeDataCapture.Common;

namespace Enterprise.ChangeDataCapture.Service
{
	[Serializable]
	class CdcCleanupProcedureFailedException : CdcException
	{
		public CdcCleanupProcedureFailedException(string captureInstance, byte[] lowWaterMark, int deleteThreshold, int result)
			: base($"Cleanup of capture instance failed with below parameters:\r\n@capture_instance = {captureInstance}\r\n@low_water_mark = {lowWaterMark}\r\n@threshold = {deleteThreshold}\r\n@retVal = {result}")
		{
		}

#if NETFRAMEWORK
		public CdcCleanupProcedureFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
