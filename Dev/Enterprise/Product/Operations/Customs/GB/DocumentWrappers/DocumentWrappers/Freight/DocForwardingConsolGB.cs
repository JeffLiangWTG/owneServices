using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Freight
{
	public class DocForwardingConsolGB : DocForwardingConsol
	{
		protected DocForwardingConsolGB(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
			: base(consol, factoryToWrap)
		{
			localFactory = factoryToWrap;
			forwardingConsol = consol;
		}

		public new static DocForwardingConsolGB New(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
		{
			DocForwardingConsolGB result = null;
			if (consol != null)
			{
				result = new DocForwardingConsolGB(consol, factoryToWrap);
			}
			return result;
		}

		/// <summary>
		/// Sum of Declarations', not Shipments', pieces
		/// </summary>
		public new ZString Packages
		{
			get { return (from DeclarationWrapper d in Declarations select (int)d.AdsParticipant.TotalNoOfPacks).Sum().ToString(); }
		}

		/// <summary>
		/// Sum of Declarations', not Shipments', masses
		/// </summary>
		public new ZString TotalWeight
		{
			get { return (from DeclarationWrapper d in Declarations select (int)(d.AdsParticipant.TotalWeightAds)).Sum().ToString(); }
		}

		public ZInt EntriesCount
		{
			get { return (from DeclarationWrapper w in Declarations select w.AdsParticipant.DeclarationsCount).Sum(); }
		}

		protected override ZString CtStatusCore
		{
			get
			{
				var sortedList = (from DeclarationWrapper w in Declarations select w.AdsParticipant.CtStatus).OrderBy(n => n, new CommunityTransitStatusComparer());
				return sortedList.FirstOrDefault();
			}
		}
		public DeclarationWrapperCollection Declarations
		{
			get { return declarations ?? (declarations = new DeclarationWrapperCollection(forwardingConsol, localFactory)); }
		}

		DeclarationWrapperCollection declarations;
		readonly BusinessObjectFactory localFactory;
		readonly ForwardingConsol forwardingConsol;
	}
}
