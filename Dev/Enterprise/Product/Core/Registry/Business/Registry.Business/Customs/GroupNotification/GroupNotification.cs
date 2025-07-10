using System;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GroupNotification : RegistryBusinessObjectTemplate
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052: WI00790489 - Class is inherited and cannot be static")]
		public class Schema
		{
			public const string SendMode = nameof(GroupNotification.SendMode);
			public const string SendGroupPK = nameof(GroupNotification.SendGroupPK);
		}

		public GroupNotification()
		{
		}

		public GroupNotification(ZString sendMode, ZGuid sendGroupPK)
		{
			this.SendMode = sendMode;
			this.SendGroupPK = sendGroupPK;
		}

		public GroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public static GroupNotification Default
		{
			get { return new GroupNotification(StaffMemberOrNominatedGroup, Core.Constants.Groups.PostMastersGroupPK); }
		}

		[List(nameof(SendModeList))]
		[MaxLength(3)]
		public ZString SendMode
		{
			get { return sendMode; }
			set
			{
				SetNonPersistentPropertyValue(SendModeInfo, ref sendMode, value);
				if (!IsValidationSuspended)
				{
					ValidateSendMode();
				}
				if (SendMode == Core.Constants.EmailTo.NoEmails || SendMode == Core.Constants.EmailTo.StaffMember)
				{
					SendGroupPK = ZGuid.Empty;
				}
			}
		}
		ZString sendMode;

		public ZPropertyInfo SendModeInfo
		{
			get { return GetZPropertyInfo(Schema.SendMode); }
		}

		void ValidateSendMode()
		{
			SendModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SendModeInfo);
			ListValidation.ErrorIfInvalidCode(SendModeInfo);
			if (DoNotSendToStaffMembers)
			{
				if (SendMode == Core.Constants.EmailTo.StaffMember || SendMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup || SendMode == StaffMemberOrNominatedGroup)
				{
					SendModeInfo.AddError(Res.GetString("8ba29091-45f1-45b0-886e-46c3be4991cd", "Staff Member Notification is not valid for this registry."));
				}
			}
		}

		public bool DoNotSendToStaffMembers { get; set; }

		[List(nameof(SendGroupList))]
		public ZGuid SendGroupPK
		{
			get { return sendGroupPK; }
			set
			{
				SetNonPersistentPropertyValue(SendGroupPKInfo, ref sendGroupPK, value);
				if (!IsValidationSuspended)
				{
					ValidateSendGroupPK();
				}
			}
		}
		ZGuid sendGroupPK;

		public ZPropertyInfo SendGroupPKInfo
		{
			get { return GetZPropertyInfo(Schema.SendGroupPK); }
		}

		public bool SendGroupPK_ReadOnly
		{
			get { return SendMode == Core.Constants.EmailTo.NoEmails || SendMode == Core.Constants.EmailTo.StaffMember; }
		}

		public const string StaffMemberOrNominatedGroup = "EOG";

		void ValidateSendGroupPK()
		{
			SendGroupPKInfo.ClearAllNotifications();
			if (SendMode == Core.Constants.EmailTo.NominatedGroup || SendMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup
				|| SendMode == StaffMemberOrNominatedGroup)
			{
				MandatoryValidation.CheckEntered(SendGroupPKInfo);
			}
			ListValidation.ErrorIfInvalidPK(SendGroupPKInfo);
		}

		public CodeDescriptionPairList SendModeList
		{
			get
			{
				if (sendModeList == null)
				{
					sendModeList = GetSendModeList();
				}
				return sendModeList;
			}
		}
		CodeDescriptionPairList sendModeList;

		protected virtual CodeDescriptionPairList GetSendModeList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.EmailTo);
			result.AddPair(StaffMemberOrNominatedGroup, ResString.GetMultilingualString("97189151-de13-4b52-b039-59705266eab4", "Email Staff Member or Nominated Group"));
			return result;
		}

		public IBusinessObjectCollection SendGroupList
		{
			get { return sendGroupList ?? (sendGroupList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IGlbGroupCollection>(), new object[] { CurrentFactory })); }
		}
		IBusinessObjectCollection sendGroupList;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateSendMode();
			ValidateSendGroupPK();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GroupNotification(fallbackLevel, factory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SendMode = reader.ReadElementString(Schema.SendMode);
			SendGroupPK = new Guid(reader.ReadElementString(Schema.SendGroupPK));
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SendMode, SendMode);
			writer.WriteElementString(Schema.SendGroupPK, SendGroupPK.ToString());
		}
	}
}
