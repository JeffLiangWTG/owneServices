using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyCMRDepotMessage : ICMRDepotMessage
	{
		public DummyCMRDepotMessage(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		#region ICMRDepotMessage Members

		ZString ICMRDepotMessage.LloydsNumber
		{
			get { return lloydsNumber; }
		}
		public ZString lloydsNumber;

		ZString ICMRDepotMessage.VoyageNumber
		{
			get { return voyageNumber; }
		}
		public ZString voyageNumber;

		ZString ICMRDepotMessage.OriginPremiseID
		{
			get { return originPremiseID; }
		}
		public ZString originPremiseID;

		ZString ICMRDepotMessage.DestinationPremiseID
		{
			get { return destinationPremiseID; }
		}
		public ZString destinationPremiseID;

		ZString ICMRDepotMessage.OurPremiseID
		{
			get { return ourPremiseID; }
		}
		public ZString ourPremiseID;

		CMRDepotMessageType ICMRDepotMessage.MessageType
		{
			get { return messageType; }
		}
		public CMRDepotMessageType messageType;

		ICMRDepotMessageLine[] ICMRDepotMessage.Lines
		{
			get { return lines.ToArray(); }
		}
		public List<DummyCMRDepotMessageLine> lines = new List<DummyCMRDepotMessageLine>();

		BusinessObjectFactory ICMRDepotMessage.Factory
		{
			get { return factory; }
		}
		public BusinessObjectFactory factory;

		CMRCUSRESMessage ICMRDepotMessage.LinkOrCloneMessage(BusinessObject businessObjectToLink)
		{
			objectsToLink.Add(businessObjectToLink);
			return null;
		}
		public List<BusinessObject> objectsToLink = new List<BusinessObject>();

		bool ICMRDepotMessage.IsSea
		{
			get { return isSea; }
		}
		public bool isSea;

		void ICMRDepotMessage.AddUnmatchedContainer(CARSTRecord carstRecord)
		{
			this.AddUnmatchedContainer(carstRecord, UnmatchedContainers);
		}

		public Dictionary<string, List<CARSTRecord>> UnmatchedContainers
		{
			get { return unmatchedContainers ?? (unmatchedContainers = new Dictionary<string, List<CARSTRecord>>()); }
		}
		Dictionary<string, List<CARSTRecord>> unmatchedContainers;

		#endregion
	}
}
