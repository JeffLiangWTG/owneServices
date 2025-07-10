using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class SignOff : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SignOff>",
				ResString.GetMultilingualString("72617947-be37-416d-910a-5077b3bbddb6", "Returns the sign off text as well as the currently logged in user's title and published contact details. These are defined in the Registry and Staff forms."),
				new List<(string example, object expectedResult)> { ("<SignOff>", (NoResString)@"Best Regards,

John Doe
Manager
 Email: user@wisetechglobal.com Work: 123456789 Work Extension: 789 Fax: +61 2 9025 1199 Home: +61 2 9025 1100 Mobile: 123456789") });
		}

		[ReturnsResourceString]
		protected override object GetReplacementCore(string macro, Report report)
		{
			StringBuilder result = new StringBuilder();

			bool registryAvailable = Env.Instance.CurrentCompany != null && Env.Instance.CurrentBranch != null && Env.Instance.CurrentDepartment != null;

			if (registryAvailable)
			{
				string signOffText = Env.Registry.SignOffText;
				if (!string.IsNullOrEmpty(signOffText))
				{
					result.AppendLine(signOffText);
					result.AppendLine();
				}
			}

			result.Append(GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_FullName.ToString() : Enterprise.Core.Constants.ProductName);

			if (registryAvailable && DocumentsDataRegistry.Instance.ShowUserTitleOnDocuments.Value && GlbStaff.CurrentUser != null)
			{
				result.AppendLine();
				result.Append(GlbStaff.CurrentUser.GS_Title);
			}

			if (registryAvailable && DocumentsDataRegistry.Instance.ShowPublishedStaffDetailsOnDocuments.Value && GlbStaff.CurrentUser != null)
			{
				result.AppendLine();
				AppendDetailLine(result, GlbStaff.CurrentUser.GS_PublishEmailAddress, Res.GetString("f1566a91-01a0-4d88-b38a-430c3f361b9d", "Email:"), GlbStaff.CurrentUser.GS_EmailAddress);
				AppendDetailLine(result, GlbStaff.CurrentUser.GS_PublishWorkPhone, Res.GetString("c339ab1b-60ef-4289-bc66-dc5c747169da", "Work:"), GlbStaff.CurrentUser.GS_WorkPhone);
				AppendDetailLine(result, GlbStaff.CurrentUser.GS_PublishWorkExtension, Res.GetString("61fa01ba-c59a-45e1-862a-b613d62a19cc", "Work Extension:"), GlbStaff.CurrentUser.GS_WorkExtension);
				AppendDetailLine(result, GlbStaff.CurrentUser.GS_PublishFaxNum, Res.GetString("f275c202-0ebf-47a0-9d42-8d0469c7ad83", "Fax:"), GlbStaff.CurrentUser.GS_FaxNum);
				AppendDetailLine(result, GlbStaff.CurrentUser.GS_PublishHomePhone, Res.GetString("eb78f687-9e4b-4726-b808-1b001f51d38f", "Home:"), GlbStaff.CurrentUser.GS_HomePhone);
				AppendDetailLine(result, GlbStaff.CurrentUser.GS_PublishMobilePhone, Res.GetString("8fe9fdcc-3e59-428c-a0d7-984270f6da32", "Mobile:"), GlbStaff.CurrentUser.GS_MobilePhone);
			}

			return result.ToString();
		}

		void AppendDetailLine(StringBuilder stringBuilder, bool publishDetail, string detailDescription, string detailText)
		{
			if ((publishDetail) && !string.IsNullOrEmpty(detailText))
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(detailDescription);
				stringBuilder.Append(" ");
				stringBuilder.Append(detailText);
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex("^" + RegexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		const string RegexPattern = @"<\s*SignOff\s*>";

		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
