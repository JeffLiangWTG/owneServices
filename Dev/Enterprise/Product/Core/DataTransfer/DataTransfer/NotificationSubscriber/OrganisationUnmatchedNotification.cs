using System;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTransfer
{
	public class OrganisationUnmatchedNotification : BusinessObjectAfterSaveNotification
	{
		public OrganisationUnmatchedNotification(OrgHeader unmatchedOrg, CodeDescriptionPairList additionalNotificationInfo)
			: base(unmatchedOrg, NotificationSubscriberType.OrganisationUnmatched)
		{
			fDisplayMessageCore = String.Empty;
			fMultiLineDisplayMessageCore = String.Empty;
			AdditionalNotificationInfo = additionalNotificationInfo ?? new CodeDescriptionPairList();
		}

		readonly CodeDescriptionPairList AdditionalNotificationInfo;

		protected override string DisplayMessageCore
		{
			get
			{
				if (BusinessEntity != null && !BusinessEntity.IsDeleted)
				{
					fDisplayMessageCore = Res.GetString("97352186-026d-4085-bdd0-f6faf0d24566", "Failed to matched organization with code/name '") + ((OrgHeader)BusinessEntity).OH_Code + "' / '" + ((OrgHeader)BusinessEntity).OH_FullNameTruncated + "'";
					foreach (CodeDescriptionPair entry in AdditionalNotificationInfo)
					{
						fDisplayMessageCore += ", " + entry.Code + ": " + entry.Description;
					}
				}
				return fDisplayMessageCore;
			}
		}
		string fDisplayMessageCore;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string MultiLineDisplayMessageCore
		{
			get
			{
				fMultiLineDisplayMessageCore = ZString.Empty;
				var unmatchedOrg = (OrgHeader)BusinessEntity;
				if (unmatchedOrg != null && !unmatchedOrg.IsDeleted)
				{
					var builder = new StringBuilder();
					AppendLine(builder, "Code", unmatchedOrg.OH_Code);
					AppendLine(builder, "Full Name", unmatchedOrg.OH_FullNameTruncated);
					AppendLine(builder, "Address", unmatchedOrg.MainAddress.OA_Address1);
					AppendLine(builder, "", unmatchedOrg.MainAddress.OA_Address2);
					AppendLine(builder, "Closest Port", unmatchedOrg.OH_RL_NKClosestPort);
					AppendLine(builder, "Phone", unmatchedOrg.MainAddress.OA_Phone);
					AppendLine(builder, "Fax", unmatchedOrg.MainAddress.OA_Fax);
					AppendLine(builder, "Email", unmatchedOrg.MainAddress.OA_Email);
					foreach (CodeDescriptionPair entry in AdditionalNotificationInfo)
					{
						AppendLine(builder, entry.Code, entry.Description);
					}
					fMultiLineDisplayMessageCore = builder.ToString();
				}
				return fMultiLineDisplayMessageCore;
			}
		}
		string fMultiLineDisplayMessageCore;

		#region Implementation

		protected void AppendLine(StringBuilder builder, ZString name, ZString data)
		{
			builder.AppendFormat("{0, -15}  {1, -30}\r\n", name.Left(15), data.Left(30));
		}

		#endregion
	}
}
