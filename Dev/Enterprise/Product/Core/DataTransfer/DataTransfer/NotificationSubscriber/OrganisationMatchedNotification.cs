using System;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer
{
	public class OrganisationMatchedNotification : BusinessObjectAfterSaveNotification
	{
		public OrganisationMatchedNotification(Xsd.Organisation inputOrg, OrgHeader matchedOrg, CodeDescriptionPairList additionalNotificationInfo)
			: base(matchedOrg, NotificationSubscriberType.OrganisationMatched)
		{
			this.inputOrg = inputOrg;
			fMultiLineDisplayMessageCore = String.Empty;
			fDisplayMessageCore = String.Empty;
			AdditionalNotificationInfo = additionalNotificationInfo ?? new CodeDescriptionPairList();
		}

		readonly CodeDescriptionPairList AdditionalNotificationInfo;

		protected override string DisplayMessageCore
		{
			get
			{
				var header = BusinessEntity as OrgHeader;
				if (header != null && !header.IsDeleted)
				{
					if (header.PK == Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation)
					{
						var nameAndCode = Res.GetString("b9161e03-29ed-4e3a-a262-75a644d29fa4", "Name: '{0}'", inputOrg.OrganisationDetails.Name) + (!inputOrg.OwnerCode.IsEmpty ? "  " + Res.GetString("2f7a72af-7af6-452c-8d6b-2e69f876a3c8", "Code: '{0}'", inputOrg.OwnerCode) : "");
						fDisplayMessageCore = Res.GetString("79f06162-5e53-46d5-b035-cb0aaa58e61e", "Organization not found - Assigned to UNMATCHED organization ({0})", nameAndCode);
					}
					else
					{
						if (!header.IsInDatabase)
						{
							return ZString.Empty;
						}
						fDisplayMessageCore = Res.GetString("7682b1d6-90f5-48cf-9f38-271e2d6ed05b", "Successfully matched organization with code '{0}'", header.OH_Code);
					}
					foreach (CodeDescriptionPair entry in AdditionalNotificationInfo)
					{
						fDisplayMessageCore += ", " + entry.Code + ": " + entry.Description;
					}
				}
				return fDisplayMessageCore;
			}
		}
		string fDisplayMessageCore;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override string MultiLineDisplayMessageCore
		{
			get
			{
				fMultiLineDisplayMessageCore = ZString.Empty;
				var matchedOrg = (OrgHeader)BusinessEntity;
				if (matchedOrg != null && !matchedOrg.IsDeleted)
				{
					var builder = new StringBuilder();
					AppendLine(builder, "", "Input", "Matched");
					var separator = new string('-', 30);
					AppendLine(builder, "", separator, separator);
					AppendLine(builder, "Code", "-", matchedOrg.OH_Code);
					AppendLine(builder, "Full Name", inputOrg.OrganisationDetails.Name, matchedOrg.OH_FullName);
					AppendMainAddressDetails(builder, matchedOrg);
					foreach (CodeDescriptionPair entry in AdditionalNotificationInfo)
					{
						AppendLine(builder, entry.Code, "-", entry.Description);
					}
					fMultiLineDisplayMessageCore = builder.ToString();
				}
				return fMultiLineDisplayMessageCore;
			}
		}
		string fMultiLineDisplayMessageCore;

		#region Implementation

		protected void AppendLine(StringBuilder builder, ZString name, ZString input, ZString matched)
		{
			builder.AppendFormat("{0, -15}  {1, -30}  {2, -30}\r\n", name.Left(15), input.Left(30), matched.Left(30));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		void AppendMainAddressDetails(StringBuilder builder, OrgHeader matchedOrg)
		{
			var addressLine1 = "";
			var addressLine2 = "";
			var phone = "";
			var fax = "";
			var email = "";

			var mainAddress = inputOrg.OrganisationDetails.Addresses.GetMainAddress();
			if (mainAddress != null)
			{
				addressLine1 = mainAddress.AddressLine1;
				addressLine2 = mainAddress.AddressLine2;
				phone = GetFirstTelephoneNumber(mainAddress.TelephoneNumbers, Xsd.TelephoneNumberNumberType.Business);
				fax = GetFirstTelephoneNumber(mainAddress.TelephoneNumbers, Xsd.TelephoneNumberNumberType.Fax);
				email = mainAddress.Email;
			}

			AppendLine(builder, "Address", addressLine1, matchedOrg.MainAddress.OA_Address1);
			AppendLine(builder, "", addressLine2, matchedOrg.MainAddress.OA_Address2);
			AppendLine(builder, "Closest Port", inputOrg.OrganisationDetails.Location.Value, matchedOrg.OH_RL_NKClosestPort);
			AppendLine(builder, "Phone", phone, matchedOrg.MainAddress.OA_Phone);
			AppendLine(builder, "Fax", fax, matchedOrg.MainAddress.OA_Fax);
			AppendLine(builder, "Email", email, matchedOrg.MainAddress.OA_Email);
		}

		ZString GetFirstTelephoneNumber(Xsd.TelephoneNumberCollection telephoneNumbers, Xsd.TelephoneNumberNumberType type)
		{
			foreach (Xsd.TelephoneNumber number in telephoneNumbers)
			{
				if (number.NumberType == type)
				{
					return number.Value;
				}
			}

			return "";
		}
		readonly Xsd.Organisation inputOrg;

		#endregion
	}
}
