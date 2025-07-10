using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[DebuggerDisplay("NotifySenderOnSuccessOrAcknowledgement: {NotifySenderOnSuccessOrAcknowledgement} NotifySenderOnError: {NotifySenderOnError} NotifySenderOnDiscrepancy: {NotifySenderOnDiscrepancy} NotifyGroupOnSuccessOrAcknowledgement: {NotifyGroupOnSuccessOrAcknowledgement} NotifyGroupOnError: {NotifyGroupOnError} NotifyGroupOnDiscrepancy: {NotifyGroupOnDiscrepancy} GroupForSuccessOrAcknowledgement: {GroupForSuccessOrAcknowledgement} GroupForErrorsAndDiscrepancies: {GroupForErrorsAndDiscrepancies}")]
	public class InboundMessageNotificationsRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string NotifyGroup = "NotifyGroup";
			public const string NotifyUserType = "NotifyUserType";
			public const string NotifyGroupWhenUserFound = "NotifyGroupWhenUserFound";
		}

		#endregion

		#region NotifyGroupWhenUserFound

		public ZBool NotifyGroupWhenUserFound
		{
			get { return notifyGroupWhenUserFound; }
			set
			{
				SetNonPersistentPropertyValue(NotifyGroupWhenUserFoundInfo, ref notifyGroupWhenUserFound, value);
			}
		}

		ZBool notifyGroupWhenUserFound;

		public ZPropertyInfo NotifyGroupWhenUserFoundInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyGroupWhenUserFound, "Notify Group when User found"); }
		}

		#endregion

		#region NotifyGroup

		[List("CompleteGroupList")]
		public ZGuid NotifyGroup
		{
			get { return notifyGroup; }
			set
			{
				SetNonPersistentPropertyValue(NotifyGroupInfo, ref notifyGroup, value);

				if (!IsValidationSuspended)
				{
					ValidateNotifyGroup();
				}

				NotifyGroupInfo.RefreshBinding();
			}
		}
		ZGuid notifyGroup;

		public ZPropertyInfo NotifyGroupInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyGroup, "Notify Group"); }
		}

		public IBusinessObjectCollection CompleteGroupList
		{
			get
			{
				return completeGroupList ?? (completeGroupList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IGlbGroupCollection>(), new object[] { CurrentFactory }));
			}
		}
		IBusinessObjectCollection completeGroupList;

		#endregion

		#region Notify User Type

		[MaxLength(3)]
		public ZString NotifyUserType
		{
			get { return notifyUserType; }
			set
			{
				SetNonPersistentPropertyValue(NotifyUserTypeInfo, ref notifyUserType, value);

				if (!IsValidationSuspended)
				{
					ValidateNotifyUsers();
				}

				NotifyGroupInfo.RefreshBinding();
			}
		}
		ZString notifyUserType;

		public ZPropertyInfo NotifyUserTypeInfo
		{
			get { return GetZPropertyInfo(Schema.NotifyUserType, "Notify User Type"); }
		}

		public CodeDescriptionPairList NotifyUserTypeList
		{
			get
			{
				return notifyUserTypeList ?? (notifyUserTypeList = new InboundMessageNotificationsNotifyUserTypeList());
			}
		}
		CodeDescriptionPairList notifyUserTypeList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateNotifyGroup();
			ValidateNotifyUsers();
		}

		void ValidateNotifyGroup()
		{
			NotifyGroupInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(NotifyGroupInfo, CompleteGroupList);
		}

		public void ValidateNotifyUsers()
		{
			NotifyUserTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(NotifyUserTypeInfo, new InboundMessageNotificationsNotifyUserTypeList());
		}

		public bool IsEmpty
		{
			get
			{
				return this.NotifyGroup == ZGuid.Empty &&
						 this.NotifyUserType == InboundMessageNotificationsNotifyUserTypeList.Codes.None &&
						 this.NotifyGroupWhenUserFound;
			}
		}

		#endregion

		#region Overriden Methods

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			NotifyGroup = ZGuid.Empty;
			NotifyGroupWhenUserFound = true;
			NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.None;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InboundMessageNotificationsRule();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.NotifyGroup, NotifyGroup.ToString());
			writer.WriteElementString(Schema.NotifyUserType, NotifyUserType.ToString());
			writer.WriteElementString(Schema.NotifyGroupWhenUserFound, NotifyGroupWhenUserFound.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			NotifyGroup = new ZGuid(reader.ReadElementString(Schema.NotifyGroup));
			NotifyUserType = new ZString(reader.ReadElementString(Schema.NotifyUserType));
			NotifyGroupWhenUserFound = new ZBool(reader.ReadElementString(Schema.NotifyGroupWhenUserFound));
		}

		#endregion
	}
}
