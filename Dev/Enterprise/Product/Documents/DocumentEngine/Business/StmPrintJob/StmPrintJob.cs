using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using PdfSharp.Pdf.IO;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;
using DocumentConverter = Enterprise.DocumentEngine.FileFormatUtilities.DocumentConverter;
using Watermark = Enterprise.RemotePrinting.Engine.Watermark;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintJob : AutoStmPrintJob, Enterprise.Integration.DocumentEngine.IStmPrintJob
	{
		public static StmPrintJob New(BusinessObjectFactory factory)
		{
			return factory.New<StmPrintJob>();
		}

		public StmPrintJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SP_IsScheduled), ConcurrencyPolicy.Ignore); // This field is updated by triggers on StmPrintJobQueue
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SP_GB = GlbBranch.CurrentBranch?.PK ?? ZGuid.Empty;
			SP_EmailAttachments = "default.XLS";
			SP_GS_NKJobSubmittedBy = GlbStaff.CurrentUser?.GS_Code ?? ZString.Empty;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public const string LanguageDelimiter = "~";

		public override void Delete()
		{
			DeleteStoredAttachment();
			DeleteCVRStmNote();

			base.Delete();
		}

		public override ZString SP_Status
		{
			get { return base.SP_Status; }
			set
			{
				if (value == nameof(PrintJobStatus.FAL) && SP_Status != value && SP_FailureReason.IsEmpty && failStackTrace == null)
				{
					failStackTrace = new StackTrace();
				}

				base.SP_Status = value;
			}
		}

		StackTrace failStackTrace;

		public override void OnSaving()
		{
			base.OnSaving();

			if (
#if DEBUG
				(!Globals.IsTest || ReportErrorWithoutDestinationForTest) &&
#endif
				!IsInDatabase &&
				((SP_JobType.EqualsIgnoringCase(nameof(PrintType.PRN)) && SP_SQ.IsEmpty) || (SP_JobType.EqualsIgnoringCase(nameof(PrintType.EML)) && EmailToRecipients.Value.IsEmpty))
			)
			{
				ErrorReporter.ReportOnce("StmPrintJob_NoDestination", "Destination is not specified when a StmPrintJob is saving. SP_JobType: " + SP_JobType);
			}

			if (SP_Status == nameof(PrintJobStatus.FAL) && (SP_StatusInfo.HasChanges || !IsInDatabase) && SP_FailureReason.IsEmpty)
			{
				var stackTrace = failStackTrace?.ToString();
				if (string.IsNullOrEmpty(stackTrace))
				{
					stackTrace = new StackTrace().ToString();
					var factorySaveIndex = stackTrace.IndexOf(nameof(BusinessObjectFactory) + "." + nameof(BusinessObjectFactory.Save) + "(", StringComparison.Ordinal);
					if (factorySaveIndex > 0)
					{
						stackTrace = stackTrace.Substring(factorySaveIndex);
					}
				}

				var failReason = new ZString((NoResString)"Failed at " + stackTrace).SubstringSafe(0, Schema.SP_FailureReasonMaxLength);
				SP_FailureReason = failReason;

				failStackTrace = null;
			}
		}

		//Temporarily added to solve CS01111146
		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded
				&& SP_DocumentName.StartsWith((NoResString)"MAWB (Copy for Shipping Advice)")
				&& AttachmentRequiresConversion)
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedPrintJob = newFactory.Load<StmPrintJob>(PK);
				if (reloadedPrintJob.SP_CustomProperties.IsEmpty)
				{
					const string codes = "0123456789ABCDEF";
					var originalValue = SP_CustomProperties;

					var originalValueToStringBuilder = new StringBuilder((NoResString)"0x", originalValue.Length * 2 + 2);

					for (var i = 0; i < originalValue.Length; i++)
					{
						int index = originalValue[i];
						originalValueToStringBuilder.Append(codes[(index & 0xF0) >> 4]);
						originalValueToStringBuilder.Append(codes[index & 0x0F]);
					}

					var message = new StringBuilder();
					message.AppendLine($"Saved Empty {StmPrintJobSchema.SP_CustomProperties.Name}");
					message.AppendLine($"DocumentName: {SP_DocumentName}");
					message.AppendLine($"AttachmentType: {SP_EmailAttachmentFormat}");
					message.AppendLine($"EmailSubject: {SP_EmailSubjectLine}");
					message.AppendLine($"ParentTableName: {SP_ParentTableName}");
					message.AppendLine($"Attachments: {SP_EmailAttachments}");
					message.AppendLine($"Original SP_CustomProperties: {originalValueToStringBuilder}");

					ErrorReporter.ReportOnce("SavedEmptySP_CustomProperties", message.ToString());
				}
			}
		}

#if DEBUG
		public bool ReportErrorWithoutDestinationForTest { get; set; }
