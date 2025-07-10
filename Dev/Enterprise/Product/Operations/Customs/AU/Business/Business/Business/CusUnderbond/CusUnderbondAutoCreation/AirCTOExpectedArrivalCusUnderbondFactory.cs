using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOExpectedArrivalCusUnderbondFactory : ExpectedArrivalCusUnderbondFactory
	{
		class CTOCusHAWBInformationProvider : ICTOCusHAWBInformationProvider
		{
			public CTOCusHAWBInformationProvider(CMRUBMREQRMessage message, int lineNumber)
			{
				this.message = message;
				this.lineNumber = lineNumber;
			}

			readonly CMRUBMREQRMessage message;
			readonly int lineNumber;

			#region ICTOCusHAWBInformationProvider Members

			public ZString MAWB
			{
				get
				{
					return message.GetMAWB(lineNumber);
				}
			}

			public ZDateTime ArrivalDate
			{
				get
				{
					return message.ArrivalDate;
				}
			}

			public ZString FlightNumber
			{
				get
				{
					return message.FlightNumber;
				}
			}

			#endregion
		}

		protected internal ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParentInternal(CMRUBMREQRMessage message, int lineNumber) => LoadOrCreateUnderbondParent(message, lineNumber);
		protected override ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParent(CMRUBMREQRMessage message, int lineNumber)
		{
			return CTOCusHAWB.Load(message.Factory, new CTOCusHAWBInformationProvider(message, lineNumber));
		}

		protected internal bool IsInterestedInUBMREQRInternal(CMRUBMREQRMessage message) => IsInterestedInUBMREQR(message);
		protected override bool IsInterestedInUBMREQR(CMRUBMREQRMessage message)
		{
			return message.IsAir && UnderbondRelatedToCTO(message) || AirCTORecordExists(message);
		}

		public bool AirCTORecordExists(CMRUBMREQRMessage message)
		{
			bool result = false;
			if (!((ICusHAWBInformationProvider)message).MAWB.IsEmpty)
			{
				ZQuery filter = new ZQuery(CusHAWBSchema.CS_HAWB, ((ICusHAWBInformationProvider)message).MAWB);
				result = message.Factory.LoadTop1<CTOCusHAWB>(filter) != null;
			}
			return result;
		}
	}
}
