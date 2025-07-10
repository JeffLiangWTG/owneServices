<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.Admin" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<%@ Register TagPrefix="edi" Tagname="PasswordChangeRequirements" src="PasswordChangeRequirementsControl.ascx" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>CargoWise | My Account > Change Password</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
	<form id="form1" runat="server">
		<!-- Theme Container -->
		<div id="login" class="set-master-password">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-content">
				<h1>Change your password</h1>
				<edi:ZTextLabel id="ChangePasswordInstructionsLabel" runat="server" Text="Your password controls access to the following accounts:"></edi:ZTextLabel>

				<div id="contact-lists" class="login-row">
					<div id="ContactsBox" class="table-wrapper" runat="server">
						<table>
							<thead>
							<tr>
								<th>
									Person
								</th>
								<th>
									Related Account(s)
								</th>
							</tr>
							</thead>
							<tbody>
							<tr style="background-color: #f7f7f7;">
								<td>
									<edi:ZTextLabel id="Person" runat="server" BindTo="Name"></edi:ZTextLabel> 
								</td>
								<td>
									<edi:ZTextLabelNoEncode id="RelatedAccounts" runat="server"></edi:ZTextLabelNoEncode> 
								</td>
							</tr>
							</tbody>
						</table>
					</div>
				</div>

				<div class="login-row">
					<asp:textbox id="CurrentPassword" runat="server" TextMode="Password" Placeholder="Current password"></asp:textbox>
				</div>
				<div class="login-row">
					<asp:textbox id="NewPassword" runat="server" TextMode="Password" Placeholder="New password"
						OnKeyUp="return checkPasswordRequirements(document.getElementById('NewPassword').value);"></asp:textbox>
				</div>
				<div class="login-row">
					<asp:textbox id="NewPasswordConfirm" runat="server" TextMode="Password" Placeholder="Confirm new password"></asp:textbox>
				</div>

				<div class="login-row password-requirements">
					<edi:PasswordChangeRequirements id="passwordChangeRequirements" runat="server" />
				</div>

				<div class="login-row">
					<asp:Button id="Update" runat="server" Text="Change Password" onclick="Update_Click"></asp:Button>
				</div>
				<div class="login-row">
					<asp:Hyperlink id="BackLink" runat="server" CssClass="link-button master-password-back-button">Back</asp:Hyperlink>
				</div>

				<edi:ZTextLabel id="PasswordChangeMessage" runat="server" CssClass="ErrorMessage" />
			</div>
			<div id="login-footer">
				<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
				<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy">Privacy Policy</a>
				<a target="_blank" href="https://www.wisetechglobal.com/legal#cookies">Cookie Usage</a>
				<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse">Terms of Use</a>
			</div>
		</div>
		<!-- End Theme Container -->
	</form>
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script type="text/javascript" src="../Scripts/passwordChangeMouseRightClickPaste.js"></script>
	<script>
		setupToggleButtonAvailabilityEventListener(document.getElementById('Update'), [document.getElementById('CurrentPassword'), document.getElementById('NewPassword'), document.getElementById('NewPasswordConfirm')]);
	</script>
</body>
</html>
