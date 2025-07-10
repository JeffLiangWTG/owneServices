<%@ Page Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="True" CodeBehind="Invoices.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.Invoices" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="server">
	<div class="wrapper">
		<p id="Breadcrumb" runat="server">
			<a href="../../index.html" class="pagetrail" target="_parent">Home</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
			<a href="../Default.aspx" class="pagetrail">My Account</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
		</p>

		<h1>Invoices</h1>
		<hr size="1" />

		<div class="ContentSection">
			<table>
				<tr>
					<td><label for="YearDropDownList">Year:</label></td>
					<td><edi:ZDropDownList ID="YearDropDownList" runat="server" AutoPostBack="true" /></td>
					<td style="width:20px"></td> <!-- Spacer -->
					<td><label for="ProductDropDownList">Product:</label></td>
					<td><edi:ZDropDownList ID="ProductDropDownList" runat="server" AutoPostBack="true" /></td>
				</tr>
			</table>

			<h2>Current Outstanding Invoices</h2>
			<edi:ZDataGrid ID="OutstandingInvoicesDataGrid" runat="server" 
				AllowPaging="true" PageSize="15" 
				BorderWidth="1" CellSpacing="0" GridLines="None" UseAccessibleHeader="true"
				HeaderStyle-CssClass="DetailsHeader" ItemStyle-CssClass="DetailsCell" CssClass="DetailedDataGrid"
				OnItemDataBound="InvoicesDataGrid_ItemDataBound">
				<Columns>
					<asp:BoundColumn DataField="AH_TransactionNum" HeaderText="Invoice No." ItemStyle-Width="80px" ItemStyle-HorizontalAlign="center"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_TransactionType"  HeaderText="Type" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="center"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_RX_NKTransactionCurrency" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="right"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_OSTotal" HeaderText="Amount" ItemStyle-Width="60px" ItemStyle-HorizontalAlign="right" DataFormatString="{0:0.00}"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_InvoiceDate" HeaderText="Invoice Date" ItemStyle-Width="105px" ItemStyle-HorizontalAlign="center" DataFormatString="{0:dd-MMM-yyyy}"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_DueDate" HeaderText="Due Date" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="center" DataFormatString="{0:dd-MMM-yyyy}"></asp:BoundColumn>
					<asp:TemplateColumn ItemStyle-HorizontalAlign="center" ItemStyle-Width="60px">
						<ItemTemplate>
							<asp:HyperLink ID="InvoiceLink" runat="server" Text="Detail" />
						</ItemTemplate>
					</asp:TemplateColumn>
				</Columns>
				<PagerStyle CssClass="ResultsTablePager" />
			</edi:ZDataGrid>

			<h2>Historical Invoices</h2>
			<edi:ZDataGrid ID="HistoricalInvoicesDataGrid" runat="server"
				AllowPaging="true" PageSize="15" 
				BorderWidth="1" CellSpacing="0" GridLines="None" UseAccessibleHeader="true"
				HeaderStyle-CssClass="DetailsHeader" ItemStyle-CssClass="DetailsCell" CssClass="DetailedDataGrid"
				OnItemDataBound="InvoicesDataGrid_ItemDataBound">
				<Columns>
					<asp:BoundColumn DataField="AH_TransactionNum" HeaderText="Invoice No." ItemStyle-Width="80px" ItemStyle-HorizontalAlign="center"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_TransactionType"  HeaderText="Type" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="center"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_RX_NKTransactionCurrency" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="right"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_OSTotal" HeaderText="Amount" ItemStyle-Width="60px" ItemStyle-HorizontalAlign="right" DataFormatString="{0:0.00}"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_InvoiceDate" HeaderText="Invoice Date" ItemStyle-Width="105px" ItemStyle-HorizontalAlign="center" DataFormatString="{0:dd-MMM-yyyy}"></asp:BoundColumn>
					<asp:BoundColumn DataField="AH_DueDate" HeaderText="Due Date" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="center" DataFormatString="{0:dd-MMM-yyyy}"></asp:BoundColumn>
					<asp:TemplateColumn ItemStyle-HorizontalAlign="center" ItemStyle-Width="60px">
						<ItemTemplate>
							<asp:HyperLink ID="InvoiceLink" runat="server" Text="Detail" />
						</ItemTemplate>
					</asp:TemplateColumn>
				</Columns>
				<PagerStyle CssClass="ResultsTablePager" />
			</edi:ZDataGrid>

			<br /><br />
		</div>
	</div>
</asp:Content>
