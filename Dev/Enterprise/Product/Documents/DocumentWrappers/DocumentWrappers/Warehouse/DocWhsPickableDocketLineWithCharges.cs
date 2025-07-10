using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickableDocketLineWithCharges : DocWhsPickableDocketLine
	{
		#region static

		public static new DocWhsPickableDocketLineWithCharges New(WhsPickableDocketLine whsPickableDocketLine, BusinessObjectFactory factoryToWrap)
		{
			return (whsPickableDocketLine == null) ? null : new DocWhsPickableDocketLineWithCharges(whsPickableDocketLine, factoryToWrap);
		}

		#endregion

		#region constructors

		DocWhsPickableDocketLineWithCharges(WhsPickableDocketLine whsPickableDocketLine, BusinessObjectFactory factoryToWrap)
			: base(whsPickableDocketLine, factoryToWrap)
		{
		}

		#endregion

		#region Charge Headers

		ZDecimal fChargeHeader1;
		public ZDecimal ChargeHeader1
		{
			get { return fChargeHeader1; }
			set { fChargeHeader1 = value; }
		}

		ZDecimal fChargeHeader2;
		public ZDecimal ChargeHeader2
		{
			get { return fChargeHeader2; }
			set { fChargeHeader2 = value; }
		}

		ZDecimal fChargeHeader3;
		public ZDecimal ChargeHeader3
		{
			get { return fChargeHeader3; }
			set { fChargeHeader3 = value; }
		}

		ZDecimal fChargeHeader4;
		public ZDecimal ChargeHeader4
		{
			get { return fChargeHeader4; }
			set { fChargeHeader4 = value; }
		}

		ZDecimal fChargeHeader5;
		public ZDecimal ChargeHeader5
		{
			get { return fChargeHeader5; }
			set { fChargeHeader5 = value; }
		}

		ZDecimal fChargeHeader6;
		public ZDecimal ChargeHeader6
		{
			get { return fChargeHeader6; }
			set { fChargeHeader6 = value; }
		}

		ZDecimal fChargeHeader7;
		public ZDecimal ChargeHeader7
		{
			get { return fChargeHeader7; }
			set { fChargeHeader7 = value; }
		}

		ZDecimal fChargeHeader8;
		public ZDecimal ChargeHeader8
		{
			get { return fChargeHeader8; }
			set { fChargeHeader8 = value; }
		}

		ZDecimal fChargeHeader9;
		public ZDecimal ChargeHeader9
		{
			get { return fChargeHeader9; }
			set { fChargeHeader9 = value; }
		}

		ZDecimal fChargeHeader10;
		public ZDecimal ChargeHeader10
		{
			get { return fChargeHeader10; }
			set { fChargeHeader10 = value; }
		}

		#endregion

	}
}
