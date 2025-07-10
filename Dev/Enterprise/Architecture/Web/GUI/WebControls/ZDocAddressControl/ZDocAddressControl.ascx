<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ZDocAddressControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.ZDocAddressControl" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<script runat="server">

    protected void OrgFindBox_TextChanged(object sender, EventArgs e)
    {

    }
</script>

<asp:Label ID="DocAddressLabel" runat="server"></asp:Label>&nbsp;&nbsp;
<edi:ZCheckBox id="OverrideCheckBox" runat="server" BindTo="E2_AddressOverride" Text="Override" OnCheckedChanged="OverrideCheckBox_CheckedChanged" AutoPostBack="True"></edi:ZCheckBox>
<asp:MultiView ID="DocAddressMultiView" runat="server" ActiveViewIndex="0">
    <asp:View ID="OrgAddressView" runat="server">
		<table class="ResultsTable">
    		<tr>
				<td colspan="2">
					<edi:ZGuidFindBox id="OrgFindBox" runat="server" AutoPostBack="True"></edi:ZGuidFindBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="AddressLabel" runat="server">Address</asp:Label>
                </td>
                <td>
				    <edi:ZGuidDropDownList id="AddressDropEdit" runat="server" AutoPostBack="True" DataValueField="PK" DataTextField="OA_Code"
					    BindToList="Organisation.Addresses" BindTo="E2_OA_Address"
					    ShowEmptyItem="True"></edi:ZGuidDropDownList>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label ID="OrgFullNameLabel" runat="server"></asp:Label><br />
                    <asp:Label ID="OrgAddressLabel" runat="server"></asp:Label><br />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="ContactLabel" runat="server">Contact</asp:Label>
                </td>
                <td>
				    <edi:ZGuidDropDownList id="OrgContact" runat="server" AutoPostBack="True" DataValueField="PK" DataTextField="OC_ContactName"
					    BindToList="Organisation.Contacts" BindTo="ContactPK"
					    ShowEmptyItem="True"></edi:ZGuidDropDownList>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label ID="OrgPhoneLabel" runat="server"></asp:Label><br />
                    <asp:Label ID="OrgFaxLabel" runat="server"></asp:Label><br />
                    <asp:Label ID="OrgEmailLabel" runat="server"></asp:Label><br />
                    <asp:Label ID="OrgWebLabel" runat="server"></asp:Label>&nbsp;<asp:HyperLink ID="OrgWebLink" runat="server"></asp:HyperLink>
                 </td>
            </tr>
        </table>
    </asp:View>
    <asp:View ID="JobDocAddressView" runat="server">
		<table class="ResultsTable">
            <tr>
                <td colspan="3">
                    <asp:Label ID="AddressOverrideLabel" runat="server">Address</asp:Label>
                </td>
            </tr>
            <tr>
                <td>Co.:</td>
                <td colspan="2">
                    <edi:ZTextBox ID="CompanyTextBox" runat="server" BindTo="E2_CompanyName" Width="100%"></edi:ZTextBox>
                </td>
            </tr>
            <tr>
                <td>Addr.:</td>
                <td colspan="2">
                    <edi:ZTextBox ID="AddressLine1TextBox" runat="server" BindTo="E2_Address1" Width="100%"></edi:ZTextBox>
                </td>
            </tr>
            <tr>
                <td></td>
                <td colspan="2">
                    <edi:ZTextBox ID="AddressLine2TextBox" runat="server" BindTo="E2_Address2" Width="100%"></edi:ZTextBox>
                </td>
            </tr>
            <tr>
                <td>P/C:</td>
                <td style="width: 60px">
                    <edi:ZTextBox ID="PostCodeTextBox" runat="server" BindTo="E2_Postcode" Width="50px"></edi:ZTextBox>
                </td>
                <td>City:&nbsp;
                    <edi:ZTextBox ID="CityTextBox" runat="server" BindTo="E2_City"></edi:ZTextBox>
                </td>
            </tr>
            <tr>
                <td>Region:</td>
                <td style="width: 60px">
                    <edi:ZTextBox ID="RegionTextBox" runat="server" BindTo="E2_State" Width="50px"></edi:ZTextBox>
                </td>
                <td>Country:&nbsp;
					<edi:ZFindBox id="CountryFindBox" runat="server" BindTo="E2_RN_NKCountryCode"
						AutoPostBack="False" ModuleID="RefCountryWeb" Width="120px"></edi:ZFindBox>
                </td>
            </tr>
            <tr>
                <td colspan="3" align="right">
                    <edi:ZCheckBox id="ResidentialCheckBox" runat="server" BindTo="E2_IsResidential" Text="Residential Address"></edi:ZCheckBox>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Label ID="ContactOverrideLabel" runat="server">Contact</asp:Label>
                </td>
                </tr>
            <tr>
                <td>Cont.:</td>
                <td colspan="2">
                    <edi:ZTextBox ID="ContactNameTextBox" runat="server" BindTo="E2_Contact" Width="100%"></edi:ZTextBox>
                </td>
            </tr>
            <tr>
                <td>Ph.:</td>
                <td colspan="2">
                    <edi:ZTextBox ID="PhoneTextBox" runat="server" BindTo="E2_Phone" Width="100%"></edi:ZTextBox>
                </td>
            </tr>
            <tr>
                <td>Fax:</td>
                <td colspan="2">
                    <edi:ZTextBox ID="FaxTextBox" runat="server" BindTo="E2_Fax" Width="100%"></edi:ZTextBox>
                </td>
            </tr>
            <tr>
                <td>Email:</td>
                <td colspan="2">
                    <edi:ZTextBox ID="EmailTextBox" runat="server" BindTo="E2_Email" Width="100%"></edi:ZTextBox>
                </td>
            </tr>
        </table>
    </asp:View>
</asp:MultiView>
