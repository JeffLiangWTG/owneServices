using System;
using System.Net;
using System.Text;

namespace CargoWise.IO
{
	[Serializable]
	public class FtpException : ApplicationException
	{
		public enum FtpExceptionType
		{
			Connect,
			Disconnect,
			GetFile,
			PutFile,
			AppendFile,
			Delete,
			ListRemoteFiles,
			CheckFileExists,
			RenameFile,
			Download,
			Upload,
			Timeout
		}

		public FtpException(FtpExceptionType exceptionType, string errorMessage, Exception innerException)
			: base(errorMessage, innerException)
		{
			this.Type = exceptionType;
		}

		public FtpException(FtpExceptionType exceptionType, string errorMessage)
			: base(errorMessage)
		{
			this.Type = exceptionType;
		}

#if NETFRAMEWORK
		protected FtpException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly FtpExceptionType Type;

		public string FullMessage
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(Message);
				WebException webException = InnerException as WebException;
				if (webException != null)
				{
					if (!string.IsNullOrEmpty(webException.Message))
					{
						if (builder.Length > 0)
						{
							builder.Append(Environment.NewLine);
						}

						builder.Append(webException.Message);
					}

					FtpWebResponse ftpResponse = webException.Response as FtpWebResponse;
					if (ftpResponse != null && !string.IsNullOrEmpty(ftpResponse.StatusDescription))
					{
						if (builder.Length > 0)
						{
							builder.Append(Environment.NewLine);
						}

						builder.Append(ftpResponse.StatusDescription);
					}
				}

				return builder.ToString();
			}
		}
	}
}
