<%@ Page Language="c#" AutoEventWireup="false" Codebehind="GridLayoutPage.aspx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.GridLayoutPage" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
</head>
<body id="DefaultBody" runat="server" class="ZIFramePage">
	<form id="Form1" method="post" runat="server">
<div id="GridColumnsCustomisation" runat="server">
<table id="Table1" class="ResultsTable">
	<tr>
		<td>Available Columns:</td>
		<td style="width: 49px">&nbsp;</td>
		<td>Selected Columns:</td>
		<td>&nbsp;</td>
	</tr>
	<tr>
		<td><edi:ZListBox ID="AvailableColumnsListBox" runat="server" Rows="10" BindTo="SelectedAvailableColumn" BindToList="AvailableColumns" DataTextField="HeaderText" DataValueField="ColumnNumber" Width="120px" ></edi:ZListBox></td>
		<td align="center">
			&nbsp;<asp:Button ID="SelectOneButton" runat="server" Text=">" Width="30px" />&nbsp;<br />
			&nbsp;<asp:Button ID="RemoveOneButton" runat="server" Text="<" Width="30px" OnClientClick="javascript:var layout = document.getElementById('CurrentLayoutListBox'); if (parseInt(layout.options[layout.selectedIndex].value) < 0) { alert('Cannot remove required column.'); }" />&nbsp;<br />
			&nbsp;&nbsp;<br />
			&nbsp;<asp:Button ID="DefaultButton" runat="server" Text="Reset" />&nbsp;<br />
		</td>
		<td><edi:ZListBox ID="CurrentLayoutListBox" runat="server" Rows="10" BindTo="SelectedLayoutColumn" AllowSorting="False" BindToList="CurrentLayout" DataTextField="HeaderText" DataValueField="ColumnNumber" Width="120px"></edi:ZListBox></td>
		<td>
			&nbsp;<asp:Button ID="MoveUpButton" runat="server" Text="Up" Width="45px" />&nbsp;<br />
			&nbsp;<asp:Button ID="MoveDownButton" runat="server" Text="Down" Width="45px" />&nbsp;<br />
		</td>
	</tr>
</table>
</div>
</form>
</body>
</html>
