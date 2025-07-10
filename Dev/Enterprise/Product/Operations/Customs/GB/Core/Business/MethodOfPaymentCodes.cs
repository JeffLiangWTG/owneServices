using CargoWise.Types;

namespace Enterprise.Customs.GB.Business
{
	public static class MethodOfPaymentCodes
	{
		public const string A = "A";
		public const string B = "B";
		public const string C = "C";
		public const string D = "D";
		public const string E = "E";
		public const string F = "F";
		public const string H = "H";
		public const string N = "N";
		public const string P = "P";
		public const string Q = "Q";
		public const string R = "R";
		public const string S = "S";
		public const string T = "T";
		public const string U = "U";
		public const string V = "V";
		public const string W = "W";
		public const string X = "X";
		public const string Y = "Y";
		public const string Z = "Z";

		public static class CDS
		{
			public static ZString[] DeferredMethodsOfPayment => new ZString[] { E, R };
			public static ZString[] GuaranteeDeferredMethodsOfPayment => new ZString[] { R, S, T, U, V, Z };
			public static ZString[] ImmediateMethodsOfPayment => new ZString[] { A, B, C, H, P };
			public static ZString[] ImmediateCashMethodsOfPayment => new ZString[] { A, P };
		}

		public static class CHIEF
		{
			public static ZString[] DeferredMethodsOfPayment => new ZString[] { F, Q };
			public static ZString[] GuaranteeDeferredMethodsOfPayment => new ZString[] { Q, S, T, U, V, W, X, Y };
			public static ZString[] ImmediateMethodsOfPayment => new ZString[] { A, D };
		}
	}
}
