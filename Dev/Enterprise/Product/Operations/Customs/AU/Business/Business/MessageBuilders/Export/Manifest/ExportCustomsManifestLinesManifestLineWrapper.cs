using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestLinesManifestLineWrapper : IManifestLineWrapper
	{
		public ExportCustomsManifestLinesManifestLineWrapper(ExportCustomsManifestLines line, int lineNumber)
		{
			this.line = line;
			fLineNumber = lineNumber;
		}

		public int PackCount
		{
			get
			{
				return line.EL_NumberOfPackages;
			}
		}

		public int ContainerCount
		{
			get
			{
				return line.EL_NumberOfContainers;
			}
		}

		public ZString AirWaybillNumber
		{
			get
			{
				return line.EL_AirWayBill;
			}
		}

		public ZString HouseBillNumber
		{
			get { return line.EL_AirWayBill; }
		}

		public ZString CAN
		{
			get
			{
				return line.IsCANLine ? line.EL_CAN : ZString.Empty;
			}
		}

		public ZString CCAN
		{
			get
			{
				return line.IsCCANLine ? line.EL_CAN : ZString.Empty;
			}
		}

		public ZString CountryOfDestination
		{
			get
			{
				return line.EL_RN_NKCountryOfDestination;
			}
		}

		public ZString GoodsOwner
		{
			get
			{
				return line.Owner != null ? line.Owner.OH_FullNameTruncated : line.EL_GoodsOwner;
			}
		}

		public ZString GoodsOwnerPartyID
		{
			get
			{
				return (line.Owner != null) ? line.Owner.LocalBusinessRegNo : line.EL_GoodsOwnerPartyID;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				return line.EL_GoodsDescription;
			}
		}

		public ZString ExemptionCode
		{
			get
			{
				return IsExemption ? line.EL_TypeOfCAN : ZString.Empty;
			}
		}

		public int LineNumber
		{
			get
			{
				return fLineNumber;
			}
		}

		public ZString LineActionCode
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString Reference
		{
			get
			{
				return line.EL_LineNo.ToString();
			}
		}

		#region Implementation

		protected bool IsExemption
		{
			get
			{
				return line.IsExemptLine;
			}
		}

		protected ExportCustomsManifestLines line;
		protected int fLineNumber;

		#endregion
	}
}
