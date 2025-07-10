<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EmailSentNotification.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.EmailSentNotification" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<!DOCTYPE html>

<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CargoWise | My Account > Home > My Account Login</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
	<form id="Form1" runat="server">
		<!-- Theme Container -->
		<div id="login">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-content">
				<div runat="server" id="HeaderDiv" visible="true">
					<h1>Account Verification</h1>
				</div>
				<edi:ZTextLabel id="MessageLabel" runat="server" CssClass="ErrorMessage" style="font-weight:normal" />
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
</body>
</html>
