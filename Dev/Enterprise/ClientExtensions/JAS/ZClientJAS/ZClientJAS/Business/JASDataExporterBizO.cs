using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.Business
{
	public abstract class JASDataExporterBizO : NonPersistentBusinessObject
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "This class is designed to be inherited by other classes, allowing shared functionality without enforcing static behavior.")]
		public class Schema
		{
			public const string DeliveryMethod = "DeliveryMethod";
			public const string IsDirectoryDeliveryMethod = "IsDirectoryDeliveryMethod";
			public const string IsEmailDeliveryMethod = "IsEmailDeliveryMethod";
			public const string ExportDirectory = "ExportDirectory";
			public const string EmailAddress = "EmailAddress";
			public const string EmailGroupPK = "EmailGroupPK";
			public const string IsIndividualEmailRecipient = "IsIndividualEmailRecipient";
			public const string IsGroupEmailRecipient = "IsGroupEmailRecipient";
		}

		#endregion

		public JASDataExporterBizO()
			: base(new BusinessObjectFactory())
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DeliveryMethod = EmailDeliveryMethodCode;
			RecipientType = EmailRecipientType.Individual;
		}

		protected abstract string EmailSubject { get; }

		#region Deliver Files

		protected virtual void DeliverFiles(params string[] fullPathToSourceFiles)
		{
			try
			{
				if (IsEmailDeliveryMethod)
				{
					DeliverByEmail(fullPathToSourceFiles);
				}
				else
				{
					DeliverToDirectory(fullPathToSourceFiles);
				}
			}
			finally
			{
				foreach (string fullPathToSourceFile in fullPathToSourceFiles)
				{
					File.Delete(fullPathToSourceFile);
				}
			}
		}

		void DeliverByEmail(string[] fullPathToSourceFiles)
		{
			EmailDef email = new EmailDef();
			email.Subject = EmailSubject;
			foreach (string fullPathToSourceFile in fullPathToSourceFiles)
			{
				string fileName = Path.GetFileName(fullPathToSourceFile);
				email.Attachments.Add(new AttachmentDef(fileName, fullPathToSourceFile));
			}

			if (IsGroupEmailRecipient)
			{
				Env.OutgoingMailManager.CreateAndSave(email, EmailGroupPK.ToGuid(), GroupSourceLocator.GetFromGroup(Factory.Load<GlbGroup>(EmailGroupPK)));
			}
			else
			{
				email.AddRecipientForUserCommunication(EmailAddress);
				Env.OutgoingMailManager.CreateAndSave(email);
			}
		}

		void DeliverToDirectory(string[] fullPathToSourceFiles)
		{
			foreach (string fullPathToSourceFile in fullPathToSourceFiles)
			{
				string fileName = Path.GetFileName(fullPathToSourceFile);
				string targetFileFullPath = Path.Combine(ExportDirectory, fileName);
				File.Copy(fullPathToSourceFile, targetFileFullPath, true);
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public virtual JASDataExporterBizOValidation Validation
		{
			get { return GetNewJASDataExporterBizOValidation(); }
		}

		protected virtual JASDataExporterBizOValidation GetNewJASDataExporterBizOValidation()
		{
			return new JASDataExporterBizOValidation(this);
		}

		#endregion

		#region DeliveryMethod

		public const string EmailDeliveryMethodCode = "EML";
		public const string DirectoryDeliveryMethodCode = "DIR";

		[MaxLength(3)]
		public ZString DeliveryMethod
		{
			get { return fDeliveryMethod; }
			set
			{
				CheckMaximumLength(DeliveryMethodInfo, value);
				SetNonPersistentPropertyValue(DeliveryMethodInfo, ref fDeliveryMethod, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeliveryMethod();
				}

				RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryMethodInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryMethod)); }
		}

		public CodeDescriptionPairList DeliveryMethodList
		{
			get
			{
				if (fDeliveryMethodList == null)
				{
					fDeliveryMethodList = new CodeDescriptionPairList();
					fDeliveryMethodList.AddPair(EmailDeliveryMethodCode, "Email to Recipient / Group");
					fDeliveryMethodList.AddPair(DirectoryDeliveryMethodCode, "Export to Directory");
				}
				return fDeliveryMethodList;
			}
		}

		public ZBool IsDirectoryDeliveryMethod
		{
			get { return DeliveryMethod == DirectoryDeliveryMethodCode; }
		}

		public ZPropertyInfo IsDirectoryDeliveryMethodInfo
		{
			get { return GetZPropertyInfo(nameof(IsDirectoryDeliveryMethod)); }
		}

		public ZBool IsEmailDeliveryMethod
		{
			get { return DeliveryMethod == EmailDeliveryMethodCode; }
		}

		public ZPropertyInfo IsEmailDeliveryMethodInfo
		{
			get { return GetZPropertyInfo(nameof(IsEmailDeliveryMethod)); }
		}

		ZString fDeliveryMethod;
		CodeDescriptionPairList fDeliveryMethodList;

		#endregion

		#region ExportDirectory

		[MaxLength(300)]
		public ZString ExportDirectory
		{
			get { return fExportDirectory; }
			set
			{
				if (fExportDirectory != value)
				{
					CheckMaximumLength(ExportDirectoryInfo, value);
					SetNonPersistentPropertyValue(ExportDirectoryInfo, ref fExportDirectory, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateExportDirectory();
					}
				}
			}
		}

		public ZPropertyInfo ExportDirectoryInfo
		{
			get { return GetZPropertyInfo(nameof(ExportDirectory)); }
		}

		ZString fExportDirectory;

		#endregion

		#region EmailAddress

		[MaxLength(60)]
		public ZString EmailAddress
		{
			get { return fEmailAddress; }
			set
			{
				if (fEmailAddress != value)
				{
					CheckMaximumLength(EmailAddressInfo, value);
					SetNonPersistentPropertyValue(EmailAddressInfo, ref fEmailAddress, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateEmailAddress();
					}
				}
			}
		}

		public ZPropertyInfo EmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(EmailAddress)); }
		}

		ZString fEmailAddress;

		#endregion

		#region Email Group

		public ZGuid EmailGroupPK
		{
			get { return fEmailGroupPK; }
			set
			{
				if (fEmailGroupPK != value)
				{
					SetNonPersistentPropertyValue(EmailGroupPKInfo, ref fEmailGroupPK, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateEmailGroupPK();
					}
				}
			}
		}

		public ZPropertyInfo EmailGroupPKInfo
		{
			get { return GetZPropertyInfo(nameof(EmailGroupPK)); }
		}

		public GlbGroupCollection EmailGroups
		{
			get
			{
				if (fEmailGroups == null)
				{
					fEmailGroups = new GlbGroupCollection(Factory);
				}
				return fEmailGroups;
			}
		}

		ZGuid fEmailGroupPK;
		GlbGroupCollection fEmailGroups;

		#endregion

		#region EmailRecipientType

		enum EmailRecipientType
		{
			Individual,
			Group
		}

		[BusinessObjectTestExclude]
		public ZBool IsIndividualEmailRecipient
		{
			get { return RecipientType == EmailRecipientType.Individual; }
			set
			{
				if (value)
				{
					RecipientType = EmailRecipientType.Individual;
				}
				IsIndividualEmailRecipientInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsIndividualEmailRecipientInfo
		{
			get { return GetZPropertyInfo(nameof(IsIndividualEmailRecipient)); }
		}

		[BusinessObjectTestExclude]
		public ZBool IsGroupEmailRecipient
		{
			get { return RecipientType == EmailRecipientType.Group; }
			set
			{
				if (value)
				{
					RecipientType = EmailRecipientType.Group;
				}
				IsGroupEmailRecipientInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsGroupEmailRecipientInfo
		{
			get { return GetZPropertyInfo(nameof(IsGroupEmailRecipient)); }
		}

		EmailRecipientType RecipientType
		{
			get { return fRecipientType; }
			set
			{
				if (fRecipientType != value)
				{
					fRecipientType = value;
					RefreshBinding();
				}
			}
		}

		EmailRecipientType fRecipientType;

		#endregion
	}
}
