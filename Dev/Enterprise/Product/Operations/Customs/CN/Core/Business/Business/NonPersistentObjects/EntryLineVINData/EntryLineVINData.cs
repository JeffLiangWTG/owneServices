using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class EntryLineVINData : NonPersistentBusinessObject
	{
		public EntryLineVINData(EntryLineProductQualification productQualification, VINData vinData) : base(vinData.Factory)
		{
			this.productQualification = Argument.NotNull(productQualification, nameof(productQualification));
			vINData = Argument.NotNull(vinData, nameof(vinData));
		}
		readonly VINData vINData;
		readonly EntryLineProductQualification productQualification;

		public ZShort EntryLineNo => productQualification.EntryLineNo;
		public ZInt ProductQualificationSequence => productQualification.Sequence;
		public ZInt Sequence { get; set; }
		public ZString VIN => vINData.XC_VIN;
		public ZString ChassisNo => vINData.XC_ChassisNo;
		public ZString EngineNo => vINData.XC_EngineNo;
		public ZString ModelEN => vINData.XC_ModelEN;
		public ZString ProductNameCN => vINData.XC_ProductNameCN;
		public ZString ProductNameEN => vINData.XC_ProductNameEN;
		public ZString QGP => vINData.XC_QGP;

		public ZDateTime BillOfLadingDate { get; set; }
		public ZString InvoiceNumber { get; set; }
		public ZDecimal InvoiceQuantity { get; set; }
		public ZDecimal UnitPrice { get; set; }
	}
}
