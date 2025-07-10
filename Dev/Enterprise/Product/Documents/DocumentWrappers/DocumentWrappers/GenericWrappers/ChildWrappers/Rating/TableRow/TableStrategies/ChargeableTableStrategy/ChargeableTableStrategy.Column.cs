using System;
using CargoWise.Types;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	partial class ChargeableTableStrategy
	{
		sealed class Column : IEquatable<Column>, IComparable<Column>
		{
			public Column(ColumnType type)
				: this(type, 0, ZString.Empty) { }

			public Column(ColumnType type, ZDecimal value, ZString unit)
			{
				this.Type = type;
				this.Value = value;
				this.Unit = unit;
			}

			public ColumnType Type { get; }
			ZDecimal Value { get; }
			ZString Unit { get; }

			public bool Equals(Column other)
				=> other != null
					&& other.Type == Type
					&& other.Value == Value;

			public override bool Equals(object obj) => Equals(obj as Column);

			public override int GetHashCode()
			{
				unchecked
				{
					uint tmp = (uint)Type.GetHashCode();
					tmp = ((tmp >> 5) | (tmp << 27)) ^ (uint)Value.GetHashCode();
					return (int)tmp;
				}
			}

			public override string ToString()
			{
				switch (Type)
				{
					case ColumnType.Base:
						return Res.GetString("2082cbc6-7440-4d60-97fc-c602e6cb6238", "Base");

					case ColumnType.Min:
						return Res.GetString("7c60c48a-e2f3-47e4-93a3-288d75f78df5", "Min");

					case ColumnType.Flat:
						return Res.GetString("4d7bd472-9a2f-48a9-a3f1-dceb1d273569", "Flat");

					case ColumnType.First:
						return Res.GetString("9760ecf0-a774-44c8-8b53-1bd39cf20a6f", "First");

					case ColumnType.Additional:
						return Res.GetString("30fd9495-069f-41c1-b943-7fa2a9912c21", "Additional");

					case ColumnType.Unit:
						return Res.GetString("00a42ec8-e482-4d7b-8b33-8b54ac23994e", "Per {0}", Unit);

					case ColumnType.Minus:
						return Res.GetString("0bf475f0-f075-4b38-bc2f-61b66e33317f", "-{0:0} per {1}", Value, Unit);

					case ColumnType.Plus:
						return Res.GetString("c6c73fa9-6b2f-444c-9635-f3ac75333081", "+{0:0} per {1}", Value, Unit);

					case ColumnType.Max:
						return Res.GetString("81ebe83b-0dab-4b9b-95dd-a36329b524ef", "Max");

					default:
						throw new InvalidOperationException(string.Format("Unknown column type. ('{0}')", Type));
				}
			}

			public int CompareTo(Column other)
			{
				var diff = Type.CompareTo(other.Type);

				if (diff != 0)
				{
					return diff;
				}

				diff = Value.CompareTo(other.Value);

				if (diff != 0)
				{
					return diff;
				}

				return Unit.CompareTo(other.Unit);
			}
		}
	}
}
