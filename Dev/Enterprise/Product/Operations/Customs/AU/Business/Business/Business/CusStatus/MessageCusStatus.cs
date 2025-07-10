using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MessageCusStatus : CalculatedCusStatus
	{
		public MessageCusStatus(ZPropertyInfo wrappedPropertyInfo, ICalculatedCusStatusCalculator calculator)
			: base(wrappedPropertyInfo, calculator, new CMRBaseStatuses(), CMRBaseStatuses.Codes.NotSent)
		{
		}

		#region Constants

		public const string WaitingStartsWith = "W";
		public const string AcceptedStartsWith = "A";
		public const string RejectedStartsWith = "R";

		#endregion

		#region Properties

		#region IsWaiting

		public bool IsWaiting
		{
			get { return this.Code.StartsWith(WaitingStartsWith); }
		}

		#endregion

		#region IsAccepted

		public bool IsAccepted
		{
			get { return this.Code.StartsWith(AcceptedStartsWith); }
		}

		#endregion

		#region IsRejected

		public bool IsRejected
		{
			get { return this.Code.StartsWith(RejectedStartsWith); }
		}

		#endregion

		#region IsNotSent

		public bool IsNotSent
		{
			get { return this.Code == CMRBaseStatuses.Codes.NotSent; }
		}

		#endregion

		#endregion
	}
}
