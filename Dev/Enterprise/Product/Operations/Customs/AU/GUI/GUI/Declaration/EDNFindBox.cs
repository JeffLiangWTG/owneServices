using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class EDNFindBox : ZCodeFindBox
	{
		protected override string Code
		{
			get { return base.Code; }
			set
			{
				var bm = BindingContext[DataSource, new KBindingMemberInfo(DataMember).BindingPath];
				var invoiceLine = bm.GetCurrent() as JobComInvoiceLine;
				var invoiceHeader = bm.GetCurrent() as JobComInvoiceHeader;
				var declaration = invoiceLine != null ? invoiceLine.Declaration :
					(invoiceHeader != null ? invoiceHeader.JobDeclaration : bm.GetCurrent() as JobDeclaration);
				if (declaration != null)
				{
					base.Code = declaration.GetEntryNumberFromDeclarationReference(value);
				}
			}
		}
	}
}
