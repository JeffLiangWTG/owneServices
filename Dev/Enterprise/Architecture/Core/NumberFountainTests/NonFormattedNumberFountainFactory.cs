using System;
using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.NumberFountain.Testing
{
	internal class NonFormattedNumberFountainFactoryMethodCallSystemWithMainConnectionTest : NonFormattedNumberFountainFactoryMethodCallSystemTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}
	}

	internal class NonFormattedNumberFountainFactoryMethodCallSystemWithAnotherConnectionTest : NonFormattedNumberFountainFactoryMethodCallSystemTest
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

	internal abstract class NonFormattedNumberFountainFactoryMethodCallSystemTest : NonFormattedNumberFountainSystemTest
	{
		protected override INumberFountain GetFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue)
		{
			return new NonFormattedNumberFountainFactory(name, rollover, minValue, maxValue).New();
		}
	}

	internal class NonFormattedNumberFountainFactoryMethodCallWithMainConnectionTest : NonFormattedNumberFountainFactoryMethodCallTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Db.Connection.Dispose();
		}
	}

	internal class NonFormattedNumberFountainFactoryMethodCallWithAnotherConnectionTest : NonFormattedNumberFountainFactoryMethodCallTest
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

	internal abstract class NonFormattedNumberFountainFactoryMethodCallTest : NonFormattedNumberFountainFactoryTest
	{
		protected override INumberFountain GetFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue)
		{
			return new NonFormattedNumberFountainFactory(name, ownerPk, rollover, minValue, maxValue).New();
		}
	}

	internal class NonFormattedNumberFountainFactoryStaticCallSystemWithMainConnectionTest : NonFormattedNumberFountainFactoryStaticCallSystemTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Db.Connection.Dispose();
		}
	}

	internal class NonFormattedNumberFountainFactoryStaticCallSystemWithAnotherConnectionTest : NonFormattedNumberFountainFactoryStaticCallSystemTest
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

	internal abstract class NonFormattedNumberFountainFactoryStaticCallSystemTest : NonFormattedNumberFountainSystemTest
	{
		protected override INumberFountain GetFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue)
		{
			return NonFormattedNumberFountainFactory.New(name, rollover, minValue, maxValue);
		}
	}

	internal class NonFormattedNumberFountainFactoryStaticCallWithMainConnectionTest : NonFormattedNumberFountainFactoryStaticCallTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Db.Connection.Dispose();
		}
	}

	internal class NonFormattedNumberFountainFactoryStaticCallWithAnotherConnectionTest : NonFormattedNumberFountainFactoryStaticCallTest
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

	internal abstract class NonFormattedNumberFountainFactoryStaticCallTest : NonFormattedNumberFountainFactoryTest
	{
		protected override INumberFountain GetFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue)
		{
			return NonFormattedNumberFountainFactory.New(name, ownerPk, rollover, minValue, maxValue);
		}
	}

	internal abstract class NonFormattedNumberFountainSystemTest : NonFormattedNumberFountainFactoryTest
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
	internal abstract class NonFormattedNumberFountainFactoryTest : NonFormattedNumberFountainTest
	{
	}
}
