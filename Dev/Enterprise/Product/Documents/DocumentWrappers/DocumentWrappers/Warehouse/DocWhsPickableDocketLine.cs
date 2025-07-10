using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickableDocketLine : DocWhsDocketLine
	{
		#region Static

		public static DocWhsPickableDocketLine New(WhsPickableDocketLine whsPickableDocketLine, BusinessObjectFactory factoryToWrap)
		{
			return (whsPickableDocketLine == null) ? null : new DocWhsPickableDocketLine(whsPickableDocketLine, factoryToWrap);
		}

		#endregion

		#region Constructors

		protected DocWhsPickableDocketLine(WhsPickableDocketLine whsPickableDocketLine, BusinessObjectFactory factoryToWrap)
			: base(whsPickableDocketLine, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		WhsPickableDocketLine WhsPickableDocketLine
		{
			get { return (WhsPickableDocketLine)WrappedObject; }
		}

		public override DocWhsDocket Docket
		{
			get { return WhsPickableDocketLine.DocketType == typeof(WhsOrder) ? DocWhsPickableDocket.New((WhsOrder)WhsPickableDocketLine.Docket, Factory) : null; }
		}

		#region AddLineForRollUp

		public void AddLineForRollUp(WhsPickableDocketLine line)
		{
			if (line.WE_OP != WhsPickableDocketLine.WE_OP)
			{
				throw new ArgumentException(string.Format(
					"Only lines for the same product can be rolled up. You added a Line for Product '{0}' to a Line for Product '{1}'.", line.ProductDesc, WhsPickableDocketLine.ProductDesc));
			}

			RolledUpLines.Add(line);
		}

		List<WhsPickableDocketLine> RolledUpLines
		{
			get
			{
				if (rolledUpLines == null)
				{
					rolledUpLines = new List<WhsPickableDocketLine>();
					if (WhsPickableDocketLine != null)
					{
						rolledUpLines.Add(WhsPickableDocketLine);
					}
				}
				return rolledUpLines;
			}
		}
		List<WhsPickableDocketLine> rolledUpLines;

		#endregion

		#endregion

		#region Properties

		#region LineNoCore

		protected override ZInt LineNoCore
		{
			get
			{
				ZInt min = WhsPickableDocketLine.WE_LineNo;
				foreach (WhsPickableDocketLine line in RolledUpLines)
				{
					if (min > line.WE_LineNo)
					{
						min = line.WE_LineNo;
					}
				}
				return min;
			}
		}

		#endregion

		#region UnitsCore

		protected override ZDecimal UnitsCore
		{
			get
			{
				ZDecimal result = 0m;
				foreach (WhsPickableDocketLine line in RolledUpLines)
				{
					result += line.WE_TransactionQuantity;
				}
				return result;
			}
		}

		#endregion

		#region UnitsMetCore

		protected override ZDecimal UnitsMetCore
		{
			get
			{
				ZDecimal result = 0m;
				foreach (WhsPickableDocketLine line in RolledUpLines)
				{
					result += line.SumOfUnitsMet;
				}
				return result;
			}
		}

		#endregion

		#region PalletsSent

		public override ZShort PalletsSent
		{
			get
			{
				ZShort result;
				var part = WhsDocketLine.SupplierPart;
				if (part != null)
				{
					result = (short)part.UnitConverter.Convert(UnitsMet, part.OP_StockKeepingUnit, "PLT");
				}
				else
				{
					result = base.PalletsSent;
				}
				return result;
			}
		}

		#endregion

		#region PackagesSent

		public override ZInt PackagesSent
		{
			get
			{
				ZInt result;
				var part = WhsDocketLine.SupplierPart;
				if (part != null)
				{
					result = (ZInt)part.UnitConverter.Convert(UnitsMet, part.OP_StockKeepingUnit, "PKG");
					return result;
				}
				else
				{
					result = base.PackagesSent;
				}
				return result;
			}
		}

		#endregion

		public MultilingualString PartAttribute2Name
		{
			get
			{
				MultilingualString attributeName = (NoResString)ZString.Empty;
				if (Docket != null && Docket.Client != null && Docket.Client.MiscServ != null)
				{
					attributeName = Docket.Client.MiscServ.IMPartAttrib2Name;
				}
				return attributeName;
			}
		}

		public MultilingualString PartAttribute3Name
		{
			get
			{
				MultilingualString attributeName = (NoResString)ZString.Empty;
				if (Docket != null && Docket.Client != null && Docket.Client.MiscServ != null)
				{
					attributeName = Docket.Client.MiscServ.IMPartAttrib3Name;
				}
				return attributeName;
			}
		}

		public ZString ProductDescription
		{
			get { return Product != null ? Product.Desc : ZString.Empty; }
		}

		#endregion
	}
}
