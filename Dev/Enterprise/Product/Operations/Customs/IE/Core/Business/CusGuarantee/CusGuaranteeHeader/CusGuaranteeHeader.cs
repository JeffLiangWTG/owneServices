using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Business
{
	public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.IE.ICusGuaranteeHeader, IMessageAttachee, IRelatedJob
	{
		public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbBranch Branch => GlbBranch.CurrentBranch;

		public IRelatedJob RelatedJob => this;

		public GlbStaff CustomsAgent => null;

		public ZString LogicalStatus { get; set; }
		public ZString EntryStatus { get => null; set => throw new NotImplementedException(); }

		public ZString MovementReferenceNumber => null;

		public ZString JobNumber => HumanReadableNameCore;

		public ZString JobDescription => null;

		public ZString JobStatus => null;

		IEnumerable<Enterprise.Messaging.Business.EDIMessage> IMessageAttachee.Messages => Messages.Cast<Enterprise.Messaging.Business.EDIMessage>();

		protected override bool SupportsMessagesCore => true;
	}
}
