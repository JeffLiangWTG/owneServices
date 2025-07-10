<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UpdateContactInformation.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.UpdateContactInformation" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>CargoWise | My Account > Update Contact Information</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
	<div id="login-whole-page">
		<div id="login-page-left-div">
			<form id="LoginForm" runat="server">
				<!-- Theme Container -->
				<div id="login">
					<div id="login-content">
						<div id="login-company-logo-container">
							<img id="login-company-logo" alt="CargoWise" src="../Images/blue-cargowise-logo.svg" />
						</div>
						<h1><edi:ZTextLabel ID="UpdateContactInformationHeadingLabel" runat="server" Text="Personal Recovery Email"></edi:ZTextLabel></h1>
						<div id="ContactInformationBox" runat="server">
							<div class="login-row">
								<edi:ZTextLabel ID="GreetingLabel" runat="server"></edi:ZTextLabel>
							</div>
							<div style="margin:15px;"></div>
							<div class="login-row">
								<edi:ZTextLabel ID="UpdateContactInformationInstructionsLabel" runat="server" Text="We've noticed that a personal recovery email has not been setup. Please enter an email:"></edi:ZTextLabel>
							</div>
							<div style="margin:15px;"></div>
					
							<div class="login-row">
								<edi:ZTextBox runat="server" ID="PersonalEmailTextBox" ClientIDMode="Static" class="emailTextBox" Placeholder="Personal Recovery Email"/>
							</div>

							<div class="login-row">
								<edi:ZTextLabel runat="server" ID="MessageLabel" class="ErrorMessage" />
							</div>

							<div style="margin:15px;"></div>

							<div id="login-button-panel-div">
								<edi:ZLinkButton ID="SkipButton" runat="server" Text="Skip" CssClass="link-button back-button-2" ClientIDMode="Static" OnClick="SkipButton_OnClick" />
								<asp:Button ID="SaveEmailButton" runat="server" Text="Save Email" CssClass="ProfileButton" ClientIDMode="Static" OnClick="SaveEmailButton_OnClick" />
							</div>

							<div style="margin:15px;"></div>

							<div class="login-row">
								<edi:ZButton ID="ContinueButton" runat="server" Text="Continue" CssClass="ProfileButton" ClientIDMode="Static" OnClick="ContinueButton_OnClick" />
								<edi:ZCheckBox ID="DoNotAskMeAgainCheckBox" runat="server" CssClass="RememberMeBox" ClientIDMode="Static" Text="Do not ask me again"></edi:ZCheckBox>
							</div>

							<div style="margin:15px;"></div>
				
							<div class="login-row">
								<div class="RegisterPersonalEmailMessage">
									<span id="RegisterPersonalEmailMessage" runat="server" ClientIDMode="Static"></span>
								</div>
							</div>
						</div>
						<edi:ZTextLabel id="ErrorMessage" runat="server" CssClass="ErrorMessage" />
					</div>
				</div>
				<div id="login-footer">
					<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy"><u>Privacy</u></a>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse"><u>Terms</u></a>
				</div>
				<!-- End Theme Container -->
			</form>
		</div>
		<div id="login-supply-chain-image-container">
			<img id="supply-chain-image" alt="Enabling and empowering the world's supply chains" src="../Images/supply-chain-without-whitespace.svg"/>			
		</div>
	</div>
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script>
		setupToggleButtonAvailabilityEventListener(document.getElementById('SaveEmailButton'), [document.getElementById('PersonalEmailTextBox')]);
	</script>
</body>
</html>
