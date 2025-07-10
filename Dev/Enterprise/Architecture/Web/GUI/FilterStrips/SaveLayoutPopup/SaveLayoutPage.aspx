<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SaveLayoutPage.aspx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.FilterStrips.SaveLayoutPage" %>
<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls"	TagPrefix="edi" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
</head>

<body class="ZIFramePage">
	<form id="Form1" method="post" runat="server">
		<input id="HiddenFilterNameInput" runat="server" type="hidden" />
		<table><tr style="font-size:6px"><td>&nbsp;</td></tr></table> <!-- whitespace -->
		<table>
		
			<tr>
				<td style="width:8px">&nbsp;</td>
				<td>
					Enter a description for this layout:&nbsp;&nbsp;
				</td>
				<td>
					<edi:ZTextBox ID="LayoutNameTextBox" runat="server" bindto="LayoutName" style="width:95%" />
				</td>
			</tr>
			
			<tr>
				<td style="width:8px">&nbsp;</td>
				<td>&nbsp;</td>
				<td>
					<edi:ZCheckBox ID="PublishLayoutCheckBox" runat="server" bindto="IsPublished" text="Publish this layout for all users" /><br>
					<edi:ZCheckBox ID="PublishCompanyLayoutCheckBox" runat="server" bindto="IsPublishedForCompany" text="Publish this layout for all users in this company" />
				</td>
			</tr>

		</table>
		<table><tr style="font-size:6px"><td>&nbsp;</td></tr></table> <!-- whitespace -->
	</form>
</body>

</html>
