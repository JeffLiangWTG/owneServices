<%@ Register TagPrefix="cc1" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page language="c#" Codebehind="ZCodeDescriptionTreeViewPage.aspx.cs" AutoEventWireup="false" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.ZCodeDescriptionTreeViewPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title></title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<link href="ZCodeDescription.css" type="text/css" rel="stylesheet">
	</HEAD>
	<body MS_POSITIONING="GridLayout" class="ZIFramePage">
		<form id="Form1" method="post" runat="server">
			<cc1:ZTreeViewWithDescription id="TreeView" style="Z-INDEX: 101; LEFT: 24px; POSITION: absolute; TOP: 24px" runat="server"
				BorderStyle="Solid" BorderColor="Black" BorderWidth="1px" Width="480px" Height="456px" DescriptionHeight="75px"
				QuantityHeight="40px"></cc1:ZTreeViewWithDescription>
		</form>
	</body>
</HTML>
