using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using CargoWise.Types;

namespace Enterprise.Client.WCB
{
#if NETFRAMEWORK
	[Serializable]
#endif
	public class WCBException : Exception
	{
		public WCBException(WCBExceptionType type)
		{
			this.Type = type;
			SetMessage(ZString.Empty);
		}

		public WCBException(WCBExceptionType type, ZString additionalInfo)
		{
			this.Type = type;
			SetMessage(additionalInfo);
		}

#if NETFRAMEWORK
		protected WCBException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public enum WCBExceptionType
		{
			NoInvoiceHeaderFound,
			NoPortCodeFound,
			InvalidFileFormat,
			NoInvoiceLinesForHeader
		}

		void SetMessage(ZString additionalInfo)
		{
			switch (this.Type)
			{
				case WCBExceptionType.NoInvoiceHeaderFound:
					Message = string.Format("Invoice Header Key ({0}) Not Found", additionalInfo);
					break;

				case WCBExceptionType.InvalidFileFormat:
					Message = "Invalid Header and/or Footer Records";
					break;

				case WCBExceptionType.NoInvoiceLinesForHeader:
					Message = string.Format("No Invoice Lines For Header ({0})", additionalInfo);
					break;

				case WCBExceptionType.NoPortCodeFound:
					Message = string.Format("No Port Code Found in Line ({0})", additionalInfo);
					break;
			}
		}

		public new string Message
		{
			get
			{
				return message;
			}
			private set { message = value; }
		}
		string message;

		public WCBExceptionType Type
		{
			get { return type; }
			private set { type = value; }
		}
		WCBExceptionType type;
	}
}
