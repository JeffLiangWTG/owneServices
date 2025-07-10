<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageLayoutsPage.aspx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.FilterStrips.ManageLayoutsPage" %>

<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" TagPrefix="edi" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
</head>

<body class="ZIFramePage">
    <form id="Form1" method="post" runat="server">
        <input id="HiddenFilterNameInput" runat="server" type="hidden" />
        <table>
            <tr style="font-size: 6px">
                <td>&nbsp;</td>
            </tr>
        </table>
        <!-- whitespace -->
        <table>
            <tr>
                <td style="width: 8px">&nbsp;</td>
                <td colspan="3">
                    <edi:zbutton id="DeleteButton" runat="server" text="Delete Layout" style="min-width: 60px; color: red" />
                    <edi:zbutton id="RenameButton" runat="server" text="Rename Layout" style="min-width: 60px;" />
                </td>
                <!--<td style="font-weight:bold">&nbsp;Filter Layouts:</td>-->
            </tr>

            <tr>
                <td></td>
                <td>
                    <edi:zlistbox id="FilterLayoutsListBox" runat="server" bindto="SelectedFilterLayoutName" bindtolist="FilterLayouts" rows="15" width="250px" style="overflow-x:auto;" />
                </td>
                <td style="width: 2px">&nbsp;</td>
                <td valign="top">
                    <table id="FilterContentsTable" runat="server" />
                </td>
            </tr>
        </table>
        <table>
            <tr style="font-size: 6px">
                <td>&nbsp;</td>
            </tr>
        </table>
        <!-- whitespace -->
    </form>
</body>

</html>
