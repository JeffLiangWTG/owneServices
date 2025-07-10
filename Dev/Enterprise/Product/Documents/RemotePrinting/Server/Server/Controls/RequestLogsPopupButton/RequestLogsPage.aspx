<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RequestLogsPage.aspx.cs" Inherits="Enterprise.RemotePrinting.Server.Controls.RequestLogsPage" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
</head>
<body class="ZIFramePage">
	<form id="form1" method="post" runat="server">
		<table>
			<tr>
				<td class="clientlog-label">From Date:</td>
				<td></td>
				<td class="clientlog-label">
					<edi:ZDateEdit ID="StartDate" runat="server" BindTo="FromDate"></edi:ZDateEdit>
				</td>
			</tr>
			<tr>
				<td class="clientlog-label">To Date:</td>
				<td></td>
				<td class="clientlog-label">
					<edi:ZDateEdit ID="EndDate" runat="server" BindTo="ToDate"></edi:ZDateEdit>
				</td>
			</tr>
			<tr>
				<td class="clientlog-label">Email Address:</td>
				<td></td>
				<td class="clientlog-label">
					<edi:ZTextBox ID="EmailID" runat="server" BindTo="EmailAddress"></edi:ZTextBox>
				</td>
			</tr>
			<tr>
				<td class="clientlog-label">Log types:</td>
				<td></td>
				<td class="clientlog-label">
					<edi:ZCheckBox ID="Logs" BindTo="Logs" runat="server" Text="Desktop Logs" />
					<edi:ZCheckBox ID="ServiceLogs" BindTo="ServiceLogs" runat="server" Text="Service Logs" /><br />
					<edi:ZCheckBox ID="WindowsEvents" BindTo="WindowsEvents" runat="server" Text="Windows Events" />
					<edi:ZCheckBox ID="InstallLogs" BindTo="InstallLogs" runat="server" Text="Install Logs" />
				</td>
			</tr>
		</table>
	</form>
</body>
</html>
