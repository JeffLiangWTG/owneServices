using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class NotificationForwardingParty : CusCodeDataWithSequenceNumberLine, ISynchroniserReadOnlyMembersProvider
	{
		public NotificationForwardingParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("27625B3F-3662-4AD6-9709-59C0457F87D1", "Notification Forwarding Party"); }
		}

		[ResourceStringData("JPNotificationForwardingParty|CY_Data", Caption = "Notification Forwarding Party", ShortCaption = "NFP", MediumCaption = "Notification Party")]
		[MaxLength(5)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		public new NotificationForwardingPartyLookups Lookups
		{
			get { return (NotificationForwardingPartyLookups)base.Lookups; }
		}

		public new NotificationForwardingPartyValidation Validation
		{
			get { return (NotificationForwardingPartyValidation)base.Validation; }
		}

		public const string NFPType = "NFP";

		#region Implementation

		protected override ShortSequenceNumberGenerator GetSequenceNumberGenerator(BusinessObject bizObj)
		{
			var header = (INotificationForwardingPartySequenceNumberHeader)bizObj;
			return header.SequenceNumberGenerator;
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new NotificationForwardingPartyLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new NotificationForwardingPartyValidation(this);
		}

		protected override string Type
		{
			get { return NFPType; }
		}

		#endregion

		#region ISynchroniserReadOnlyMembersProvider Members

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		#endregion
	}
}
