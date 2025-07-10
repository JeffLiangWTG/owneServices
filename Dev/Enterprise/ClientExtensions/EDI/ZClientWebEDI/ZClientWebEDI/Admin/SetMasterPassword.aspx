<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SetMasterPassword.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.SetMasterPassword" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<%@ Register TagPrefix="edi" TagName="PasswordChangeRequirements" Src="PasswordChangeRequirementsControl.ascx" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CargoWise | My Account > Set Master Password</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
	<div id="login-whole-page">
		<div id="login-page-left-div">
			<form id="LoginForm" runat="server">
				<div id="login" class="set-master-password">
					<div id="login-content">

						<div id="login-company-logo-container">
							<img id="login-company-logo" alt="CargoWise" src="../Images/blue-cargowise-logo.svg" />
						</div>

						<div class="login-row">
							<h1>My Account Password</h1>
							<edi:ZTextLabel ID="SetMasterPasswordHeadingLabel" runat="server" Text="Please enter a new password. This will apply to the following linked user account(s)."></edi:ZTextLabel>
						</div>

						<div id="contact-lists" class="choose-company-table">
							<div id="ContactsBox" class="table-wrapper" runat="server">
								<table class="reset-password-table">
									<thead>
										<tr>
											<th class="set-password-table-header">Company Code</th>
											<th class="set-password-table-header">Account(s)</th>
										</tr>
									</thead>
									<tbody>
										<edi:ZRepeater ID="LoginContactsRepeater" runat="server" BindTo="LoginContacts">
											<HeaderTemplate>
											</HeaderTemplate>
											<ItemTemplate>
												<tr>
													<td>
														<edi:ZTextLabel ID="OrganisationCode" runat="server" Text='<%# Eval("OrganisationCode") %>'></edi:ZTextLabel>
													</td>
													<td>
														<div style="display: flex; flex-direction: column">
															<edi:ZTextLabel ID="CompanyName" runat="server" Text='<%# Eval("WorkingAddressCompanyName") %>'></edi:ZTextLabel>
															<edi:ZTextLabel ID="Email" runat="server" Text='<%# Eval("OC_Email") %>'></edi:ZTextLabel>
														</div>
													</td>
												</tr>
											</ItemTemplate>
											<FooterTemplate>
											</FooterTemplate>
										</edi:ZRepeater>
									</tbody>
								</table>
							</div>
						</div>

						<div style="margin: 15px;"></div>

						<div id="set-password-page-panel">
							<div class="login-row">
								<asp:TextBox ID="NewPassword" runat="server" TextMode="Password" Placeholder="New password" OnKeyUp="return checkPasswordRequirements(document.getElementById('NewPassword').value);"></asp:TextBox>
							</div>

							<div style="margin: 15px;"></div>

							<div class="login-row">
								<asp:TextBox ID="NewPasswordConfirm" runat="server" TextMode="Password" Placeholder="Confirm new password"></asp:TextBox>
							</div>

							<div style="margin: 15px;"></div>

							<div class="login-row password-requirements">
								<edi:PasswordChangeRequirements ID="passwordChangeRequirements" runat="server" />
							</div>

							<div style="margin: 15px;"></div>

							<div class="login-row">
								<asp:Button ID="Update" runat="server" Text="Set Password" OnClick="Update_Click"></asp:Button>
							</div>
						</div>
						<edi:ZTextLabel ID="PasswordChangeMessage" runat="server" CssClass="ErrorMessage"></edi:ZTextLabel>
						<p>
							Go <u>
								<asp:HyperLink ID="GoBackLoginLink" runat="server" NavigateUrl="../Login/Login.aspx">back</asp:HyperLink></u> to the login page.
						</p>
					</div>
				</div>
				<div id="login-footer">
					<strong>&copy;
						<edi:ZTextLabel ID="CopyrightYear" runat="server"></edi:ZTextLabel>
						WiseTech Global</strong>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy" style="text-decoration-line: underline">Privacy</a>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse" style="text-decoration-line: underline">Terms</a>
				</div>
			</form>
		</div>
		<div id="login-supply-chain-image-container">
			<img id="supply-chain-image" alt="Enabling and empowering the world's supply chains" src="../Images/supply-chain-without-whitespace.svg" />
		</div>
	</div>
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script type="text/javascript" src="../Scripts/passwordChangeMouseRightClickPaste.js"></script>
	<script>
		setupToggleButtonAvailabilityEventListener(document.getElementById('Update'), [document.getElementById('NewPassword'), document.getElementById('NewPasswordConfirm')]);
	</script>
</body>
</html>
