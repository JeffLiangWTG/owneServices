using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.Business
{
	public class EDIInterchangeEDIMessageCollection : DependentBusinessObjectCollection<EDIMessage, EDIInterchange>
	{
		public EDIInterchangeEDIMessageCollection(EDIInterchange master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		public virtual new EDIMessage AddNew(Type bizoType)
		{
			return base.AddNew(bizoType);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var interchange = Master;
			EDIMessage messageAdded = (EDIMessage)bizOAdded;
			if (messageAdded.EM_ApplicationCode.IsEmpty)
			{
				messageAdded.EM_ApplicationCode = interchange.EI_ApplicationCode;
			}

			if (messageAdded.EM_GB.IsEmpty)
			{
				messageAdded.EM_GB = interchange.EI_GB;
			}

			if (messageAdded.EM_GE.IsEmpty)
			{
				messageAdded.EM_GE = GlbDepartment.CurrentDepartment.PK;
			}

			if (messageAdded.EM_MessageType.IsEmpty)
			{
				messageAdded.EM_MessageType = interchange.EI_ApplicationCode;
			}

			if (messageAdded.EM_Status.IsEmpty)
			{
				messageAdded.EM_Status = EDIMessage.Status.Queued;
			}

			if (messageAdded.EM_MessageSubType.IsEmpty)
			{
				messageAdded.EM_MessageSubType = "XXX";
			}

			if (messageAdded.EM_TransportType.IsEmpty)
			{
				messageAdded.EM_TransportType = interchange.EI_TransportType;
			}
		}

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			EDIMessage message = child as EDIMessage;
			if (message != null)
			{
				message.EM_EI = Master.PK;
			}
		}

		#endregion
	}
}
