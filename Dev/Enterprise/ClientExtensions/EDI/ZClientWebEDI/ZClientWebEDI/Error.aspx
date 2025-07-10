<%@ Page language="c#" Codebehind="Error.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.Error" %>
<%@ Register TagPrefix="cc1" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Application Error</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	</HEAD>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server" style="text-align:center;padding-top:50px">
				<DIV id="Title">
					<h1 runat="server" id="PageTitle">Application Error</h1>
				</DIV>
				<div>
					<asp:Literal id="MessageDescription" runat="server" Text="A problem has been encountered with the Web Application. Sorry for any inconvenience this may have caused."></asp:Literal>
				</div>
				<div>
					<asp:HyperLink id="HomeLink" runat="server" />
				</div>
			</div>
		</form>
	</body>
</HTML>
