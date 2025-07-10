using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class VoyageDestinationAIRAARAmendmentGenerator : CMRAmendmentGenerator
	{
		public VoyageDestinationAIRAARAmendmentGenerator(CustomsVoyageDestinationWrapper destinationWrapper) : base(destinationWrapper)
		{
			this.DestinationWrapper = destinationWrapper;
		}

		#region Overridden Methods

		protected readonly CustomsVoyageDestinationWrapper DestinationWrapper;
		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo)
		{
			return new AIRAARMessageBuilder(bizo as CustomsVoyageDestinationWrapper);
		}

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo)
		{
			return (bizo as CustomsVoyageDestinationWrapper).Messages;
		}

		protected override ZPropertyInfo[] UniqueIdentifierInfos
		{
			get { throw new NotSupportedException("We do not go though this message to detect a unique key change"); }
		}

		protected override bool UniqueIdentifierBeingChanged
		{
			get
			{
				string factoryHash = GetUniqueKeyHash(DestinationWrapper);
				string dBHash = GetUniqueKeyHash(CustomsVoyageDestinationWrapper.Load(new BusinessObjectFactory(), DestinationWrapper.Destination.PK));
				return factoryHash != dBHash;
			}
		}

		string GetUniqueKeyHash(CustomsVoyageDestinationWrapper destinationWrapper)
		{
			if (destinationWrapper == null)
			{
				return ZString.Empty;
			}
			else
			{
				return destinationWrapper.FlightNo + destinationWrapper.DateTimeOfDepartureUTC.ToString("yyMMddhhmm") + destinationWrapper.LastOverseasPortOfDeparture + destinationWrapper.PortOfArrival;
			}
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			return CustomsVoyageDestinationWrapper.Load(new BusinessObjectFactory(), ((CustomsVoyageDestinationWrapper)businessObject).Destination.PK);
		}

		#endregion
	}
}
