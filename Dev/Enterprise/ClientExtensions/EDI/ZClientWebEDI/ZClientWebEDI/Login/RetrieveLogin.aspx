<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RetrieveLogin.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.RetrieveLogin" %>
<%@ Register assembly="Enterprise.ZArchitecture.Web.GUI" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" tagprefix="edi" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>CargoWise | My Account > Retrieve Login Details</title>
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
						<h1>Forgot your password?</h1>

						<asp:Panel id="RetrieveLoginPanel" runat="server">
							<p>A reset password link will be sent to the email address you enter below.</p>
						
							<div class="login-row">
								<edi:ZTextBox ID="EmailTextBox" BindTo="Email" runat="server" Placeholder="Email address"></edi:ZTextBox>
							</div>

							<div id="block-separator"></div>
										
							<div id="login-button-panel-div">
								<asp:HyperLink ID="LoginLink1" runat="server" NavigateUrl="Login.aspx" CssClass="link-button back-button-2">Back</asp:HyperLink>
								<asp:Button ID="RetrieveButton" runat="server" Text="Retrieve" OnClick="RetrieveButton_Click" />
							</div>
					
							<edi:ZTextLabel id="InvalidEmailMessage" runat="server" CssClass="ErrorMessage" Visible="false"/>
						</asp:Panel>

						<div id="LoginDetailsRetrievedMessage" runat="server" visible="false">
							<p id="link-sent-paragraph">
								<edi:ZTextLabel id="EmailSentMessage" runat="server"/> Go <asp:HyperLink ID="LoginLink2" runat="server" NavigateUrl="Login.aspx"><u>back</u></asp:HyperLink> to the login page.
							</p>
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
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script>
		setupToggleButtonAvailabilityEventListener(document.getElementById('RetrieveButton'), [document.getElementById('EmailTextBox')]);
	</script>
</body>
</html>
