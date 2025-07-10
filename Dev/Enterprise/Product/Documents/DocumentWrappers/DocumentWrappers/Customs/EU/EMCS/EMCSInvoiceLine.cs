using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS
{
	public class EMCSInvoiceLine : DocBaseWrapper
	{
		public static EMCSInvoiceLine New(EMCSJobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
		{
			return invoiceLine is null ? null : new EMCSInvoiceLine(invoiceLine, factory);
		}

		protected EMCSInvoiceLine(EMCSJobComInvoiceLine invoiceLine, BusinessObjectFactory factory) : base(invoiceLine, factory)
		{
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly EMCSJobComInvoiceLine InvoiceLine;

		public ZShort LineNo => InvoiceLine.JI_LineNo;
		public ZString Box18PackagesMarksAndDescription => EMCSDeclarationWrapperHelper.GetPackagesData(InvoiceLine);
		public ZString Box19CommodityCode => InvoiceLine.JI_Tariff;
		public ZString Box20Quantity => EMCSDeclarationWrapperHelper.GetItemCustomsQuantityAndUnity(InvoiceLine);
		public ZString Box21GrossMass => EMCSDeclarationWrapperHelper.GetItemMass(InvoiceLine, false);
		public ZString Box22NetMass => EMCSDeclarationWrapperHelper.GetItemMass(InvoiceLine, netMass: true);
		public ZString Box23CustomsStatus => ZString.Empty;
	}
}
