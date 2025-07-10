using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentWrappers;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class DocEdiCustomerUserAccount : DocBaseWrapper
	{
		readonly EdiCustomerUserAccount Parent;
		readonly string Url;
		DocEdiCustomerUserAccount(EdiCustomerUserAccountEmailWrapper objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
		{
			Parent = objectToWrap.Account;
			Url = objectToWrap.Url;
		}

		public static DocEdiCustomerUserAccount New(EdiCustomerUserAccountEmailWrapper objectToWrap, BusinessObjectFactory factory) => new DocEdiCustomerUserAccount(objectToWrap, factory);

		[DocumentField("Person Name")]
		public ZString PersonName => Parent?.WebAccessContact?.Person?.PER_FullName ?? ZString.Empty;

		[DocumentField("PersonEmail")]
		public ZString PersonEmail => Parent?.WebAccessContact?.Person?.PER_EmailAddress ?? ZString.Empty;

		[DocumentField("User Account Email")]
		public ZString UserAccountEmail => Parent?.EUA_Email ?? ZString.Empty;

		[DocumentField("Verify Button")]
		public ZString VerifyButton
		{
			get
			{
				var result = new ZStringBuilder();
				result.Append("<!--[if mso]>");
				result.Append(FormattableString.Invariant($"<v:roundrect href='{Url}' style='mso-wrap-style:none; mso-position-horizontal:center' arcsize='10%' strokecolor='#00a8e1' strokeweight='0px' fillcolor='#00a8e1'>"));
				result.Append("<v:textbox style='mso-fit-shape-to-text:true'>");
				result.Append(FormattableString.Invariant($"<center style='color:#ffffff; font-family:sans-serif; font-size:11px; font-weight:bold;'>{Res.GetString("E233BADE-74D8-47D9-84E7-B08BB7261FD1", "Verify Email")}</center>"));
				result.Append("</v:textbox></v:roundrect>");
				result.Append("<![endif]-->");
				result.Append("<![if !mso]>");
				result.Append("<table cellspacing='0' cellpadding='0'>");
				result.Append("<tr><td align='center' width='150' height='30' style='-webkit-border-radius: 4px; -moz-border-radius: 4px; border-radius: 4px; color: #ffffff; display: block; background-color:#00a8e1!important'>");
				result.Append(FormattableString.Invariant($"<a href='{Url}' style='font-size:11px; font-weight:bold; font-family:sans-serif; text-decoration:none; line-height:25px; width:100%; display:inline-block; border:1px solid transparent;'>"));
				result.Append(FormattableString.Invariant($"<span style='color:#ffffff;'>{Res.GetString("E233BADE-74D8-47D9-84E7-B08BB7261FD1", "Verify Email")}</span>"));
				result.Append("</a></td></tr></table>");
				result.Append("<![endif]>");
				return result.ToString();
			}
		}

		[DocumentField("Verify Link As Text")]
		public ZString VerifyLinkAsText => Url;
	}
}
