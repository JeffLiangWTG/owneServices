using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	[DependentBusinessObject(typeof(EdiHelpErrorLog), "Occurrences")]
	public class HelpErrorLogOccurrence : AutoHelpErrorLogOccurrence
	{
		public HelpErrorLogOccurrence(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public string UserLoginName
		{
			get
			{
				if (userLoginName == null)
				{
					ExceptionXml xml = new ExceptionXml(HO_XMLData);
					userLoginName = xml.UserLoginName;
				}
				return userLoginName;
			}
		}

		string userLoginName;

		public string ErrorType
		{
			get
			{
				if (exceptionDescription == null)
				{
					var xml = new ExceptionXml(HO_XMLData);
					exceptionDescription = xml.ExceptionDescription;
				}

				if (exceptionDescription == "Failed to upgrade database")
				{
					return HelpErrorLogOccurrenceLookups.ErrorType.DatabaseUpgradeFailure;
				}

				return HelpErrorLogOccurrenceLookups.ErrorType.Unspecified;
			}
		}

		string exceptionDescription;

		public const string DefaultToString = "No details have been supplied for this occurrence";

		#region Database Code

		public ZString DatabaseCode
		{
			get
			{
				LicenceDatabase database = Database;
				return (database != null) ? database.LD_ServerCode : ZString.Empty;
			}
		}

		public ZPropertyInfo DatabaseCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseCode)); }
		}

		#endregion

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			unCompressedXmlData = null;
		}
		WeakReference unCompressedXmlData;

		public ZString HO_XMLData
		{
			get
			{
				string result = null;

				if (unCompressedXmlData != null)
				{
					result = unCompressedXmlData.Target as string;
				}
				if (result == null)
				{
					result = MessageEncoding.UTF8WithoutBOM.GetString(HO_CompressedXmlData);
					result = AddMissingWebInfoTags(result);
				}
				unCompressedXmlData = new WeakReference(result);
				return result;
			}
			set
			{
				unCompressedXmlData = new WeakReference(value);
				HO_CompressedXmlData = MessageEncoding.UTF8WithoutBOM.GetBytes(value);
			}
		}

		public override ZBlob HO_CompressedXmlData
		{
			get { return base.HO_CompressedXmlData; }
			set
			{
				base.HO_CompressedXmlData = value;
				unCompressedXmlData = null;
			}
		}

		ZString AddMissingWebInfoTags(ZString xMLData)
		{
			if (xMLData.IndexOf("<WebInfo>", StringComparison.Ordinal) < 0)
			{
				if (xMLData.IndexOf("<EnvironmentInfo><RequestedURL>", StringComparison.Ordinal) > -1 && xMLData.IndexOf("<DatabaseInfo>", StringComparison.Ordinal) > -1)
				{
					xMLData = xMLData.ReplaceIgnoringCase("<EnvironmentInfo><RequestedURL>", "<EnvironmentInfo><WebInfo><RequestedURL>");
					xMLData = xMLData.ReplaceIgnoringCase("<DatabaseInfo>", "</WebInfo><DatabaseInfo>");
				}
			}
			return xMLData;
		}

		public override string ToString()
		{
			string result;
			if (HO_ExceptionDateTime.IsEmpty)
			{
				result = DefaultToString;
			}
			else
			{
				result = HO_ExceptionDateTime + " for " + HO_Company + " on Server " + HO_ServerName + " with Exe built at " + HO_EXEDateTime;
			}
			return result;
		}

		public ZString TestRigOrigin
		{
			get
			{
				if (testRigOrigin == null)
				{
					var xml = new ExceptionXml(HO_XMLData);
					xml.PopulateFromXML();
					testRigOrigin = xml.TestRigOrigin;
				}

				return string.IsNullOrEmpty(testRigOrigin) ? "From Prod" : testRigOrigin;
			}
		}

		string testRigOrigin;

		public ZPropertyInfo TestRigOriginInfo
		{
			get { return GetZPropertyInfo(nameof(TestRigOrigin)); }
		}

		public ZString FinalKey
		{
			get
			{
				if (finalKey == null && Issue != null && Issue.LoadFinalKeys)
				{
					ExceptionXml xml = new ExceptionXml(HO_XMLData);
					xml.PopulateFromXML();
					finalKey = xml.KeyFields.LogKey;
				}

				return finalKey;
			}
		}

		string finalKey;

		internal void ResetFinalKeyForTest() => finalKey = null;

		public ZPropertyInfo FinalKeyInfo
		{
			get { return GetZPropertyInfo(nameof(FinalKey)); }
		}

		public ZString SessionIdAsText
		{
			get { return !HO_SessionID.IsEmpty ? new ZString(HO_SessionID.ToString()) : ZString.Empty; }
		}

		public ZPropertyInfo SessionIdAsTextInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SessionIdAsText), (sender) => { return HO_SessionIDInfo; }); }
		}

		public ZString SequenceAsText
		{
			get { return HO_Sequence >= 0 ? new ZString(HO_Sequence.ToString()) : ZString.Empty; }
		}

		public ZPropertyInfo SequenceAsTextInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SequenceAsText), (sender) => { return HO_SequenceInfo; }); }
		}

		public ZDateTime HO_ExceptionDateTimeLocal
		{
			get { return HO_ExceptionDateTime.ToLocalBranchTime(); }
		}

		public ZDateTime HO_EXEDateTimeLocal
		{
			get { return HO_EXEDateTime.ToLocalBranchTime(); }
		}

		public ZString LicenceCode
		{
			get
			{
				var result = ZString.Empty;
				var clientCompany = ClientCompany;
				if (clientCompany != null)
				{
					result = clientCompany.LicenceCode;
				}
				else
				{
					var database = Database;
					if (database != null)
					{
						result = database.EnterpriseCode + "___" + database.LD_ServerCode;
					}
				}
				return result;
			}
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			EdiHelpErrorLog log = Factory.Load<EdiHelpErrorLog>(HO_HE);
			if (!IsInDatabase)
			{
				log.HE_FailCount++;
			}
			log.UpdateFirstAndLastReported(this);
			log.UpdateFirstAndLastExeVersionDate(this);
			log.UpdateFirstAndLastVersionNumber(this);
			if (!IsInDatabase && !log.HE_FixedDate.IsEmpty)
			{
				HO_XMLData = "<EDI_Exception_Report> <ExceptionDescription>  --- Because this issue has been fixed this log has been cleared to save space. ---  </ExceptionDescription><ExceptionDetails/></EDI_Exception_Report>";
			}
		}

		#endregion

		#region Related Business Objects

		public LicenceDatabase Database
			=> Factory.Load<LicenceDatabase>(HO_LD);

		public EdiHelpErrorLog Issue
		{
			get { return Factory.Load<EdiHelpErrorLog>(HO_HE); }
		}

		public ClientCompany ClientCompany
		{
			get { return Factory.Load<ClientCompany>(HO_LCC); }
		}

		public ReleaseBuild ReleaseBuild
		{
			get { return Factory.Load<ReleaseBuild>(HO_HL); }
		}

		#endregion
	}
}
