<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserAccountRelationshipControl.ascx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.UserAccountRelationshipControl" %>
<asp:label>
	The following accounts are currently merged under your name. If you no longer require access to My Account via any of the below accounts, simply uncheck the account and click Save Changes.<br />
	<br />
	<b>Note:</b><br />
	<br />
	Before proceeding, please ensure that you have a Personal Recovery Email set against your Contact Information details. This will allow you to recover access to your account if you forget your password or if your employment circumstances change.<br />
</asp:label>

<div class="table-wrapper">
	<div id="contact-lists">
		<table>
			<thead>
				<tr>
					<th>
						Organization
					</th>
					<th>
						Active Account
					</th>
					<th>
						License Type
					</th>
					<th>
						System Information
					</th>
				</tr>
			</thead>
			<tbody>
			<edi:zrepeater id="ContactUserAccountGroupingRepeater" runat="server" BindTo="ContactUserAccountWrappers">
				<HeaderTemplate>
				</HeaderTemplate>
				<ItemTemplate>
					<tr>
						<td>
							<b>
								<edi:ZTextLabel id="Organisation" runat="server" Text='<%# Eval("Organisation") %>'></edi:ZTextLabel>
							</b>
						</td>
						<td style="text-align: center">
							<edi:ZCheckBox id="IsContactRelationshipActive" runat="server"/>
						</td>
						<td>
							<edi:ZTextLabel id="LicenceType" runat="server" Text='<%# Eval("LicenceType") %>'></edi:ZTextLabel>
						</td>
						<td>
							<edi:ZTextLabel id="SystemInfo" runat="server" Text='<%# Eval("SystemInfo") %>'></edi:ZTextLabel>
						</td>
						<edi:ZNumericLabel id="ReferenceNumber" runat="server" Visible="False" Text='<%# Eval("ReferenceNumber") %>'></edi:ZNumericLabel>
					</tr>
				</ItemTemplate>
				<FooterTemplate>
				</FooterTemplate>
			</edi:zrepeater>
			</tbody>
		</table>
	</div>
</div>
<div class="login-row">
	<edi:ZButton id="SaveChangesButton" runat="server" Text="Save Changes" CssClass="ProfileButton" OnClick="SaveChangesButton_Click"></edi:ZButton>
</div>
<asp:Panel ID="ConfirmationDiv" runat="server" CssClass="CiModalOff">
	<div class="CiModalContent">
		<p>
			You are about to deactivate login for the account that you are currently logged in as.
			<br>If you continue, your login account will be deactivated and you will be logged out immediately.
		</p>
		<p>Would you like to continue?</p>
		<br/>
		<div align="center">
			<edi:ZButton ID="ConfirmationYes" runat="server" Text="Yes" CssClass="ProfileButton" OnClick="DeactivateLoginContactConfirmationYes_Click" />
			<edi:ZButton ID="ConfirmationNo" runat="server" Text="No" CssClass="ProfileButton" OnClick="DeactivateLoginContactConfirmationNo_Click" />
		</div>
	</div>
</asp:Panel>
