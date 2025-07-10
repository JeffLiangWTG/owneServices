using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[DebuggerDisplay("NotifySenderOnSuccessOrAcknowledgement: {NotifySenderOnSuccessOrAcknowledgement} NotifySenderOnError: {NotifySenderOnError} NotifySenderOnDiscrepancy: {NotifySenderOnDiscrepancy} NotifyGroupOnSuccessOrAcknowledgement: {NotifyGroupOnSuccessOrAcknowledgement} NotifyGroupOnError: {NotifyGroupOnError} NotifyGroupOnDiscrepancy: {NotifyGroupOnDiscrepancy} GroupForSuccessOrAcknowledgement: {GroupForSuccessOrAcknowledgement} GroupForErrorsAndDiscrepancies: {GroupForErrorsAndDiscrepancies}")]
	public class TrackAndTraceNotificationsRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string NotifySenderOnSuccessOrAcknowledgement = "NotifySenderOnSuccessOrAcknowledgement";
			public const string NotifySenderOnError = "NotifySenderOnError";
			public const string NotifySenderOnDiscrepancy = "NotifySenderOnDiscrepancy";
			public const string NotifyGroupOnSuccessOrAcknowledgement = "NotifyGroupOnSuccessOrAcknowledgement";
			public const string NotifyGroupOnError = "NotifyGroupOnError";
			public const string NotifyGroupOnDiscrepancy = "NotifyGroupOnDiscrepancy";
			public const string GroupForSuccessOrAcknowledgement = "GroupForSuccessOrAcknowledgement";
			public const string GroupForErrorsAndDiscrepancies = "GroupForErrorsAndDiscrepancies";
		}

		#endregion

		#region Constructors

		public TrackAndTraceNotificationsRule()
			: this(null)
		{
		}

		public TrackAndTraceNotificationsRule(TrackAndTraceNotificationsRuleVisibilityProvider visibilityProvider)
			: base()
		{
			this.VisibilityProvider = visibilityProvider;
		}

		#endregion

		#region Properties

		#region Visibility

		public TrackAndTraceNotificationsRuleVisibilityProvider VisibilityProvider { get; internal set; }

		#endregion

		#region SuccessOrAcknowledgment

		public ZBool NotifySenderOnSuccessOrAcknowledgement
		{
			get { return notifySenderOnSuccessOrAcknowledgement; }
			set { SetNonPersistentPropertyValue(NotifySenderOnSuccessOrAcknowledgementInfo, ref notifySenderOnSuccessOrAcknowledgement, value); }
		}
		ZBool notifySenderOnSuccessOrAcknowledgement;

		public ZPropertyInfo NotifySenderOnSuccessOrAcknowledgementInfo
		{
			get { return GetZPropertyInfo(Schema.NotifySenderOnSuccessOrAcknowledgement, "Notify Sender On Success Or Acknowledgement"); }
		}

		public ZBool NotifyGroupOnSuccessOrAcknowledgement
		{
			get { return notifyGroupOnSuccessOrAcknowledgement; }
			set
			{
				SetNonPersistentPropertyValue(NotifyGroupOnSuccessOrAcknowledgementInfo, ref notifyGroupOnSuccessOrAcknowledgement, value);

				if (!value)
				{
					GroupForSuccessOrAcknowledgement = ZGuid.Empty;
				}
			}
		}
		ZBool notifyGroupOnSuccessOrAcknowledgement;

		public ZPropertyInfo NotifyGroupOnSuccessOrAcknowledgementInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyGroupOnSuccessOrAcknowledgement, "Notify Group On Success Or Acknowledgement"); }
		}

		[List("CompleteGroupList")]
		public ZGuid GroupForSuccessOrAcknowledgement
		{
			get { return groupForSuccessOrAcknowledgement; }
			set
			{
				SetNonPersistentPropertyValue(GroupForSuccessOrAcknowledgementInfo, ref groupForSuccessOrAcknowledgement, value);

				if (!IsValidationSuspended)
				{
					ValidateGroupForSuccessOrAcknowledgement();
				}

				GroupForSuccessOrAcknowledgementInfo.RefreshBinding();
			}
		}
		ZGuid groupForSuccessOrAcknowledgement;

		public ZPropertyInfo GroupForSuccessOrAcknowledgementInfo
		{
			get { return GetZPropertyInfo(Schema.GroupForSuccessOrAcknowledgement, "Group for Success Or Acknowledgement"); }
		}

		protected bool GroupForSuccessOrAcknowledgement_ReadOnly
		{
			get { return !NotifyGroupOnSuccessOrAcknowledgement; }
		}

		#endregion

		#region ErrorsAndDiscrepancies

		public ZBool NotifySenderOnError
		{
			get { return notifySenderOnError; }
			set { SetNonPersistentPropertyValue(NotifySenderOnErrorInfo, ref notifySenderOnError, value); }
		}
		ZBool notifySenderOnError;

		public ZPropertyInfo NotifySenderOnErrorInfo
		{
			get { return GetZPropertyInfo(Schema.NotifySenderOnError, "Notify Sender On Error"); }
		}

		public ZBool NotifySenderOnDiscrepancy
		{
			get { return notifySenderOnDiscrepancy; }
			set { SetNonPersistentPropertyValue(NotifySenderOnDiscrepancyInfo, ref notifySenderOnDiscrepancy, value); }
		}
		ZBool notifySenderOnDiscrepancy;

		public ZPropertyInfo NotifySenderOnDiscrepancyInfo
		{
			get { return GetZPropertyInfo(Schema.NotifySenderOnDiscrepancy, "Notify Sender On Discrepancy"); }
		}

		public ZBool NotifyGroupOnError
		{
			get { return notifyGroupOnError; }
			set
			{
				SetNonPersistentPropertyValue(NotifyGroupOnErrorInfo, ref notifyGroupOnError, value);

				if (!value && !NotifyGroupOnDiscrepancy)
				{
					GroupForErrorsAndDiscrepancies = ZGuid.Empty;
				}
			}
		}
		ZBool notifyGroupOnError;

		public ZPropertyInfo NotifyGroupOnErrorInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyGroupOnError, "Notify Group On Error"); }
		}

		public ZBool NotifyGroupOnDiscrepancy
		{
			get { return notifyGroupOnDiscrepancy; }
			set
			{
				SetNonPersistentPropertyValue(NotifyGroupOnDiscrepancyInfo, ref notifyGroupOnDiscrepancy, value);

				if (!value && !NotifyGroupOnError)
				{
					GroupForErrorsAndDiscrepancies = ZGuid.Empty;
				}
			}
		}
		ZBool notifyGroupOnDiscrepancy;

		public ZPropertyInfo NotifyGroupOnDiscrepancyInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyGroupOnDiscrepancy, "Notify Group On Discrepancy"); }
		}

		[List("CompleteGroupList")]
		public ZGuid GroupForErrorsAndDiscrepancies
		{
			get { return groupForErrorsAndDiscrepancies; }
			set
			{
				SetNonPersistentPropertyValue(GroupForErrorsAndDiscrepanciesInfo, ref groupForErrorsAndDiscrepancies, value);

				if (!IsValidationSuspended)
				{
					ValidateGroupForErrorsAndDiscrepancies();
				}

				GroupForErrorsAndDiscrepanciesInfo.RefreshBinding();
			}
		}
		ZGuid groupForErrorsAndDiscrepancies;

		public ZPropertyInfo GroupForErrorsAndDiscrepanciesInfo
		{
			get { return GetZPropertyInfo(Schema.GroupForErrorsAndDiscrepancies, "Group For Errors And Discrepancies"); }
		}

		protected bool GroupForErrorsAndDiscrepancies_ReadOnly
		{
			get { return !NotifyGroupOnError && !NotifyGroupOnDiscrepancy; }
		}

		#endregion

		#endregion

		#region Complete Group List

		public IBusinessObjectCollection CompleteGroupList
		{
			get { return completeGroupList ?? (completeGroupList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IGlbGroupCollection>(), new object[] { CurrentFactory })); }
		}
		IBusinessObjectCollection completeGroupList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateGroupForSuccessOrAcknowledgement();
			ValidateGroupForErrorsAndDiscrepancies();
		}

		void ValidateGroupForSuccessOrAcknowledgement()
		{
			GroupForSuccessOrAcknowledgementInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(GroupForSuccessOrAcknowledgementInfo, CompleteGroupList);

			if (NotifyGroupOnSuccessOrAcknowledgement)
			{
				MandatoryValidation.CheckEntered(GroupForSuccessOrAcknowledgementInfo);
			}
		}

		void ValidateGroupForErrorsAndDiscrepancies()
		{
			GroupForErrorsAndDiscrepanciesInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(GroupForErrorsAndDiscrepanciesInfo, CompleteGroupList);

			if (NotifyGroupOnError || NotifyGroupOnDiscrepancy)
			{
				MandatoryValidation.CheckEntered(GroupForErrorsAndDiscrepanciesInfo);
			}
		}

		#endregion

		#region Overriden Methods

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			NotifyGroupOnDiscrepancy = false;
			NotifyGroupOnError = false;
			NotifyGroupOnSuccessOrAcknowledgement = false;
			NotifySenderOnDiscrepancy = true;
			NotifySenderOnError = true;
			NotifySenderOnSuccessOrAcknowledgement = true;
			GroupForErrorsAndDiscrepancies = ZGuid.Empty;
			GroupForSuccessOrAcknowledgement = ZGuid.Empty;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TrackAndTraceNotificationsRule(VisibilityProvider);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.NotifyGroupOnDiscrepancy, NotifyGroupOnDiscrepancy.ToString());
			writer.WriteElementString(Schema.NotifyGroupOnError, NotifyGroupOnError.ToString());
			writer.WriteElementString(Schema.NotifyGroupOnSuccessOrAcknowledgement, NotifyGroupOnSuccessOrAcknowledgement.ToString());
			writer.WriteElementString(Schema.NotifySenderOnDiscrepancy, NotifySenderOnDiscrepancy.ToString());
			writer.WriteElementString(Schema.NotifySenderOnError, NotifySenderOnError.ToString());
			writer.WriteElementString(Schema.NotifySenderOnSuccessOrAcknowledgement, NotifySenderOnSuccessOrAcknowledgement.ToString());
			writer.WriteElementString(Schema.GroupForErrorsAndDiscrepancies, GroupForErrorsAndDiscrepancies.ToString());
			writer.WriteElementString(Schema.GroupForSuccessOrAcknowledgement, GroupForSuccessOrAcknowledgement.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			NotifyGroupOnDiscrepancy = new ZBool(reader.ReadElementString(Schema.NotifyGroupOnDiscrepancy));
			NotifyGroupOnError = new ZBool(reader.ReadElementString(Schema.NotifyGroupOnError));
			NotifyGroupOnSuccessOrAcknowledgement = new ZBool(reader.ReadElementString(Schema.NotifyGroupOnSuccessOrAcknowledgement));
			NotifySenderOnDiscrepancy = new ZBool(reader.ReadElementString(Schema.NotifySenderOnDiscrepancy));
			NotifySenderOnError = new ZBool(reader.ReadElementString(Schema.NotifySenderOnError));
			NotifySenderOnSuccessOrAcknowledgement = new ZBool(reader.ReadElementString(Schema.NotifySenderOnSuccessOrAcknowledgement));
			GroupForErrorsAndDiscrepancies = new ZGuid(reader.ReadElementString(Schema.GroupForErrorsAndDiscrepancies));
			GroupForSuccessOrAcknowledgement = new ZGuid(reader.ReadElementString(Schema.GroupForSuccessOrAcknowledgement));
		}

		#endregion
	}
}
