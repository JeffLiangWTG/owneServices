using System;

using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT
{
	[Serializable]
	public class TNTErrorType : ErrorType
	{
		public TNTErrorType(string message)
			: base(message)
		{
		}

		public static readonly ErrorType InvalidFileName = new TNTErrorType(String.Format("Invalid file name.  File name should be in the format <Branch>.<File Type>.<Date>.<Time>{0}", QuantumFile.Extension));
		public static readonly ErrorType InvalidRecordDelimiter = new TNTErrorType(@"Invalid Record Delimiter.  Record Delimiter must contain a dot (""."").");
	}
}
