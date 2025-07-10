using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEAOUTMessage : CMRMessage
	{
		public CMRSEAOUTMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SEAOUT;
		}

		#endregion

		public IEnumerable<SEAOUTMessageLineSentToCustoms> LineCollection
		{
			get
			{
				if (fLineCollection == null)
				{
					fLineCollection = new List<SEAOUTMessageLineSentToCustoms>();
					if (CUSCAR != null)
					{
						foreach (SegmentGroup7 group7 in CUSCAR.Group7)
						{
							fLineCollection.Add(new SEAOUTMessageLineSentToCustoms(group7));
						}
					}
				}
				return fLineCollection;
			}
		}
		List<SEAOUTMessageLineSentToCustoms> fLineCollection;
	}
}