#endif

		void DeleteCVRStmNote()
		{
			if (EmailFaxCoverNote_CVRStmNote != null)
			{
				EmailFaxCoverNote_CVRStmNote.Delete();
			}
		}

		public void DeleteStoredAttachment()
		{
			if (File.Exists(StoredAttachmentFilename))
			{
				try
				{
					File.SetAttributes(StoredAttachmentFilename, FileAttributes.Normal);
					File.Delete(StoredAttachmentFilename);
					if (Path.GetExtension(StoredAttachmentFilename).Equals(".HTML", StringComparison.OrdinalIgnoreCase))
					{
						string imagesDirectoryName = DocumentConverter.GetSafeImagesDirectoryName(StoredAttachmentFilename);
						string imagesDirectoryPath = Path.Combine(Path.GetDirectoryName(StoredAttachmentFilename), imagesDirectoryName);
						if (Directory.Exists(imagesDirectoryPath))
						{
							TempDirectory.DeleteDirectory(imagesDirectoryPath, false);
						}
					}
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{
						throw;
					}

					// TempFileWithDelayedDelete class will retry to delete the file for 15 seconds and then give up.
					// If it can't be deleted, it will be picked up by docengine's monthly cleanup procedure
					TempFileWithDelayedDelete.NewWithFilename(StoredAttachmentFilename, 0).Dispose();
				}
				finally
				{
					StoredAttachmentFilename = ZString.Empty;
				}
			}
		}

		public void CreateLogOnParent(Event @event, params KeyValuePair<string, string>[] parameters)
		{
			var logTemplates = GetLogTemplates()
				.ToArray();

			if (logTemplates.Any())
			{
				foreach (var template in logTemplates)
				{
					var logParameters = template.Parameters
						.Concat(parameters ?? Enumerable.Empty<KeyValuePair<string, string>>());

					var mergedLogParameters = logParameters
						.ToLookup(p => p.Key, p => p.Value)
						.ToDictionary(p => p.Key, p => p.First());

					CreateLogOnParent(@event,
						template.ParentPK,
						template.ParentTableName,
						template.User,
						mergedLogParameters);
				}
			}
			else
			{
				CreateLogOnParent(@event,
					SP_ParentGuid,
					SP_ParentTableName,
					SP_GS_NKJobSubmittedBy,
					parameters);
			}
		}

		void CreateLogOnParent(Event @event, ZGuid parentPK, ZString parentTableName, ZString user, IEnumerable<KeyValuePair<string, string>> parameters)
		{
			if (parentPK.IsValid && !parentTableName.IsEmpty)
			{
				var log = Factory.New<StmALog>();

				try
				{
					using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
					{
						if (!user.IsEmpty)
						{
							log.SL_GS_NKUser = user;
						}

						log.SL_SE_NKEvent = @event.Code;

						var anyCCsExist = CarbonCopyRecipients.Any() || BlindCarbonCopyRecipients.Any();

						var recipient = (anyCCsExist ? (NoResString)"Main: " : "") + SP_Destination;

						if (CarbonCopyRecipients.Any())
						{
							recipient += (NoResString)", CC: " + CarbonCopyRecipients.Select(x => x.SPR_EmailAddress).Aggregate((x, y) => x + (NoResString)", " + y);
						}

						if (BlindCarbonCopyRecipients.Any())
						{
							recipient += (NoResString)", BCC: " + BlindCarbonCopyRecipients.Select(x => x.SPR_EmailAddress).Aggregate((x, y) => x + (NoResString)", " + y);
						}

						var referenceFreeText = string.Concat(recipient, " - ", SP_AbbreviatedEmailSubjectLine);

						log.SL_Reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(referenceFreeText, parameters);
						log.SL_Parent = parentPK;
						log.SL_Table = parentTableName;
						log.SL_EventTime = ZDateTime.Now;

						if (@event.Code == Events.DocumentSent.Code && PrintQueue != null)
						{
							PrintQueue.SQ_LastUsedDateTimeUtc = ZDateTime.UtcNow;
						}
					}

#if DEBUG
					LastLogCreatedOnParent = log;
#endif
				}
				catch (Exception)
				{
					log.Delete();
					throw;
				}
			}
		}

#if DEBUG
		internal StmALog LastLogCreatedOnParent { get; private set; }
