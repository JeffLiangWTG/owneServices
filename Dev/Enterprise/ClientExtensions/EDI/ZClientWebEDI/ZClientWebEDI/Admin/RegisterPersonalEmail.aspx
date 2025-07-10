<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterPersonalEmail.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.RegisterPersonalEmail" %>

<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" TagPrefix="edi" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta http-equiv="X-UA-Compatible" content="IE=Edge" />
	<title>Register Personal Email</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
	<style type="text/css">
		#divLogo {
			margin: 20px;
			text-align: center;
		}

		html, body, table, td, th, h1, h2, h3, h4, h5, h6, p, a, span, div, pre, input, select, #LanguageList {
			font-family: 'Noto Sans', sans-serif !important;
			font-size: 11pt !important;
		}

		.registerResult {
			margin: 0 auto 30px auto;
			padding: 20px 0;
			max-width: 500px;
		}
	</style>

</head>
<body>
	<form id="form1" runat="server">
		<div id="divLogo">
			<asp:HyperLink ID="LogoImage" runat="server" />
		</div>
		<div id="RegisterResult" class="registerResult" runat="server">
			<p>
				<img id="ResultImage" src="~/Images/success.png" alt="" runat="server" /><edi:ZTextLabel ID="ResultMessageLabel" runat="server"/>
			</p>
		</div>
		<div class="registerResult">
			<edi:ZTextLabel ID="ErrorMessage" runat="server"/>
		</div>
	</form>
</body>
</html>
