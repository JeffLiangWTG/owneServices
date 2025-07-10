using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.NumberFountain.Internal;

namespace Enterprise.NumberFountain.Testing
{
	class FormattedNumberFountainWithMainConnectionTest : FormattedNumberFountainTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}
	}

	class FormattedNumberFountainWithAnotherConnectionTest : FormattedNumberFountainTest
	{
		protected override DbConnection TestConnection()
		{
			var conn = Db.NewExtraConnectionToMainDb();
			connections.Add(conn);
			return conn;
		}

		readonly List<DbConnection> connections = new List<DbConnection>();

		protected override void TearDown()
		{
			base.TearDown();
			foreach (var conn in connections)
			{
				conn.Dispose();
			}
		}
	}

	abstract class FormattedNumberFountainTest : NumberFountainTestBase
	{
		const string Prefix = "pref ";
		const string Suffix = " suf!";
		const int FormatDigits = 10;

		protected override void Constructor(string name, Guid ownerPk)
		{
			const bool rollover = true;
			const int minValue = 1;
			const int maxValue = 9;
			const string prefix = "asd";
			const string suffix = "qwer";
			const int formatDigits = 15;

			var fountain =
				(IFormattedFountain)GetFormattedFountain(name, ownerPk, rollover, minValue, maxValue, prefix, suffix, formatDigits);

			TestFountainConstructor(fountain, name, ownerPk, rollover, minValue, maxValue);
			TestFormattedFountainConstructor(fountain, prefix, suffix, formatDigits);
		}

		static void TestFormattedFountainConstructor(IFormattedFountain fountain, string prefix, string suffix,
			int formatDigits)
		{
			AssertEquals(prefix, fountain.Prefix);
			AssertEquals(suffix, fountain.Suffix);
			AssertEquals(formatDigits, fountain.FormatDigits);
		}

		public void TestFixedValuesGetNext()
		{
			FixedValuesGetNext(GetName(), Guid.Empty, TestConnection());
		}

		public void TestFixedValuesGetNextOwned()
		{
			FixedValuesGetNext(GetName(), Guid.NewGuid(), TestConnection());
		}

		void FixedValuesGetNext(string name, Guid ownerPk, DbConnection connection)
		{
			using (connection.BeginTransactionWithManager())
			{
				const int minValue = 1;
				const int maxValue = 119;
				var fountain = GetFormattedFountain(name, ownerPk, false, minValue, maxValue, "C", "$", 5);
				var transaction = Transaction(connection);
				var dbConnection = Connection(connection);
				AssertExceptionThrown(
					typeof(ArgumentNullException),
					() => fountain.GetNextFormatted(null, null)
					);

				for (var i = minValue; i <= 9; i++)
				{
					AssertEquals("Next value", $"C0000{i}$", fountain.GetNextFormatted(dbConnection, transaction));
				}
				for (var i = 10; i <= 99; i++)
				{
					AssertEquals("Next value", $"C000{i}$", fountain.GetNextFormatted(dbConnection, transaction));
				}
				for (var i = 100; i <= maxValue; i++)
				{
					AssertEquals("Next value", $"C00{i}$", fountain.GetNextFormatted(dbConnection, transaction));
				}

				AssertExceptionThrown(
					typeof(NumberFountainMaximumValueReachedException),
					() => fountain.GetNextFormatted(dbConnection, transaction)
					);

				connection.CommitTransaction();
			}
		}

		protected override INumberFountain GetFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue)
		{
			return GetFormattedFountain(name, ownerPk, rollover, minValue, maxValue, Prefix, Suffix, FormatDigits);
		}

		protected virtual INumberFountain GetFormattedFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue, string prefix, string suffix, int formatDigits)
		{
			return new FormattedNumberFountain(name, ownerPk, rollover, minValue, maxValue, prefix, suffix, formatDigits);
		}

		protected override string IntToStringForGetNext(int i)
		{
			return Prefix + i.ToString().PadLeft(FormatDigits, '0') + Suffix;
		}
	}
}