#endif

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public void CreateLogTemplates(params StmALog[] logs)
		{
			if (logs == null
				|| !logs.Any())
			{
				return;
			}

			var doc = new XDocument(
				new XDeclaration("1.0", "utf-8", null),
				new XElement("Root", logs.Select(CreateXElement)));

			SP_DSNTemplate = doc.ToString(SaveOptions.DisableFormatting);
		}

		XElement CreateXElement(StmALog log)
		{
			return new XElement(LogTemplateSchema.ElementName,
				new XElement(LogTemplateSchema.ParentPK, log.SL_Parent),
				new XElement(LogTemplateSchema.ParentTableName, log.SL_Table),
				new XElement(LogTemplateSchema.User, log.SL_GS_NKUser),
				new XElement(LogTemplateSchema.Reference, log.ReferenceFreeText),
				new XElement(LogTemplateSchema.Parameters, log.Parameters.Select(CreateXElement)));
		}

		XElement CreateXElement(KeyValuePair<string, string> parameter)
		{
			return new XElement(ParameterSchema.ElementName,
				new XElement(ParameterSchema.Type, parameter.Key),
				new XElement(ParameterSchema.Value, parameter.Value));
		}

		IEnumerable<LogTemplate> GetLogTemplates()
		{
			if (SP_DSNTemplate.IsEmpty)
			{
				yield break;
			}

			XDocument doc;

			try
			{
				doc = XDocument.Parse(SP_DSNTemplate);
			}
			catch (System.Xml.XmlException)
			{
				ErrorReporter.ReportOnce("Misformatted DSNTemplate", SP_DSNTemplate);
				yield break;
			}

			var root = doc.Elements().Single();

			foreach (var element in root.Elements())
			{
				Guid parentPK;
				var parentPKInXml = element.Element(LogTemplateSchema.ParentPK)?.Value;

				if (!Guid.TryParse(parentPKInXml, out parentPK))
				{
					ErrorReporter.ReportOnce("Misformatted DSNTemplate ParentPK", $"ParentPK to parse: {parentPKInXml}/r/n{SP_DSNTemplate}");
					continue;
				}

				yield return new LogTemplate
				{
					ParentPK = parentPK,
					ParentTableName = element.Element(LogTemplateSchema.ParentTableName)?.Value,
					User = element.Element(LogTemplateSchema.User)?.Value,
					Reference = element.Element(LogTemplateSchema.Reference)?.Value,
					Parameters = element.Element(LogTemplateSchema.Parameters)?.Elements().Select(CreateKeyValuePair)
				};
			}
		}

		KeyValuePair<string, string> CreateKeyValuePair(XElement element)
		{
			return new KeyValuePair<string, string>(
				element.Element(ParameterSchema.Type)?.Value,
				element.Element(ParameterSchema.Value)?.Value);
		}

		#region SuppressResourceStringsCheckRegion

		sealed class LogTemplateSchema
		{
			public const string ElementName = "LogTemplate";
			public const string ParentPK = "ParentPK";
			public const string ParentTableName = "ParentTableName";
			public const string User = "User";
			public const string Reference = "Reference";
			public const string Parameters = "Parameters";
		}

		sealed class ParameterSchema
		{
			public const string ElementName = "Paramter";
			public const string Type = "Type";
			public const string Value = "Value";
		}

		#endregion

		sealed class LogTemplate
		{
			public Guid ParentPK { get; set; }
			public string ParentTableName { get; set; }
			public string User { get; set; }
			public string Reference { get; set; }
			public IEnumerable<KeyValuePair<string, string>> Parameters { get; set; }
		}

		#region Properties

		public override ZString SP_ParentTableName
		{
			get { return base.SP_ParentTableName; }
			set
			{
				ZString oldValue = SP_ParentTableName;
				base.SP_ParentTableName = value;
				if (!IsCopying && value.Length == 2 && oldValue != SP_ParentTableName)
				{
					ErrorReporter.ReportOnce("Setting StmPrintJob.SP_ParentTableName to a TablePrefix", "StmPrintJob.SP_ParentTableName should not be set to TablePrefix (" + value + "). It should be set to the TableName");
				}
			}
		}

		#region SP_CustomProperties
		public override ZBlob SP_CustomProperties
		{
			get { return base.SP_CustomProperties; }
			set
			{
				var oldValueLength = SP_CustomProperties.Length;

				base.SP_CustomProperties = value;

				if (value.IsEmpty && oldValueLength != 0)
				{
					var stringBuilder = new StringBuilder();
					stringBuilder.AppendLine($"OldValueLength: {oldValueLength}");
					stringBuilder.AppendLine("StackTrace: " + System.Environment.StackTrace);
					CustomPropertiesLatestSetToEmptyStackTrace = stringBuilder.ToString();
				}
			}
		}

		string CustomPropertiesLatestSetToEmptyStackTrace { get; set; }

		#endregion

		#region SP_JobTypeDisplayName

		public ZString SP_JobTypeDisplayName
		{
			get
			{
				try
				{
					return PrintTypeName.Name(SP_JobType);
				}
				catch (ArgumentException)
				{
					return SP_JobType;
				}
			}
		}

		public ZPropertyInfo SP_JobTypeDisplayNameInfo
		{
			get { return GetZPropertyInfo(nameof(SP_JobTypeDisplayName)); }
		}

		#endregion

		#region SP_StatusDisplayName

		public ZString SP_StatusDisplayName
		{
			get
			{
				try
				{
					return PrintJobStatusName.Name(SP_Status);
				}
				catch (ArgumentException)
				{
					return SP_Status;
				}
			}
		}

		public ZPropertyInfo SP_StatusDisplayNameInfo => GetZPropertyInfo(nameof(SP_StatusDisplayName));

		#endregion

		#region SP_Destination
		[EmailAddress]
		[MaxLength("SP_DestinationMaxLength")]
		public ZString SP_Destination
		{
			get
			{
				if (SP_SQ.IsValid)
				{
					var queue = Factory.Load<StmPrintQueue>(SP_SQ);
					if (queue != null)
					{
						return queue.SQ_DisplayName;
					}
				}

				return IsEmailJob ? EmailToRecipients.Value : SP_FaxDestination;
			}
			set
			{
				if (IsEmailJob)
				{
					EmailToRecipients.Value = value;
				}
				else
				{
					SP_FaxDestination = value;
				}

				SP_DestinationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SP_DestinationInfo => GetZPropertyInfo(nameof(SP_Destination));
		public int SP_DestinationMaxLength => IsEmailJob ? -1 : Schema.SP_FaxDestinationMaxLength;

		#endregion

		#region Company / Branch Information

		/// <summary>
		/// Cached list of Company Code & Name
		/// </summary>
		internal static CodeDescriptionPairList CompanyInformation
		{
			get
			{
				if (companyInformation == null)
				{
					companyInformation = new CodeDescriptionPairList();
					GlbCompanyCollection companies = new GlbCompanyCollection(new BusinessObjectFactory());
					foreach (GlbCompany company in companies)
					{
						companyInformation.AddPair(company.PK, company.GC_Code, company.GC_Name);
					}
				}
				return companyInformation;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList companyInformation;

		/// <summary>
		/// Cached list of Branch Code & Name
		/// </summary>
		static CodeDescriptionPairList BranchInformation
		{
			get
			{
				if (branchInformation == null)
				{
					branchInformation = new CodeDescriptionPairList();
					GlbBranchCollection branches = new GlbBranchCollection(new BusinessObjectFactory());
					branches.Load();

					foreach (GlbBranch branch in branches)
					{
						branchInformation.AddPair(branch.Company.PK, branch.GB_Code, branch.GB_BranchName);
					}
				}
				return branchInformation;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList branchInformation;

		#region Test Only
#if DEBUG

		public void ClearCompanyAndBranchInformationForTesting()
		{
			companyInformation = null;
			branchInformation = null;
		}

#endif
		#endregion

		#endregion

		public override ZGuid SP_GB
		{
			get { return base.SP_GB; }
			set
			{
				fSP_AbbreviatedEmailSubjectLine = ZString.Empty;
				base.SP_GB = value;
			}
		}

		public ZGuid ProperBranchPK => (!SP_GB.IsEmpty || SP_GB.IsValid) ? SP_GB : GlbBranch.GetFirstActiveBranch().PK;

		#region SP_AbbreviatedEmailSubjectLine

		ZString fSP_AbbreviatedEmailSubjectLine;
		public ZString SP_AbbreviatedEmailSubjectLine
		{
			get
			{
				if (fSP_AbbreviatedEmailSubjectLine.IsEmpty)
				{
					string abbreviatedEmailSubjectLine = SP_EmailSubjectLine;
					int leftmostCompanyMatchPosition = abbreviatedEmailSubjectLine.Length + 1;
					CodeElement matchingCompany = null;

					foreach (CodeElement companyInfo in CompanyInformation)
					{
						int positionOfThisCompanysNameInSubjectLine = abbreviatedEmailSubjectLine.IndexOf(companyInfo.Description);
						if (positionOfThisCompanysNameInSubjectLine >= 0 && positionOfThisCompanysNameInSubjectLine < leftmostCompanyMatchPosition)
						{
							leftmostCompanyMatchPosition = positionOfThisCompanysNameInSubjectLine;
							matchingCompany = companyInfo;
						}
					}

					if (matchingCompany != null)
					{
						if (matchingCompany.Description.Length > 0)
						{
							abbreviatedEmailSubjectLine = abbreviatedEmailSubjectLine.Replace(matchingCompany.Description, matchingCompany.Code);
						}

						int leftmostBranchMatchPosition = abbreviatedEmailSubjectLine.Length + 1;
						CodeElement matchingBranch = null;

						foreach (CodeElement branchInfo in BranchInformation)
						{
							if ((ZGuid)branchInfo.PK == (ZGuid)matchingCompany.PK)
							{
								int positionOfThisBranchsNameInSubjectLine = abbreviatedEmailSubjectLine.IndexOf(branchInfo.Description);
								if (positionOfThisBranchsNameInSubjectLine >= 0 && positionOfThisBranchsNameInSubjectLine < leftmostBranchMatchPosition)
								{
									leftmostBranchMatchPosition = positionOfThisBranchsNameInSubjectLine;
									matchingBranch = branchInfo;
								}
							}
						}

						if (matchingBranch != null)
						{
							if (matchingBranch.Description.Length > 0)
							{
								abbreviatedEmailSubjectLine = abbreviatedEmailSubjectLine.Replace(matchingBranch.Description, matchingBranch.Code);
							}
						}
					}
					fSP_AbbreviatedEmailSubjectLine = abbreviatedEmailSubjectLine;
				}
				return fSP_AbbreviatedEmailSubjectLine;
			}
		}

		#endregion

		public ZString EmailFaxCoverNote
		{
			get
			{
				var cVRStmNote = EmailFaxCoverNote_CVRStmNote;
				if (EmailFaxCoverNote_CVRStmNote != null)
				{
					return EmailFaxCoverNote_CVRStmNote.ST_NoteText;
				}

				return Notes.HasNotes ? (Notes.GetAllNotes().First() as StmNote).ST_NoteText : (ZString)"";
			}
		}

		StmNote EmailFaxCoverNote_CVRStmNote
		{
			get
			{
				var filter = new ZQuery(StmNoteSchema.ST_NoteType, "CVR");
				filter.AddToFilter(StmNoteSchema.ST_ParentID, this.PK);
				filter.AddToFilter(StmNoteSchema.ST_Table, StmPrintJob.Schema.TableName);

				return Factory.LoadTop1<StmNote>(filter);
			}
		}

		#region SP_UserLoginName

		public ZString SP_UserLoginName
		{
			get { return Staff != null ? Staff.GS_LoginName : ZString.Empty; }
		}

		public ZPropertyInfo SP_UserLoginNameInfo
		{
			get { return GetZPropertyInfo(nameof(SP_UserLoginName)); }
		}

		#endregion

		#region SP_UserFullName

		public ZString SP_UserFullName
		{
			get { return Staff != null ? Staff.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo SP_UserFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(SP_UserFullName)); }
		}

		#endregion

		#region SP_RetryAttempts

		public override ZByte SP_RetryAttempts
		{
			get => base.SP_RetryAttempts;
			set
			{
				if (value >= 3)
				{
					SP_Status = nameof(PrintJobStatus.FAL);
				}

				base.SP_RetryAttempts = value;
			}
		}

		#endregion

		#region SP_RunDateTimeLocal

		public ZDateTime SP_RunDateTimeLocal
		{
			get { return SP_RunDateTime.IsValid ? Env.Time.GetLocalTimeFromUtc(SP_RunDateTime.ToDateTime()) : SP_RunDateTime; }
		}

		public ZPropertyInfo SP_RunDateTimeLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SP_RunDateTimeLocal), o => SP_RunDateTimeInfo); }
		}

		#endregion

		public ZString BlobType
		{
			get
			{
				string extension = Path.GetExtension(PathValidation.GetSafeFilename(SP_EmailAttachments)).Trim('.');
				if (extension.Length > 4)
				{
					extension = extension.Substring(0, 4);
				}

				return extension.ToUpperInvariant();
			}
		}

		/// <summary>
		/// Are the results of this print job sent externally (outside of Enterprise)
		/// </summary>
		public ZBool IsDeliveredExternally
		{
			get { return IsEmailJob || IsFaxJob || IsPrintJob || IsFtpJob; }
		}

		public ZBool IsEmailJob
		{
			get { return SP_JobType.EqualsIgnoringCase(nameof(PrintType.EML)); }
		}

		public ZBool IsFaxJob
		{
			get { return SP_JobType.EqualsIgnoringCase(nameof(PrintType.FAX)); }
		}

		public ZBool IsPrintJob
		{
			get { return SP_JobType.EqualsIgnoringCase(nameof(PrintType.PRN)) || SP_JobType.EqualsIgnoringCase(nameof(PrintType.PRS)); }
		}

		public ZBool IsDocumentDeliverySuccessfulJob
		{
			get { return SP_JobType.EqualsIgnoringCase(nameof(PrintType.DDS)); }
		}

		public ZBool IsSmsJob
		{
			get { return SP_JobType.EqualsIgnoringCase(nameof(PrintType.SMS)); }
		}

		public ZBool IsFtpJob
		{
			get { return SP_JobType.EqualsIgnoringCase(nameof(PrintType.FTP)); }
		}

		public ZBool ShouldSign => SP_SignBy != DocumentsSignBy.NON;

		public ZBool IsUnsignedDosJob
		{
			get { return SP_SignBy == DocumentsSignBy.DOS && !SP_IsSigned; }
		}

		public ZBool IsExcelAttachment
		{
			get { return AttachmentTypeList.Codes.Xls.Equals(SP_EmailAttachmentFormat, StringComparison.OrdinalIgnoreCase) || AttachmentTypeList.Codes.Xlsx.Equals(SP_EmailAttachmentFormat, StringComparison.OrdinalIgnoreCase); }
		}

		#region CopyRecipients

		#region EmailToRecipients

		[ChildEditable(true)]
		public StmPrintJobCopyRecipientCollection EmailToRecipients
		{
			get
			{
				if (emailToRecipients == null)
				{
					emailToRecipients = new StmPrintJobCopyRecipientCollection(this, Core.Constants.CopyRecipientType.EmailToRecipient);
					RegisterEditableChildObject(emailToRecipients);
				}
				return emailToRecipients;
			}
		}

		StmPrintJobCopyRecipientCollection emailToRecipients;

		#endregion

		#region CarbonCopyRecipients

		[ChildEditable(true)]
#if DEBUG
		public virtual 
#else
		public
#endif
		StmPrintJobCopyRecipientCollection CarbonCopyRecipients
		{
			get
			{
				if (carbonCopyRecipients == null)
				{
					carbonCopyRecipients = new StmPrintJobCopyRecipientCollection(this, Core.Constants.CopyRecipientType.CarbonCopyRecipient);
					RegisterEditableChildObject(carbonCopyRecipients);
				}
				return carbonCopyRecipients;
			}
		}

		StmPrintJobCopyRecipientCollection carbonCopyRecipients;

		#endregion

		#region BlindCarbonCopyRecipients

		[ChildEditable(true)]
		public StmPrintJobCopyRecipientCollection BlindCarbonCopyRecipients
		{
			get
			{
				if (blindCarbonCopyRecipients == null)
				{
					blindCarbonCopyRecipients = new StmPrintJobCopyRecipientCollection(this, Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient);
					RegisterEditableChildObject(blindCarbonCopyRecipients);
				}
				return blindCarbonCopyRecipients;
			}
		}

		StmPrintJobCopyRecipientCollection blindCarbonCopyRecipients;

		#endregion

		#endregion

		#endregion

		#region Related Objects

		public GlbStaff Staff
		{
			get { return Factory.LoadFromNaturalKey(typeof(GlbStaff), GlbStaffSchema.GS_Code, SP_GS_NKJobSubmittedBy) as GlbStaff; }
		}

		public StmPrintQueue PrintQueue
		{
			get { return Factory.Load<StmPrintQueue>(SP_SQ); }
		}

		ZString sq_ServerName;

		[ResourceStringData("01EE2A6F-CF16-4205-A9E7-20003028DCC3", Caption = "Server Name", FullDescription = "Print Server Name")]
		public ZString SQ_ServerName
		{
			get
			{
				if (sq_ServerName.IsEmpty && PrintQueue != null)
				{
					sq_ServerName = PrintQueue.SQ_ServerName;
				}
				return sq_ServerName;
			}
		}

		public ZPropertyInfo SQ_ServerNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(SQ_ServerName));
			}
		}

		public StmDeliveryGroup DeliveryGroup
		{
			get { return Factory.Load<StmDeliveryGroup>(SP_SB_DeliveryGroup); }
		}

		public Watermark Watermark
		{
			get
			{
				if (Env.Instance.IsProductionSystem)
				{
					if (fWatermark == null)
					{
						var registryValue = DocumentsDataRegistry.Instance.Watermark.Value;

						if (!SP_WatermarkImage.IsEmpty)
						{
							fWatermark = new ImageWatermark(
								SP_WatermarkImage,
								Watermark.GetHorizontalAlignment(registryValue.HorizontalAlignment),
								Watermark.GetVerticalAlignment(registryValue.VerticalAlignment),
								registryValue.HorizontalOffset,
								registryValue.VerticalOffset,
								registryValue.Rotation);
						}
						else if (!SP_WatermarkText.IsEmpty)
						{
							fWatermark = new TextWatermark(
								SP_WatermarkText,
								Watermark.GetHorizontalAlignment(registryValue.HorizontalAlignment),
								Watermark.GetVerticalAlignment(registryValue.VerticalAlignment),
								registryValue.HorizontalOffset,
								registryValue.VerticalOffset,
								registryValue.Rotation,
								Color.FromArgb((int)registryValue.Opacity, 0, 0, 0),
								(NoResString)"Arial",
								registryValue.FontSize,
								FontStyle.Bold);
						}
					}
				}
				else
				{
					fWatermark = WatermarkHelper.GetNonCommercialUseWatermark();
				}

				return fWatermark;
			}
		}

		Watermark fWatermark;

		#endregion

		#region Attachment Information

		/// <summary>
		/// The filepath of the file to print/fax/attach to email. (This is after it
		/// has been processed and converted to the specified delivery format)
		/// </summary>
		public ZString StoredAttachmentFilename
		{
			get { return fStoredAttachmentFilename; }
			set
			{
				fStoredAttachmentFilename = value;

				if (!value.IsEmpty)
				{
					FileInfo info = new FileInfo(fStoredAttachmentFilename);
					var sizeInKB = (int)Math.Ceiling(info.Length / 1024d);
					fStoredAttachmentSizeKB = sizeInKB;
				}
				else
				{
					fStoredAttachmentSizeKB = 0;
				}
			}
		}

		ZString fStoredAttachmentFilename;

		/// <summary>
		/// The size of the attachment in Kb
		/// </summary>
		public ZInt StoredAttachmentSizeKB
		{
			get { return fStoredAttachmentSizeKB; }
#if DEBUG
			set { fStoredAttachmentSizeKB = value; }
#endif
		}

		public bool IsGeneratedFromReport => SP_RelatedBusinessContext == Core.Constants.DocManagerCodes.ReportStatistic;

		public bool IsEmailJobFromReportRun => SP_JobType == nameof(PrintType.EML) && SP_ParentTableName == StmReportRunSchema.Constants.TableName && IsGeneratedFromReport;

		public string GetFileFormatForXlsConversion() => IsGeneratedFromReport ? SP_EmailAttachmentFormat.ToString() : SystemDataRegistry.Instance.EDocImportFileFormat.Value;

		public string GetDataTypeForAllocateDocument(string filePath)
		{
			if (IsGeneratedFromReport)
			{
				var extension = Path.GetExtension(filePath);
				return extension.TrimStart(new char[] { '.' }).ToUpper(CultureInfo.CurrentCulture);
			}

			return string.Empty;
		}

		public string SignOption => SP_SignBy == DocumentsSignBy.DOS ? PdfSigningOptionCodes.Placeholder : SP_SignBy == DocumentsSignBy.PFX ? DocumentsSignBy.PFX : DocumentsSignBy.NON;

		ZInt fStoredAttachmentSizeKB;

		/// <summary>
		/// Saves the blob in the print job to a file on the file system.
		/// The file is converted to the final format which it will be delivered (e.g. TIF for fax, or the user specified type if email)
		/// </summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void SaveAttachmentToFilesystem(int attachmentNumber)
		{
			if (DeliveryGroup.SB_IsProcessed)
			{
				var faxTifFilePath = Temp.GetTempFileNameWithExtension("TIF");
				var safeFileName = PathValidation.GetSafeFilename(SP_EmailAttachments);
				var originalFilePath = PathValidation.GetFilePathWithValidLength(GetUniqueHumanReadableFilePath(Temp.TempPath, Path.GetFileNameWithoutExtension(safeFileName), Path.GetExtension(safeFileName)));
				var extension = DocumentConverter.GetFileExtensionFromAttachmentFormat(SP_EmailAttachmentFormat, attachmentNumber);
				var convertedFilePath = PathValidation.GetFilePathWithValidLength(GetUniqueHumanReadableFilePath(Temp.TempPath, Path.GetFileNameWithoutExtension(originalFilePath), "." + extension));

				try
				{
					if (AttachmentRequiresConversion)
					{
						string outputFile;
						OutputFormatType formatType;

						if (IsFaxJob)
						{
							outputFile = faxTifFilePath;
							formatType = OutputFormatType.FAX;
						}
						else
						{
							outputFile = convertedFilePath;
							if (!Enum.TryParse(SP_EmailAttachmentFormat, true, out formatType))
							{
								formatType = OutputFormatType.TIF;
								outputFile = GetUniqueHumanReadableFilePath(Temp.TempPath, Path.GetFileNameWithoutExtension(originalFilePath), ".TIF");
							}
						}
						outputFile = ConvertFileExtensionToLowercase(outputFile);

						if (Enum.TryParse(BlobType, true, out PrintJobBlobType enumBlobType))
						{
							switch (enumBlobType)
							{
								case PrintJobBlobType.XLS:
								case PrintJobBlobType.XLSX:
									try
									{
										if (SP_CustomProperties.IsEmpty)
										{
											var message = new StringBuilder();
											message.AppendLine($"{StmPrintJobSchema.SP_CustomProperties.Name} is empty when trying to open it as an Xls.");
											message.AppendLine($"DocumentName: {SP_DocumentName}");
											message.AppendLine($"AttachmentType: {SP_EmailAttachmentFormat}");
											message.AppendLine($"EmailSubject: {SP_EmailSubjectLine}");
											message.AppendLine($"ParentTableName: {SP_ParentTableName}");
											message.AppendLine($"Attachments: {SP_EmailAttachments}");
											message.AppendLine($"LatestSetToEmptyStackTrace: {CustomPropertiesLatestSetToEmptyStackTrace}");
											ErrorReporter.ReportOnce("TryToOpenEmptyXlsStream", message.ToString());
										}

										DocumentConverter.ConvertFromExcel_IncludingHTML(SP_CustomProperties, outputFile, formatType, Watermark, Env.Registry.PDFTIFColourDepth, SP_IsLocalCulture, SP_FlexCelLineSpacing, attachmentNumber, ShouldSign, SignOption, SP_UserFullName, SP_GB);
										break;
									}
									catch (ExcelInterfaceException e)
									{
										if ((e.Type == ExcelInterfaceExceptionType.CouldNotOpenFile || e.Type == ExcelInterfaceExceptionType.CouldNotOpenStream || e.Type == ExcelInterfaceExceptionType.ErrorInvalidColumn) && (formatType == OutputFormatType.PDFC || formatType == OutputFormatType.PDF))
										{
											var errorMsg = Res.GetString("41145999-1F28-44E9-BD31-487907E4C5C1",
												"An error occurred when converting file {0} to format {1}. The original format was retained.", safeFileName,
												formatType);
											ErrorsWhenConverting.Add((errorMsg, e));
											outputFile = ConvertFileExtensionToLowercase(originalFilePath);
											FileSaveHelper.SaveBlobAsFile(SP_CustomProperties, outputFile);
											break;
										}
										else if (e.Type == ExcelInterfaceExceptionType.FileFormatNotSupported && !SP_EmailAttachmentFormat.EqualsIgnoringCase(AttachmentTypeList.Codes.Pdfc))
										{
											var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)@"Email Subject: [{0}]
Attachments: [{1}]
Document Name: [{2}]
Related Business Context: [{3}]
Parent Table Name: [{4}]
Attachment Format: [{5}]
PK: [{6}]", SP_EmailSubjectLine, SP_EmailAttachments, SP_DocumentName, SP_RelatedBusinessContext, SP_ParentTableName, SP_EmailAttachmentFormat, PK);

											ErrorReporter.ReportOnce("Corrupted stream could not be loaded. Print Job informations :", errorMessage, e);
										}

										throw;
									}

								case PrintJobBlobType.TIF:
									if (formatType == OutputFormatType.FAX)
									{
										DocumentConverter.ConvertToFax(SP_CustomProperties, outputFile);
									}
									else if (IsEmailJob && formatType == OutputFormatType.PDFC)
									{
										DocumentConverter.ConvertTIFToPDF(SP_CustomProperties, outputFile);
									}
									else
									{
										throw new InvalidOperationException("File format not supported: " + BlobType);
									}
									break;

								default:
									throw new InvalidOperationException("File format not supported: " + BlobType);
							}
						}
						else
						{
							throw new InvalidOperationException("File format not supported: " + BlobType);
						}

						StoredAttachmentFilename = outputFile;
					}
					else
					{
						if (Path.GetExtension(originalFilePath).ToUpperInvariant() == ".TIF")
						{
							bool isJpeg;
							using (var imageStream = new MemoryStream(SP_CustomProperties))
							{
								try
								{
									using (var image = Image.FromStream(imageStream))
									{
										isJpeg = image.RawFormat.Guid == ImageFormat.Jpeg.Guid;
									}
								}
								catch (Exception exception)
								{
									if (!exception.IsCriticalException()
											&& exception.IsBadImageFormatException())
									{
										throw new ImageFormatException();
									}
									throw;
								}
							}

							if (isJpeg)
							{
								originalFilePath = GetUniqueHumanReadableFilePath(Temp.TempPath, Path.GetFileNameWithoutExtension(originalFilePath), ".JPG");
							}
						}

						originalFilePath = ConvertFileExtensionToLowercase(originalFilePath);
						FileSaveHelper.SaveBlobAsFile(SP_CustomProperties, originalFilePath);
						StoredAttachmentFilename = originalFilePath;
					}
					EncryptStoredAttachmentFileIfNeeded(StoredAttachmentFilename);
				}
				finally // delete the files we didn't use
				{
					if (!StoredAttachmentFilename.EqualsIgnoringCase(faxTifFilePath))
					{
						TempFile.Delete(faxTifFilePath);
					}

					if (!StoredAttachmentFilename.EqualsIgnoringCase(originalFilePath))
					{
						TempFile.Delete(originalFilePath);
					}

					if (!StoredAttachmentFilename.EqualsIgnoringCase(convertedFilePath) && SP_PDFEncryptedPassword.IsEmpty)
					{
						TempFile.Delete(convertedFilePath);
					}
				}
			}
		}

		string ConvertFileExtensionToLowercase(string filePath)
		{
			var extension = Path.GetExtension(filePath);
			return Path.ChangeExtension(filePath, extension.ToLowerInvariant());
		}

		void EncryptStoredAttachmentFileIfNeeded(string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				var extension = Path.GetExtension(path);
				if (".pdf".Equals(extension, StringComparison.OrdinalIgnoreCase) && !SP_PDFEncryptedPassword.IsEmpty)
				{
					var passwordForOpening = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(SP_PDFEncryptedPassword);
					EncryptPDFFileOpeningAccess(path, passwordForOpening);
				}
				else if ((".xls".Equals(extension, StringComparison.OrdinalIgnoreCase) || ".xlsx".Equals(extension, StringComparison.OrdinalIgnoreCase)) && !SP_ExcelEncryptedPassword.IsEmpty)
				{
					DocumentProtector.EncryptExcelFileOpeningAccess(path, SP_ExcelEncryptedPassword);
				}
			}
		}

		void EncryptPDFFileOpeningAccess(string path, string passwordForOpening)
		{
			// WI00549860 - Encrypt invoice PDF
			// Add this ugly loop to solve the PDF file Image's color would be change after first encryption.
			// It's a work around to avoid bug of PDF Sharp. Will remove once solutions found in the future.
			for (int i = 0; i < 2; i++)
			{
				// If the PDF file has already be encrypted with password, then we need password to open it.
				// If the PDF is not encrypted, password would be ignored.
				// http://www.pdfsharp.net/wiki/ProtectDocument-sample.ashx
				using (var pdfDocument = PdfReader.Open(path, passwordForOpening))
				{
					pdfDocument.SecuritySettings.UserPassword = passwordForOpening;
					pdfDocument.Save(path);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public List<(string message, Exception exception)> ErrorsWhenConverting { get; } = new List<(string message, Exception exception)>();

		/// <summary>
		/// Use this function if you want a human readable filename.
		/// It will check to see if any other file exists in the given directory
		/// with the same name. If it does it will modify the name given to make it
		/// unique.
		/// </summary>
		/// <param name="directory">Directory to check</param>
		/// <param name="fileNameOnly">The file name only (no path information)</param>
		/// <param name="extension">Extension to append to the filename</param>
		public static string GetUniqueHumanReadableFilePath(string directory, string fileNameOnly, string extension)
		{
			string fileToReturn = Path.Combine(directory, fileNameOnly + extension);
			int numericSuffix = 1;

			while (File.Exists(fileToReturn))
			{
				fileToReturn = Path.Combine(directory, fileNameOnly + "[" + numericSuffix++ + "]" + extension);
			}

			return fileToReturn;
		}

		#endregion

		#region Execution

		public ZBool AttachmentRequiresConversion
		{
			get
			{
				if (SP_EmailAttachmentFormat.EqualsIgnoringCase(AttachmentTypeList.Codes.Pdfc) && IsEmailJob && !Enum.IsDefined(typeof(PrintJobBlobType), BlobType.ToString()))
				{
					return false;
				}

				return SP_EmailAttachmentFormat != OrgConstants.AttachmentType.FIL &&
							(IsFaxJob || (IsEmailJob || IsFtpJob || IsUnsignedDosJob) &&
							!BlobType.EqualsIgnoringCase(DocumentConverter.GetFileExtensionFromAttachmentFormat(SP_EmailAttachmentFormat)));
			}
		}

		public ZBool IsInSameMergedJob(StmPrintJob printJobToCompare)
		{
			return (SP_JobType == nameof(PrintType.EML)) &&
				(SP_JobType.EqualsIgnoringCase(printJobToCompare.SP_JobType) &&
				HasSameElementsInAnyOrder(EmailToRecipients.Emails, printJobToCompare.EmailToRecipients.Emails) &&
				HasSameElementsInAnyOrder(CarbonCopyRecipients.Emails, printJobToCompare.CarbonCopyRecipients.Emails) &&
				HasSameElementsInAnyOrder(BlindCarbonCopyRecipients.Emails, printJobToCompare.BlindCarbonCopyRecipients.Emails) &&
				SP_SB_DeliveryGroup == printJobToCompare.SP_SB_DeliveryGroup);
		}

		bool HasSameElementsInAnyOrder(IEnumerable<string> first, IEnumerable<string> second)
			=> new HashSet<string>(first).SetEquals(second);

		public PrintEngineJob GetPrintEngineJob(byte[] fileContents)
		{
			return new PrintEngineJob(
				GetSerialisablePrintJob(fileContents),
				PrintQueue.GetSerialisablePrintQueue(),
				Watermark,
				SP_FlexCelLineSpacing);
		}

		internal SerialisablePrintJob GetSerialisablePrintJob(byte[] fileContents)
		{
			var serialisableJob = new SerialisablePrintJob
			{
				BlobType = BlobType,
				Contents = fileContents,
				Copies = SP_Copies,
				EmailSubjectLine = StoredAttachmentFilename.IsEmpty ? Res.GetString("ceb7bb94-7aa5-4ad4-9462-d492f6de224d", "(No Subject)") : (string)StoredAttachmentFilename,
				EscapeSequence = SP_EscapeSequence,
				JobPk = PK.ToGuid(),
				QueueName = PrintQueue.SQ_QueueName,
				QueueStateChangedStamp = PrintQueue.SQ_PrintQueueStateChanged.IsEmpty ? Guid.Empty : PrintQueue.SQ_PrintQueueStateChanged.ToGuid(),
				HasWatermark = !SP_WatermarkImage.IsEmpty || !SP_WatermarkText.IsEmpty
			};

			return serialisableJob;
		}

		public bool DocManagerSupportsBusinessContext
		{
			get { return (!SP_RelatedBusinessContext.IsEmpty && DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(SP_RelatedBusinessContext) != null); }
		}

		internal virtual void NotifyDelivered()
		{
			if (DocManagerSupportsBusinessContext && (IsPrintJob || IsEmailJob || IsFaxJob || IsFtpJob || IsSmsJob))
			{
				SP_JobType = nameof(PrintType.DDS);

				if (!SP_EDocsProcessed)
				{
					StmDeliveryGroup deliveryGroup = Factory.New<StmDeliveryGroup>();
					deliveryGroup.SB_IsProcessed = true;

					SP_RetryAttempts = 0;
					SP_SB_DeliveryGroup = deliveryGroup.PK;
				}
			}

			if (!DocManagerSupportsBusinessContext || DocManagerSupportsBusinessContext && IsDocumentDeliverySuccessfulJob && SP_EDocsProcessed)
			{
				Delete();
			}
		}

		internal virtual void MarkEDocsProcessed()
		{
			if (IsDocumentDeliverySuccessfulJob)
			{
				Delete();
			}
			else
			{
				SP_EDocsProcessed = true;
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new StmPrintJobFetchStrategy(this);
		}

		#endregion

		#region TestData
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			SP_ParentTableName = "Test";
		}
#endif
		#endregion
	}
}
