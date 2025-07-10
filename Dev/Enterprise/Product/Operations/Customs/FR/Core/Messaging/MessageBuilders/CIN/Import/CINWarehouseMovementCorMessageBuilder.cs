using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN
{
	public class CINWarehouseMovementCorMessageBuilder : CINImportMessageBuilder
	{
		public enum LineMode
		{
			Edit, New, Deleted
		}

		public CINWarehouseMovementCorMessageBuilder(ICINHeader header, ICINHeader previousValues, string messageId) : base(header, messageId)
		{
			this.previousValues = previousValues;
		}

		protected override string MessageType => "WarehouseMovement-Cor";
		protected override string MessageBodyTag => "WarehouseMovementCor";

		protected override IEnumerable<XElement> GetMessageBodyElements => PopulateGoodsDifferences();

		IEnumerable<XElement> PopulateGoodsDifferences()
		{
			if (previousValues == null)
			{
				return null;
			}

			var lines = GetChangedLines(header, previousValues);

			return lines.Select(x => PopulateDetailedGoods(x));
		}

		public static IEnumerable<ICINLine> GetChangedLines(ICINHeader header, ICINHeader previousValues)
		{
			var lines = (from newValue in header.Bills
						 join oldValue in previousValues.Bills on newValue.PK equals oldValue.PK
						 where (newValue.NoPieces != oldValue.NoPieces || newValue.Mass != oldValue.Mass)
						 select new CINLineDifference(newValue, oldValue, LineMode.Edit)).ToList();

			var previousPKs = previousValues.Bills.Select(px => px.PK);
			var newPKs = header.Bills.Select(px => px.PK);

			lines.AddRange(header.Bills.Where(nx => !previousPKs.Contains(nx.PK)).Select(x => new CINLineDifference(x, x, LineMode.New)));
			lines.AddRange(previousValues.Bills.Where(nx => !newPKs.Contains(nx.PK)).Select(x => new CINLineDifference(x, x, LineMode.Deleted)));

			return lines;
		}

		readonly ICINHeader previousValues;

		public class CINLineDifference : ICINLine
		{
			public CINLineDifference(ICINLine newLine, ICINLine oldLine, LineMode mode)
			{
				this.newLine = newLine;
				this.oldLine = oldLine;
				lineMode = mode;
			}

			public ZGuid PK => newLine.PK;

			public ZString Type => newLine.Type;

			public ZString ReferenceNumber => newLine.ReferenceNumber;

			public ZInt NoPieces
			{
				get
				{
					switch (lineMode)
					{
						case LineMode.Deleted:
							return -oldLine.NoPieces;
						case LineMode.New:
							return newLine.NoPieces;
						default:
							return newLine.NoPieces - oldLine.NoPieces;
					}
				}
			}

			public ZInt TotalNoPieces
			{
				get
				{
					switch (lineMode)
					{
						case LineMode.Deleted:
							return 0;
						default:
							return newLine.TotalNoPieces;
					}
				}
			}

			public ZDecimal Mass
			{
				get
				{
					switch (lineMode)
					{
						case LineMode.Deleted:
							return -oldLine.Mass;
						case LineMode.New:
							return newLine.Mass;
						default:
							return newLine.Mass - oldLine.Mass;
					}
				}
			}

			public ZDecimal TotalMass
			{
				get
				{
					switch (lineMode)
					{
						case LineMode.Deleted:
							return 0;
						default:
							return newLine.TotalMass;
					}
				}
			}

			public ZString DescriptionOfGoods => newLine.DescriptionOfGoods;

			public bool IsAirwayBill => newLine.IsAirwayBill;

			readonly ICINLine newLine;
			readonly ICINLine oldLine;
			readonly LineMode lineMode;
		}
	}
}
