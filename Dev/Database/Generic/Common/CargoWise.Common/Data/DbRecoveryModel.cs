using System;
using System.Collections.Generic;

namespace CargoWise.Data
{
	public readonly struct DbRecoveryModel : IEquatable<DbRecoveryModel>
	{
		DbRecoveryModel(int code)
		{
			Code = code;
		}

		public int Code { get; }

		public string Name
		{
			get
			{
				switch (Code)
				{
					case SqlCodeFull:
						return "FULL";
					case SqlCodeSimple:
						return "SIMPLE";
					case SqlCodeBulkLogged:
						return "BULK_LOGGED";
					default:
						throw new NotSupportedException($"Invalid code: {Code}");
				}
			}
		}

		public bool Equals(DbRecoveryModel other) => Code == other.Code;
		public override bool Equals(object obj) => obj is DbRecoveryModel other && Equals(other);
		public override int GetHashCode() => Code;
		public override string ToString() => Name;

		public static bool operator ==(DbRecoveryModel left, DbRecoveryModel right) => left.Equals(right);
		public static bool operator !=(DbRecoveryModel left, DbRecoveryModel right) => !left.Equals(right);

		public static DbRecoveryModel Get(int code) => Get(m => m.Code == code) ?? throw new ArgumentOutOfRangeException(nameof(code));
		public static DbRecoveryModel Get(string name) => Get(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentOutOfRangeException(nameof(name));

		static DbRecoveryModel? Get(Func<DbRecoveryModel, bool> predicate)
		{
			foreach (var model in All)
			{
				if (predicate(model))
				{
					return model;
				}
			}

			return null;
		}

		public static DbRecoveryModel Full { get; } = new DbRecoveryModel(SqlCodeFull);
		public static DbRecoveryModel Simple { get; } = new DbRecoveryModel(SqlCodeSimple);
		public static DbRecoveryModel BulkLogged { get; } = new DbRecoveryModel(SqlCodeBulkLogged);

		public static IEnumerable<DbRecoveryModel> All
		{
			get
			{
				yield return Full;
				yield return Simple;
				yield return BulkLogged;
			}
		}

		const int SqlCodeFull = 1;
		const int SqlCodeBulkLogged = 2;
		const int SqlCodeSimple = 3;
	}
}
