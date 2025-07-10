<%@ Page Language="c#" AutoEventWireup="true" CodeBehind="EmailVerification.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.EmailVerification" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<!DOCTYPE html>

<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CargoWise | My Account > Verify Email</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body runat="server">
	<div id="login-whole-page">
		<div id="login-page-left-div">
			<form id="LoginForm" runat="server" class="login-form">
				<!-- Theme Container -->
				<div id="login">
					<div id="login-page">
						<div id="login-content">
							<div id="login-company-logo-container">
								<img id="login-company-logo" alt="CargoWise" src="../Images/blue-cargowise-logo.svg" />
							</div>

							<h1>Email verification</h1>

							<asp:Label runat="server">A request has been made with MyAccount to verify your account.</asp:Label><br />
							<asp:Label runat="server">Verification is initiated by an action taken by you from a Wisetech system, or by a WiseTechGlobal specialist.</asp:Label><br />
							<asp:Label runat="server">If you are expecting email verification, please click below to proceed.</asp:Label><br />

							<asp:Button ID="verifyEmail" runat="server" Text="Verify" OnClick="VerifyEmail"></asp:Button>
						</div>
					</div>
				</div>
				<div id="login-footer">
					<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy" style="text-decoration-line: underline" >Privacy</a>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse" style="text-decoration-line: underline" >Terms</a>
				</div>
				<!-- End Theme Container -->
			</form>
		</div>
		<div id="login-supply-chain-image-container">
			<img id="supply-chain-image" alt="Enabling and empowering the world's supply chains" src="../Images/supply-chain-without-whitespace.svg"/>			
		</div>
	</div>
</body>
</html>
