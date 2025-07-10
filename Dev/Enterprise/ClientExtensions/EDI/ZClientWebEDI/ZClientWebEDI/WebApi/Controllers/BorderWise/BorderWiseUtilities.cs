using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise
{
	public static class BorderWiseUtilities
	{
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Random API requires int")]
		const int MaxDelayInMilliseconds = 50;
		[SuppressThreadStaticFieldMessage] // ThreadLocal is thread safe
		static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

		public static void AddContactCertificate(OrgContact contact, string certificateType, string reference, string comment = null)
		{
			AddContactCertificate(contact, certificateType, reference, ZDateTime.Empty, ZDateTime.Empty, comment);
		}

		public static void AddContactCertificate(OrgContact contact, string certificateType, string reference, ZDateTime issueDate, ZDateTime expiryDate, string comment = null)
		{
			var certificate = contact.Certificates.AddNew();
			certificate.XZ_Type = certificateType;
			if (!string.IsNullOrEmpty(reference))
			{
				certificate.XZ_RefNumber = SubStringSafe(reference, GenRegCertAccredMaintListSchema.XZ_RefNumber.MaxLength);
			}

			if (!string.IsNullOrEmpty(comment))
			{
				certificate.XZ_Comment = SubStringSafe(comment, GenRegCertAccredMaintListSchema.XZ_Comment.MaxLength);
			}

			if (!issueDate.IsEmpty)
			{
				certificate.XZ_IssueDate = issueDate;
			}

			if (!expiryDate.IsEmpty)
			{
				certificate.XZ_ExpiryOrDueDate = expiryDate;
			}
		}

		public static string SubStringSafe(string input, int maxLength)
		{
			Argument.NotNull(input, nameof(input));

			return input.Length > maxLength ? input.Substring(0, maxLength) : input;
		}

		public static void ChangeSecurityRight(OrgContact contact, bool granted, BusinessObjectFactory factory)
		{
			var securityRight = (OrgSecurity)contact.Header.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, EDIWebSecurityRightsList.BorderWise.SecurityItemName))[0];
			var contactSecurityRight = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, securityRight.PK))[0];
			contactSecurityRight.OZ_Granted = granted;
			securityRight.OX_Granted = granted;
			contact.Header.RefreshSecurityRights();
			contact.RefreshSecurityRights();
			factory.Save();
		}

		public static void SendEmail(string fromAddress, string toAddress, string subject, string body, GuidRegistryItem groupRegistryItem = null, bool save = true, BusinessObjectFactory factory = null)
		{
			var emailDef = new HtmlEmailDef
			{
				FromAddress = fromAddress,
				FromDisplayName = "BorderWise Support",
				ReplyTo = fromAddress,
			};

			if (!string.IsNullOrWhiteSpace(toAddress))
			{
				emailDef.AddRecipientForUserCommunication(toAddress);
			}

			emailDef.Subject = subject;
			emailDef.Body = LoadHtmlUsingTemplate(body);

			try
			{
				if (save)
				{
					if (groupRegistryItem == null)
					{
						Env.OutgoingMailManager.CreateAndSave(emailDef);
					}
					else
					{
						Env.OutgoingMailManager.CreateAndSave(emailDef, groupRegistryItem.Value, GroupSourceLocator.GetFromRegistryItem(groupRegistryItem));
					}
				}
				else
				{
					if (groupRegistryItem == null)
					{
						Env.OutgoingMailManager.Create(factory, emailDef);
					}
					else
					{
						Env.OutgoingMailManager.Create(factory, emailDef, groupRegistryItem.Value, GroupSourceLocator.GetFromRegistryItem(groupRegistryItem));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Exception occurred when sending the following email:\r\n\r\n{0}\r\n{1}", subject, body), ex);
			}
		}

		public static Guid CreateTokenCore(string tokenName, string tokenValue, BusinessObjectFactory factory)
		{
			var token = Guid.NewGuid();
			var tokenRecord = factory.New<StmData>();
			tokenRecord.SD_Name = tokenName + token;
			tokenRecord.SD_BinaryValue = ZBlob.FromAscii(tokenValue);
			return token;
		}

		public static string GenerateResetPasswordUrl(OrgContact contact)
		{
			var info = new PasswordResetInfo()
			{
				Product = ProductTypes.Codes.BorderWise,
				ContactEmail = contact.Email,
				OrgCode = contact.OrgCode,
				NavigateUrl = new Uri(DataRegistry.Instance.BorderWiseWebAddress)
			};
			var json = JsonConvert.SerializeObject(info);
			var resetTokenType = ((IPasswordInstructionEmailSource)contact).ShouldSendMasterPassword ? AccessTokenTypes.ResetMasterPassword : AccessTokenTypes.ResetPassword;
			var token = new TokenizedAccessControl().CreateLimitedToken(resetTokenType, new AccessTokenInfo(json, Guid.Empty, "INV"), TimeSpan.FromHours(24), 1);
			return contact.GeneratePasswordInstructionUrl(token, PasswordInstructionType.Reset);
		}

		public static string LoadHtmlUsingTemplate(string body)
		{
			var emailTemplate = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html xmlns: o=""urn:schemas-microsoft-com:office:office"" xmlns: v=""urn:schemas-microsoft-com:vml"" xmlns=""http://www.w3.org/1999/xhtml"">
	<head>
		<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
		<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
		<title>BorderWise</title>
		<!--[if !mso]><!-->     
		<link href=""https://fonts.googleapis.com/css?family=DM+Sans"" rel=""stylesheet"" type=""text/css"">
		<!--<![endif]-->
		<style type=""text/css"">
			#outlook a {{
				padding: 0;
			}}
			body {{
				width: 100% !important;
				-webkit-text-size-adjust: 100%;
				-ms-text-size-adjust: 100%;
				margin: 0;
				padding: 0;
			}}
			.ExternalClass {{
				width: 100%;
			}}
			.ExternalClass,
			.ExternalClass p,
			.ExternalClass span,
			.ExternalClass font,
			.ExternalClass th,
			.ExternalClass div {{
				line-height: 100%;
			}}
			.background-table {{
				margin: 0;
				padding: 0;
				width: 100% !important;
				min-width: 100% !important;
				line-height: 100% !important;
			}}
			img {{
				outline: none;
				text-decoration: none;
				-ms-interpolation-mode: bicubic;
				display: block;
			}}
			a img {{
				border: none;
			}}
		</style>
		<style type=""text/css"">
			table {{
				border-collapse: collapse;
			}}
			th {{
				border-collapse: collapse;
			}}
			@media all and (max-width: 802px) {{
				.background-table.has-width-802.email-content {{
						width: 100% !important;
				}}
				.background-table.has-width-802 img {{
				max-width: 100%;
				height: auto;
				}}
				.background-table.has-width-802.mobile-display-block {{
						display: block;
						width: 100%;
				}}
				.background-table.has-width-802.mobile-display-none {{
						display: none;
				}}
				.background-table.has-width-802.mobile-display-table-caption {{
						display: table-caption!important;
				}}
				.background-table.has-width-802.mobile-display-table-header-group {{
						display: table-header-group!important;
				}}
				.background-table.has-width-802.mobile-display-table-row {{
						display: table-row!important;
				}}
				.background-table.has-width-802.mobile-display-table-footer-group {{
						display: table-footer-group!important;
				}}
				.background-table.has-width-802.mobile-width-auto {{
						width: auto!important;
				}}
			}}
			@media screen and (device-width: 768px) and (device-height: 1024px) {{
				body {{
						min-width: 701px!important;
				}}
			}}
		</style>
		<style type=""text/css"">
			a {{
				color: #102A43;
				text-decoration: underline;
			}}
			.header a {{
				color: #a6a9ac;
			}}
			.footer a {{
				color: #ffffff;
				text-decoration: none;
			}}
			h1 {{
				color: #1D1765;
			}}
			h2 {{
				color: #1D1765;
			}}
			h3 {{
				color: #1D1765;
			}}
			h4 {{
				color: #1D1765;
			}}
		</style>
	</head>
	<body class=""md-r"" style='margin: 0px; padding: 0px; width: 100%; color: rgb(15, 42, 70); line-height: 20px; font-family: ""DM Sans"", sans-serif; font-size: 14px; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%;'>
		<table width=""802"" class=""background-table has-width-802"" role=""presentation"" style=""margin: 0px; padding: 0px; width: 100%; text-align: left; line-height: 100%; min-width: 802px;"" border=""0"" cellspacing=""0"" cellpadding=""0"" valign=""top"">
			<tbody>
				<tr>
					<th valign=""top"" data-selectedcell=""__SELECTED_CELL_d94ee14f0f234ead8d35f12f19daa5e4"">
						<table width=""802"" align=""center"" class=""email-content"" role=""presentation"" style=""margin: 0px auto; width: 802px;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
							<tbody>
								<tr>
									<th valign=""top"">
										<div>
											<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
												<tbody>
													<tr>
														<th class=""header vertical-grid"" valign=""top"" style=""text-align: left;"">
															<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																<tbody>
																	<tr>
																		<th height=""10"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																	</tr>
																	<tr>
																		<th class=""horizontal-grid"" valign=""top"" style=""text-align: left;"">
																			<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																				<tbody>
																					<tr>
																						<th width=""15"" class=""mobile-display-none vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																					</tr>
																				</tbody>
																			</table>
																		</th>
																	</tr>
																	<tr>
																		<th height=""40"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																	</tr>
																</tbody>
															</table>
														</th>
													</tr>
												</tbody>
											</table>
										</div>
										<div>
											<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
												<tbody>
													<tr>
														<th class=""vertical-grid"" valign=""top"" style=""text-align: left;"" bgcolor=""#0092f7"">
															<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																<tbody>
																	<tr>
																		<th class=""horizontal-grid"" valign=""top"" style=""text-align: left;"">
																			<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																				<tbody>
																					<tr>
																						<th align=""left"" class=""banner-image image-container"" valign=""middle"" style=""text-align: left;"" bgcolor=""#0092f7"">
																							<a style=""color: rgb(16, 42, 67); text-decoration: none;"" href=""https://app.borderwise.com/"" target=""_blank"" tid=""BW.png [Image Link]"">
																								<img width=""800"" height=""100"" style=""border: currentColor; border-image: none; text-decoration: none; display: block; -ms-interpolation-mode: bicubic;"" alt=""BorderWise"" 
																									src=""https://app.borderwise.com/assets/img/BWEmailBanner.png""
																									tid=""BorderWise"" embedded=""false"">
																							</a></th>
																					</tr>
																				</tbody>
																			</table>
																		</th>
																	</tr>
																</tbody>
															</table>
														</th>
													</tr>
												</tbody>
											</table>
										</div>
										<div>
											<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
												<tbody>
													<tr>
														<th class=""vertical-grid"" valign=""top"" style=""text-align: left;"" bgcolor=""#ffffff"">
															<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																<tbody>
																	<tr>
																		<th height=""40"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																	</tr>
																	<tr>
																		<th class=""horizontal-grid"" valign=""top"" style=""text-align: left;"">
																			<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																				<tbody>
																					<tr>
																						<th width=""15"" class=""mobile-display-none vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																						<th width=""25"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																						<th class=""text-container"" valign=""top"" style='text-align: left; color: rgb(15, 42, 70); line-height: 20px; font-family: ""DM Sans"", sans-serif; font-size: 14px; font-weight: 400; max-width: 700px;'>
																							{0}
																						<th width=""25"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																						<th width=""15"" class=""mobile-display-none vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																					</tr>
																				</tbody>
																			</table>
																		</th>
																	</tr>
																	<tr>
																		<th height=""10"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																	</tr>
																	<tr>
																		<th class=""horizontal-grid"" valign=""top"" style=""text-align: left;"">
																			<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																				<tbody>
																					<tr>
																						<th width=""15"" class=""mobile-display-none vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																						<th width=""25"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																						<th class=""text-container"" valign=""top"" style='text-align: left; color: rgb(15, 42, 70); line-height: 20px; font-family: ""DM Sans"", sans-serif; font-size: 14px; font-weight: normal;'>
																							<p>Regards,<br/>
																								BorderWise - WiseTech Global Team
																							</p>
																						</th>
																						<th width=""25"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																						<th width=""15"" class=""mobile-display-none vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																					</tr>
																				</tbody>
																			</table>
																		</th>
																	</tr>
																	<tr>
																		<th height=""20"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
																	</tr>
																</tbody>
															</table>
														</th>
													</tr>
												</tbody>
											</table>
										</div>
										<div>
											<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
												<tbody>
													<tr>
														<th class=""footer vertical-grid"" valign=""top"" style=""text-align: left;"" bgcolor=""#0092f7"">
															<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																<tbody>
																	<tr>
																		<th height=""25"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																	</tr>
																	<tr>
																		<th class=""horizontal-grid"" valign=""top"" style=""text-align: left;"">
																			<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																				<tbody>
																					<tr>
																						<th width=""15"" class=""mobile-display-none vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																						<th width=""25"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																						<th class=""horizontal-grid"" valign=""top"" style=""text-align: left;"">
																							<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																								<tbody>
																									<tr>
																										<th class=""mobile-display-block vertical-grid"" valign=""top"" style=""text-align: left;"">
																											<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																												<tbody>
																													<tr>
																														<th class=""text-container"" valign=""top"" style='text-align: left; color: rgb(255, 255, 255); line-height: 22px; font-family: ""DM Sans"", sans-serif; font-size: 16px; font-weight: normal;' bgcolor=""#0092f7"">
																															Empowering and enabling the logistics industry globally.
																														</th>
																													</tr>
																													<tr>
																														<th height=""20"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																													</tr>
																													<tr>
																														<th class=""horizontal-grid"" valign=""top"" style=""text-align: left;"">
																															<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
																																<tbody>
																																	<tr>
																																		<th align=""left"" class=""image-container"" valign=""top"" style=""text-align: left;"" bgcolor=""#0092f7""><a style=""color: rgb(255, 255, 255); text-decoration: none;"" href=""http://www.linkedin.com/company/13728?trk=tyah"" target=""_blank"" tid=""linkedin.png [Image Link]""><img width=""15"" height=""15"" style=""border: currentColor; border-image: none; text-decoration: none; display: block; -ms-interpolation-mode: bicubic;"" alt=""LinkedIn"" src=""https://app.borderwise.com/assets/img/icons/linkedin.png"" tid=""LinkedIn"" embedded=""false""></a></th>
																																		<th width=""10"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																																		<th align=""left"" class=""image-container"" valign=""top"" style=""text-align: left;"" bgcolor=""#0092f7""><a style=""color: rgb(255, 255, 255); text-decoration: none;"" href=""https://www.facebook.com/WiseTechGlobal"" target=""_blank"" tid=""facebook.png [Image Link]""><img width=""15"" height=""15"" style=""border: currentColor; border-image: none; text-decoration: none; display: block; -ms-interpolation-mode: bicubic;"" alt=""Facebook"" src=""https://app.borderwise.com/assets/img/icons/facebook.png"" tid=""Facebook"" embedded=""false""></a></th>
																																		<th width=""10"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																																		<th align=""left"" class=""image-container"" valign=""top"" style=""text-align: left;"" bgcolor=""#0092f7""><a style=""color: rgb(255, 255, 255); text-decoration: none;"" href=""https://twitter.com/WiseTechGlobal"" target=""_blank"" tid=""twitter.png [Image Link]""><img width=""15"" height=""15"" style=""border: currentColor; border-image: none; text-decoration: none; display: block; -ms-interpolation-mode: bicubic;"" alt=""Twitter"" src=""https://app.borderwise.com/assets/img/icons/twitter.png"" tid=""Twitter"" embedded=""false""></a></th>
																																		<th width=""10"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																																		<th align=""left"" class=""image-container"" valign=""top"" style=""text-align: left;"" bgcolor=""#0092f7""><a style=""color: rgb(255, 255, 255); text-decoration: none;"" href=""http://www.youtube.com/user/WiseTechGlobal"" target=""_blank"" tid=""youtube.png [Image Link]""><img width=""15"" height=""15"" style=""border: currentColor; border-image: none; text-decoration: none; display: block; -ms-interpolation-mode: bicubic;"" alt=""YouTube"" src=""https://app.borderwise.com/assets/img/icons/youtube.png"" tid=""YouTube"" embedded=""false""></a></th>
																																		<th width=""30"" class=""mobile-display-none vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																																		<th width=""30"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																																		<th class=""text-container"" valign=""top"" style='text-align: left; color: rgb(255, 255, 255); line-height: 14px; font-family: ""DM Sans"", sans-serif; font-size: 12px; font-weight: normal;' bgcolor=""#0092f7""><a style=""color: rgb(255, 255, 255); text-decoration: none;"" href=""https://www.wisetechglobal.com/"" tid=""wisetechglobal.com"">wisetechglobal.com</a></th>
																																	</tr>
																																</tbody>
																															</table>
																														</th>
																													</tr>
																												</tbody>
																											</table>
																										</th>
																									</tr>
																								</tbody>
																							</table>
																						</th>
																						<th width=""40"" class=""vertical-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																					</tr>
																				</tbody>
																			</table>
																		</th>
																	</tr>
																	<tr>
																		<th height=""25"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;"" bgcolor=""#0092f7""> &nbsp;</th>
																	</tr>
																</tbody>
															</table>
														</th>
													</tr>
												</tbody>
											</table>
										</div>
										<div>
											<table width=""100%"" role=""presentation"" style=""min-width: 100%;"" border=""0"" cellspacing=""0"" cellpadding=""0"">
												<tbody>
													<tr>
														<th height=""350"" class=""horizontal-gutter"" valign=""top"" style=""text-align: left; line-height: 1px; font-size: 1px;""> &nbsp;</th>
													</tr>
												</tbody>
											</table>
										</div>
									</th>
								</tr>
							</tbody>
						</table>
					</th>
				</tr>
			</tbody>
		</table>
	</body>
</html>";

			return string.Format(CultureInfo.CurrentCulture, emailTemplate, body ?? string.Empty);
		}

		public static void RandomDelay()
		{
			Thread.Sleep(Random.Value.Next(0, MaxDelayInMilliseconds));
		}
	}
}
