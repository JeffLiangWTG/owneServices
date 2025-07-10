<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Diagnostics.aspx.cs" Inherits="Enterprise.RemotePrinting.Server.SupportDiagnostics" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Support Diagnostics</title>
</head>
<body>
	<h4 class="header">CargoWise WebPrint Support Diagnostics </h4>
	<form id="form1" runat="server">

		<div class="support-info">
			<asp:Label ID="VersionNumberLabel" runat="server" Text="Version Number" CssClass="support-label"></asp:Label>
			<asp:Label ID="VersioNumberVal" runat="server" Text="" CssClass="support-value"></asp:Label>
		</div>
		<div class="support-info">
			<asp:Label ID="VersionDateLabel" runat="server" Text="Version Date" CssClass="support-label"></asp:Label>
			<asp:Label ID="VersionDate" runat="server" Text="" CssClass="support-value"></asp:Label>
		</div>
		<div class="support-info">
			<asp:Label ID="LicenseCodeLabel" runat="server" Text="License Code" CssClass="support-label"></asp:Label>
			<asp:Label ID="LicenseCode" runat="server" Text="" CssClass="support-value"></asp:Label>

		</div>
		<div class="support-info">
			<asp:Label ID="ReleaseLabel" runat="server" Text="Release" CssClass="support-label"></asp:Label>
			<asp:Label ID="ReleaseText" runat="server" Text="" CssClass="support-value"></asp:Label>
		</div>

		<br />
		<h6 class="sub-header">Print Queues </h6>
		<edi:ZDataGrid ID="PrintQueuesDataGrid" runat="server"
			BindTo="PrintQueues" CssClass="SupportGrid">
			<ItemStyle CssClass="SupportCell"></ItemStyle>
			<HeaderStyle CssClass="SupportHeader"></HeaderStyle>
		</edi:ZDataGrid>
		<br />
		<h6 class="sub-header">WebPrint Clients connected via Signal-R </h6>
		<edi:ZDataGrid ID="SignalRClientGrid" runat="server"
			BindTo="SignalRClients" CssClass="SupportGrid">
			<ItemStyle CssClass="SupportCell"></ItemStyle>
			<HeaderStyle CssClass="SupportHeader"></HeaderStyle>
		</edi:ZDataGrid>
		<br />
		<br />
	</form>

	
</body>
</html>
