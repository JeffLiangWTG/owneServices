<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WiseTechAcademyAutoLogin.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.WiseTechAcademyAutoLogin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript">
		function SubmitLoginForm() {
			LoginForm.submit();
		}
	</script>
</head>
<body id="DefaultBody" runat="server">
    <form id="LoginForm" runat="server">
        <div>
            <asp:HiddenField runat="server" id="Token" />
            <asp:HiddenField runat="server" id="TenantId" />
            <asp:HiddenField runat="server" id="Product" />
            <asp:HiddenField runat="server" id="OrgPk" />
            <asp:HiddenField runat="server" id="OrgName" />
            <asp:HiddenField runat="server" id="ContactWorkingAddressOrgName" />
            <asp:HiddenField runat="server" id="ContactWorkingAddressCountry" />
            <asp:HiddenField runat="server" id="ContactLocation" />
            <asp:HiddenField runat="server" id="LicenceDatabaseMasterOrgCode" />
            <asp:HiddenField runat="server" id="LicenceDatabaseMasterOrgName" />
            <asp:HiddenField runat="server" id="LicenceDatabaseBillingOrgCode" />
            <asp:HiddenField runat="server" id="LicenceDatabaseBillingOrgName" />
            <asp:HiddenField runat="server" id="UserId" />
            <asp:HiddenField runat="server" id="ContactPk" />
            <asp:HiddenField runat="server" id="ContactName" />
            <asp:HiddenField runat="server" id="ContactEmail" />
            <asp:HiddenField runat="server" id="PersonalEmail" />
            <asp:HiddenField runat="server" id="PersonIDs" />
            <asp:HiddenField runat="server" id="Path" />
            <asp:HiddenField runat="server" id="Target" />
            <asp:HiddenField runat="server" id="QuickstartId" />
            <asp:HiddenField runat="server" id="SearchText" />
            <asp:HiddenField runat="server" id="CourseId" />
            <asp:HiddenField runat="server" id="Type" />
            <asp:HiddenField runat="server" id="ProgramId" />
            <asp:HiddenField runat="server" id="AutoLoginOriginProduct" />
            <asp:HiddenField runat="server" id="AutoLoginOriginProductVersion" />
        </div>
    </form>
</body>
</html>
