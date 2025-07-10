<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RequestUpgrade.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.My_account.RequestUpgrade" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

    <form id="form1" runat="server">
    <link type="text/css" rel="stylesheet" href="../BaseStyle.css" />
    <center>
<!-- content -->
      <table border="0">
        <tbody>
        <tr valign="middle" align="center">
          <td bgColor="white">
				<br/>
				<!-- /title --><!-- #BeginEditable "Body" -->
                <center><h2><edi:ZTextLabel id="LabelReleaseBuildName" font-name="Verdana" Text="" runat="server"/><br/></h2>
                <p><edi:ZTextLabel id="LabelPleaseSelectUpgradeMethod" Text="Please select the upgrade method that you want to use." runat="server"/></p>
                <edi:ZTextLabel id="LabelUpgradeMethodInfo" Text="" font-size="10pt" runat="server" ForeColor="Red" />
                <edi:ZTextLabel id="LabelStatus" Text="" font-size="10pt" runat="server" ForeColor="Green" />
                <table>
                    <tr>
                        <td>
                            <edi:ZTextLabel id="LabelUpgradeMethod" font-name="Verdana" Text="Upgrade Method: " font-size="10pt" runat="server"/>
                        </td>
                        <td>
                            <asp:DropDownList id="UpgradeMethod" runat="server" AutoPostBack="true"></asp:DropDownList>
                        </td>
                        <td>
                            <asp:Button ID="RequestUpgradeButton" runat="server" Text="Request Upgrade" Height="20px" Width="120px"/>
                        </td>
                    </tr>
                </table>
                <br/>
					<p><edi:ZTextLabel id="LabelUpgradeMethodDescription" runat="server" Text="" /></p>
                    <asp:Button ID="CloseButton" runat="server" Text="Close Window" Height="20px" Width="120px"/>
                </center>
           </td>
          </tr></tbody></table><!-- /content -->
</center>
</form>
