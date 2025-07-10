using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Messaging.Business
{
	/// <summary>
	/// Summary description for EDIMessageFlattenedCollection.
	/// </summary>
	public class EDIMessageFlattenedCollection : BusinessObjectCollection<EDIMessage>
	{
		internal EDIMessageFlattenedCollection(EDIMessageCollection baseCollection, ZQuery interchangeResponseFilter, ZString applicationCode)
			: base(baseCollection.Factory)
		{
			this.BaseCollection = baseCollection;
			this.InterchangeResponseFilter = interchangeResponseFilter;
			this.applicationCode = applicationCode;
			foreach (EDIMessage message in baseCollection)
			{
				AddMessageAndAnyInterchangeResponses(message);
			}

			HookEvents();
		}
		readonly ZString applicationCode;

		#region Implementation

		public override void Load()
		{
			BaseCollection.Load();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(EDIMessage);
		}

		protected void HookEvents()
		{
			if (!EventsHooked)
			{
				BaseCollection.CountChanged += new CollectionCountChangedEventHandler(BaseCollection_CountChanged);
				EventsHooked = true;
			}
		}
		protected bool EventsHooked;

		protected EDIMessageCollection BaseCollection;
		protected ZQuery InterchangeResponseFilter;

		protected void BaseCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				AddMessageAndAnyInterchangeResponses((EDIMessage)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				RemoveMessageAndAnyInterchangeResponses((EDIMessage)e.BizObject);
			}
		}

		protected void AddMessageAndAnyInterchangeResponses(EDIMessage message)
		{
			if (applicationCode.IsEmpty || applicationCode == message.EM_ApplicationCode)
			{
				EDIInterchange interchange = message.Interchange;
				if (interchange != null)
				{
					foreach (EDIMessage interchangeMessage in interchange.InterchangeAcknowledgementMessages)
					{
						if (InterchangeResponseMatchesFilter(interchangeMessage))
						{
							Add(interchangeMessage);
						}
					}
				}
				Add(message);
			}
		}

		protected void RemoveMessageAndAnyInterchangeResponses(EDIMessage message)
		{
			EDIInterchange interchange = message.Interchange;
			if (interchange != null)
			{
				foreach (EDIMessage interchangeResponseMessage in interchange.InterchangeAcknowledgementMessages)
				{
					Remove(interchangeResponseMessage);
				}
			}
			Remove(message);
		}

		protected bool InterchangeResponseMatchesFilter(EDIMessage interchangeResponse)
		{
			return InterchangeResponseFilter == null || interchangeResponse.MatchesFilter(InterchangeResponseFilter);
		}

		#endregion
	}
}
