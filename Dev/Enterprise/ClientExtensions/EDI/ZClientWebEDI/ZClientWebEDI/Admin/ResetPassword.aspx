<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.ResetPassword" %>
<%@ Register TagPrefix="cc1" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<%@ Register TagPrefix="edi" Tagname="AccountVerification" src="../Login/AccountVerificationControl.ascx"%>
<%@ Register TagPrefix="edi" Tagname="PasswordChangeRequirements" src="PasswordChangeRequirementsControl.ascx" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>CargoWise | My Account > Reset Password</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<style type="text/css">
	body { counter-reset: AutoNumCounter; }
	.AutoNum:before {
		content: counter(AutoNumCounter) ". ";
		counter-increment: AutoNumCounter;
	}
</style>
<body>
	<form id="form1" runat="server">
		<!-- Theme Container -->
		<div id="login">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-content">
				<h1>Reset your password</h1>
				<div id="HeadingMessageDiv" runat="server">
					<div>
						<cc1:ZTextLabel id="PasswordExpiredMessageLabel" runat="server" Text="Your password has expired due to an enforced password rotation policy. Please enter a new password." Visible="False"></cc1:ZTextLabel>
					</div>
				</div>

				<div class="login-row">
					<cc1:ZTextLabel ID="OrgCodeLabel" runat="server" Text="Company code: "></cc1:ZTextLabel>
					<cc1:ZDropDownList ID="OrgCodeDropDownList" runat="server" DataValueField="OH_Code" DisplayStyle="CodeOnly" EnableViewState="False" AutoPostBack="true" OnSelectedIndexChanged="OrgCodeDropDownList_SelectedIndexChanged">
					</cc1:ZDropDownList>
				</div>
				
				<div runat="server" id="PasswordDiv">
					<div class="login-row">
						<asp:textbox id="NewPassword" runat="server" TextMode="Password" Placeholder="New password" OnKeyUp="return checkPasswordRequirements(document.getElementById('NewPassword').value);"></asp:textbox>
					</div>
					
					<div class="login-row">
						<asp:textbox id="NewPasswordConfirm" runat="server" TextMode="Password" Placeholder="Confirm new password"></asp:textbox>
					</div>

					<div class="login-row password-requirements">
						<edi:PasswordChangeRequirements id="passwordChangeRequirements" runat="server" />
					</div>

					<div class="login-row">
						<asp:Button id="Update" runat="server" Text="Reset password" onclick="Update_Click"></asp:Button>
						<asp:Hyperlink id="BackLink" runat="server" CssClass="link-button back-button">Back</asp:Hyperlink>
					</div>

					<cc1:ZTextLabel id="PasswordChangeMessage" runat="server" CssClass="ErrorMessage" />
					<div id="GoBackMessage" runat="server" visible="false">
						<p>
							Go <asp:HyperLink ID="GoBackLoginLink" runat="server" NavigateUrl="../Login/Login.aspx">back</asp:HyperLink> to the login page.
						</p>
					</div>
				</div>
				<div runat="server" id="LoginOptionsDiv" visible="false">
					<edi:AccountVerification id="AccountVerification" runat="server" />
				</div>
			</div>
			<div id="login-footer">
				<strong>&copy; <cc1:ZTextLabel id="CopyrightYear" runat="server"></cc1:ZTextLabel> WiseTech Global</strong>
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
		setupToggleButtonAvailabilityEventListener(document.getElementById('Update'), [document.getElementById('NewPassword'), document.getElementById('NewPasswordConfirm')]);
	</script>
</body>
</html>
