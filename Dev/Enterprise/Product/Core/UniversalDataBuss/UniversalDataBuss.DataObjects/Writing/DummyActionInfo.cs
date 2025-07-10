using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Writing
{
	public class DummyActionInfo : IUniversalActionInfo
	{
		public ZString ActionType
		{
			get { return ""; }
		}

		public BusinessObjectFactory FactoryForProcessing
		{
			get;
			set;
		}

		public BusinessObject ParentBO
		{
			get { return null; }
		}

		public IStmALog TriggeringEvent
		{
			get { return null; }
		}

		public ZString PurposeCode
		{
			get { return ""; }
		}

		public RecipientRoleDetail[] RecipientRoleDetails
		{
			get { return Array.Empty<RecipientRoleDetail>(); }
		}

		public ZDateTimeOffset TriggerActualDate
		{
			get { return ZDateTimeOffset.Empty; }
		}

		public ZInt TriggerCount
		{
			get { return 0; }
		}

		public ZString TriggerDescription
		{
			get { return ""; }
		}

		public ZString TriggerEventCode
		{
			get { return ""; }
		}

		public ZString TriggerReference
		{
			get { return ""; }
		}

		public ZDateTimeOffset TriggerScheduledDate
		{
			get { return ZDateTimeOffset.Empty; }
		}

		public TriggerType TriggerType
		{
			get { return TriggerType.Manual; }
		}

		public void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode) { }

		public IOrgHeader RecipientOrganization => null;

		public INotifications Notifications { get; set; }
	}
}
