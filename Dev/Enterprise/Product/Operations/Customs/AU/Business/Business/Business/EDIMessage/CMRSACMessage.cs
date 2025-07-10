using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IMessageFunctionCode
	{
		bool IsOriginalMessage { get; }
	}

	public class CMRSACMessage : CMRCUSDECMessage, IPaymentIncluded, IMessageFunctionCode
	{
		public CMRSACMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IsOriginalMessage

		public bool IsOriginalMessage
		{
			get { return RevisionNumber == "9"; }
		}

		#endregion

		#region IPaymentIncluded Members

		GISSegmentMessageSection IPaymentIncluded.GISSegments
		{
			get { return CUSDEC != null ? CUSDEC.GIS : null; }
		}

		bool IPaymentIncluded.IsPaymentIncluded
		{
			get { return IsPaymentIncludedCalculator.IsPaymentIncluded; }
		}

		IsPaymentIncludedCalculator IsPaymentIncludedCalculator
		{
			get
			{
				if (fIsPaymentIncludedCalculator == null)
				{
					fIsPaymentIncludedCalculator = new IsPaymentIncludedCalculator(this);
				}
				return fIsPaymentIncludedCalculator;
			}
		}
		IsPaymentIncludedCalculator fIsPaymentIncludedCalculator;

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SAC;
		}

		#endregion

	}
}
