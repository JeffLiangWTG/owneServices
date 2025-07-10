using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobVoyageAIRIARAmendmentGenerator : CMRAmendmentGenerator
	{
		public JobVoyageAIRIARAmendmentGenerator(CustomsJobVoyageWrapper voyageWrapper) : base(voyageWrapper)
		{
			this.VoyageWrapper = voyageWrapper;
		}

		#region Overridden Methods

		protected readonly CustomsJobVoyageWrapper VoyageWrapper;
		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo)
		{
			return new AIRIARMessageBuilder(bizo as CustomsJobVoyageWrapper);
		}

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo)
		{
			return (bizo as CustomsJobVoyageWrapper).Messages;
		}

		protected override ZPropertyInfo[] UniqueIdentifierInfos
		{
			get { throw new NotSupportedException("We do not go though this message to detect a unique key change"); }
		}

		protected override bool UniqueIdentifierBeingChanged
		{
			get
			{
				string factoryHash = GetUniqueKeyHash(VoyageWrapper);
				string dBHash = GetUniqueKeyHash(CustomsJobVoyageWrapper.Load(new BusinessObjectFactory(), VoyageWrapper.Voyage.PK));
				return factoryHash != dBHash;
			}
		}

		string GetUniqueKeyHash(CustomsJobVoyageWrapper voyageWrapper)
		{
			if (voyageWrapper == null)
			{
				return ZString.Empty;
			}
			else
			{
				return voyageWrapper.FlightNo + voyageWrapper.DateTimeOfDepartureUTC.ToString("yyMMddhhmm") + voyageWrapper.LastOverseasPortOfDeparture;
			}
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			return CustomsJobVoyageWrapper.Load(new BusinessObjectFactory(), ((CustomsJobVoyageWrapper)businessObject).Voyage.PK);
		}

		#endregion
	}
}
