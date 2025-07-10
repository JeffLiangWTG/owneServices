using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class RFPNumberWrapper : IRFPNumber
	{
		public RFPNumberWrapper(RFPNumber rfpNumber, RFPNumberCollection rfpLines, IEnumerable<IRFPContainer> containers)
		{
			Argument.NotNull(rfpNumber, "rfpNumber");
			Argument.NotNull(rfpLines, "rfpLines");
			Argument.NotNull(containers, "containers");
			RFPNumber = rfpNumber.ZA_RFPNumber;
			RFPLines = GetRFPLines(rfpLines, containers);
		}

		#region IRFPNumber Members

		public ZString RFPNumber { get; private set; }
		public IEnumerable<IRFPLine> RFPLines { get; private set; }

		#endregion

		#region Implementation

		IEnumerable<IRFPLine> GetRFPLines(RFPNumberCollection rfpLines, IEnumerable<IRFPContainer> containers)
		{
			foreach (RFPNumber line in rfpLines)
			{
				if (line.ZA_RFPNumber == RFPNumber)
				{
					yield return new RFPLineWrapper(line, containers);
				}
			}
		}

		#region RFPLineWrapper

		class RFPLineWrapper : IRFPLine
		{
			public RFPLineWrapper(RFPNumber rfpLine, IEnumerable<IRFPContainer> containers)
			{
				Argument.NotNull(rfpLine, "rfpLine");
				Argument.NotNull(containers, "containers");
				this.rfpLine = rfpLine;
				Containers = containers;
			}

			#region IRFPLine Members

			public ZInt RFPLineNumber
			{
				get { return rfpLine.ZA_RFPLine; }
			}

			public ZDecimal NetLineQuantity
			{
				get { return rfpLine.ZA_RFPNetQuantity; }
			}

			public ZString NetQuantityUnit
			{
				get { return rfpLine.ZA_RFPQtyUM; }
			}

			public ZDecimal PackQuantity
			{
				get { return rfpLine.ZA_RFPPackCount; }
			}

			public ZString PackType
			{
				get { return rfpLine.ZA_RFPPackType; }
			}

			public IEnumerable<IRFPContainer> Containers { get; private set; }

			#endregion

			readonly RFPNumber rfpLine;
		}

		#endregion

		#endregion
	}
}
