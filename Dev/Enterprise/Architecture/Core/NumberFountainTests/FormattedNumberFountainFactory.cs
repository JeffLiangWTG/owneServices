using System;
using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.NumberFountain.Testing
{
	internal class FormattedNumberFountainFactoryMethodCallWithMainConnectionTest : FormattedNumberFountainFactoryMethodCallTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}
	}

	internal class FormattedNumberFountainFactoryMethodCallWithAnotherConnectionTest : FormattedNumberFountainFactoryMethodCallTest
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

	internal abstract class FormattedNumberFountainFactoryMethodCallTest : FormattedNumberFountainFactorySystemTest
	{
		protected override INumberFountain GetFormattedFountain(string name, Guid ownerPk, bool rollover, int minValue,
			int maxValue, string prefix, string suffix, int formatDigits)
		{
			Prefix = prefix;
			FormatDigits = formatDigits;
			Suffix = suffix;

			return new FormattedNumberFountainFactory(name, ownerPk, prefix, rollover,
				minValue, maxValue, formatDigits, suffix).New();
		}
	}

	internal class FormattedNumberFountainFactoryMethodCallSystemWithMainConnectionTest : FormattedNumberFountainFactoryMethodCallSystemTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}
	}

	internal class FormattedNumberFountainFactoryMethodCallSystemWithAnotherConnectionTest : FormattedNumberFountainFactoryMethodCallSystemTest
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

	internal abstract class FormattedNumberFountainFactoryMethodCallSystemTest : FormattedNumberFountainFactorySystemTest
	{
		protected override INumberFountain GetFormattedFountain(string name, Guid ownerPk, bool rollover, int minValue,
			int maxValue, string prefix, string suffix, int formatDigits)
		{
			Prefix = prefix;
			FormatDigits = formatDigits;
			Suffix = suffix;

			return new FormattedNumberFountainFactory(name, prefix, rollover,
				minValue, maxValue, formatDigits, suffix).New();
		}
	}

	internal class FormattedNumberFountainFactoryStaticCallSystemWithMainConnectionTest : FormattedNumberFountainFactoryStaticCallSystemTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}
	}

	internal class FormattedNumberFountainFactoryStaticCallSystemWithAnotherConnectionTest : FormattedNumberFountainFactoryStaticCallSystemTest
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

	internal abstract class FormattedNumberFountainFactoryStaticCallSystemTest : FormattedNumberFountainFactorySystemTest
	{
		protected override INumberFountain GetFormattedFountain(string name, Guid ownerPk, bool rollover, int minValue,
			int maxValue, string prefix, string suffix, int formatDigits)
		{
			Prefix = prefix;
			FormatDigits = formatDigits;
			Suffix = suffix;

			return FormattedNumberFountainFactory.New(name, prefix, rollover, minValue, maxValue, formatDigits, suffix);
		}
	}

	internal class FormattedNumberFountainFactoryStaticCallWithMainConnectionTest : FormattedNumberFountainFactoryStaticCallTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}
	}

	internal class FormattedNumberFountainFactoryStaticCallWithAnotherConnectionTest : FormattedNumberFountainFactoryStaticCallTest
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

	internal abstract class FormattedNumberFountainFactoryStaticCallTest : FormattedNumberFountainFactoryTest
	{
		protected override INumberFountain GetFormattedFountain(string name, Guid ownerPk, bool rollover, int minValue,
			int maxValue, string prefix, string suffix, int formatDigits)
		{
			Prefix = prefix;
			FormatDigits = formatDigits;
			Suffix = suffix;

			return FormattedNumberFountainFactory.New(name, ownerPk, prefix, rollover, minValue, maxValue, formatDigits,
				suffix);
		}
	}

	internal abstract class FormattedNumberFountainFactorySystemTest : FormattedNumberFountainFactoryTest
	{
		protected override void Constructor(string name, Guid ownerPk)
		{
			// block test for unsystem (owned) fountain
			base.Constructor(name, Guid.Empty);
		}
	}

	/// <summary>
	/// All classes inherited from this one have only difference in fabric caller method
	/// </summary>
	internal abstract class FormattedNumberFountainFactoryTest : FormattedNumberFountainTest
	{
		protected FormattedNumberFountainFactoryTest()
		{
			Suffix = "";
			Prefix = "";
			FormatDigits = 2;
		}

		protected int FormatDigits { get; set; }
		protected string Prefix { get; set; }
		protected string Suffix { get; set; }

		protected override string IntToStringForGetNext(int i)
		{
			return Prefix + i.ToString().PadLeft(FormatDigits, '0') + Suffix;
		}
	}
}
