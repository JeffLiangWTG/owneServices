using System;

using Enterprise.ZArchitecture;

namespace Enterprise.Customs.MY.Business
{
	[Serializable]
	public class MYDataErrorType : ErrorType
	{
		public MYDataErrorType(string message)
			: base(message)
		{
		}

		public static readonly MYDataErrorType UnknownPortOperator = new MYDataErrorType("Unknown port operator");
		public static readonly MYDataErrorType BerthNumberInvalid = new MYDataErrorType("Invalid berth number");
		public static readonly MYDataErrorType BerthNumberTooBig = new MYDataErrorType("Berth number too big");
		public static readonly MYDataErrorType TranshipmentError = new MYDataErrorType("Transhipment Problem");
	}
}
